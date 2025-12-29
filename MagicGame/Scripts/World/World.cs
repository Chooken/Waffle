using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Serializer;

namespace MagicGame.Scripts;

public class World
{
    private Dictionary<Vector2Int, Tilemap<Tile>> _chunks = new();
    private Vector2Int _chunkSize;

    private Vector3Int _focus;
    private List<(Tile, Vector3Int)> _entities = new();
    
    // Rendering
    private Buffer<GpuTile>[] _gpuTiles;

    public World(Vector2Int chunkSize)
    {
        _chunkSize = chunkSize;
        _gpuTiles = new Buffer<GpuTile>[9];

        for (int i = 0; i < 9; i++)
        {
            _gpuTiles[i] = new Buffer<GpuTile>(BufferUsage.GraphicsStorageRead, chunkSize.x * chunkSize.y);
        }
    }

    public void SetEntity(Vector3Int postion, Tile tile)
    {
        _entities.Add((tile, postion));
    }

    public void SetFocus(Vector3Int pos)
    {
        _focus = pos;
    }

    public Tile GetTile(int x, int y)
    {
        var chunk = new Vector2Int(
            (int)MathF.Floor((float)x / _chunkSize.x), 
            (int)MathF.Floor((float)y / _chunkSize.y));

        if (_chunks.TryGetValue(chunk, out var tilemap))
        {
            if (tilemap.TryGetTile(WMath.Mod(x, _chunkSize.x), WMath.Mod(y, _chunkSize.y), out var tile))
            {
                return tile;
            }
        }

        return default;
    }

    public void SetTile(int x, int y, Tile tile)
    {
        var chunk = new Vector2Int(
            (int)MathF.Floor((float)x / _chunkSize.x), 
            (int)MathF.Floor((float)y / _chunkSize.y));

        if (!_chunks.ContainsKey(chunk))
        {
            _chunks[chunk] = new Tilemap<Tile>(_chunkSize.x, _chunkSize.y);
        }
        
        _chunks[chunk].SetTile(WMath.Mod(x, _chunkSize.x), WMath.Mod(y, _chunkSize.y), tile);;
    }

    public void Update(Camera camera)
    {
        Vector2Int playerChunk = new Vector2Int(
            (int)Math.Floor((float)_focus.x / _chunkSize.x),
            (int)Math.Floor((float)_focus.y / _chunkSize.y));

        for (int y = -1; y <= 1; y++)
        for (int x = -1; x <= 1; x++)
        {
            if (IsChunkOffscreen(x, y, camera))
                continue;
            
            UploadChunk(playerChunk, new Vector2Int(x, y));
        }

        foreach (var (entityTile, position) in _entities)
        {
            Vector2Int entityChunk = new Vector2Int(
                (int)Math.Floor((float)position.x / _chunkSize.x),
                (int)Math.Floor((float)position.y / _chunkSize.y));

            Vector2Int relative = entityChunk - playerChunk;

            if (Math.Abs(relative.x) > 1 || Math.Abs(relative.y) > 1)
            {
                continue;
            }
            
            Vector2Int entityOffset = new Vector2Int(
                WMath.Mod(position.x, _chunkSize.x),
                WMath.Mod(position.y, _chunkSize.y));

            GpuTile tempTile = _gpuTiles[relative.x + 1 + (relative.y + 1) * 3]
                [entityOffset.x + _chunkSize.x * entityOffset.y];
            _gpuTiles[relative.x + 1 + (relative.y + 1) * 3]
                [entityOffset.x + _chunkSize.x * entityOffset.y] = new GpuTile()
            {
                Position = tempTile.Position with { z = entityTile.Height },
                TileIndex = entityTile.TileIndex,
                PaletteIndex = entityTile.PaletteIndex,
                FadeIndex = Math.Abs(entityTile.Height - _focus.z),
            };
        }
        
        for (int y = -1; y <= 1; y++)
        for (int x = -1; x <= 1; x++)
        {
            if (IsChunkOffscreen(x, y, camera))
                continue;
            
            TilemapRenderer.QueueTiles(_gpuTiles[x + 1 + (y + 1) * 3]);
        }
        
        _entities.Clear();
    }

