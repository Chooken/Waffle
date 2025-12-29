using WaffleEngine;

namespace MagicGame.Scripts;

public static class TilemapEditor
{
    private static int _selectedTile = 33;
    private static int _selectedColor = 6;
    private static int _selectedHeight = 0;

    public static Vector3Int PlayerPos;

    public static Tile SelectedTile => new Tile()
    {
        TileIndex = _selectedTile,
        PaletteIndex = _selectedColor,
        Height = _selectedHeight,
    };

    public static void UpdateSelectedBlock()
    {
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Equals))
        {
            _selectedTile = Math.Max(Math.Min(_selectedTile + 1, 63), 0);
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Minus))
        {
            _selectedTile = Math.Max(Math.Min(_selectedTile - 1, 63), 0);
        }

        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.LeftBracket))
        {
            _selectedHeight -= 1;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.RightBracket))
        {
            _selectedHeight += 1;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num1))
        {
            _selectedColor = 0;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num2))
        {
            _selectedColor = 1;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num3))
        {
            _selectedColor = 2;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num4))
        {
            _selectedColor = 3;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num5))
        {
            _selectedColor = 4;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num6))
        {
            _selectedColor = 5;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num7))
        {
            _selectedColor = 6;
        }
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Num8))
        {
            _selectedColor = 7;
        }
    }

    public static void UpdatePlayer()
    {
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.A))
            PlayerPos.x -= 1;
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.D))
            PlayerPos.x += 1;
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.S))
            PlayerPos.y -= 1;
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.W))
            PlayerPos.y += 1;

        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Space))
            PlayerPos.z += 1;
        
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.LeftShift))
            PlayerPos.z -= 1;
    }

    public static void Update(World world, Camera camera)
    {
        var mouseClip = camera.ScreenToClipSpace(Input.Mouse.Position);
        
        var tile = world.ClipToWorldSpace(mouseClip, camera);

        var tilePos = new Vector3Int((int)Math.Floor(tile.x), (int)Math.Floor(tile.y), SelectedTile.Height);
        
        world.SetEntity(tilePos, SelectedTile);

        if (Input.Mouse.IsLeftPressed)
        {
            world.SetTile(tilePos.x, tilePos.y, SelectedTile);
        }

        if (Input.Mouse.IsRightPressed)
        {
            
            world.SetTile(tilePos.x, tilePos.y, world.GetTile(tilePos.x, tilePos.y) with
            {
                TileIndex = 0,
            });
        }
    }
}