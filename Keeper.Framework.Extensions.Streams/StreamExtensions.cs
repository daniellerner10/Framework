using System.Diagnostics.CodeAnalysis;

namespace Keeper.Framework.Extensions.Streams;

public static class StreamExtensions
{
    /// <summary>
    /// Return all stream as bytes.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="bufferSize">bufferSize. Defaults to 4096.</param>
    /// <returns>An array of bytes.</returns>
    public static byte[] ReadAllBytes(this Stream stream, int bufferSize = 4096)
    {
        var originalPosition = 0L;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            var readBuffer = new byte[bufferSize];
            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    var nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        long byteSize = ((long)readBuffer.Length) * 2;
                        var temp = new byte[byteSize > int.MaxValue - 56 ? int.MaxValue - 56 : byteSize];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Return all stream as bytes.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="bufferSize">bufferSize. Defaults to 4096.</param>
    /// <returns>An array of bytes.</returns>
    [SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Logic of method demands expanded method.")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "It is necessary")]
    public static async Task<byte[]> ReadToEndAsync(this Stream stream, int bufferSize = 4096)
    {
        long originalPosition = 0;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            var readBuffer = new byte[bufferSize];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    var nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        var temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            var buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }

            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = originalPosition;
        }
    }

    public static void WriteShort(this TextWriter writer, short n) =>
        writer.Write(BitConverter.GetBytes(n));
}