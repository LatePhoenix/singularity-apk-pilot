namespace Installer.Core.Models;

public sealed record WizardCopy(
    string Headline,
    string Body,
    string PrimaryAction,
    string Help,
    string Advanced);
