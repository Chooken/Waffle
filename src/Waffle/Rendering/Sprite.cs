using Raylib_cs;
using System.Numerics;

namespace WaffleEngine
{
    public struct Sprite
    {
        public int PixelsPerUnit = 16;

        private string _folder;
        private string _file;

        public Sprite(string folder, string file)
        {
            _file = file;
            _folder = folder;
        }

        public Texture2D GetTexture() => AssetLoader.GetTexture(_folder, _file);
    }
}