    private bool IsChunkOffscreen(int x, int y, Camera camera)
    {
        var min = new Vector2(
            x * _chunkSize.x - WMath.Mod(_focus.x, _chunkSize.x) - 0.5f, 
            y * _chunkSize.y -  WMath.Mod(_focus.y, _chunkSize.y) - 0.5f);
        
        var max = new Vector2(
            _chunkSize.x + x * _chunkSize.x - WMath.Mod(_focus.x, _chunkSize.x) - 0.5f, 
            _chunkSize.y + y * _chunkSize.y -  WMath.Mod(_focus.y, _chunkSize.y) - 0.5f);

        var camWidth = camera.Width * 0.5f;
        var camHeight = camera.Height * 0.5f;

        return min.x > camWidth || max.x < -camWidth ||
               min.y > camHeight || max.y < -camHeight;
    }

    private void UploadChunk(Vector2Int chunk, Vector2Int offset)
    {
        chunk += offset;
        
        if (offset.x < -1 || offset.x > 1 || 
            offset.y < -1 || offset.y > 1)
            return;
        
        var tiles = _gpuTiles[offset.x + 1 + (offset.y + 1) * 3];
        tiles.Clear();
        
        for (int y = 0; y < _chunkSize.y; y++)
        for (int x = 0; x < _chunkSize.x; x++)
        {
            GpuTile gpuTile = new GpuTile();
            gpuTile.Position = new Vector4(
                x + offset.x * _chunkSize.x - WMath.Mod(_focus.x, _chunkSize.x) - 0.5f, 
                y + offset.y * _chunkSize.y -  WMath.Mod(_focus.y, _chunkSize.y) - 0.5f, 
                0, 1);
            
            if (_chunks.TryGetValue(chunk, out Tilemap<Tile>? tilemap) && tilemap.TryGetTile(x, y, out Tile tile))
            {
                gpuTile.Position.z = tile.Height;
                gpuTile.TileIndex = tile.TileIndex;
                gpuTile.PaletteIndex = tile.PaletteIndex;
                gpuTile.FadeIndex = Math.Abs(tile.Height - _focus.z);
            }
            
            tiles.Add(gpuTile);
        }
    }

    public Vector2 ClipToWorldSpace(Vector2 screenPos, Camera camera)
    {
        Vector2Int playerChunk = new Vector2Int(
            (int)Math.Floor((float)_focus.x / _chunkSize.x),
            (int)Math.Floor((float)_focus.y / _chunkSize.y));
        
        Vector2Int playerOffset = new Vector2Int(
            WMath.Mod(_focus.x, _chunkSize.x),
            WMath.Mod(_focus.y, _chunkSize.y));
        
        System.Numerics.Matrix4x4.Invert(System.Numerics.Matrix4x4.CreateTranslation(-playerOffset.x - 0.5f, -playerOffset.y - 0.5f, 10), out var iv);
        System.Numerics.Matrix4x4.Invert(camera.GetProjectionMatrix(), out var ip);

        var viewPos = System.Numerics.Vector2.Transform(screenPos, ip);
        var worldPos = System.Numerics.Vector2.Transform(viewPos, iv);

        return new Vector2(
            worldPos.X + playerChunk.x * _chunkSize.x, 
            worldPos.Y + playerChunk.y * _chunkSize.y);
    }

    public void Save()
    {
        foreach (var (key, value) in _chunks)
        {
            Yaml.StartSerialization(out var buffer, out var emitter);
            
            emitter.BeginMapping();
            
            emitter.WriteString("x");
            emitter.WriteInt32(key.x);
            
            emitter.WriteString("y");
            emitter.WriteInt32(key.y);
            
            emitter.WriteString("tilemap");
            value.Serialize(ref emitter);
            
            emitter.EndMapping();

            Yaml.TryEndSerialization($"world/{key.x}x{key.y}.chunk", buffer);
        }
    }

    public void Load()
    {
        var files = Directory.EnumerateFiles("world", "*.chunk");

        foreach (var file in files)
        {
            if (!Yaml.TryStartDeserialize(file, out var parser))
                continue;
            
            if (parser.CurrentEventType != ParseEventType.MappingStart)
                continue;

            parser.Read();
            
            // Read x Tag
            parser.Read();
            
            if (!parser.TryReadScalarAsInt32(out int x))
                continue;
            
            // Read y Tag
            parser.Read();
            
            if (!parser.TryReadScalarAsInt32(out int y))
                continue;
            
            // Read tilemap Tag
            parser.Read();
            
            if (!Tilemap<Tile>.TryDeserialize(ref parser, out var tilemap))
            {
                continue;
            }
            
            _chunks[new Vector2Int(x, y)] = tilemap;
        }
    }
}