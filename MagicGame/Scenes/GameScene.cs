using MagicGame.Scripts;
using MagicGame.Scripts.BookEditor;
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
    private Camera _camera;

    private Book book;
    
    public bool OnSceneLoaded()
    {
        WindowManager.TryOpenWindow("Magic Game", "game-window", 800, 600, out _window);
        Assets.TryLoadAssetBundle("core");

        if (!TilemapRenderer.Init())
        {
            return false;
        }
        
        _world = new World(new Vector2Int(32, 32));
        _swapchainTexture = new GpuTexture();
        _camera = new Camera(_window, 16, 0, 100);
        
        _world.Load();

        string source = """
                        
                        block main
                        push 5
                        call do
                        push 7
                        block do
                        push do
                                    
                        """;

        var program = Parser.Parse(source, new Dictionary<string, int>());

        VirtualMachine vm = new VirtualMachine(new List<Func<Stack<int>, bool>>());
        vm.SetProgram(program, 1);

        while (!vm.ProgramFinished)
        {
            if (vm.Step().IsError)
                break;
        }

        book = new Book();
        //book.Open();
        
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

        TilemapEditor.Update(_world, _camera);
        
        _world.Update(_camera);
        book.Update();
        
        if (_window is null)
            return;
        
        var queue = new ImQueue();

        var copyPass = queue.AddCopyPass();
        
        TilemapRenderer.CopyTiles(copyPass);
        
        copyPass.End();

        if (!queue.TryGetSwapchainTexture(_window, ref _swapchainTexture))
        {
            return;
        }
        
        var renderpass = queue.AddRenderPass(new ColorTargetSettings()
        {
            ClearColor = TilemapRenderer.ClearColor,
            GpuTexture = _swapchainTexture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        });
        
        TilemapRenderer.RenderTiles(renderpass, _camera);
        
        renderpass.End();
        queue.Submit();
    }

    public void OnSceneExit()
    {
        _world.Save();
    }
}