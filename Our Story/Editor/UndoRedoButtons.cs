using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class UndoButton : Rect
{
    public UndoButton()
    {
        Default(() => new RectSettings
        {
            Padding = (8, 8),
            BorderRadius = 8,
            Alignment = new UiAlignment
            {
                Vertical = UiAlignmentVertical.Center,
                Horizontal = UiAlignmentHorizontal.Center,
            },
        });
            
        OnHover((ref RectSettings settings) =>
        {
            settings.Cursor = Cursor.Pointer;
            settings.Color = TextureEditor.ElementHighlight;
        });
        
        OnMouseDown((ref RectSettings settings) =>
        {
            TextureEditor.CommandList.Undo();
        });
        
        OnHold((ref RectSettings settings) =>
        {
            settings.Color = TextureEditor.ElementPressColor;
        });
        
        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Fixed(16),
                Height = Ui.Fixed(16),
                Color = TextureEditor.FontColor,
                BorderRadius = (8, 2, 8, 2),
            })
        );
    }
}

public class RedoButton : Rect
{
    public RedoButton()
    {
        Default(() => new RectSettings
        {
            Padding = (8, 8),
            BorderRadius = 8,
            Alignment = new UiAlignment
            {
                Vertical = UiAlignmentVertical.Center,
                Horizontal = UiAlignmentHorizontal.Center,
            },
        });
            
        OnHover((ref RectSettings settings) =>
        {
            settings.Cursor = Cursor.Pointer;
            settings.Color = TextureEditor.ElementHighlight;
        });
        
        OnMouseDown((ref RectSettings settings) =>
        {
            TextureEditor.CommandList.Redo();
        });
        
        OnHold((ref RectSettings settings) =>
        {
            settings.Color = TextureEditor.ElementPressColor;
        });
        
        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Fixed(16),
                Height = Ui.Fixed(16),
                Color = TextureEditor.FontColor,
                BorderRadius = (2, 8, 2, 8),
            })
        );
    }
}