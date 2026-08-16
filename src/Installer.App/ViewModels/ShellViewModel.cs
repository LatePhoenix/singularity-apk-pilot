using System.IO;
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
    private readonly IRecoveryService _recovery;
    private readonly IDiagnosticsService _diagnostics;
    private readonly IManifestService _manifests;
    private readonly IUserDataPaths _userData;
    private readonly IAppLogger _logger;
    private readonly Dictionary<WizardStep, WizardPageViewModel> _pages;
    private CancellationTokenSource? _installCts;

    public ShellViewModel(
        IWizardFlowService flow,
        IDeviceService devices,
        IDeviceMonitorService monitor,
        IInstallService install,
        IRecoveryService recovery,
        IDiagnosticsService diagnostics,
        IManifestService manifests,
        IUserDataPaths userData,
        IAppLogger logger)
    {
        _flow = flow;
        _devices = devices;
        _monitor = monitor;
        _install = install;
        _recovery = recovery;
        _diagnostics = diagnostics;
        _manifests = manifests;
        _userData = userData;
        _logger = logger;
        _pages = new Dictionary<WizardStep, WizardPageViewModel>
        {
            [WizardStep.Welcome] = new WelcomePageViewModel(),
            [WizardStep.ConnectDevice] = new ConnectDevicePageViewModel(),
            [WizardStep.DeviceDetected] = new DeviceDetectedPageViewModel(),
            [WizardStep.Authorization] = new AuthorizationPageViewModel(),
            [WizardStep.DeveloperMode] = new DeveloperModePageViewModel(),
            [WizardStep.ReadyToInstall] = new ReadyToInstallPageViewModel(),
            [WizardStep.Installing] = new InstallingPageViewModel(),
            [WizardStep.InstallProblem] = new InstallProblemPageViewModel(),
            [WizardStep.Complete] = new CompletePageViewModel()
        };

        var loaded = _manifests.Load();
        Manifest = loaded.IsSuccess && loaded.Value is not null ? loaded.Value : InstallManifest.Placeholder;
        PayloadWarning = loaded.IsSuccess
            ? File.Exists(Manifest.ApkPath)
                ? ""
                : "The app file is missing from this installer. Place the .apk in payloads\\current and restart."
            : loaded.Error ?? "";
        State = _flow.CreateInitialState(Manifest);
        ApplyState();
        _monitor.DevicesChanged += OnDevicesChanged;
    }

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
        State.CurrentStep is WizardStep.InstallProblem or WizardStep.Complete;

    public bool ShowPrimary => State.CurrentStep != WizardStep.Installing;

    public bool ShowCancel => State.CurrentStep == WizardStep.Installing;

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
                Advance(WizardTrigger.Continue, await DetectPrimaryAsync());
                break;
            case WizardStep.DeviceDetected:
                Advance(WizardTrigger.Continue, State.Device);
                break;
            case WizardStep.Authorization:
                Advance(WizardTrigger.ConfirmAuthorization, await DetectPrimaryAsync() ?? State.Device);
                break;
            case WizardStep.DeveloperMode:
                Advance(WizardTrigger.ConfirmDeveloperMode, await DetectPrimaryAsync() ?? State.Device);
                break;
            case WizardStep.ReadyToInstall:
                await InstallAsync();
                break;
            case WizardStep.InstallProblem:
                await AutoFixOrRetryAsync();
                break;
            case WizardStep.Complete:
                Advance(WizardTrigger.Done);
                break;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _installCts?.Cancel();
        Advance(WizardTrigger.Cancel, State.Device);
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

    private async Task InstallAsync()
    {
        if (State.Device is null)
        {
            Advance(WizardTrigger.Continue);
            return;
        }

        Advance(WizardTrigger.Install, State.Device);
        _installCts = new CancellationTokenSource();
        try
        {
            var request = new InstallRequest(Manifest, State.Device);
            var result = await _install.InstallAsync(request, _installCts.Token);
            if (!result.Success && result.Error is not null)
            {
                result = result with { SuggestedActions = _recovery.Suggest(result.Error.Value, Manifest) };
            }

            Advance(WizardTrigger.InstallFinished, State.Device, result);
        }
        catch (OperationCanceledException)
        {
            Advance(WizardTrigger.Cancel, State.Device);
        }
        catch (Exception ex)
        {
            _logger.Error("Install threw.", ex);
            var failed = InstallResult.Failed(InstallError.UnknownInstallFailure, ex.Message, _recovery.Suggest(InstallError.UnknownInstallFailure, Manifest));
            Advance(WizardTrigger.InstallFinished, State.Device, failed);
        }
    }

    private async Task AutoFixOrRetryAsync()
    {
        if (State.Device is null || State.LastInstallResult is null)
        {
            await RefreshDeviceAsync();
            Advance(WizardTrigger.Retry, await DetectPrimaryAsync());
            return;
        }

        var request = new InstallRequest(Manifest, State.Device);
        Advance(WizardTrigger.AutoFix, State.Device);
        try
        {
            var fixedResult = await _recovery.TryAutoFixAsync(request, State.LastInstallResult);
            var result = fixedResult ?? State.LastInstallResult;
            if (result is { Success: false, Error: not null })
            {
                result = result with { SuggestedActions = _recovery.Suggest(result.Error.Value, Manifest) };
            }

            Advance(WizardTrigger.InstallFinished, State.Device, result);
        }
        catch (Exception ex)
        {
            _logger.Error("Auto-fix failed.", ex);
            Advance(WizardTrigger.InstallFinished, State.Device, State.LastInstallResult);
        }
    }

    private async Task<DeviceInfo?> DetectPrimaryAsync()
    {
        var detected = await _devices.DetectAsync();
        return _devices.SelectPrimary(detected);
    }

    private async Task RefreshDeviceAsync()
    {
        var primary = await DetectPrimaryAsync();
        if (primary is not null)
        {
            Advance(WizardTrigger.DeviceRefresh, primary);
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

        var primary = _devices.SelectPrimary(devices);
        if (primary is null)
        {
            return;
        }

        Advance(WizardTrigger.DeviceRefresh, primary);
    }

    private void Advance(WizardTrigger trigger, DeviceInfo? device = null, InstallResult? result = null)
    {
        State = _flow.Advance(State, trigger, device, result);
        ApplyState();
    }

    private void ApplyState()
    {
        CurrentPage = _pages[State.CurrentStep];
        CurrentPage.Apply(State);
        OnPropertyChanged(nameof(ShowSecondaryExport));
        OnPropertyChanged(nameof(ShowPrimary));
        OnPropertyChanged(nameof(ShowCancel));
    }
}
