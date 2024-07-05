using System.Numerics;

namespace WaffleEngine
{
    public class InitScene : Scene
    {
        private Camera _camera;
        private Scene _next_scene;

        public InitScene(Scene next_scene)
        {
            _next_scene = next_scene;
        }

        public override void Init()
        {
            AssetLoader.LoadFolderAsync("core");

            AssetLoader.LoadFolder("init");

            _camera = new Camera(0, 0, 0);

            Sprite sprite = new Sprite("init", "codeaphobic_logo_horizontal");
            sprite.PixelsPerUnit = 64;

            Sprite sprite_1 = new Sprite("init", "WaffleEngine64");

            //this.World.Create(
            //     new Transform(),
            //     sprite
            //);

            this.World.Create(
                new Transform { Position = new Vector3(0, 0, 0) },
                sprite_1
            );

            this.World.Create(
                new Transform { Position = new Vector3(1, 1, 1) },
                sprite_1
            );

            this.World.Create(
                new Transform { Position = new Vector3(-1, -1, -1) },
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
