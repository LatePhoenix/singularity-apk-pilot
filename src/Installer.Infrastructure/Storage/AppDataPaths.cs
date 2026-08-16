namespace Installer.Infrastructure.Storage;

public static class AppDataPaths
{
    public static string Root =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SingularityApkInstaller");

    public static string Logs => Path.Combine(Root, "logs");

    public static string Diagnostics => Path.Combine(Root, "diagnostics");
}
