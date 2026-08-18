namespace Installer.Core.Services.Devices;

public static class UsbDeviceClassifier
{
    public const string QuestVendorId = "VID_2833";

    private static readonly string[] AndroidVendorIds =
    [
        "VID_18D1", "VID_04E8", "VID_22B8", "VID_0BB4", "VID_2A70", "VID_0E8D",
        "VID_12D1", "VID_2717", "VID_0FCE", "VID_0502", "VID_1004", "VID_04C5",
        "VID_0489", "VID_2B4C", "VID_0B05", "VID_1D4D", "VID_0FCE"
    ];

    private static readonly string[] AndroidNeedles =
    [
        "Android", "ADB", "MTP", "Pixel", "Samsung", "Google", "OnePlus", "Xiaomi", "Motorola"
    ];

    public static bool IsQuestVendor(string value) =>
        Contains(value, QuestVendorId) || Contains(value, "Oculus") || Contains(value, "Quest");

    public static bool IsAndroidVendor(string value)
    {
        if (AndroidVendorIds.Any(id => Contains(value, id)))
        {
            return true;
        }

        return AndroidNeedles.Any(needle => Contains(value, needle));
    }

    public static bool IsAdbInterface(string value) =>
        Contains(value, "Class_ff") && Contains(value, "SubClass_42")
        || Contains(value, "ANDROIDADB")
        || Contains(value, "Android ADB")
        || Contains(value, "ADB Interface");

    public static bool IsMtpInterface(string value) =>
        Contains(value, "MTP")
        || Contains(value, "WPD")
        || Contains(value, "Class_06")
        || Contains(value, "Portable Device")
        || Contains(value, "SID_MS");

    public static bool LooksLikeMissingDriver(string deviceDesc, string? service, string? classGuid)
    {
        if (Contains(deviceDesc, "Unknown USB") || Contains(deviceDesc, "Unknown device"))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(service)
            && (string.IsNullOrWhiteSpace(classGuid) || Contains(classGuid, "36FC9E60")))
        {
            return true;
        }

        return false;
    }

    private static bool Contains(string? value, string needle) =>
        !string.IsNullOrEmpty(value) && value.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
