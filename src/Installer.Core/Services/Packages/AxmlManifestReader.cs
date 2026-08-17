using System.Text;
using Installer.Core.Models;

namespace Installer.Core.Services.Packages;

public static class AxmlManifestReader
{
    private const int ResXmlType = 0x0003;
    private const int StringPoolType = 0x0001;
    private const int StartElementType = 0x0102;
    private const int EndElementType = 0x0103;
    private const int Utf8Flag = 1 << 8;
    private const int TypeString = 3;
    private const int TypeIntDec = 16;
    private const int TypeIntHex = 17;
    private const uint AttrVersionCode = 0x0101021b;
    private const uint AttrVersionName = 0x0101021c;
    private const uint AttrName = 0x01010003;
    private const uint AttrLabel = 0x01010001;

    public static ApkIdentity? TryRead(byte[] data, string sourcePath)
    {
        try
        {
            if (data.Length < 8)
            {
                return null;
            }

            var fileType = BitConverter.ToUInt16(data, 0);
            var fileSize = BitConverter.ToInt32(data, 4);
            if (fileType != ResXmlType || fileSize <= 0 || fileSize > data.Length)
            {
                return null;
            }

            string[] strings = [];
            uint[] resourceIds = [];
            string packageId = "";
            string versionName = "";
            var versionCode = 0;
            string? split = null;
            string? label = null;
            string? launcher = null;
            string? currentActivity = null;
            var inActivity = false;
            var hasMain = false;
            var hasLauncher = false;

            var offset = 8;
            while (offset + 8 <= data.Length && offset + 8 <= fileSize)
            {
                var type = BitConverter.ToUInt16(data, offset);
                var chunkSize = BitConverter.ToInt32(data, offset + 4);
                if (chunkSize < 8 || offset + chunkSize > data.Length)
                {
                    break;
                }

                if (type == StringPoolType)
                {
                    strings = ReadStringPool(data, offset);
                }
                else if (type == 0x0180)
                {
                    resourceIds = ReadResourceMap(data, offset, chunkSize);
                }
                else if (type == StartElementType)
                {
                    ReadStartElement(
                        data, offset, strings, resourceIds,
                        ref packageId, ref versionName, ref versionCode, ref split, ref label,
                        ref currentActivity, ref inActivity, ref hasMain, ref hasLauncher);
                }
                else if (type == EndElementType)
                {
                    var name = ReadName(data, offset, strings);
                    if (inActivity && string.Equals(name, "activity", StringComparison.OrdinalIgnoreCase))
                    {
                        if (hasMain && hasLauncher && launcher is null)
                        {
                            launcher = currentActivity;
                        }

                        inActivity = false;
                        hasMain = false;
                        hasLauncher = false;
                        currentActivity = null;
                    }
                }

                offset += chunkSize;
            }

            if (string.IsNullOrWhiteSpace(packageId))
            {
                return null;
            }

            return new ApkIdentity(packageId, versionName, versionCode, split, label, launcher, sourcePath);
        }
        catch
        {
            return null;
        }
    }

    private static string[] ReadStringPool(byte[] data, int chunkStart)
    {
        var stringCount = BitConverter.ToInt32(data, chunkStart + 8);
        var flags = BitConverter.ToInt32(data, chunkStart + 16);
        var stringsStart = BitConverter.ToInt32(data, chunkStart + 20);
        var utf8 = (flags & Utf8Flag) != 0;
        var strings = new string[Math.Max(0, stringCount)];
        var dataStart = chunkStart + stringsStart;
        for (var i = 0; i < stringCount; i++)
        {
            var relative = BitConverter.ToInt32(data, chunkStart + 28 + i * 4);
            var pos = dataStart + relative;
            strings[i] = utf8 ? ReadUtf8(data, pos) : ReadUtf16(data, pos);
        }

        return strings;
    }

    private static uint[] ReadResourceMap(byte[] data, int chunkStart, int chunkSize)
    {
        var count = Math.Max(0, (chunkSize - 8) / 4);
        var ids = new uint[count];
        for (var i = 0; i < count; i++)
        {
            ids[i] = BitConverter.ToUInt32(data, chunkStart + 8 + i * 4);
        }

        return ids;
    }

