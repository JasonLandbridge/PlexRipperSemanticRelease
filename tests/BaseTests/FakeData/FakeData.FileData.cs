using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static byte[] GetDownloadFile(double sizeInMib)
    {
        // convert mib to byte
        var b = new byte[(long)ByteSize.FromMebiBytes(sizeInMib).Bytes];
        RandomInstance.NextBytes(b);
        return b;
    }

    public static MemoryStream GetFileStream(double sizeInMib = 0) =>
        sizeInMib > 0 ? new MemoryStream(GetDownloadFile(sizeInMib)) : new MemoryStream();
}
