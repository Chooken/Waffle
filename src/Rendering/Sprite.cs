

namespace WaffleEngine
{
    public struct Sprite(Material material)
    {
        public int PixelsPerUnit = 16;
        public Material Material = material;
    }
    
    public struct SpriteOpaque(Material material)
    {
        public int PixelsPerUnit = 16;
        public Material Material = material;
    }
}
