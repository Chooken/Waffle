using System.Numerics;

namespace WaffleEngine
{
    public class InitScene : Scene
    {
        private Camera _camera;
        private Scene _next_scene;
        private Shader _sprite_shader;
        private DefaultSpriteMaterial _default_sprite_material;

        public InitScene(Scene next_scene)
        {
            _next_scene = next_scene;
        }

        public override void Init()
        {
            AssetLoader.LoadFolderAsync("core");

            AssetLoader.LoadFolder("init");

            _camera = new Camera(0, 0, 0);

            _sprite_shader = new Shader(
                $"{Environment.CurrentDirectory}/assets/shaders/basic_sprite.vert", 
                $"{Environment.CurrentDirectory}/assets/shaders/basic_sprite.frag");

            _default_sprite_material =
                new DefaultSpriteMaterial(_sprite_shader, Texture.GetTexture("init", "WaffleEngine64"));

            Sprite sprite_1 = new Sprite(_default_sprite_material);

            this.World.Create(
                new Transform( ).SetPosition(0, 0, 0),
                sprite_1
            );

            this.World.Create(
                new Transform( ).SetPosition(1, 1, 1),
                sprite_1
            );

            this.World.Create(
                new Transform( ).SetPosition(-1, -1, -1),
                sprite_1
            );
        }

        public override void Update()
        {
            SpriteRenderer.RenderZSorted(ref this.World, ref _camera);

            if (!AssetLoader.IsAsyncFinished || Time.TimeSinceStart < 3)
                return;

            SceneManager.ChangeScene(_next_scene);
        }

        public override void Deinit()
        {
            AssetLoader.UnloadFile("init", "codeaphobic_logo_horizontal");
            
            base.Deinit();
        }
    }
}
