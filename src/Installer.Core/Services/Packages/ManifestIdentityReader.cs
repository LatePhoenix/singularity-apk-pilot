using System.Text;
using System.Xml.Linq;
using Installer.Core.Models;

namespace Installer.Core.Services.Packages;

public static class ManifestIdentityReader
{
    public static ApkIdentity? TryRead(byte[] data, string sourcePath)
    {
        if (data.Length == 0)
        {
            return null;
        }

        if (LooksLikeXml(data))
        {
            return TryReadXml(data, sourcePath);
        }

        return AxmlManifestReader.TryRead(data, sourcePath);
    }

    private static bool LooksLikeXml(byte[] data)
    {
        var offset = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? 3 : 0;
        while (offset < data.Length && (data[offset] == (byte)' ' || data[offset] == (byte)'\t' || data[offset] == (byte)'\r' || data[offset] == (byte)'\n'))
        {
            offset++;
        }

        return offset < data.Length && data[offset] == (byte)'<';
    }

    private static ApkIdentity? TryReadXml(byte[] data, string sourcePath)
    {
        try
        {
            var text = Encoding.UTF8.GetString(data);
            var doc = XDocument.Parse(text);
            var manifest = doc.Root;
            if (manifest is null)
            {
                return null;
            }

            XNamespace android = "http://schemas.android.com/apk/res/android";
            var packageId = (string?)manifest.Attribute("package") ?? "";
            var versionName = (string?)manifest.Attribute(android + "versionName")
                              ?? (string?)manifest.Attribute("versionName")
                              ?? "";
            var versionCodeText = (string?)manifest.Attribute(android + "versionCode")
                                  ?? (string?)manifest.Attribute("versionCode")
                                  ?? "0";
            _ = int.TryParse(versionCodeText, out var versionCode);
            var split = (string?)manifest.Attribute("split")
                        ?? (string?)manifest.Attribute(android + "split");
            var app = manifest.Element("application");
            var label = (string?)app?.Attribute(android + "label") ?? (string?)app?.Attribute("label");
            var launcher = FindLauncher(manifest, android);

            return new ApkIdentity(packageId, versionName, versionCode, string.IsNullOrWhiteSpace(split) ? null : split, label, launcher, sourcePath);
        }
        catch
        {
            return null;
        }
    }

    private static string? FindLauncher(XElement manifest, XNamespace android)
    {
        foreach (var activity in manifest.Descendants("activity"))
        {
            var name = (string?)activity.Attribute(android + "name") ?? (string?)activity.Attribute("name");
            foreach (var filter in activity.Elements("intent-filter"))
            {
                var hasMain = filter.Elements("action").Any(a =>
                    ((string?)a.Attribute(android + "name") ?? (string?)a.Attribute("name")) == "android.intent.action.MAIN");
                var hasLauncher = filter.Elements("category").Any(c =>
                    ((string?)c.Attribute(android + "name") ?? (string?)c.Attribute("name")) == "android.intent.category.LAUNCHER");
                if (hasMain && hasLauncher)
                {
                    return name;
                }
            }
        }

        return null;
    }
}