    private static void ReadStartElement(
        byte[] data,
        int chunkStart,
        string[] strings,
        uint[] resourceIds,
        ref string packageId,
        ref string versionName,
        ref int versionCode,
        ref string? split,
        ref string? label,
        ref string? currentActivity,
        ref bool inActivity,
        ref bool hasMain,
        ref bool hasLauncher)
    {
        var nameIndex = BitConverter.ToInt32(data, chunkStart + 20);
        var name = GetString(strings, nameIndex);
        var attributeStart = BitConverter.ToUInt16(data, chunkStart + 24);
        var attributeSize = BitConverter.ToUInt16(data, chunkStart + 26);
        var attributeCount = BitConverter.ToUInt16(data, chunkStart + 28);
        if (attributeSize < 20)
        {
            attributeSize = 20;
        }

        var attrBase = chunkStart + 8 + attributeStart;
        if (string.Equals(name, "manifest", StringComparison.OrdinalIgnoreCase))
        {
            for (var i = 0; i < attributeCount; i++)
            {
                var attr = attrBase + i * attributeSize;
                var attrNameIndex = BitConverter.ToInt32(data, attr + 4);
                var attrName = GetString(strings, attrNameIndex);
                var resourceId = attrNameIndex >= 0 && attrNameIndex < resourceIds.Length ? resourceIds[attrNameIndex] : 0;
                var value = ReadAttributeValue(data, attr, strings);
                if (string.Equals(attrName, "package", StringComparison.OrdinalIgnoreCase))
                {
                    packageId = value;
                }
                else if (string.Equals(attrName, "split", StringComparison.OrdinalIgnoreCase))
                {
                    split = string.IsNullOrWhiteSpace(value) ? split : value;
                }
                else if (resourceId == AttrVersionName || string.Equals(attrName, "versionName", StringComparison.OrdinalIgnoreCase))
                {
                    versionName = value;
                }
                else if (resourceId == AttrVersionCode || string.Equals(attrName, "versionCode", StringComparison.OrdinalIgnoreCase))
                {
                    _ = int.TryParse(value, out versionCode);
                }
            }
        }
        else if (string.Equals(name, "application", StringComparison.OrdinalIgnoreCase))
        {
            for (var i = 0; i < attributeCount; i++)
            {
                var attr = attrBase + i * attributeSize;
                var attrNameIndex = BitConverter.ToInt32(data, attr + 4);
                var resourceId = attrNameIndex >= 0 && attrNameIndex < resourceIds.Length ? resourceIds[attrNameIndex] : 0;
                var attrName = GetString(strings, attrNameIndex);
                if (resourceId == AttrLabel || string.Equals(attrName, "label", StringComparison.OrdinalIgnoreCase))
                {
                    var value = ReadAttributeValue(data, attr, strings);
                    if (!value.StartsWith('@') && !string.IsNullOrWhiteSpace(value))
                    {
                        label = value;
                    }
                }
            }
        }
        else if (string.Equals(name, "activity", StringComparison.OrdinalIgnoreCase))
        {
            inActivity = true;
            hasMain = false;
            hasLauncher = false;
            currentActivity = ReadNameAttribute(data, attrBase, attributeCount, attributeSize, strings, resourceIds);
        }
        else if (string.Equals(name, "action", StringComparison.OrdinalIgnoreCase) && inActivity)
        {
            var action = ReadNameAttribute(data, attrBase, attributeCount, attributeSize, strings, resourceIds);
            if (action == "android.intent.action.MAIN")
            {
                hasMain = true;
            }
        }
        else if (string.Equals(name, "category", StringComparison.OrdinalIgnoreCase) && inActivity)
        {
            var category = ReadNameAttribute(data, attrBase, attributeCount, attributeSize, strings, resourceIds);
            if (category == "android.intent.category.LAUNCHER")
            {
                hasLauncher = true;
            }
        }
    }

    private static string? ReadNameAttribute(byte[] data, int attrBase, int count, int size, string[] strings, uint[] resourceIds)
    {
        for (var i = 0; i < count; i++)
        {
            var attr = attrBase + i * size;
            var attrNameIndex = BitConverter.ToInt32(data, attr + 4);
            var attrName = GetString(strings, attrNameIndex);
            var resourceId = attrNameIndex >= 0 && attrNameIndex < resourceIds.Length ? resourceIds[attrNameIndex] : 0;
            if (resourceId == AttrName || string.Equals(attrName, "name", StringComparison.OrdinalIgnoreCase))
            {
                return ReadAttributeValue(data, attr, strings);
            }
        }

        return null;
    }

    private static string ReadAttributeValue(byte[] data, int attr, string[] strings)
    {
        var raw = BitConverter.ToInt32(data, attr + 8);
        var dataType = data[attr + 15];
        var typed = BitConverter.ToUInt32(data, attr + 16);
        if (dataType == TypeString)
        {
            return GetString(strings, (int)typed);
        }

        if (dataType is TypeIntDec or TypeIntHex)
        {
            return typed.ToString();
        }

        return raw >= 0 ? GetString(strings, raw) : "";
    }

    private static string ReadName(byte[] data, int chunkStart, string[] strings)
    {
        var nameIndex = BitConverter.ToInt32(data, chunkStart + 16);
        return GetString(strings, nameIndex);
    }

    private static string GetString(string[] strings, int index) =>
        index >= 0 && index < strings.Length ? strings[index] : "";

    private static string ReadUtf16(byte[] data, int pos)
    {
        var charCount = BitConverter.ToUInt16(data, pos);
        pos += 2;
        if ((charCount & 0x8000) != 0)
        {
            charCount = (ushort)(((charCount & 0x7FFF) << 16) | BitConverter.ToUInt16(data, pos));
            pos += 2;
        }

        return Encoding.Unicode.GetString(data, pos, charCount * 2);
    }

    private static string ReadUtf8(byte[] data, int pos)
    {
        var charLen = DecodeLength(data, ref pos);
        var byteLen = DecodeLength(data, ref pos);
        _ = charLen;
        return Encoding.UTF8.GetString(data, pos, byteLen);
    }

    private static int DecodeLength(byte[] data, ref int pos)
    {
        var first = data[pos++];
        if ((first & 0x80) == 0)
        {
            return first;
        }

        return ((first & 0x7F) << 8) | data[pos++];
    }
}
