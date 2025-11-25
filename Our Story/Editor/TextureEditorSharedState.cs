using WaffleEngine;

namespace OurStory.Editor;

public class TextureEditorSharedState
{
    public HSVColor SelectedColor;
    public ICanvasTool? SelectedTool;
    public bool UseMinMax;
    public Dictionary<Type, ICanvasTool> Tools = new ();

    public void SelectColor(HSVColor color)
    {
        SelectedColor = color;
    }
}