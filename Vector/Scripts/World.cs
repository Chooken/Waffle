using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace Vector.Scenes;

public class World
{
    public List<Character> Characters;
    public Camera Camera = new Camera(8, 0, 100);
    
    public bool GroundUploaded = false;
    public Texture white = new Texture(2, 2);
    public Mesh Ground = Mesh.Quad(new Vector3(-100, -100, 1), new Vector3(100, 0, 1), new (0,0,0,1));
    
    public void Update(ImQueue queue)
    {
        if (!GroundUploaded)
        {
            Span<byte> colors = white.ByteArray();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = 255;
            }
            
            var copypass = queue.AddCopyPass();
            copypass.Upload(white);
            copypass.Upload(Ground);
            copypass.End();
        }
    }
    
    public void Render(ImQueue queue, GpuTexture target)
    {
        if (!Assets.TryGetShader("builtin", "mesh", out var shader))
        {
            return;
        }

        var matrix = Camera.GetProjectionMatrix(target.Width, target.Height);
        
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(1,1,1,1),
            GpuTexture = target,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);
        
        renderPass.Bind(shader);
        
        renderPass.Bind(Ground);
        
        renderPass.Bind(white);
        
        renderPass.SetUniforms(new WaffleUniforms
        {
            MVP = matrix,
        });
        
        renderPass.DrawIndexedPrimatives((uint)Ground.Indices.Count, 1, 0, 0, 0);
        
        renderPass.End();
    }
}