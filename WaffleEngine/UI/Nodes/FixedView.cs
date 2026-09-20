namespace WaffleEngine.UI.Nodes;

public class FixedView : INode
{
    public enum Alignment
    {
        Start,
        Middle,
        End
    }

    public Alignment VerticalAlignment = Alignment.Start;
    public Alignment HorizontalAlignment = Alignment.Start;
    public IVector2 Size;

    public override void OnUpdate()
    {
        float v_align = VerticalAlignment switch { 
            Alignment.Start => 0, 
            Alignment.Middle => 0.5f, 
            Alignment.End => 1,
            _ => 0
        };

        float h_align = HorizontalAlignment switch { 
            Alignment.Start => 0, 
            Alignment.Middle => 0.5f, 
            Alignment.End => 1,
            _ => 0
        };

        int v_offset = (int)((Rect.Height - Size.y) * v_align);
        int h_offset = (int)((Rect.Width - Size.x) * h_align);

        foreach (var child in Children)
        {
            child.SetRect(new IRect()
            {
                x = h_offset,
                y = v_offset,
                w = Size.x,
                h = Size.y
            });
        }
    }
}