using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class UndoButton : Rect
{
    public UndoButton()
    {
        Default((ref RectSettings settings) => settings = settings with
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
            .Default((ref RectSettings settings) => settings = settings with
            {
                Width = Ui.Pixels(16),
                Height = Ui.Pixels(16),
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
        Default((ref RectSettings settings) => settings = settings with
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
            .Default((ref RectSettings settings) => settings = settings with
            {
                Width = Ui.Pixels(16),
                Height = Ui.Pixels(16),
                Color = TextureEditor.FontColor,
                BorderRadius = (2, 8, 2, 8),
            })
        );
    }
}