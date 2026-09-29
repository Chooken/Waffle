using WaffleEngine;
using WaffleEngine.UI;
using FillPanelType = Vector.Editor.UI.ColorPanel;

namespace Vector.Editor.UI;

// Splits the window into top toolbar, left layers panel, fill picker, and
// the viewport. Children must be added in this order so panels win mouse
// events over the viewport (INode gives activation to the first node under
// the cursor):
//   [0] toolbar, [1] shape list, [2] color panel, [3] viewport.
public class EditorLayout : INode
{
    public INode Toolbar => Children[0];
    public INode ShapeList => Children[1];
    public INode FillPanel => Children[2];
    public INode Viewport => Children[3];

    public override void OnUpdate()
    {
        if (Children.Count < 4)
            return;

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
            y = Rect.y + Theme.ToolbarHeight,
            w = Theme.SideWidth,
            h = Rect.h - Theme.ToolbarHeight - FillPanelType.PanelH,
        });
        FillPanel.SetRect(new IRect
        {
            x = Rect.x,
            y = Rect.y + Rect.h - FillPanelType.PanelH,
            w = Theme.SideWidth,
            h = FillPanelType.PanelH,
        });
        Viewport.SetRect(new IRect
        {
            x = Rect.x + Theme.SideWidth,
            y = Rect.y + Theme.ToolbarHeight,
            w = Rect.w - Theme.SideWidth,
            h = Rect.h - Theme.ToolbarHeight,
        });
    }
}
