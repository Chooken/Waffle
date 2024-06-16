using System;
using WaffleEngine;

namespace Game.Core
{
    public class TitleScreen : Scene
    {
        private static void Main(string[] args)
        {
            Application.StartingScene(new TitleScreen());
        }

        public override void Update()
        {
            //Log.Error("AHHAHAH");
            //Log.Info("Key W: {0}", Keyboard.IsDown(Keycode.W));
            //Log.Info("Mouse Left: {0}", Mouse.IsLeft());
        }

        public override void Render()
        {
            Raylib_cs.Raylib.DrawFPS(10,10);
        }
    }
}
