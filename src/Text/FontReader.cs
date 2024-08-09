namespace WaffleEngine;

// Sebastian Legues Font Reader with some minor preference tweaks.
// BinaryReader wrapper for more convient reading of ttf files.
public class FontReader : IDisposable
{
    public readonly Stream Stream;
    public readonly BinaryReader Reader;

    readonly bool _convert_to_little_endian;
    bool _is_disposed;

    const byte ByteMask = 0b11111111;

    public FontReader(string path_to_font)
    {
        Stream = File.Open(path_to_font, FileMode.Open);
        Reader = new BinaryReader(Stream);
        _convert_to_little_endian = BitConverter.IsLittleEndian;
    }

    public void Skip16BitEntries(int num) => SkipBytes(num * 2);
    public void Skip32BitEntries(int num) => SkipBytes(num * 4);
    public void SkipBytes(int num_bytes) => Reader.BaseStream.Seek(num_bytes, SeekOrigin.Current);
    public void SkipBytes(uint num_bytes) => Reader.BaseStream.Seek(num_bytes, SeekOrigin.Current);

    public void GoTo(uint byte_offset_from_origin, out uint prev)
    {
        prev = GetLocation();
        Reader.BaseStream.Seek(byte_offset_from_origin, SeekOrigin.Begin);
    }

    public void GoTo(uint byte_offset_from_origin) => Reader.BaseStream.Seek(byte_offset_from_origin, SeekOrigin.Begin);
    public void GoTo(int byte_offset_from_origin) => Reader.BaseStream.Seek(byte_offset_from_origin, SeekOrigin.Begin);
    public void GoTo(long byte_offset_from_origin) => Reader.BaseStream.Seek(byte_offset_from_origin, SeekOrigin.Begin);
    public uint GetLocation() => (uint)Reader.BaseStream.Position;

    public string ReadString(int num_bytes)
    {
        Span<char> tag = stackalloc char[num_bytes];

        for (int i = 0; i < tag.Length; i++)
            tag[i] = (char)Reader.ReadByte();

        return tag.ToString();
    }

    public string ReadTag() => ReadString(4);

    public double ReadFixedPoint2Dot14() => UInt16ToFixedPoint2Dot14(ReadUInt16());

    public static double UInt16ToFixedPoint2Dot14(UInt16 raw)
    {
        return (Int16)(raw) / (double)(1 << 14);
    }

    public byte ReadByte() => Reader.ReadByte();

    public sbyte ReadSByte() => Reader.ReadSByte();

    public Int32 ReadInt32() => (Int32)ReadUInt32();

    public Int32 ReadInt16() => (Int16)ReadUInt16();

    public UInt16 ReadUInt16()
    {
        UInt16 value = Reader.ReadUInt16();
        if (_convert_to_little_endian)
        {
            value = ToLittleEndian(value);
        }
        return value;
    }

    public UInt32 ReadUInt32()
    {
        UInt32 value = Reader.ReadUInt32();
        if (_convert_to_little_endian)
        {
            value = ToLittleEndian(value);
        }
        return value;
    }

    static UInt32 ToLittleEndian(UInt32 big_endian_value)
    {
        UInt32 a = (big_endian_value >> 24) & ByteMask;
        UInt32 b = (big_endian_value >> 16) & ByteMask;
        UInt32 c = (big_endian_value >> 8) & ByteMask;
        UInt32 d = (big_endian_value >> 0) & ByteMask;
        return a | b << 8 | c << 16 | d << 24;
    }

    static UInt16 ToLittleEndian(UInt16 big_endian_value)
    {

        return (UInt16)(big_endian_value >> 8 | big_endian_value << 8);

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_is_disposed)
        {
            if (disposing)
            {
                Stream.Dispose();
                Reader.Dispose();
            }
            _is_disposed = true;
        }
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}