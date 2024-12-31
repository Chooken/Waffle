

using Arch.Core;
using System.Numerics;

namespace WaffleEngine
{
    public struct Sprite
    {
        public int PixelsPerUnit = 16;
        public Material Material;
        public Vector2 Offset;

        public Sprite(Material material, Vector2 offset)
        {
            Material = material;
            Offset = offset;

            _query_out_of_date = true;
        }

        public static QueryDescription Query
        {
            get
            {
                if (_query_out_of_date)
                {
                    _sprite_query = new QueryDescription().WithAll<Transform, Sprite>();
                }

                return _sprite_query;
            }
        }
        private static QueryDescription _sprite_query;
        private static bool _query_out_of_date = false;
    }
    
    public struct SpriteOpaque
    {
        public int PixelsPerUnit = 16;
        public Material Material;
        public Vector2 Offset;

        public SpriteOpaque(Material material, Vector2 offset)
        {
            Material = material;
            Offset = offset;

            _query_out_of_date = true;
        }

        public static QueryDescription Query
        {
            get
            {
                if (_query_out_of_date)
                {
                    _sprite_query = new QueryDescription().WithAll<Transform, SpriteOpaque>();
                }

                return _sprite_query;
            }
        }
        private static QueryDescription _sprite_query;
        private static bool _query_out_of_date = false;
    }
}
