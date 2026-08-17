using System.IO.Compression;
using System.Text;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Packages;

namespace Installer.Core.Tests.Packages;

public sealed class ApkInspectorTests
{
    [Fact]
    public void Reads_xml_manifest_from_apk_zip()
    {
        var path = WriteApk(XmlManifest("com.singularity.demo", "1.2.3", 12, null, "Demo", ".MainActivity"));
        try
        {
            var identity = new ApkInspector().Inspect(path);
            Assert.NotNull(identity);
            Assert.Equal("com.singularity.demo", identity.PackageId);
            Assert.Equal("1.2.3", identity.VersionName);
            Assert.Equal(12, identity.VersionCode);
            Assert.Equal("Demo", identity.Label);
            Assert.Equal(".MainActivity", identity.LauncherActivity);
            Assert.False(identity.IsSplit);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Reads_split_name()
    {
        var path = WriteApk(XmlManifest("com.singularity.demo", "1.0", 1, "config.arm64_v8a", "Demo", null));
        try
        {
            var identity = new ApkInspector().Inspect(path);
            Assert.NotNull(identity);
            Assert.True(identity.IsSplit);
            Assert.Equal("config.arm64_v8a", identity.SplitName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Groups_same_package_as_one_set()
    {
        var baseApk = WriteApk(XmlManifest("com.singularity.demo", "1.0", 1, null, "Demo", ".MainActivity"));
        var splitApk = WriteApk(XmlManifest("com.singularity.demo", "1.0", 1, "config.arm64_v8a", "Demo", null));
        var other = WriteApk(XmlManifest("com.other.app", "2.0", 2, null, "Other", null));
        try
        {
            var factory = new InstallSetFactory(new ApkInspector(), new Temp());
            var sets = factory.Group([baseApk, splitApk, other]);
            Assert.Equal(2, sets.Count);
            var demo = Assert.Single(sets, s => s.PackageId == "com.singularity.demo");
            Assert.True(demo.IsSplitSet);
            Assert.False(demo.LooksLikeMissingSplits);
            Assert.Equal(2, demo.ApkPaths.Count);
            Assert.Contains(sets, s => s.PackageId == "com.other.app");
        }
        finally
        {
            File.Delete(baseApk);
            File.Delete(splitApk);
            File.Delete(other);
        }
    }

    [Fact]
    public void Orphan_split_looks_like_missing_base()
    {
        var splitApk = WriteApk(XmlManifest("com.singularity.demo", "1.0", 1, "config.arm64_v8a", "Demo", null));
        try
        {
            var set = Assert.Single(new InstallSetFactory(new ApkInspector(), new Temp()).Group([splitApk]));
            Assert.True(set.LooksLikeMissingSplits);
        }
        finally
        {
            File.Delete(splitApk);
        }
    }

    [Fact]
    public void Inspects_xapk_bundle()
    {
        var inner = WriteApk(XmlManifest("com.singularity.demo", "3.0", 3, null, "Demo", ".MainActivity"));
        var bundle = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xapk");
        try
        {
            using (var zip = ZipFile.Open(bundle, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(inner, "com.singularity.demo.apk");
            }

            var extracted = new ApkInspector().InspectBundle(bundle, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
            var identity = Assert.Single(extracted);
            Assert.Equal("com.singularity.demo", identity.PackageId);
        }
        finally
        {
            File.Delete(inner);
            File.Delete(bundle);
        }
    }

    [Fact]
    public void Groups_apks_bundle_as_one_set()
    {
        var inner = WriteApk(XmlManifest("com.singularity.demo", "3.0", 3, null, "Demo", ".MainActivity"));
        var split = WriteApk(XmlManifest("com.singularity.demo", "3.0", 3, "config.arm64_v8a", "Demo", null));
        var bundle = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".apks");
        try
        {
            using (var zip = ZipFile.Open(bundle, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(inner, "base.apk");
                zip.CreateEntryFromFile(split, "split.apk");
            }

            var set = Assert.Single(new InstallSetFactory(new ApkInspector(), new Temp()).Group([bundle]));
            Assert.Equal("com.singularity.demo", set.PackageId);
            Assert.True(set.IsSplitSet);
            Assert.Equal(2, set.ApkPaths.Count);
            Assert.False(set.LooksLikeMissingSplits);
        }
        finally
        {
            File.Delete(inner);
            File.Delete(split);
            File.Delete(bundle);
        }
    }

    private static string XmlManifest(string packageId, string versionName, int versionCode, string? split, string label, string? launcher)
    {
        var splitAttr = split is null ? "" : $" split=\"{split}\"";
        var activity = launcher is null
            ? ""
            : $"""
                <activity android:name="{launcher}">
                  <intent-filter>
                    <action android:name="android.intent.action.MAIN"/>
                    <category android:name="android.intent.category.LAUNCHER"/>
                  </intent-filter>
                </activity>
              """;
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <manifest xmlns:android="http://schemas.android.com/apk/res/android" package="{packageId}" android:versionName="{versionName}" android:versionCode="{versionCode}"{splitAttr}>
              <application android:label="{label}">
                {activity}
              </application>
            </manifest>
            """;
    }

    private static string WriteApk(string xml)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".apk");
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        var entry = zip.CreateEntry("AndroidManifest.xml");
        using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(xml);
        stream.Write(bytes, 0, bytes.Length);
        return path;
    }

    private sealed class Temp : ITempFileService
    {
        public string CreateTempDirectory(string prefix = "sai-")
        {
            var path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
