using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine;

namespace WaffleEngine
{
    public static class SpriteRenderer
    {
        public static void Render(Sprite sprite, Camera camera)
        {
            Raylib.BeginMode3D(camera);

            //Raylib.DrawRectangleV(new Vector2(-0.5f,-0.5f), new Vector2(1, 1), Color.Pink);
            sprite.Draw();

            Raylib.EndMode3D();
        }

        public static void Render()
        {

        }
    }
}
