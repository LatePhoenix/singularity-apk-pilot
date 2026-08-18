using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.App.ViewModels.Wizard;
using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.App.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IWizardFlowService _flow;
    private readonly IDeviceService _devices;
    private readonly IDeviceMonitorService _monitor;
    private readonly IInstallService _install;
    private readonly IInstallSetFactory _installSets;
    private readonly IRecoveryService _recovery;
    private readonly IDiagnosticsService _diagnostics;
    private readonly IManifestService _manifests;
    private readonly IUserDataPaths _userData;
    private readonly IWirelessAdbService _wireless;
    private readonly IDeviceHealthService _health;
    private readonly IUpdateCheckService _updates;
    private readonly IAdbClient _adb;
    private readonly IAppLogger _logger;
    private readonly Dictionary<WizardStep, WizardPageViewModel> _pages;
    private CancellationTokenSource? _installCts;
    private InstallRequest? _lastRequest;

    public ShellViewModel(
        IWizardFlowService flow,
        IDeviceService devices,
        IDeviceMonitorService monitor,
        IInstallService install,
        IInstallSetFactory installSets,
        IApkInspector inspector,
        IRecentsStore recents,
        IRecoveryService recovery,
        IDiagnosticsService diagnostics,
        IManifestService manifests,
        IUserDataPaths userData,
        IWirelessAdbService wireless,
        IDeviceHealthService health,
        IUpdateCheckService updates,
        IAdbClient adb,
        IAppLogger logger)
    {
        _flow = flow;
        _devices = devices;
        _monitor = monitor;
        _install = install;
        _installSets = installSets;
        _recovery = recovery;
        _diagnostics = diagnostics;
        _manifests = manifests;
        _userData = userData;
        _wireless = wireless;
        _health = health;
        _updates = updates;
        _adb = adb;
        _logger = logger;
        _pages = new Dictionary<WizardStep, WizardPageViewModel>
        {
            [WizardStep.Welcome] = new WelcomePageViewModel(),
            [WizardStep.ConnectDevice] = new ConnectDevicePageViewModel(),
            [WizardStep.DeviceDetected] = new DeviceDetectedPageViewModel(),
            [WizardStep.Authorization] = new AuthorizationPageViewModel(),
            [WizardStep.DeveloperMode] = new DeveloperModePageViewModel(),
            [WizardStep.ReadyToInstall] = new ReadyToInstallPageViewModel(inspector, recents, installSets),
            [WizardStep.Installing] = new InstallingPageViewModel(),
            [WizardStep.InstallProblem] = new InstallProblemPageViewModel(),
            [WizardStep.Complete] = new CompletePageViewModel()
        };

        ChoosePage.FilesChanged += () => OnPropertyChanged(nameof(CanPrimary));
        ChoosePage.UseWifiRequested += () => _ = UseWifiFromUsbAsync();
        ConnectPage.ConnectRememberedRequested += () => _ = ConnectRememberedWifiAsync();
        ConnectPage.ConnectAdvancedRequested += request => _ = ConnectAdvancedWifiAsync(request);
        ConnectPage.BindEndpoint(_wireless.LastEndpoint);
        CompletePage.OpenRequested += () => _ = OpenOnDeviceAsync();
        ProblemPage.PolicyRetryRequested += policy => _ = RetryWithPolicyAsync(policy);

        var loaded = _manifests.Load();
        Manifest = loaded.IsSuccess && loaded.Value is not null ? loaded.Value : InstallManifest.Session;
        PayloadWarning = loaded.IsSuccess ? "" : loaded.Error ?? "";
        State = _flow.CreateInitialState(Manifest);
        ApplyState();
        _monitor.DevicesChanged += OnDevicesChanged;
        _ = CheckForUpdateAsync();
    }

    private ReadyToInstallPageViewModel ChoosePage =>
        (ReadyToInstallPageViewModel)_pages[WizardStep.ReadyToInstall];

    private ConnectDevicePageViewModel ConnectPage =>
        (ConnectDevicePageViewModel)_pages[WizardStep.ConnectDevice];

    private DeviceDetectedPageViewModel DetectedPage =>
        (DeviceDetectedPageViewModel)_pages[WizardStep.DeviceDetected];

    private CompletePageViewModel CompletePage =>
        (CompletePageViewModel)_pages[WizardStep.Complete];

    private InstallProblemPageViewModel ProblemPage =>
        (InstallProblemPageViewModel)_pages[WizardStep.InstallProblem];

    private WelcomePageViewModel WelcomePage =>
        (WelcomePageViewModel)_pages[WizardStep.Welcome];

    public InstallManifest Manifest { get; }

    [ObservableProperty]
    private WizardState state = null!;

    [ObservableProperty]
    private WizardPageViewModel currentPage = null!;

    [ObservableProperty]
    private string payloadWarning = "";

    [ObservableProperty]
    private string diagnosticsPath = "";

    public bool ShowSecondaryExport =>
        State.CurrentStep is WizardStep.ConnectDevice
            or WizardStep.Authorization
            or WizardStep.DeveloperMode
            or WizardStep.InstallProblem
            or WizardStep.Complete;

    public bool ShowPrimary => State.CurrentStep != WizardStep.Installing;

    public bool ShowCancel => State.CurrentStep == WizardStep.Installing;

    public bool CanPrimary => State.CurrentStep != WizardStep.ReadyToInstall || ChoosePage.HasFiles;

    [RelayCommand]
    private async Task PrimaryAsync()
    {
        switch (State.CurrentStep)
        {
            case WizardStep.Welcome:
                await _monitor.StartAsync();
                Advance(WizardTrigger.Start);
                break;
            case WizardStep.ConnectDevice:
                await ContinueFromConnectAsync();
                break;
            case WizardStep.DeviceDetected:
                Advance(WizardTrigger.Continue, DetectedPage.SelectedDevice ?? State.Device, readyDevices: State.Ready);
                break;
            case WizardStep.Authorization:
                await ContinueFromGateAsync(WizardTrigger.ConfirmAuthorization);
                break;
            case WizardStep.DeveloperMode:
                await ContinueFromGateAsync(WizardTrigger.ConfirmDeveloperMode);
                break;
            case WizardStep.ReadyToInstall:
                await InstallAsync();
                break;
            case WizardStep.InstallProblem:
                await AutoFixOrRetryAsync();
                break;
            case WizardStep.Complete:
                ChoosePage.ClearFiles();
                Advance(WizardTrigger.Done);
                break;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _installCts?.Cancel();
        Advance(WizardTrigger.Cancel, State.Device, readyDevices: State.Ready);
    }

    [RelayCommand]
    private async Task ExportDiagnosticsAsync()
    {
        try
        {
            Directory.CreateDirectory(_userData.DiagnosticsDirectory);
            var info = await _diagnostics.ExportAsync(
                Manifest,
                State.Device,
                State.LastInstallResult,
                null,
                _userData.DiagnosticsDirectory);
            DiagnosticsPath = info.ZipPath;
            _logger.Info($"Diagnostics written to {info.ZipPath}");
        }
        catch (Exception ex)
        {
            _logger.Error("Diagnostics export failed.", ex);
            PayloadWarning = "Could not export diagnostics.";
        }
    }

    private async Task ContinueFromConnectAsync()
    {
        var detected = await _devices.DetectAsync();
        var primary = _devices.SelectPrimary(detected);
        DeviceHealth? health = null;
        if (primary is null || primary.State == DeviceConnectionState.NotConnected)
        {
            health = _health.Snapshot(detected);
        }

        Advance(WizardTrigger.Continue, primary, readyDevices: detected, health: health);
    }

    private async Task ContinueFromGateAsync(WizardTrigger trigger)
    {
        var detected = await _devices.DetectAsync();
        var primary = _devices.SelectPrimary(detected) ?? State.Device;
        DeviceHealth? health = null;
        if (primary is null || primary.State is DeviceConnectionState.NotConnected or DeviceConnectionState.Offline)
        {
            health = _health.Snapshot(detected);
        }

        Advance(trigger, primary, readyDevices: detected, health: health);
    }

    private async Task InstallAsync()
    {
        var paths = ChoosePage.SelectedPaths;
        if (paths.Count == 0)
        {
            PayloadWarning = "Add at least one APK file.";
            return;
        }

        if (State.Device is null)
        {
            Advance(WizardTrigger.Continue, readyDevices: State.Ready);
            return;
        }

        var sets = _installSets.Group(paths);
        if (sets.Count == 0)
        {
            PayloadWarning = "Add at least one APK file.";
            return;
        }

        PayloadWarning = "";
        var first = sets[0];
        State = State with
        {
            Manifest = sets.Count == 1
                ? InstallManifest.ForInstallSet(first, Manifest)
                : Manifest with { DisplayName = $"{sets.Count} apps", ApkPath = first.PrimaryPath }
        };
        Advance(WizardTrigger.Install, State.Device, readyDevices: State.Ready);
        _installCts = new CancellationTokenSource();
        try
        {
            InstallResult? result = null;
            for (var i = 0; i < sets.Count; i++)
            {
                var set = sets[i];
                var manifest = InstallManifest.ForInstallSet(set, Manifest);
                State = State with { Manifest = manifest };
                if (CurrentPage is InstallingPageViewModel installing)
                {
                    var label = string.IsNullOrWhiteSpace(set.DisplayName)
                        ? Path.GetFileName(set.PrimaryPath)
                        : set.DisplayName;
                    installing.StatusLabel = sets.Count == 1
                        ? $"Installing {label}"
                        : $"Installing {i + 1} of {sets.Count}: {label}";
                }

                var request = new InstallRequest(manifest, State.Device, Set: set);
                _lastRequest = request;
                result = await _install.InstallAsync(request, _installCts.Token);
                if (!result.Success)
                {
                    if (result.Error is not null)
                    {
                        result = result with { SuggestedActions = _recovery.Suggest(result.Error.Value, request.Manifest) };
                    }

                    Advance(WizardTrigger.InstallFinished, State.Device, result, State.Ready);
                    return;
                }
            }

            Advance(WizardTrigger.InstallFinished, State.Device, result, State.Ready);
        }
        catch (OperationCanceledException)
        {
            Advance(WizardTrigger.Cancel, State.Device, readyDevices: State.Ready);
        }
        catch (Exception ex)
        {
            _logger.Error("Install threw.", ex);
            var failed = InstallResult.Failed(InstallError.UnknownInstallFailure, ex.Message, _recovery.Suggest(InstallError.UnknownInstallFailure, Manifest));
            Advance(WizardTrigger.InstallFinished, State.Device, failed, State.Ready);
        }
    }

    private async Task AutoFixOrRetryAsync()
    {
        if (State.Device is null || State.LastInstallResult is null)
        {
            var detected = await _devices.DetectAsync();
            Advance(WizardTrigger.Retry, _devices.SelectPrimary(detected), readyDevices: detected);
            return;
        }

        var request = _lastRequest ?? RebuildLastRequest();
        if (request is null)
        {
            Advance(WizardTrigger.Retry, State.Device, readyDevices: State.Ready);
            return;
        }

        _lastRequest = request;
        Advance(WizardTrigger.AutoFix, State.Device, readyDevices: State.Ready);
        try
        {
            var last = State.LastInstallResult;
            var fixedResult = await _recovery.TryAutoFixAsync(request, last);
            var result = fixedResult ?? last;
            if (result is { Success: false, Error: not null })
            {
                result = result with { SuggestedActions = _recovery.Suggest(result.Error.Value, request.Manifest) };
            }

            Advance(WizardTrigger.InstallFinished, State.Device, result, State.Ready);
        }
        catch (Exception ex)
        {
            _logger.Error("Auto-fix failed.", ex);
            Advance(WizardTrigger.InstallFinished, State.Device, State.LastInstallResult, State.Ready);
        }
    }

    private async Task RetryWithPolicyAsync(InstallPolicy policy)
    {
        if (State.Device is null)
        {
            return;
        }

        var baseRequest = _lastRequest ?? RebuildLastRequest();
        if (baseRequest is null)
        {
            return;
        }

        var request = baseRequest with { PolicyOverride = policy };
        _lastRequest = request;
        Advance(WizardTrigger.Retry, State.Device, readyDevices: State.Ready);
        try
        {
            var result = await _install.InstallAsync(request);
            if (result is { Success: false, Error: not null })
            {
                result = result with { SuggestedActions = _recovery.Suggest(result.Error.Value, request.Manifest) };
            }

            Advance(WizardTrigger.InstallFinished, State.Device, result, State.Ready);
        }
        catch (Exception ex)
        {
            _logger.Error("Replace/remove retry failed.", ex);
            Advance(WizardTrigger.InstallFinished, State.Device, State.LastInstallResult, State.Ready);
        }
    }

    private InstallRequest? RebuildLastRequest()
    {
        if (State.Device is null)
        {
            return null;
        }

        var files = State.LastInstallResult?.Plan?.Files;
        if (files is { Count: > 0 })
        {
            var set = _installSets.Group(files).FirstOrDefault();
            if (set is not null)
            {
                return new InstallRequest(InstallManifest.ForInstallSet(set, Manifest), State.Device, Set: set);
            }
        }

        var failedPath = State.LastInstallResult?.Plan?.ApkPath;
        if (string.IsNullOrWhiteSpace(failedPath))
        {
            return new InstallRequest(State.Manifest, State.Device);
        }

        return new InstallRequest(InstallManifest.ForSelectedApk(failedPath, Manifest), State.Device);
    }

    private async Task OpenOnDeviceAsync()
    {
        if (State.Device is null || !State.Manifest.CanVerifyPackage)
        {
            return;
        }

        try
        {
            var activity = await _adb.ResolveLauncherAsync(State.Device.Serial, State.Manifest.AppId)
                           ?? State.LastInstallResult?.Plan?.LauncherActivity;
            var launched = await _adb.LaunchAsync(State.Device.Serial, State.Manifest.AppId, activity);
            if (!launched.Succeeded)
            {
                PayloadWarning = "Could not open the app on the device. Use Library → Unknown Sources.";
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Launch failed.", ex);
            PayloadWarning = "Could not open the app on the device. Use Library → Unknown Sources.";
        }
    }

    private async Task CheckForUpdateAsync()
    {
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            var message = await _updates.GetNewerInstallerMessageAsync(version);
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            WelcomePage.UpdateMessage = message;
            WelcomePage.UpdateUrl = _updates.LatestSetupUrl;
        }
        catch (Exception ex)
        {
            _logger.Warn($"Update check skipped: {ex.Message}");
        }
    }

    private async Task ConnectRememberedWifiAsync()
    {
        var endpoint = _wireless.LastEndpoint;
        if (endpoint is null)
        {
            PayloadWarning = "No saved Wi-Fi address yet. Plug in with a USB cable first, or enter an address in the Wi-Fi form.";
            return;
        }

        await RunWirelessAsync(() => _wireless.ConnectAsync(endpoint));
    }

    private Task ConnectAdvancedWifiAsync(WirelessFormRequest request)
    {
        if (request.Pairing is not null && !string.IsNullOrWhiteSpace(request.PairingCode))
        {
            var pairing = request.Pairing;
            var code = request.PairingCode;
            return RunWirelessAsync(() => _wireless.PairThenConnectAsync(pairing, code, request.Connect));
        }

        return RunWirelessAsync(() => _wireless.ConnectAsync(request.Connect));
    }

    private Task UseWifiFromUsbAsync()
    {
        if (State.Device is null || State.Device.IsWireless)
        {
            PayloadWarning = "Connect the device with a USB cable first, then switch to Wi-Fi.";
            return Task.CompletedTask;
        }

        var serial = State.Device.Serial;
        return RunWirelessAsync(() => _wireless.EnableFromUsbAsync(serial));
    }

    private async Task RunWirelessAsync(Func<Task<Installer.Core.Utilities.Result<WirelessEndpoint>>> operation)
    {
        PayloadWarning = "";
        ConnectPage.WifiStatus = "";
        ConnectPage.IsWifiBusy = true;
        ChoosePage.IsWifiBusy = true;
        try
        {
            var result = await operation();
            if (!result.IsSuccess || result.Value is null)
            {
                var message = result.Error ?? "Could not connect over Wi-Fi.";
                PayloadWarning = message;
                ConnectPage.WifiStatus = message;
                return;
            }

            ConnectPage.BindEndpoint(_wireless.LastEndpoint);
            var detected = await _devices.DetectAsync();
            var primary = _devices.SelectPrimary(detected);
            if (primary is null)
            {
                PayloadWarning = "Wi-Fi is on. Wait a moment, then continue.";
                return;
            }

            Advance(WizardTrigger.DeviceRefresh, primary, readyDevices: detected);
        }
        catch (Exception ex)
        {
            _logger.Error("Wi-Fi connection failed.", ex);
            PayloadWarning = "Could not connect over Wi-Fi.";
            ConnectPage.WifiStatus = PayloadWarning;
        }
        finally
        {
            ConnectPage.IsWifiBusy = false;
            ChoosePage.IsWifiBusy = false;
        }
    }

    private void OnDevicesChanged(object? sender, IReadOnlyList<DeviceInfo> devices)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            HandleDevices(devices);
            return;
        }

        dispatcher.Invoke(() => HandleDevices(devices));
    }

    private void HandleDevices(IReadOnlyList<DeviceInfo> devices)
    {
        if (State.CurrentStep is WizardStep.Welcome or WizardStep.Installing or WizardStep.Complete or WizardStep.InstallProblem)
        {
            return;
        }

        var ready = devices.Where(d => d.State == DeviceConnectionState.ConnectedReady).ToList();
        if (State.CurrentStep == WizardStep.DeviceDetected && ready.Count >= 2)
        {
            var keep = ready.FirstOrDefault(d => d.Serial == DetectedPage.SelectedDevice?.Serial)
                       ?? ready.FirstOrDefault(d => d.Serial == State.Device?.Serial)
                       ?? _devices.SelectPrimary(devices);
            Advance(WizardTrigger.DeviceRefresh, keep, readyDevices: devices);
            return;
        }

        var primary = _devices.SelectPrimary(devices);
        if (primary is null)
        {
            if (State.CurrentStep is WizardStep.ConnectDevice or WizardStep.Authorization or WizardStep.DeveloperMode)
            {
                Advance(WizardTrigger.DeviceRefresh, null, readyDevices: devices, health: _health.Snapshot(devices));
            }

            return;
        }

        Advance(WizardTrigger.DeviceRefresh, primary, readyDevices: devices);
    }

    private void Advance(
        WizardTrigger trigger,
        DeviceInfo? device = null,
        InstallResult? result = null,
        IReadOnlyList<DeviceInfo>? readyDevices = null,
        DeviceHealth? health = null)
    {
        State = _flow.Advance(State, trigger, device, result, readyDevices, health);
        ApplyState();
    }

    private void ApplyState()
    {
        CurrentPage = _pages[State.CurrentStep];
        CurrentPage.Apply(State);
        OnPropertyChanged(nameof(ShowSecondaryExport));
        OnPropertyChanged(nameof(ShowPrimary));
        OnPropertyChanged(nameof(ShowCancel));
        OnPropertyChanged(nameof(CanPrimary));
    }
}
