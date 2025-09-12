using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class RenderList
{
    private HashSet<Material> materials;

    public void Render(GpuTexture renderTarget)
    {
        ImQueue queue = new ImQueue();
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0f, 0f, 0f, 0f),
            GpuTexture = renderTarget,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(colorTargetSettings);

        foreach (var material in materials)
        {
            renderPass.Bind(material);
            renderPass.DrawPrimatives(6, material.Instances, 0, 0);
        }
    }

    public void Add(Material material)
    {
        materials.Add(material);
    }

    public void Clear()
    {
        materials.Clear();
    }
}