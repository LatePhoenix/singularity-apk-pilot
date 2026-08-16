using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Recovery;

public sealed class AutoFixExecutor
{
    private readonly IAdbClient _adb;
    private readonly IInstallService _install;
    private readonly RetryPolicyFactory _policies;
    private readonly IAppLogger _logger;

    public AutoFixExecutor(IAdbClient adb, IInstallService install, RetryPolicyFactory policies, IAppLogger logger)
    {
        _adb = adb;
        _install = install;
        _policies = policies;
        _logger = logger;
    }

    public async Task<InstallResult?> ExecuteAsync(InstallRequest request, InstallResult failure, CancellationToken cancellationToken = default)
    {
        if (failure.Error is null)
        {
            return null;
        }

        if (failure.Error is InstallError.MissingPayload)
        {
            _logger.Info("Auto-fix skipped: selected APK file is missing.");
            return null;
        }

        if (failure.Error is InstallError.NoDevicesFound or InstallError.CableOrUsbModeIssue)
        {
            _logger.Info("Auto-fix: restart adb server.");
            await _adb.RestartServerAsync(cancellationToken);
        }

        var nextPolicy = _policies.NextPolicy(failure.Error.Value, request.PolicyOverride ?? request.Manifest.InstallPolicy);
        var retryRequest = nextPolicy is null ? request : request with { PolicyOverride = nextPolicy };
        _logger.Info($"Auto-fix: retry install with policy {retryRequest.PolicyOverride ?? retryRequest.Manifest.InstallPolicy}.");
        return await _install.InstallAsync(retryRequest, cancellationToken);
    }
}
