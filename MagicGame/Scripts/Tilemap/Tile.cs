using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine;
using Vector3 = WaffleEngine.Vector3;
using Vector4 = WaffleEngine.Vector4;

namespace MagicGame.Scripts;

public struct Tile : ISerializable, IDeserializable<Tile>
{
    public int TileIndex;
    public int PaletteIndex;
    public int Height;
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginSequence(SequenceStyle.Flow);
        
        emitter.WriteInt32(TileIndex);
        emitter.WriteInt32(PaletteIndex);
        emitter.WriteInt32(Height);
        
        emitter.EndSequence();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Tile tile)
    {
        tile = default;
        
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
            return false;

        parser.Read();

        if (!parser.TryReadScalarAsInt32(out var tileIndex)) return false;
        if (!parser.TryReadScalarAsInt32(out var paletteIndex)) return false;
        if (!parser.TryReadScalarAsInt32(out var height)) return false;
        
        if (parser.CurrentEventType != ParseEventType.SequenceEnd)
            return false;

        parser.Read();

        tile = new Tile()
        {
            TileIndex = tileIndex,
            PaletteIndex = paletteIndex,
            Height = height,
        };
        return true;
    }
}

public struct GpuTile
{
    public Vector4 Position;
    public int TileIndex;
    public int PaletteIndex;
    public int FadeIndex;
    public int Rotation;
}