using System.Numerics;

namespace WaffleEngine
{
    public class InitScene : Scene
    {
        private Camera _camera = new Camera(0, 0, 0);
        private Sprite _logo = new Sprite("init", "codeaphobic_logo_horizontal").WithPosition(new Vector2(0, 0));

        private DateTime _start;

        private Scene _next_scene;

        public InitScene(Scene next_scene)
        {
            _next_scene = next_scene;
        }

        public override void Start()
        {
            AssetLoader.LoadFolderAsync("core");

            AssetLoader.LoadFolder("init");

            _start = DateTime.Now;

            _logo.PixelsPerUnit = 64;
        }

        public override void End()
        {
            AssetLoader.UnloadFile("init", "codeaphobic_logo_horizontal");
        }

        public override void Update()
        {
            if (!AssetLoader.IsAsyncFinished || (DateTime.Now - _start).TotalSeconds < 3)
                return;

            SceneManager.ChangeScene(_next_scene);
        }

        public override void Render()
        {
            SpriteRenderer.Render(_logo, _camera);
        }
    }
}
