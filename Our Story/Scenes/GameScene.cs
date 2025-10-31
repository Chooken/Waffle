using System.Numerics;
using OurStory.Editor;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;
using WaffleEngine.UI;
using Vector2 = WaffleEngine.Vector2;
using Vector3 = WaffleEngine.Vector3;

namespace OurStory.Scenes;

public class GameScene : IScene
{
    private Window? _window;
    private UIToplevel _ui;
    private GpuTexture _gameTexture;
    private GpuTexture _uiTexture = new ();
    private GpuTexture _swapchainTexture = new ();

    private SpriteBatch _batch = new SpriteBatch();
    private Sprite _sprite;
    private Transform _spriteTransform;
    private Camera _camera = new Camera();

    private TextureEditor editor;
    
    public bool OnSceneLoaded()
    {
        if (!Assets.TryLoadAssetBundle("Core"))
            return false;
        
        if (!WindowManager.TryOpenMainWindow("Our Story", 800, 600, out _window))
            return false;

        if (!Assets.TryGetTexture("Core", "Character_hat", out var texture))
            return false;
        
        if (!Assets.TryGetShader("builtin", "sprite", out var shader))
            return false;

        editor = new TextureEditor();
        editor.Start();
        
        shader.SetPipeline(new PipelineSettings()
        {
            VertexAttributes = null
        });
        
        uint gameSize = 128;

        _sprite = new Sprite()
        {
            Texture = texture,
            SpriteShader = shader,
            PixelsPerUnit = (int)(gameSize * 0.5f),
        };

        _spriteTransform = new Transform(Vector3.Zero, Vector3.One, Quaternion.Identity);
        
        _gameTexture = new GpuTexture(GpuTextureSettings.Default(gameSize * 4, gameSize * 4) with
        {
            ColorTarget = true,
        });

        _ui = new UIToplevel((WindowSdl)_window);
        _ui.Root = new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.PercentageWidth(100),
                Height = UISize.PercentageHeight(100),
                ChildAnchor = UIAnchor.Center,
                ChildDirection = UIDirection.None,
            })
            .AddUIElement(new UICrt(new Vector2(gameSize, gameSize), 0.5f)
                .Default(() => new UISettings()
                {
                    Width = UISize.PercentageWidth(100),
                    Height = UISize.PercentageHeight(100),
                    BorderRadius = new UIBorderRadius(5, 5, 5, 5, UISizeType.PercentageWidth),
                    Texture = _gameTexture,
                })
            );

        return true;
    }

    public void OnSceneUpdate()
    {
        Update();
        Render();
    }

    private void Update()
    {
        float y = MathF.Sin((float)DateTime.Now.TimeOfDay.TotalSeconds * MathF.PI * 0.01f);
        
        _spriteTransform.SetPosition(new Vector3(y, 0, 0));
        _batch.AddSprite(_sprite, _spriteTransform);
        
        _spriteTransform.SetPosition(new Vector3(0, y, 0));
        _batch.AddSprite(_sprite, _spriteTransform);
        
        _ui.Update();
        
        editor.Update();

        // _spriteTransform.SetPosition(new Vector3(0, 0, 0));
        // _batch.AddSprite(_sprite, _spriteTransform);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(_window, ref _swapchainTexture);

        ImCopyPass copyPass = queue.AddCopyPass();
        _batch.Upload(copyPass);
        copyPass.End();

        ImRenderPass spriteRenderPass = queue.AddRenderPass(new ColorTargetSettings()
        {
            ClearColor = Color.RGBA255(24, 24, 24, 255),
            GpuTexture = _gameTexture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store
        });
        
        _batch.Render(spriteRenderPass);
        spriteRenderPass.End();
        
        _uiTexture = _ui.Render(queue);
        queue.AddBlitPass(_uiTexture, _swapchainTexture, true);
        queue.Submit();
        
        editor.Render();
    }

    public void OnSceneExit()
    {
        
    }
}