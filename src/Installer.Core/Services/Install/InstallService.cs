using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Recovery;

namespace Installer.Core.Services.Install;

public sealed class InstallService : IInstallService
{
    private readonly IAdbClient _adb;
    private readonly InstallPlanner _planner;
    private readonly InstallVerifier _verifier;
    private readonly ErrorClassifier _classifier;
    private readonly IAppLogger _logger;

    public InstallService(
        IAdbClient adb,
        InstallPlanner planner,
        InstallVerifier verifier,
        ErrorClassifier classifier,
        IAppLogger logger)
    {
        _adb = adb;
        _planner = planner;
        _verifier = verifier;
        _classifier = classifier;
        _logger = logger;
    }

    public InstallPlan CreatePlan(InstallRequest request) => _planner.Create(request);

    public async Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default)
    {
        var plan = CreatePlan(request);
        var serial = request.Device.Serial;
        var combined = "";

        try
        {
            if (plan.RequiresUninstallFirst)
            {
                var uninstall = await _adb.UninstallAsync(serial, plan.PackageId, cancellationToken);
                combined = uninstall.CombinedOutput;
                _logger.Info($"Uninstall exit {uninstall.ExitCode}");
            }

            var install = await _adb.InstallAsync(serial, plan.ApkPath, plan.AdbFlags, cancellationToken);
            combined = string.IsNullOrWhiteSpace(combined)
                ? install.CombinedOutput
                : combined + Environment.NewLine + install.CombinedOutput;

            if (!install.Succeeded || LooksLikeFailure(install.CombinedOutput))
            {
                var error = _classifier.Classify(install.CombinedOutput);
                return InstallResult.Failed(error, combined, [], plan, install.ExitCode);
            }

            if (plan.VerifyAfterInstall)
            {
                var present = await _verifier.VerifyAsync(serial, plan.PackageId, cancellationToken);
                if (!present)
                {
                    var error = InstallError.UnknownInstallFailure;
                    return InstallResult.Failed(error, combined, [], plan, install.ExitCode);
                }
            }

            return InstallResult.Succeeded(request.Manifest.BuildVersion, combined, plan);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error("Install failed.", ex);
            var error = _classifier.Classify(ex.Message);
            return InstallResult.Failed(error, ex.Message, [], plan);
        }
    }

    private static bool LooksLikeFailure(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return false;
        }

        return output.Contains("Failure", StringComparison.OrdinalIgnoreCase)
               || output.Contains("error:", StringComparison.OrdinalIgnoreCase)
               || output.Contains("INSTALL_FAILED", StringComparison.OrdinalIgnoreCase);
    }
}
