using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory;

public class RenderUI : Query<UIToplevel>
{
    private GpuTexture source;
    private GpuTexture destination = new GpuTexture();
    
    public override void Run(ref UIToplevel component)
    {
        foreach (var window in WindowManager.Windows)
        {
            ImQueue queue = new ImQueue();
            queue.TryGetSwapchainTexture(window, ref destination);
            source = component.Render(queue);
            queue.AddBlitPass(source, destination, true);
            queue.Submit();
        }
    }
}