using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class NodeTree
{
    public INode Root { get; private set; }
    public INode? Active { get; private set; }
    public INode? Focused { get; private set; }
    public Stack<IRect> Clipstack { get; private set; }
    
    public IRect UnitRect = IRect.Zero;
    
    public NodeTree(INode root)
    {
        root.Tree = this;
        this.Root = root;
        this.Clipstack = new Stack<IRect>();
        root.OnInit();
    }
    
    public void SetActive(INode? active)
    {
        this.Active = active;
    }

    public void SetFocused(INode? focused)
    {
        this.Focused = focused;
    }

    public void Update(GpuTexture target)
    {
        if (target.Width == 0 || target.Height == 0)
        {
            return;
        }
        
        UnitRect = new IRect
        {
            x = 0,
            y = 0,
            w = (int)target.Width,
            h = (int)target.Height,
        };
        
        bool process_events = true;
        Root.PropagateStates(ref process_events);
        
        Root.SetRect(UnitRect);
        Root.PropagateUpdate();
    }

    public void Draw(ImQueue queue, GpuTexture target, bool clear)
    {
        if (UnitRect == IRect.Zero)
        {
            return;
        }
        
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0,0,0,0),
            GpuTexture = target,
            LoadOperation = clear ? LoadOperation.Clear : LoadOperation.Load,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);
        
        Root.Draw(renderPass, UnitRect);
        
        renderPass.End();
    }
}