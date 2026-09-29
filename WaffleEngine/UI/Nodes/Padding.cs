namespace WaffleEngine.UI.Nodes;

// Insets every direct child to the padded box. Fill-composition semantics:
// each child receives the full inset rect (for single-child wrapping or
// full-bleed overlays). Layouts that position children individually
// (scrolling lists, toolbars) keep their own math but should derive it
// from IRect.Inset() instead of spreading padding arithmetic around.
public class Padding(int Left, int Right, int Top, int Bottom) : INode
{
    public static Padding Uniform(int pad)
    {
        return new Padding(pad, pad, pad, pad);
    }

    public override void OnUpdate()
    {
        IRect box = Rect.Inset(Left, Top, Right, Bottom);
        foreach (INode child in Children)
            child.SetRect(box);
    }
}
