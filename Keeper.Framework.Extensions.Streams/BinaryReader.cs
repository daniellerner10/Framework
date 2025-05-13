using Keeper.Framework.Extensions.Collections;

namespace Keeper.Framework.Extensions.Streams;

public class BinaryReader : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private readonly ArrayBuilder<byte> _arrayBuilder;
    private byte[] _remainder;

    public bool EndOfStream { get; private set; }

    public BinaryReader(Stream stream, long bufferSize = 4096)
    {
        _stream = stream;
        _buffer = new byte[bufferSize];
        _arrayBuilder = new(0);
        _remainder = [];
    }

    public byte[] ReadLine()
    {
        byte[] bufferToRead;

        while (true)
        {
            int read;
            if (_remainder.Length > 0)
            {
                bufferToRead = _remainder;
                read = _remainder.Length;
            }
            else
            {
                _stream.Flush();
                read = _stream.Read(_buffer, 0, _buffer.Length);
                if (read == 0)
                {
                    EndOfStream = true;
                    return _arrayBuilder.ToArray();
                }
                bufferToRead = _buffer;
            }

            for (var i = 0; i < read; i++)
            {
                if (bufferToRead[i] == 13)
                {
                    var endOfLine = i;

                    if (i < read - 1)
                    {
                        var beginOfLine = bufferToRead[i + 1] == 10 ? i + 2 : i + 1;

                        _arrayBuilder.AddArray(bufferToRead[..endOfLine]);
                        var returnValue = _arrayBuilder.ToArray();
                        _arrayBuilder.Clear();

                        if (beginOfLine < read)
                            _remainder = bufferToRead[beginOfLine..read];
                        else
                            _remainder = [];

                        return returnValue;
                    }
                    else
                    {
                        _arrayBuilder.AddArray(bufferToRead[..endOfLine]);
                        var returnValue = _arrayBuilder.ToArray();
                        _arrayBuilder.Clear();

                        _stream.Flush();
                        read = _stream.Read(_buffer, 0, _buffer.Length);
                        if (read == 0)
                        {
                            EndOfStream = true;
                        }
                        else
                        {
                            _remainder = _buffer[0] == 10 ? _buffer[1..read] : _buffer[..read];
                        }
                        return returnValue;
                    }
                }
                else if (bufferToRead[i] == 10)
                {
                    var endOfLine = i;
                    var beginOfLine = i + 1;

                    _arrayBuilder.AddArray(bufferToRead[..endOfLine]);
                    var returnValue = _arrayBuilder.ToArray();
                    _arrayBuilder.Clear();

                    if (beginOfLine < read)
                        _remainder = bufferToRead[beginOfLine..read];
                    else
                        _remainder = [];

                    return returnValue;
                }
            }

            _arrayBuilder.AddArray(bufferToRead);
            _remainder = [];
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _stream.Dispose();
    }
}
