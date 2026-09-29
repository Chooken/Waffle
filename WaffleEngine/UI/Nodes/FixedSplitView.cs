namespace WaffleEngine.UI.Nodes;

public class FixedSplitView(int size, bool start, bool horizontal) : INode
{
    public override void OnUpdate()
    {
        if (Children.Count < 1) {
            return;
        }

        Children[0].SetRect(new IRect(
            start || !horizontal ? Rect.x : Rect.x + Rect.w - size,
            start || horizontal ? Rect.y : Rect.y + Rect.h - size,
            horizontal ? size : Rect.w,
            !horizontal ? size : Rect.h
            ));

        if (Children.Count < 2) {
            return;
        }

        Children[1].SetRect(new IRect(
            horizontal && start ? Rect.x + size : Rect.x,
            !horizontal && start ? Rect.y + size : Rect.y,
            horizontal ? Rect.w - size : Rect.w,
            !horizontal ? Rect.h - size : Rect.h
        ));
    }
}