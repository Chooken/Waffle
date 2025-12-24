using System.Diagnostics.CodeAnalysis;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine;

namespace MagicGame.Scripts;

public class Tilemap<T> : ISerializable, IDeserializable<Tilemap<T>> where T : struct, ISerializable, IDeserializable<T>
{
    private T[] _tiles;
    private int _width;
    private int _height;

    public Tilemap(int width, int height)
    {
        _tiles = new T[width * height];
        _width = width;
        _height = height;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < _width ||
                y >= 0 || y < _height;
    }

    public void SetTile(int x, int y, T tile)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        _tiles[x + _width * y] = tile;
    }

    public bool TryGetTile(int x, int y, out T tile)
    {
        tile = new T();
        
        if (!InBounds(x, y))
        {
            return false;
        }

        tile = _tiles[x + _width * y];

        return true;
    }

    public void RemoveTile(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        _tiles[x + _width * y] = default;
    }

    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        // Version ID
        emitter.BeginMapping();
        emitter.WriteString("version");
        emitter.WriteInt32(0);
        emitter.WriteString("width");
        emitter.WriteInt32(_width);
        emitter.WriteString("height");
        emitter.WriteInt32(_height);
        
        // Tile Data
        emitter.WriteString("tiles");
        emitter.BeginSequence(SequenceStyle.Flow);

        foreach (var tile in _tiles)
        {
            tile.Serialize(ref emitter);
        }
        
        emitter.EndSequence();
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, [NotNullWhen(true)] out Tilemap<T>? tilemap)
    {
        tilemap = null;
        
        if (parser.CurrentEventType != ParseEventType.MappingStart)
        {
            return false;
        }

        parser.Read();
        
        // Read Version Tag
        parser.Read();

        if (!parser.TryReadScalarAsInt32(out var version)) return false;
        
        // Read Width Tag
        parser.Read();
        
        if (!parser.TryReadScalarAsInt32(out var width)) return false;
        
        // Read Height Tag
        parser.Read();
        
        if (!parser.TryReadScalarAsInt32(out var height)) return false;
        
        // Read Tiles Tage
        parser.Read();
        
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            return false;
        }

        parser.Read();

        tilemap = new Tilemap<T>(width, height);

        for (int i = 0; i < width * height; i++)
        {
            if (!T.TryDeserialize(ref parser, out var tile))
            {
                return false;
            }

            tilemap._tiles[i] = tile;
        }

        if (parser.CurrentEventType != ParseEventType.SequenceEnd)
            return false;

        parser.Read();
        return true;
    }
}