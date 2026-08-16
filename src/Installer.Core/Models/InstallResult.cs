namespace Installer.Core.Models;

public sealed record InstallResult(
    bool Success,
    string? InstalledVersion,
    InstallError? Error,
    string RawOutput,
    IReadOnlyList<RecoveryAction> SuggestedActions,
    InstallPlan? Plan = null,
    int ExitCode = 0)
{
    public static InstallResult Failed(InstallError error, string rawOutput, IReadOnlyList<RecoveryAction> actions, InstallPlan? plan = null, int exitCode = 1) =>
        new(false, null, error, rawOutput, actions, plan, exitCode);

    public static InstallResult Succeeded(string? installedVersion, string rawOutput, InstallPlan plan) =>
        new(true, installedVersion, null, rawOutput, [], plan);
}
