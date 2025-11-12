using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public static partial class Ui
{
    public static void RenderToTexture(UiElement root, ImQueue queue, float scale, in GpuTexture texture)
    {
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = Color.RGBA255(20, 20, 20, 255),
            GpuTexture = texture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);

        root.Layout.CalculateFitSize(root, true);
        root.Layout.CalculatePercentages(root, true);
        root.Layout.GrowChildren(root, true);
        root.Layout.CalculateFitSize(root, false);
        root.Layout.CalculatePercentages(root, false);
        root.Layout.GrowChildren(root, false);
        root.Layout.CalculatePositions(root, Vector2.Zero);
        root.PropagateRender(renderPass, new Vector2(texture.Width, texture.Height), scale);
            
        renderPass.End();
    }
}