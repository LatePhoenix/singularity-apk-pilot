using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.App.Services;
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
    private readonly IManifestService _manifests;
    private readonly IWirelessAdbService _wireless;
    private readonly IDeviceHealthService _health;
    private readonly IUpdateCheckService _updates;
    private readonly IAdbClient _adb;
    private readonly IInstalledAppService _installedApps;
    private readonly IApkInspector _inspector;
    private readonly IRecentsStore _recents;
    private readonly IAppLogger _logger;
    private readonly ITroubleshootingService _troubleshoot;
    private readonly IQuestUsbHelperService _usbHelper;
    private readonly ISendReportUi _sendReport;
    private readonly ITroubleshootUi _troubleshootUi;
    private readonly IGuideUi _guideUi;
    private readonly IGuideCoach _guideCoach;
    private bool _helperOpen;
    private readonly Installer.Core.Services.Content.TroubleshootCopyDeck _troubleshootCopy;
    private readonly Dictionary<WizardStep, WizardPageViewModel> _pages;
    private readonly ITempFileService _temp;
    private CancellationTokenSource? _installCts;
    private CancellationTokenSource? _uninstallCts;
    private InstallRequest? _lastRequest;
    private int _installBusy;

    public ShellViewModel(
        IWizardFlowService flow,
        IDeviceService devices,
        IDeviceMonitorService monitor,
        IInstallService install,
        IInstallSetFactory installSets,
        IApkInspector inspector,
        IRecentsStore recents,
        IRecoveryService recovery,
        IManifestService manifests,
        IWirelessAdbService wireless,
        IDeviceHealthService health,
        IUpdateCheckService updates,
        IAdbClient adb,
        IInstalledAppService installedApps,
        ITroubleshootingService troubleshoot,
        IQuestUsbHelperService usbHelper,
        ISendReportUi sendReport,
        ITroubleshootUi troubleshootUi,
        IGuideUi guideUi,
        IGuideCoach guideCoach,
        Installer.Core.Services.Content.TroubleshootCopyDeck troubleshootCopy,
        ITempFileService temp,
        IAppLogger logger)
    {
        _flow = flow;
        _devices = devices;
        _monitor = monitor;
        _install = install;
        _installSets = installSets;
        _recovery = recovery;
        _manifests = manifests;
        _wireless = wireless;
        _health = health;
        _updates = updates;
        _adb = adb;
        _installedApps = installedApps;
        _inspector = inspector;
        _recents = recents;
        _troubleshoot = troubleshoot;
        _usbHelper = usbHelper;
        _sendReport = sendReport;
        _troubleshootUi = troubleshootUi;
        _guideUi = guideUi;
        _guideCoach = guideCoach;
        _troubleshootCopy = troubleshootCopy;
        _temp = temp;
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
            [WizardStep.Complete] = new CompletePageViewModel(),
            [WizardStep.InstalledApps] = new InstalledAppsPageViewModel(),
            [WizardStep.Troubleshoot] = new TroubleshootPageViewModel()
        };

        ChoosePage.FilesChanged += () =>
        {
            OnPropertyChanged(nameof(CanPrimary));
            RefreshGuide();
        };
        ChoosePage.UseWifiRequested += () => _ = UseWifiFromUsbAsync();
        ChoosePage.OpenInstalledAppsRequested += OpenInstalledApps;
        ConnectPage.ConnectRememberedRequested += () => _ = ConnectRememberedWifiAsync();
        ConnectPage.ConnectAdvancedRequested += request => _ = ConnectAdvancedWifiAsync(request);
        ConnectPage.BindEndpoint(_wireless.LastEndpoint);
        CompletePage.OpenRequested += () => _ = OpenOnDeviceAsync();
        CompletePage.OpenInstalledAppsRequested += OpenInstalledApps;
        ProblemPage.ActionRequested += action => _ = ApplyRecoveryAsync(action);
        AppsPage.RefreshRequested += () => _ = LoadInstalledAppsAsync();
        AppsPage.UninstallRequested += packageId => _ = UninstallAppAsync(packageId);
        AppsPage.CancelUninstallRequested += CancelUninstall;
        AppsPage.EnrichVisibleRequested += () => _ = EnrichVisibleAppsAsync();
        HelpPage.FamilySelected += family => _ = ApplyTroubleshootFamilyAsync(family);
        HelpPage.SwitchToQuestRequested += family => _ = ApplyTroubleshootFamilyAsync(family);
        HelpPage.BackRequested += () => _ = GoTroubleshootBackAsync();
        HelpPage.ActionRequested += kind => _ = RunTroubleshootActionAsync(kind);
        HelpPage.ConfirmInstalledRequested += () => _ = ContinueFromTroubleshootAsync();
        HelpPage.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(TroubleshootPageViewModel.ActionBusy)
                or nameof(TroubleshootPageViewModel.ShowFamilyPicker)
                or nameof(TroubleshootPageViewModel.PrimaryAction)
                or nameof(TroubleshootPageViewModel.CanGoBack))
            {
                OnPropertyChanged(nameof(CanPrimary));
                OnPropertyChanged(nameof(ShowPrimary));
                OnPropertyChanged(nameof(HelperPrimaryAction));
            }
        };
        _guideUi.ClosedByUser += () =>
        {
            IsGuideDocked = true;
            IsGuideVisible = true;
        };

        var loaded = _manifests.Load();
        Manifest = loaded.IsSuccess && loaded.Value is not null ? loaded.Value : InstallManifest.Session;
        PayloadWarning = loaded.IsSuccess ? "" : loaded.Error ?? "";
        State = _flow.CreateInitialState(Manifest);
        ApplyState();
        _monitor.DevicesChanged += OnDevicesChanged;
        _ = CheckForUpdateAsync();
        _ = Task.Run(() => _temp.DeleteStale(TimeSpan.FromDays(1)));
        _ = _monitor.StartAsync();
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

    private InstalledAppsPageViewModel AppsPage =>
        (InstalledAppsPageViewModel)_pages[WizardStep.InstalledApps];

    private WelcomePageViewModel WelcomePage =>
        (WelcomePageViewModel)_pages[WizardStep.Welcome];

    private TroubleshootPageViewModel HelpPage =>
        (TroubleshootPageViewModel)_pages[WizardStep.Troubleshoot];

    public TroubleshootPageViewModel TroubleshootPage => HelpPage;

    public InstallManifest Manifest { get; }

    [ObservableProperty]
    private WizardState state = null!;

    [ObservableProperty]
    private WizardPageViewModel currentPage = null!;

    [ObservableProperty]
    private string payloadWarning = "";

    [ObservableProperty]
    private string diagnosticsStatus = "";

    [ObservableProperty]
    private GuideScript guide = GuideScript.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDockedGuide))]
    [NotifyPropertyChangedFor(nameof(ShowMainBody))]
    [NotifyPropertyChangedFor(nameof(ShowMainIllustration))]
    [NotifyPropertyChangedFor(nameof(ShowMainHelp))]
    private bool isGuideVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDockedGuide))]
    private bool isGuideDocked = true;

    [ObservableProperty]
    private bool isGuideHelpExpanded;

    public bool ShowDockedGuide => IsGuideVisible && IsGuideDocked;

    public bool ShowMainBody => !IsGuideVisible;

    public bool ShowMainIllustration => !IsGuideVisible;

    public bool ShowMainHelp => !IsGuideVisible;

    public bool ShowSecondaryExport =>
        State.CurrentStep != WizardStep.Installing
        && State.CurrentStep != WizardStep.Troubleshoot
        && !AppsPage.IsBusy;

    public bool ShowNeedHelpConnecting =>
        State.CurrentStep is WizardStep.ConnectDevice
            or WizardStep.Authorization
            or WizardStep.DeveloperMode
        || (State.CurrentStep == WizardStep.InstallProblem && InstallProblems.NeedsConnectionHelp(State.LastInstallResult?.Error))
        || (State.CurrentStep == WizardStep.InstalledApps && AppsPage.NeedsConnectionHelp);

    public bool ShowGoBack =>
        State.CurrentStep is WizardStep.ConnectDevice
            or WizardStep.DeviceDetected
            or WizardStep.Authorization
            or WizardStep.DeveloperMode
            or WizardStep.ReadyToInstall
            or WizardStep.InstallProblem
            or WizardStep.Complete;

    public bool ShowStartOver =>
        State.CurrentStep is not WizardStep.Welcome
            and not WizardStep.ConnectDevice
            and not WizardStep.Installing
            and not WizardStep.Troubleshoot
        && !AppsPage.IsBusy;

    public bool ShowPrimary =>
        State.CurrentStep != WizardStep.Installing
        && State.CurrentStep != WizardStep.Troubleshoot
        && !AppsPage.IsBusy;

    public bool ShowHelperPrimary => !HelpPage.ShowFamilyPicker;

    public bool ShowCancel =>
        State.CurrentStep == WizardStep.Installing
        || AppsPage.IsBusy;

    public string CancelLabel => State.CurrentStep == WizardStep.Troubleshoot ? "Leave helper" : "Cancel";

    public string HelperPrimaryAction => HelpPage.PrimaryAction;

    public bool CanPrimary =>
        State.CurrentStep == WizardStep.Troubleshoot
            ? !HelpPage.ShowFamilyPicker && !HelpPage.ActionBusy
            : State.CurrentStep != WizardStep.ReadyToInstall || ChoosePage.HasFiles;

    [RelayCommand]
    private void PopOutGuide()
    {
        IsGuideVisible = true;
        IsGuideDocked = false;
        _guideUi.ShowPopOut(this);
    }

    [RelayCommand]
    private void DockGuide()
    {
        _guideUi.ClosePopOut();
        IsGuideDocked = true;
        IsGuideVisible = true;
    }

    [RelayCommand]
    private void HideGuide()
    {
        _guideUi.ClosePopOut();
        IsGuideDocked = true;
        IsGuideVisible = false;
    }

    [RelayCommand]
    private void ShowGuide()
    {
        IsGuideVisible = true;
        IsGuideDocked = true;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (!ShowGoBack)
        {
            return;
        }

        PayloadWarning = "";
        Advance(WizardTrigger.Back, State.Device, readyDevices: State.Ready, health: State.Health);
    }

    [RelayCommand]
    private void StartOver()
    {
        if (!ShowStartOver)
        {
            return;
        }

        CancelUninstall();
        _installCts?.Cancel();
        ChoosePage.ClearFiles();
        PayloadWarning = "";
        DiagnosticsStatus = "";
        var seen = _monitor.CurrentDevices;
        State = State with { Manifest = Manifest };
        Advance(
            WizardTrigger.StartOver,
            _devices.SelectPrimary(seen),
            readyDevices: seen,
            health: seen.Count > 0 ? _health.Snapshot(seen) : null);
    }

    [RelayCommand]
    private async Task PrimaryAsync()
    {
        switch (State.CurrentStep)
        {
            case WizardStep.Welcome:
                var seen = _monitor.CurrentDevices;
                Advance(
                    WizardTrigger.Start,
                    _devices.SelectPrimary(seen),
                    readyDevices: seen,
                    health: seen.Count > 0 ? _health.Snapshot(seen) : null);
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
                if (ProblemPage.PrimaryRecovery is not null)
                {
                    await ApplyRecoveryAsync(ProblemPage.PrimaryRecovery);
                }
                else
                {
                    await AutoFixOrRetryAsync();
                }
                break;
            case WizardStep.Complete:
                ChoosePage.ClearFiles();
                Advance(WizardTrigger.Done);
                break;
            case WizardStep.InstalledApps:
                CancelUninstall();
                Advance(WizardTrigger.CloseInstalledApps, State.Device, readyDevices: State.Ready);
                break;
            case WizardStep.Troubleshoot:
                if (HelpPage.PrimaryRunsAction)
                {
                    await RunTroubleshootActionAsync(HelpPage.ActionKind);
                }
                else
                {
                    await ContinueFromTroubleshootAsync();
                }
                break;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (State.CurrentStep == WizardStep.Installing)
        {
            _installCts?.Cancel();
            return;
        }

        if (State.CurrentStep == WizardStep.InstalledApps)
        {
            CancelUninstall();
            return;
        }

        if (State.CurrentStep == WizardStep.Troubleshoot)
        {
            Advance(WizardTrigger.CloseTroubleshoot, State.Device, readyDevices: State.Ready, health: State.Health);
        }
    }

    [RelayCommand]
    private Task ExportDiagnosticsAsync()
    {
        try
        {
            var result = _sendReport.Show(Manifest, State.Device, State.LastInstallResult);
            DiagnosticsStatus = result.Status;
            if (!string.IsNullOrWhiteSpace(result.Status))
            {
                _logger.Info($"Report send: {result.Status}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Diagnostics export failed.", ex);
            DiagnosticsStatus = "";
            PayloadWarning = "Could not send a report.";
        }

        return Task.CompletedTask;
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

    [RelayCommand]
    private async Task OpenTroubleshootAsync()
    {
        var detected = await _devices.DetectAsync();
        var primary = _devices.SelectPrimary(detected) ?? State.Device;
        var health = _health.Snapshot(detected);
        Advance(WizardTrigger.OpenTroubleshoot, primary, readyDevices: detected, health: health);
    }

    private async Task ApplyRecoveryAsync(RecoveryAction action)
    {
        switch (action.Kind)
        {
            case RecoveryActionKind.ShowCableHelp:
                await OpenTroubleshootAsync();
                return;
            case RecoveryActionKind.ShowAuthorization:
            case RecoveryActionKind.ShowDeveloperMode:
            case RecoveryActionKind.RetryDetection:
                await ResumeSetupAsync();
                return;
            case RecoveryActionKind.RestartAdbServer:
                try
                {
                    await _adb.RestartServerAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error("Connection helper restart failed.", ex);
                }

                await ResumeSetupAsync(retryInstallIfReady: true);
                return;
            case RecoveryActionKind.RetryInstall:
                if (InstallProblems.NeedsConnectionHelp(State.LastInstallResult?.Error)
                    || InstallProblems.NeedsDifferentFile(State.LastInstallResult?.Error))
                {
                    if (InstallProblems.NeedsDifferentFile(State.LastInstallResult?.Error))
                    {
                        Advance(WizardTrigger.Back, State.Device, readyDevices: State.Ready, health: State.Health);
                        return;
                    }

                    await ResumeSetupAsync();
                    return;
                }

                await AutoFixOrRetryAsync();
                return;
            case RecoveryActionKind.ExportDiagnostics:
                await ExportDiagnosticsAsync();
                return;
        }

        var policy = InstallProblems.PolicyFor(action.Kind);
        if (policy is not null)
        {
            await RetryWithPolicyAsync(policy.Value);
        }
    }

    private async Task ResumeSetupAsync(bool retryInstallIfReady = false)
    {
        var detected = await _devices.DetectAsync();
        var primary = _devices.SelectPrimary(detected);
        var health = detected.Count > 0 || primary is null ? _health.Snapshot(detected) : null;
        if (retryInstallIfReady && primary is { State: DeviceConnectionState.ConnectedReady })
        {
            Advance(WizardTrigger.ResumeSetup, primary, readyDevices: detected, health: health);
            if (State.CurrentStep == WizardStep.ReadyToInstall && ChoosePage.HasFiles)
            {
                await InstallAsync();
            }

            return;
        }

        Advance(WizardTrigger.ResumeSetup, primary, readyDevices: detected, health: health);
    }

    private async Task ContinueFromTroubleshootAsync()
    {
        var detected = await _devices.DetectAsync();
        var primary = _devices.SelectPrimary(detected);
        var health = _health.Snapshot(detected);
        Advance(WizardTrigger.Continue, primary, readyDevices: detected, health: health);
    }

    private async Task ApplyTroubleshootFamilyAsync(TroubleshootFamily family)
    {
        if (State.Troubleshoot is null)
        {
            return;
        }

        try
        {
            var detected = await _devices.DetectAsync();
            var primary = _devices.SelectPrimary(detected);
            var health = _health.Snapshot(detected);
            var session = _troubleshoot.SelectFamily(State.Troubleshoot, family, detected);
            Advance(WizardTrigger.DeviceRefresh, primary, readyDevices: detected, health: health, troubleshoot: session);
        }
        catch (Exception ex)
        {
            _logger.Error("Troubleshoot family switch failed.", ex);
        }
    }

    private async Task GoTroubleshootBackAsync()
    {
        if (State.Troubleshoot is null)
        {
            return;
        }

        try
        {
            var detected = await _devices.DetectAsync();
            var primary = _devices.SelectPrimary(detected);
            var health = _health.Snapshot(detected);
            var session = _troubleshoot.Back(State.Troubleshoot, detected);
            Advance(WizardTrigger.DeviceRefresh, primary, readyDevices: detected, health: health, troubleshoot: session);
        }
        catch (Exception ex)
        {
            _logger.Error("Troubleshoot back failed.", ex);
        }
    }

    private async Task RunTroubleshootActionAsync(TroubleshootActionKind kind)
    {
        HelpPage.ActionBusy = true;
        HelpPage.ActionStatus = "";
        try
        {
            switch (kind)
            {
                case TroubleshootActionKind.RestartAdbServer:
                    var restart = await _adb.RestartServerAsync();
                    if (!restart.Succeeded)
                    {
                        HelpPage.ActionStatus =
                            "The connection helper did not restart. Close other Android tools and try again.";
                        return;
                    }

                    HelpPage.ActionStatus = "Connection helper restarted.";
                    break;
                case TroubleshootActionKind.InstallUsbHelper:
                case TroubleshootActionKind.OpenDriverDownload:
                    if (_usbHelper.HasBundledInf)
                    {
                        HelpPage.ActionStatus = "Windows may ask for permission.";
                        var installed = await _usbHelper.TryInstallBundledInfAsync();
                        HelpPage.ActionStatus = installed
                            ? "USB support installed. Keep the headset plugged in, then check again."
                            : "Could not finish automatically. The Meta download page will open.";
                        if (!installed)
                        {
                            _usbHelper.OpenUrl(_usbHelper.QuestDriverUrl);
                        }
                    }
                    else
                    {
                        _usbHelper.OpenUrl(_usbHelper.QuestDriverUrl);
                        HelpPage.ActionStatus = "Install the USB helper from the page that opened, then tap I installed it.";
                    }
                    break;
                case TroubleshootActionKind.OpenPhoneUsbSupport:
                    _usbHelper.OpenUrl(_usbHelper.PhoneDriverUrl(State.Device?.Manufacturer));
                    HelpPage.ActionStatus = "Install the USB helper from the page that opened, then tap I installed it.";
                    break;
                case TroubleshootActionKind.ExportDiagnostics:
                    await ExportDiagnosticsAsync();
                    HelpPage.ActionStatus = string.IsNullOrWhiteSpace(DiagnosticsStatus)
                        ? "You can tap Send a report when you are ready."
                        : DiagnosticsStatus;
                    break;
            }

            var detected = await _devices.DetectAsync();
            var primary = _devices.SelectPrimary(detected);
            var health = _health.Snapshot(detected);
            Advance(WizardTrigger.DeviceRefresh, primary, readyDevices: detected, health: health);
        }
        catch (Exception ex)
        {
            _logger.Error("Troubleshoot action failed.", ex);
            HelpPage.ActionStatus = "That automatic step did not finish. You can still follow the instructions on this page.";
        }
        finally
        {
            HelpPage.ActionBusy = false;
        }
    }

    private static bool IsConnectionFailure(InstallError? error) =>
        InstallProblems.NeedsConnectionHelp(error);

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

        if (Interlocked.CompareExchange(ref _installBusy, 1, 0) != 0)
        {
            return;
        }

        IReadOnlyList<InstallSet> sets;
        try
        {
            // Bundles are unpacked here; that can take a while for large Quest builds.
            sets = await Task.Run(() => _installSets.Group(paths));
        }
        catch (Exception ex)
        {
            _logger.Error("Preparing app files failed.", ex);
            sets = [];
        }

        if (sets.Count == 0 || State.Device is null || State.CurrentStep != WizardStep.ReadyToInstall)
        {
            // The device or page may have changed while files were being prepared.
            Interlocked.Exchange(ref _installBusy, 0);
            if (sets.Count == 0)
            {
                PayloadWarning = "Add at least one APK file.";
            }

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
        _installCts?.Dispose();
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
            var failed = InstallResult.Failed(InstallError.UnknownInstallFailure, "The install did not complete.", _recovery.Suggest(InstallError.UnknownInstallFailure, Manifest));
            Advance(WizardTrigger.InstallFinished, State.Device, failed, State.Ready);
        }
        finally
        {
            Interlocked.Exchange(ref _installBusy, 0);
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

    private void OpenInstalledApps()
    {
        if (State.Device is not { State: DeviceConnectionState.ConnectedReady })
        {
            PayloadWarning = "Connect a device first, then open Installed apps.";
            return;
        }

        PayloadWarning = "";
        AppsPage.IsLoading = true;
        Advance(WizardTrigger.OpenInstalledApps, State.Device, readyDevices: State.Ready);
        _ = LoadInstalledAppsAsync();
    }

    private async Task LoadInstalledAppsAsync()
    {
        if (State.Device is null)
        {
            AppsPage.IsLoading = false;
            AppsPage.SetConnectionError("No device is connected.");
            return;
        }

        try
        {
            AppsPage.IsLoading = true;
            var recentIds = await RecentPackageIdsAsync();
            var listed = await _installedApps.ListAsync(State.Device.Serial, recentIds);
            if (!listed.IsSuccess || listed.Value is null)
            {
                AppsPage.IsLoading = false;
                AppsPage.SetConnectionError(listed.Error ?? "Could not read installed apps. Keep the device awake and try again.");
                AppsPage.Bind([]);
                NotifyChrome();
                return;
            }

            AppsPage.Bind(listed.Value);
            NotifyChrome();
            await EnrichVisibleAppsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error("List installed apps failed.", ex);
            AppsPage.IsLoading = false;
            AppsPage.SetConnectionError("Could not read installed apps. Keep the device awake and try again.");
            NotifyChrome();
        }
    }

    private async Task UninstallAppAsync(string packageId)
    {
        if (State.Device is null)
        {
            AppsPage.SetConnectionError("No device is connected.");
            NotifyChrome();
            return;
        }

        _uninstallCts?.Cancel();
        _uninstallCts?.Dispose();
        _uninstallCts = new CancellationTokenSource();
        AppsPage.BeginUninstall(packageId);
        NotifyChrome();
        try
        {
            var result = await _installedApps.UninstallAsync(State.Device.Serial, packageId, _uninstallCts.Token);
            AppsPage.EndUninstall(result);
            if (!result.Success)
            {
                PayloadWarning = result.Message;
            }
        }
        catch (OperationCanceledException)
        {
            AppsPage.CancelBusy();
            AppsPage.StatusMessage = "Removal was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.Error("Uninstall failed.", ex);
            AppsPage.EndUninstall(UninstallResult.Failed(packageId, "Could not remove this app."));
            PayloadWarning = "Could not remove this app.";
        }
        finally
        {
            NotifyChrome();
        }
    }

    private void CancelUninstall()
    {
        _uninstallCts?.Cancel();
        AppsPage.CancelBusy();
        NotifyChrome();
    }

    private async Task EnrichVisibleAppsAsync()
    {
        if (State.CurrentStep != WizardStep.InstalledApps || State.Device is null || AppsPage.IsBusy)
        {
            return;
        }

        var visible = AppsPage.VisibleModels
            .Where(app => string.IsNullOrWhiteSpace(app.Label))
            .Take(40)
            .ToList();
        if (visible.Count == 0)
        {
            return;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        var enriched = new List<InstalledApp>();
        foreach (var app in visible)
        {
            if (timeout.IsCancellationRequested)
            {
                break;
            }

            try
            {
                enriched.Add(await _installedApps.EnrichAsync(State.Device.Serial, app, timeout.Token));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                enriched.Add(app);
            }
        }

        if (enriched.Count > 0)
        {
            AppsPage.MergeEnrichment(enriched);
        }
    }

    private Task<HashSet<string>> RecentPackageIdsAsync()
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (State.Manifest.CanVerifyPackage)
        {
            ids.Add(State.Manifest.AppId);
        }

        var paths = ChoosePage.SelectedPaths.Concat(ChoosePage.RecentFiles).ToList();
        return Task.Run(() => AddPackageIds(ids, paths.Concat(_recents.Load().LastFiles)));
    }

    private HashSet<string> AddPackageIds(HashSet<string> ids, IEnumerable<string> paths)
    {
        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var identity = _inspector.Inspect(path);
                if (identity is { HasPackageId: true })
                {
                    ids.Add(identity.PackageId);
                }
            }
            catch
            {
                // Skip unreadable recents; listing still works.
            }
        }

        return ids;
    }

    private void NotifyChrome()
    {
        OnPropertyChanged(nameof(ShowPrimary));
        OnPropertyChanged(nameof(ShowCancel));
        OnPropertyChanged(nameof(CanPrimary));
        OnPropertyChanged(nameof(ShowSecondaryExport));
        OnPropertyChanged(nameof(ShowNeedHelpConnecting));
        OnPropertyChanged(nameof(ShowGoBack));
        OnPropertyChanged(nameof(ShowStartOver));
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

            if (State.HoldStep)
            {
                State = State with { HoldStep = false };
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

        dispatcher.BeginInvoke(() => HandleDevices(devices));
    }

    private void HandleDevices(IReadOnlyList<DeviceInfo> devices)
    {
        if (State.CurrentStep is WizardStep.Welcome or WizardStep.Installing or WizardStep.Complete or WizardStep.InstallProblem)
        {
            return;
        }

        if (State.CurrentStep == WizardStep.InstalledApps)
        {
            var connected = _devices.SelectPrimary(devices);
            if (connected is null)
            {
                CancelUninstall();
                Advance(WizardTrigger.DeviceRefresh, null, readyDevices: devices, health: _health.Snapshot(devices));
                return;
            }

            if (AppsPage.IsBusy)
            {
                return;
            }

            Advance(WizardTrigger.DeviceRefresh, connected, readyDevices: devices);
            return;
        }

        if (State.CurrentStep == WizardStep.Troubleshoot)
        {
            var health = _health.Snapshot(devices);
            var connected = _devices.SelectPrimary(devices);
            Advance(WizardTrigger.DeviceRefresh, connected, readyDevices: devices, health: health);
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
            if (State.CurrentStep is WizardStep.ConnectDevice
                or WizardStep.Authorization
                or WizardStep.DeveloperMode
                or WizardStep.ReadyToInstall
                or WizardStep.DeviceDetected)
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
        DeviceHealth? health = null,
        TroubleshootSession? troubleshoot = null)
    {
        var enteringHelper = State.CurrentStep != WizardStep.Troubleshoot;
        State = _flow.Advance(State, trigger, device, result, readyDevices, health, troubleshoot);
        ApplyState();
        PresentHelper(enteringHelper);
    }

    private void ApplyState()
    {
        if (State.CurrentStep == WizardStep.Troubleshoot)
        {
            HelpPage.Apply(State);
            if (State.Troubleshoot is not null)
            {
                HelpPage.SetActionLabel(_troubleshootCopy.ActionLabel(State.Troubleshoot, _usbHelper.HasBundledInf));
            }
        }
        else
        {
            CurrentPage = _pages[State.CurrentStep];
            CurrentPage.Apply(State);
        }

        OnPropertyChanged(nameof(ShowSecondaryExport));
        OnPropertyChanged(nameof(ShowNeedHelpConnecting));
        OnPropertyChanged(nameof(ShowGoBack));
        OnPropertyChanged(nameof(ShowStartOver));
        OnPropertyChanged(nameof(ShowPrimary));
        OnPropertyChanged(nameof(ShowHelperPrimary));
        OnPropertyChanged(nameof(ShowCancel));
        OnPropertyChanged(nameof(CancelLabel));
        OnPropertyChanged(nameof(CanPrimary));
        OnPropertyChanged(nameof(HelperPrimaryAction));
        RefreshGuide();
    }

    private void RefreshGuide()
    {
        Guide = _guideCoach.For(State, ChoosePage.HasFiles);
    }

    private void PresentHelper(bool entering)
    {
        if (State.CurrentStep == WizardStep.Troubleshoot)
        {
            if (entering && !_helperOpen)
            {
                _helperOpen = true;
                try
                {
                    _troubleshootUi.ShowDialog(this);
                }
                finally
                {
                    _helperOpen = false;
                    if (State.CurrentStep == WizardStep.Troubleshoot)
                    {
                        Advance(WizardTrigger.CloseTroubleshoot, State.Device, readyDevices: State.Ready, health: State.Health);
                    }
                }
            }

            return;
        }

        if (_helperOpen)
        {
            _troubleshootUi.Close();
        }
    }

    public void Shutdown()
    {
        _guideUi.ClosePopOut();
        _installCts?.Cancel();
        _uninstallCts?.Cancel();
        _monitor.Stop();
        _temp.DeleteAll();
    }
}
