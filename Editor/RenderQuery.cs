using Arch.Core;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public class RenderQuery : Query<Transform, Camera>
{
    private Renderer? _renderer;
    
    public override void Run(ref Transform transform, ref Camera camera)
    {
        if (_renderer is null)
            _renderer = new Renderer(camera.Window);
        
        _renderer.Render();
    }
}