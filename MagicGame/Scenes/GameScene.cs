using MagicGame.Scripts;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace MagicGame.Scenes;

public class GameScene : IScene
{
    private Window? _window;
    private GpuTexture _swapchainTexture;
    private World _world;
    private Color _background;

    private Vector3Int PlayerPos;
    
    public bool OnSceneLoaded()
    {
        WindowManager.TryOpenWindow("Magic Game", "game-window", 800, 600, out _window);
        Assets.TryLoadAssetBundle("core");
        
        if (!Assets.TryGetTexture("core", "tilesheet", out var tilesheet))
        {
            return false;
        }
        
        if (!Assets.TryGetTexture("core", "palette", out var palette))
        {
            return false;
        }
        
        _world = new World(new Vector2Int(32, 32), tilesheet, palette);
        _background = palette.GetAs<Color255>()[8];
        _swapchainTexture = new GpuTexture();
        
        _world.Load();
        
        return true;
    }

    public void OnSceneUpdate()
    {
        TilemapEditor.UpdateSelectedBlock();
        TilemapEditor.UpdatePlayer();
        
        // Player
        var height = _world.GetTile(TilemapEditor.PlayerPos.x, TilemapEditor.PlayerPos.y).Height;
        _world.SetEntity(TilemapEditor.PlayerPos, new Tile()
        {
            TileIndex = 25,
            PaletteIndex = 7,
            Height = height,
        });
        _world.SetFocus(TilemapEditor.PlayerPos with { z = height});

        TilemapEditor.Update(_world, _window);
        
        var queue = new ImQueue();
        _world.Update(queue);
        
        if (_window is null)
            return;

        if (!queue.TryGetSwapchainTexture(_window, ref _swapchainTexture))
        {
            return;
        }
        
        var renderpass = queue.AddRenderPass(new ColorTargetSettings()
        {
            ClearColor = _background,
            GpuTexture = _swapchainTexture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        });
        
        _world.Render(renderpass, _window);
        
        renderpass.End();
        queue.Submit();
    }

    public void OnSceneExit()
    {
        _world.Save();
    }
}