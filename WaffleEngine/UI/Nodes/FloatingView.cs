namespace WaffleEngine.UI.Nodes;

public class FloatingView : INode
{
    public IVector2 Position;
    public IVector2 Size;

    public override void OnUpdate()
    {
        foreach (INode child in Children)
        {
            child.SetRect(new IRect
            {
                x = Position.x,
                y = Position.y,
                w = Size.x,
                h = Size.y
            });
        }
    }
}