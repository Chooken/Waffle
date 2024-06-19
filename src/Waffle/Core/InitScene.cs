using Game.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace WaffleEngine
{
    public class InitScene : Scene
    {
        private Camera _camera = new Camera(0, 0, 0);
        private Sprite _logo = new Sprite("init", "codeaphobic_logo_horizontal").WithPosition(new Vector2(0, 0));

        private DateTime _start;

        public override void Start()
        {
            AssetLoader.LoadFolderAsync("core");

            AssetLoader.LoadFolder("init");

            _start = DateTime.Now;

            _logo.PixelsPerUnit = 64;
        }

        public override void Update()
        {
            if (!AssetLoader.IsAsyncFinished || (DateTime.Now - _start).TotalSeconds < 3)
                return;

            SceneManager.ChangeScene(new TestScene());
        }

        public override void Render()
        {
            SpriteRenderer.Render(_logo, _camera);
        }
    }
}
