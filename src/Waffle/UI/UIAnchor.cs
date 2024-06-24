using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public static class UIAnchor
    {
        public static Vector2 TopLeft => new Vector2(0,0);
        public static Vector2 Top => new Vector2(0.5f, 0);
        public static Vector2 TopRight => new Vector2(1f, 0);
        public static Vector2 Left => new Vector2(0, 0.5f);
        public static Vector2 Middle => new Vector2(0.5f, 0.5f);
        public static Vector2 Right => new Vector2(1, 0.5f);
        public static Vector2 BottomLeft => new Vector2(0, 1f);
        public static Vector2 Bottom => new Vector2(0.5f, 1f);
        public static Vector2 BottomRight => new Vector2(1, 1f);
    }
}
