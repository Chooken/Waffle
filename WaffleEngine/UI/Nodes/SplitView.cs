namespace WaffleEngine.UI.Nodes;

public class SplitView(bool left, int size, int gap) : INode
{
    public int Size = size;
    private bool _resizing;

    public override void OnEvent(NodeEvent node_event)
    {
        int left_size = left ? Size : Rect.w - Size;
        
        if (Tree.Input.Mouse.IsLeftPressed)
        {
            if (Math.Abs((int)Tree.Input.Mouse.Position.x - (Rect.x + left_size)) < gap / 2)
            {
                _resizing = true;
            }
        } 
        else if (node_event == NodeEvent.MouseClick)
        {
            _resizing = false;
        }
    }

    public override void OnUpdate()
    {
        if (_resizing)
        {
            Size = left ? (int)Tree.Input.Mouse.Position.x - Rect.x : Rect.w - ((int)Tree.Input.Mouse.Position.x - Rect.x);
        }
        
        int left_size = left ? Size : Rect.w - Size;
        
        Children[0].SetRect(new IRect(
            Rect.x,
            Rect.y,
            left_size - gap / 2,
            Rect.h
        ));

        if (Children.Count < 2) {
            return;
        }
        
        Children[1].SetRect(new IRect(
            Rect.x + left_size + gap / 2,
            Rect.y,
            Rect.w - left_size - gap / 2,
            Rect.h
        ));
    }
}