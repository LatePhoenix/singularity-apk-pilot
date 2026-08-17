using Installer.Core.Services.Adb;

namespace Installer.Core.Tests.Adb;

public sealed class AdbCommandFactoryTests
{
    private readonly AdbCommandFactory _factory = new();

    [Fact]
    public void StartServer_uses_start_server() =>
        Assert.Equal(["start-server"], _factory.StartServer().Arguments);

    [Fact]
    public void KillServer_uses_kill_server() =>
        Assert.Equal(["kill-server"], _factory.KillServer().Arguments);

    [Fact]
    public void Devices_uses_devices_l() =>
        Assert.Equal(["devices", "-l"], _factory.Devices().Arguments);

    [Fact]
    public void GetProperty_targets_serial_and_key() =>
        Assert.Equal(["-s", "ABC123", "shell", "getprop", "ro.product.model"], _factory.GetProperty("ABC123", "ro.product.model").Arguments);

    [Fact]
    public void Install_without_flags_ends_with_apk() =>
        Assert.Equal(["-s", "S1", "install", "app.apk"], _factory.Install("S1", "app.apk", []).Arguments);

    [Fact]
    public void Install_includes_flags_before_apk() =>
        Assert.Equal(["-s", "S1", "install", "-r", "-d", "-t", "-g", @"C:\payload\app.apk"], _factory.Install("S1", @"C:\payload\app.apk", ["-r", "-d", "-t", "-g"]).Arguments);

    [Fact]
    public void Uninstall_and_list_packages_target_serial()
    {
        Assert.Equal(["-s", "S1", "uninstall", "com.app"], _factory.Uninstall("S1", "com.app").Arguments);
        Assert.Equal(["-s", "S1", "shell", "pm", "list", "packages", "com.app"], _factory.ListPackages("S1", "com.app").Arguments);
    }

    [Fact]
    public void TcpIp_targets_serial_and_port() =>
        Assert.Equal(["-s", "S1", "tcpip", "5555"], _factory.TcpIp("S1", 5555).Arguments);

    [Fact]
    public void Connect_and_pair_use_endpoint()
    {
        Assert.Equal(["connect", "192.168.1.42:5555"], _factory.Connect("192.168.1.42:5555").Arguments);
        Assert.Equal(["pair", "192.168.1.42:37123", "123456"], _factory.Pair("192.168.1.42:37123", "123456").Arguments);
        Assert.Equal(["disconnect", "192.168.1.42:5555"], _factory.Disconnect("192.168.1.42:5555").Arguments);
        Assert.Equal(["disconnect"], _factory.Disconnect().Arguments);
    }

    [Fact]
    public void WifiAddresses_reads_global_ipv4() =>
        Assert.Equal(
            ["-s", "S1", "shell", "ip", "-o", "-4", "addr", "show", "scope", "global"],
            _factory.WifiAddresses("S1").Arguments);
}
