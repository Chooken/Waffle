using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI.Old;

public class UIPass(UIToplevel toplevel) : IPass
{
    public void Submit(ImQueue queue)
    {
        toplevel.Render(queue);
    }
}