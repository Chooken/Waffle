using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine.src.Waffle.Core;

namespace WaffleEngine
{
    public class Sprite
    {
        public static int PixelsPerUnit = 16;

        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;

        private string _folder;
        private string _file;

        public Sprite(string folder, string file)
        {
            Position = new Vector2(0, 0);
            Rotation = 0;
            Scale = new Vector2(1, 1);

            _file = file;
            _folder = folder;
        }

        public Sprite WithPosition(Vector2 position)
        {
            Position = position;

            return this;
        }

        public void Draw()
        {
            Texture2D texture = AssetLoader.GetTexture(_folder, _file);

            float local_width = (float)texture.Width / PixelsPerUnit;
            float local_height = (float)texture.Height / PixelsPerUnit;

            Raylib.DrawTexturePro(
                texture,
                new Rectangle(0, 0, -texture.Width, -texture.Height),
                new Rectangle(-local_width * 0.5f, -local_height * 0.5f, local_width, local_height),
                Position,
                Rotation,
                Color.White
            );
        }

        public Texture2D GetTexture()
        {
            return AssetLoader.GetTexture(_folder, _file);
        }
    }
}
