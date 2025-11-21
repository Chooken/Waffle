using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public static partial class Ui
{
    public static void RenderToTexture(UiElement root, ImQueue queue, Color clearColor, in GpuTexture texture)
    {
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = clearColor,
            GpuTexture = texture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);
        
        root.Layout.CalculateFitSize(root, true);
        root.Layout.CalculatePercentages(root, true);
        root.Layout.GrowChildren(root, true);
        root.Layout.ApplyContraints(root, false);
        root.Layout.CalculateFitSize(root, false);
        root.Layout.CalculatePercentages(root, false);
        root.Layout.GrowChildren(root, false);
        root.Layout.ApplyContraints(root, true);
        root.Layout.CalculatePositions(root, Vector2.Zero);
        root.Layout.CalculatePercentages(root, true);
        root.Layout.CalculatePercentages(root, false);
        root.PropagateRender(renderPass, new Vector2(root.Bounds.CalculatedWidth, root.Bounds.CalculatedHeight));
            
        renderPass.End();
    }
}