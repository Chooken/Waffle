namespace WaffleEngine.UI;

public class NodeTree
{
    public INode Root { get; private set; }
    public INode? Active { get; private set; }
    public INode? Focused { get; private set; }
    public Stack<IRect> Clipstack { get; private set; }
    
    public NodeTree(INode root)
    {
        this.Root = root;
    }
    
    public void SetActive(INode? active)
    {
        this.Active = active;
    }

    public void SetFocused(INode? focused)
    {
        this.Focused = focused;
    }

    public void Update(IRect root_rect)
    {
        Root.SetRect(root_rect);
        Root.PropagateUpdate();
    }

    public void Draw()
    {
        Root.Draw();
    }
}