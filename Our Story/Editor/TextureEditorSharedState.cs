using WaffleEngine;

namespace OurStory.Editor;

public class TextureEditorSharedState
{
    public Color SelectedColor;
    public float ColorBrightness;
    public ICanvasTool? SelectedTool;
    public Dictionary<Type, ICanvasTool> Tools = new ();
}