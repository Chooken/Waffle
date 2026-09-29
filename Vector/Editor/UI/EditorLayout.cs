using WaffleEngine;
using WaffleEngine.UI;

namespace Vector.Editor.UI;

// macOS-style workstation: top toolbar, bottom status bar, layers source
// list left, inspector right, viewport center. Children must be added in
// this order so panels win mouse events over the viewport (INode gives
// activation to the first node under the cursor):
//   [0] toolbar, [1] layers, [2] status, [3] inspector, [4] viewport.
public class EditorLayout : INode
{
    public INode Toolbar => Children[0];
    public INode ShapeList => Children[1];
    public INode StatusBar => Children[2];
    public INode Inspector => Children[3];
    public INode Viewport => Children[4];

    public override void OnUpdate()
    {
        if (Children.Count < 5)
            return;

        int contentTop = Rect.y + Theme.ToolbarHeight;
        int contentH = Rect.h - Theme.ToolbarHeight - Theme.StatusHeight;

        Toolbar.SetRect(new IRect
        {
            x = Rect.x,
            y = Rect.y,
            w = Rect.w,
            h = Theme.ToolbarHeight,
        });
        ShapeList.SetRect(new IRect
        {
            x = Rect.x,
            y = contentTop,
            w = Theme.SideWidth,
            h = contentH,
        });
        StatusBar.SetRect(new IRect
        {
            x = Rect.x,
            y = Rect.y + Rect.h - Theme.StatusHeight,
            w = Rect.w,
            h = Theme.StatusHeight,
        });
        Inspector.SetRect(new IRect
        {
            x = Rect.x + Rect.w - Theme.InspectorWidth,
            y = contentTop,
            w = Theme.InspectorWidth,
            h = contentH,
        });
        Viewport.SetRect(new IRect
        {
            x = Rect.x + Theme.SideWidth,
            y = contentTop,
            w = Rect.w - Theme.SideWidth - Theme.InspectorWidth,
            h = contentH,
        });
    }
}
