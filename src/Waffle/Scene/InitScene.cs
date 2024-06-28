using Flecs.NET.Core;
using System.Numerics;

namespace WaffleEngine
{
    public class InitScene : Scene
    {
        private DateTime _start;

        private Scene _next_scene;

        public InitScene(Scene next_scene)
        {
            _next_scene = next_scene;
        }

        public override void Init()
        {
            AssetLoader.LoadFolderAsync("core");

            AssetLoader.LoadFolder("init");

            _start = DateTime.Now;

            this.World.Set(new Camera(0, 0, 0));

            Sprite sprite = new Sprite("init", "codeaphobic_logo_horizontal");
            sprite.PixelsPerUnit = 64;

            Sprite sprite_1 = new Sprite("init", "WaffleEngine64");

            //this.World.Entity("Logo")
            //    .Set(new Transform())
            //    .Set(sprite);

            this.World.Entity("Logo-2")
                .Set(new Transform { Position = new Vector3(0,0,0) })
                .Set(sprite_1);

            this.World.Entity("Logo-3")
                .Set(new Transform { Position = new Vector3(1, 1, 1) })
                .Set(sprite_1);

            this.World.Entity("Logo-4")
                .Set(new Transform { Position = new Vector3(-1, -1, -1) })
                .Set(sprite_1);

            this.World.Routine("Check if loaded")
                .Kind(Ecs.OnUpdate)
                .Iter(Update);

            SpriteRenderer.Render(ref this.World);
        }

        public void Update(Iter iterator)
        {
            if (!AssetLoader.IsAsyncFinished || (DateTime.Now - _start).TotalSeconds < 3)
                return;

            //SceneManager.ChangeScene(_next_scene);
        }

        public override void Deinit()
        {
            AssetLoader.UnloadFile("init", "codeaphobic_logo_horizontal");
            this.World.Dispose();
        }
    }
}
