using Installer.Core.Models;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class DeviceClassificationTests
{
    private readonly DeviceClassificationService _sut = new();

    [Theory]
    [InlineData("Oculus", "Quest 2", "hollywood", DeviceKind.MetaQuest)]
    [InlineData("Meta", "Quest 3", "eureka", DeviceKind.MetaQuest)]
    [InlineData("oculus", "Quest_3", "eureka", DeviceKind.MetaQuest)]
    [InlineData("Google", "Pixel 9", "komodo", DeviceKind.AndroidPhone)]
    [InlineData("samsung", "SM-S928U", "e3q", DeviceKind.AndroidPhone)]
    public void Classifies_common_devices(string manufacturer, string model, string product, DeviceKind expected)
    {
        var props = new Dictionary<string, string> { ["product"] = product, ["model"] = model.Replace(' ', '_') };
        Assert.Equal(expected, _sut.Classify(manufacturer, model, props));
    }

    [Fact]
    public void Unknown_without_identity()
    {
        Assert.Equal(DeviceKind.Unknown, _sut.Classify("", "", new Dictionary<string, string>()));
    }
}
