
namespace WaffleEngine
{
    public struct UIBounds
    {
        public int Width;
        public int Height;

        public int OffsetX;
        public int OffsetY;

        public static UIBounds GetRenderBounds()
        {
            return new UIBounds
            {
                Width = Window.RenderWidth,
                Height = Window.RenderHeight,

                OffsetX = 0,
                OffsetY = 0,
            };
        }
    }
}
