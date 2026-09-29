namespace WaffleEngine.UI.Nodes;

public class SplitView(int size, int gap) : INode
{
    public int Size = size;
    private bool _resizing;

    public override void OnEvent(NodeEvent node_event)
    {
        if (node_event == NodeEvent.MouseHold)
        {
            if (Math.Abs((int)Input.Mouse.Position.x - (Rect.x + Size)) < gap / 2)
            {
                Size = (int)Input.Mouse.Position.x - Rect.x;
            }
        }
    }

    public override void OnUpdate()
    {
        Children[0].SetRect(new IRect(
            Rect.x,
            Rect.y,
            Size - gap / 2,
            Rect.h
        ));

        if (Children.Count < 2) {
            return;
        }
        
        Children[1].SetRect(new IRect(
            Rect.x + Size + gap / 2,
            Rect.y,
            Rect.w - Size - gap / 2,
            Rect.h
        ));
    }
}