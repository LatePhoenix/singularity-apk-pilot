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
    public void Package_list_matches_id()
    {
        Assert.True(_parser.IsPackageListed("package:com.singularity.exampleapp\n", "com.singularity.exampleapp"));
        Assert.False(_parser.IsPackageListed("package:com.other\n", "com.singularity.exampleapp"));
    }
}
