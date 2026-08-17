using Installer.Core.Models;
using Installer.Core.Services.Adb;

namespace Installer.Core.Tests.Devices;

public sealed class AdbOutputParserTests
{
    private readonly AdbOutputParser _parser = new();

    [Fact]
    public void Parses_quest_ready_device()
    {
        const string output = """
            List of devices attached
            1WMHH000000001    device product:hollywood model:Quest_2 device:hollywood transport_id:1
            """;

        var devices = _parser.ParseDevices(output);
        var device = Assert.Single(devices);
        Assert.Equal("1WMHH000000001", device.Serial);
        Assert.Equal("device", device.State);
        Assert.Equal("Quest_2", device.Properties["model"]);
        Assert.Equal("hollywood", device.Properties["product"]);
        Assert.Equal(DeviceConnectionState.ConnectedReady, _parser.ParseConnectionState(device.State));
    }

    [Fact]
    public void Parses_unauthorized_and_offline()
    {
        const string output = """
            List of devices attached
            SERIALA    unauthorized usb:1-2
            SERIALB    offline
            """;

        var devices = _parser.ParseDevices(output);
        Assert.Equal(2, devices.Count);
        Assert.Equal(DeviceConnectionState.Unauthorized, _parser.ParseConnectionState(devices[0].State));
        Assert.Equal(DeviceConnectionState.Offline, _parser.ParseConnectionState(devices[1].State));
    }

    [Fact]
    public void Parses_pixel_and_ignores_noise()
    {
        const string output = """
            * daemon started successfully *
            List of devices attached
            4321ABCD    device product:komodo model:Pixel_9 device:komodo transport_id:3
            """;

        var device = Assert.Single(_parser.ParseDevices(output));
        Assert.Equal("Pixel_9", device.Properties["model"]);
    }

    [Fact]
    public void Empty_output_is_no_devices()
    {
        Assert.Empty(_parser.ParseDevices("List of devices attached\n"));
    }

    [Fact]
    public void Parses_wifi_serial()
    {
        const string output = """
            List of devices attached
            192.168.1.42:5555    device product:eureka model:Quest_3 device:eureka transport_id:2
            """;

        var device = Assert.Single(_parser.ParseDevices(output));
        Assert.Equal("192.168.1.42:5555", device.Serial);
        Assert.Equal("Quest_3", device.Properties["model"]);
        Assert.Equal(DeviceConnectionState.ConnectedReady, _parser.ParseConnectionState(device.State));
    }

    [Fact]
    public void Parses_wifi_address_and_skips_usb_tether()
    {
        const string output = """
            7: usb0    inet 192.168.42.129/24 brd 192.168.42.255 scope global usb0
            8: wlan0    inet 192.168.1.42/24 brd 192.168.1.255 scope global wlan0
            """;

        Assert.Equal("192.168.1.42", _parser.ParseWifiAddress(output));
    }

    [Fact]
    public void Connect_and_pair_success_text()
    {
        Assert.True(_parser.IsConnectSuccess("connected to 192.168.1.42:5555"));
        Assert.True(_parser.IsConnectSuccess("already connected to 192.168.1.42:5555"));
        Assert.False(_parser.IsConnectSuccess("failed to connect to 192.168.1.42:5555"));
        Assert.True(_parser.IsPairSuccess("Successfully paired to 192.168.1.42:37123 [guid=adb-xxx]"));
        Assert.False(_parser.IsPairSuccess("Failed: Wrong password"));
    }

    [Fact]
    public void Package_list_matches_id()
    {
        Assert.True(_parser.IsPackageListed("package:com.singularity.exampleapp\n", "com.singularity.exampleapp"));
        Assert.False(_parser.IsPackageListed("package:com.other\n", "com.singularity.exampleapp"));
    }

    [Fact]
    public void Parses_launcher_component()
    {
        Assert.Equal("com.demo/.MainActivity", _parser.ParseLauncher("com.demo/.MainActivity\n", "com.demo"));
        Assert.Equal("com.demo/com.demo.MainActivity", _parser.ParseLauncher("priority=0\ncom.demo/com.demo.MainActivity", "com.demo"));
        Assert.Null(_parser.ParseLauncher("No activity found\n", "com.demo"));
        Assert.Equal("com.demo/.MainActivity", AdbOutputParser.ToComponent("com.demo", ".MainActivity"));
        Assert.Equal("com.demo/com.demo.Ui", AdbOutputParser.ToComponent("com.demo", "com.demo/com.demo.Ui"));
    }
}
