using System.Numerics;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using Vector4 = WaffleEngine.Vector4;

namespace MagicGame.Scripts.BookEditor;

public class Book
{
    private TextEditor _editor;
    private Buffer<GpuTile> _tiles;
    private int _width;
    private int _height;

    private bool _open = false;
    private int _page = 0;

    public Book()
    {
        _width = 17;
        _height = 13;
        
        _editor = new TextEditor(_height);
        _tiles = new Buffer<GpuTile>(BufferUsage.GraphicsStorageRead, _height * _width);

        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width; x++)
        {
            _tiles[x + y * _width] = new GpuTile()
            {
                Position = new Vector4(x - _width * 0.5f, y - _height * 0.5f, 0, 1),
                TileIndex = 8 * 8 - 1,
            };
        }

        _tiles[0] = _tiles[0] with
        {
            TileIndex = 50,
            Rotation = 1,
            PaletteIndex = 6,
        };
        
        _tiles[_width - 1] = _tiles[_width - 1] with
        {
            TileIndex = 50,
            Rotation = 2,
            PaletteIndex = 6,
        };
        
        _tiles[(_height - 1) * _width] = _tiles[(_height - 1) * _width] with
        {
            TileIndex = 50,
            PaletteIndex = 6,
        };
        
        _tiles[_height * _width - 1] = _tiles[_height * _width - 1] with
        {
            TileIndex = 50,
            Rotation = 3,
            PaletteIndex = 6,
        };

        for (int x = 1; x < _width - 1; x++)
        {
            _tiles[(_height - 1) * _width + x] = _tiles[(_height - 1) * _width + x] with
            {
                TileIndex = 49,
                Rotation = 3,
                PaletteIndex = 6,
            };
        }
        
        for (int y = 1; y < _height - 1; y++)
        {
            _tiles[y * _width] = _tiles[y * _width] with
            {
                TileIndex = 49,
                Rotation = 0,
                PaletteIndex = 6,
            };

            _tiles[y * _width + _width - 1] = _tiles[y * _width + _width - 1] with
            {
                TileIndex = 49,
                Rotation = 2,
                PaletteIndex = 6,
            };
        }
    }

    public void Open() => _open = true;
    public void Close() => _open = false;

    public void Update()
    {
        if (!_open)
            return;
        
        for (int x = 0; x < _width - 2; x++)
        {
            _tiles[x + 1] = _tiles[x + 1] with
            {
                TileIndex = _page == x ? 51 : 7,
                Rotation = 3,
                PaletteIndex = _page == x ? 7 : 6,
            };
        }
        
        TilemapRenderer.QueueTiles(_tiles);
    }
}