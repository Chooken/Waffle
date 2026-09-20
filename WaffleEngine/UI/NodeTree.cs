using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class NodeTree
{
    public INode Root { get; private set; }
    public INode? Active { get; private set; }
    public INode? Focused { get; private set; }
    public Stack<IRect> Clipstack { get; private set; }
    
    public int HeightInUnits = 0;
    
    public IRect UnitRect = IRect.Zero;
    
    public NodeTree(INode root)
    {
        root.Tree = this;
        this.Root = root;
        this.Clipstack = new Stack<IRect>();
    }

    public void SetHeightInUnits(int height)
    {
        HeightInUnits = height;
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
            w = (int)((float)target.Width / target.Height * HeightInUnits),
            h = HeightInUnits,
        };
        
        Root.SetRect(UnitRect);
        Root.PropagateUpdate();

        bool process_events = true;
        Root.PropagateStates(ref process_events);
    }

    public void Draw(ImQueue queue, GpuTexture target)
    {
        if (UnitRect == IRect.Zero)
        {
            return;
        }
        
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0,0,0,0),
            GpuTexture = target,
            LoadOperation = LoadOperation.Load,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);
        
        Root.Draw(renderPass, UnitRect);
        
        renderPass.End();
    }
}