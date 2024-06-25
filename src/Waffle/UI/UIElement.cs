using System.Numerics;

namespace WaffleEngine
{
    public abstract class UIElement
    {
        public Vector2 ScreenAnchor;
        public Vector2 ElementAnchor;

        /// <summary>
        /// The width as a percentage of the screen width.
        /// </summary>
        public float Width;

        /// <summary>
        /// The height as a percentage of the screen height.
        /// </summary>
        public float Height;

        public int GetWidthInPixels() => (int)Math.Round((Width / 100) * Window.RenderWidth);
        public int GetHeightInPixels() => (int)Math.Round((Height / 100) * Window.RenderHeight);

        public abstract void Render(UIBounds bounds);

        public abstract void Update();

        public Vector2 GetScreenPosition(UIBounds bounds)
        {
            Vector2 element_anchor_position = new Vector2(GetWidthInPixels(), GetHeightInPixels()) * ElementAnchor;
            Vector2 screen_anchor_position = new Vector2(bounds.Width, bounds.Height) * ScreenAnchor + new Vector2(bounds.OffsetX, bounds.OffsetY);

            Vector2 position = screen_anchor_position - element_anchor_position;

            return position;
        }

        public UIBounds GetElementBounds(UIBounds bounds)
        {
            Vector2 element_position = GetScreenPosition(bounds);

            return new UIBounds{
                Width = GetWidthInPixels(), 
                Height = GetHeightInPixels(), 

                OffsetX = (int)element_position.X,
                OffsetY = (int)element_position.Y,
            };
        }
    }
}
