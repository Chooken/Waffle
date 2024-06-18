using System;
using System.IO;
using System.Numerics;
using WaffleEngine;

namespace Game.Core
{
    public class TitleScreen : Scene
    {
        private static void Main(string[] args)
        {
            Application.StartingScene(new TitleScreen());
        }

        int targetFPS = 60;
        Camera camera = new Camera(0, 0, 0);
        Sprite player = new Sprite("core", "Character").WithPosition(new Vector2(0, 0));

        public override void Update()
        {
            //Log.Error("AHHAHAH");
            //Log.Info("Key W: {0}", Keyboard.IsDown(Keycode.W));
            //Log.Info("Mouse Left: {0}", Mouse.IsLeft());

            if (Keyboard.IsPressed(Keycode.F))
                Window.SetTargetFPS(targetFPS ^= 60);

            Vector2 moveVec = new Vector2(0, 0);

            if (Keyboard.IsDown(Keycode.A))
                moveVec.X += 1;
            if (Keyboard.IsDown(Keycode.D))
                moveVec.X -= 1;


            if (Keyboard.IsDown(Keycode.W))
                moveVec.Y += 1;
            if (Keyboard.IsDown(Keycode.S))
                moveVec.Y -= 1;

            camera.Move(moveVec.X * Raylib_cs.Raylib.GetFrameTime(), moveVec.Y * Raylib_cs.Raylib.GetFrameTime());
        }

        public override void Render()
        {
            SpriteRenderer.Render(player, camera);

            Raylib_cs.Raylib.DrawFPS(10,10);
        }
    }
}
