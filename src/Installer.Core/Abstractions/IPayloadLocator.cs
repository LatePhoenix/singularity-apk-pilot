namespace Installer.Core.Abstractions;

public interface IPayloadLocator
{
    string PayloadRoot { get; }
    string? FindManifestPath();
    string ResolveApkPath(string apkPath);
}
