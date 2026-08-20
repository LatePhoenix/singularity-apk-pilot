using Installer.App.Services;
using Installer.App.ViewModels;
using Installer.App.Views;
using Installer.Core.Abstractions;
using Installer.Core.Services.Adb;
using Installer.Core.Services.Content;
using Installer.Core.Services.Devices;
using Installer.Core.Services.Diagnostics;
using Installer.Core.Services.Flow;
using Installer.Core.Services.Install;
using Installer.Core.Services.Packages;
using Installer.Core.Services.Recovery;
using Installer.Core.Services.Support;
using Installer.Core.Utilities;
using Installer.Infrastructure;
using Installer.Infrastructure.Devices;
using Installer.Infrastructure.Logging;
using Installer.Infrastructure.Mail;
using Installer.Infrastructure.Packaging;
using Installer.Infrastructure.Process;
using Installer.Infrastructure.Storage;
using Installer.Infrastructure.Updates;
using Microsoft.Extensions.DependencyInjection;

namespace Installer.App.Bootstrap;

public static class ServiceRegistration
{
    public static IServiceProvider Create()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(_ => new LogRedactor(HmacKeyStore.GetOrCreate()));
        services.AddSingleton<SessionLogWriter>();
        services.AddSingleton<ISessionLog>(sp => sp.GetRequiredService<SessionLogWriter>());
        services.AddSingleton<IAppLogger, FileLogger>();
        services.AddSingleton<IPortableAdbLocator, PortableAdbLocator>();
        services.AddSingleton<IPayloadLocator, PayloadLocator>();
        services.AddSingleton<IUserDataPaths, UserDataPaths>();
        services.AddSingleton<IZipBundleWriter, ZipBundleWriter>();
        services.AddSingleton<ProcessService>();
        services.AddSingleton<IAdbProcessRunner, AdbProcessRunner>();
        services.AddSingleton<AdbCommandFactory>();
        services.AddSingleton<AdbOutputParser>();
        services.AddSingleton<IAdbClient, AdbClient>();
        services.AddSingleton<ITempFileService, TempFileService>();
        services.AddSingleton<IApkInspector, ApkInspector>();
        services.AddSingleton<IInstallSetFactory, InstallSetFactory>();
        services.AddSingleton<IRecentsStore, RecentsStore>();
        services.AddSingleton<UsbEvidenceProbe>();
        services.AddSingleton<IUsbEvidenceProbe>(sp => sp.GetRequiredService<UsbEvidenceProbe>());
        services.AddSingleton<IUsbPresenceProbe>(sp => sp.GetRequiredService<UsbEvidenceProbe>());
        services.AddSingleton<IQuestUsbHelperService, QuestUsbHelperService>();
        services.AddSingleton<TroubleshootCopyDeck>();
        services.AddSingleton<ITroubleshootingService, TroubleshootingService>();
        services.AddSingleton<IDeviceHealthService, DeviceHealthService>();
        services.AddSingleton<IUpdateCheckService, GitHubUpdateCheckService>();
        services.AddSingleton<IWirelessEndpointStore, WirelessEndpointStore>();
        services.AddSingleton<IReportRecipientStore, ReportRecipientStore>();
        services.AddSingleton<IMailComposeService, MailComposeService>();
        services.AddSingleton<IWirelessAdbService, WirelessAdbService>();
        services.AddSingleton<DeviceClassificationService>();
        services.AddSingleton<DevicePropertyService>();
        services.AddSingleton<IDeviceService, DeviceDetectionService>();
        services.AddSingleton<IDeviceMonitorService, DeviceMonitorService>();
        services.AddSingleton<InstallPlanner>();
        services.AddSingleton<InstallVerifier>();
        services.AddSingleton<PackageConflictService>();
        services.AddSingleton<PermissionGrantService>();
        services.AddSingleton<ErrorClassifier>();
        services.AddSingleton<RetryPolicyFactory>();
        services.AddSingleton<FriendlyMessageService>();
        services.AddSingleton<IInstallService, InstallService>();
        services.AddSingleton<IInstalledAppService, InstalledAppService>();
        services.AddSingleton<AutoFixExecutor>();
        services.AddSingleton<IRecoveryService, RecoveryService>();
        services.AddSingleton<QuestFlowStrategy>();
        services.AddSingleton<AndroidPhoneFlowStrategy>();
        services.AddSingleton<FlowDecisionEngine>();
        services.AddSingleton<IContentService, CopyDeckService>();
        services.AddSingleton<IGuideCoach, GuideCoachService>();
        services.AddSingleton<IWizardFlowService, WizardFlowService>();
        services.AddSingleton<IManifestService, ManifestService>();
        services.AddSingleton<LogcatCollector>();
        services.AddSingleton<EnvironmentSnapshotService>();
        services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
        services.AddSingleton<ISendReportUi, SendReportUi>();
        services.AddSingleton<ITroubleshootUi, TroubleshootUi>();
        services.AddSingleton<IGuideUi, GuideUi>();
        services.AddSingleton<ContentPackResolver>();
        services.AddSingleton<BuildStampReader>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<ShellWindow>();

        return services.BuildServiceProvider();
    }
}
