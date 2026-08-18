using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class UsbDeviceClassifierTests
{
    [Fact]
    public void Quest_vendor_matches_vid()
    {
        Assert.True(UsbDeviceClassifier.IsQuestVendor("USB\\VID_2833&PID_0183"));
        Assert.False(UsbDeviceClassifier.IsQuestVendor("USB\\VID_18D1&PID_4EE7"));
    }

    [Fact]
    public void Adb_interface_matches_class_ff_subclass_42()
    {
        Assert.True(UsbDeviceClassifier.IsAdbInterface("USB\\Class_ff&SubClass_42&Prot_01"));
        Assert.False(UsbDeviceClassifier.IsAdbInterface("USB\\Class_06&SubClass_01"));
    }

    [Fact]
    public void Missing_driver_detects_unknown_usb()
    {
        Assert.True(UsbDeviceClassifier.LooksLikeMissingDriver("Unknown USB Device (Device Descriptor Request Failed)", null, null));
        Assert.False(UsbDeviceClassifier.LooksLikeMissingDriver("Quest 2", "WinUSB", "{88BAE032-5A81-49F0-BC3D-A4FF138216D6}"));
    }
}
