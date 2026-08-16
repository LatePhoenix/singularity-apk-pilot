namespace Installer.Core.Models;

public sealed record RecoveryAction(
    string Id,
    string Title,
    string Description,
    RecoveryActionKind Kind,
    bool IsAutomatic);
