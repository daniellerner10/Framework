namespace Keeper.Framework.Extensions.Streams;

public static class StreamReaderExtensions
{
    public static void Seek(this StreamReader reader, long offset, SeekOrigin seekOrigin)
    {
        reader.BaseStream.Seek(offset, seekOrigin);
        reader.DiscardBufferedData();
    }
}
