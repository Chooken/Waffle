using Vector.Editor.Tools;
using WaffleEngine;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Top strip: tool switch + shape/history/file actions. Children are Buttons
// built in OnInit (Tree is valid there) and laid out in OnUpdate.
public class Toolbar : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    private readonly List<Button> _buttons = new();
    private Button _selectBtn = null!;
    private Button _penBtn = null!;
    private Button _closeBtn = null!;
    private Button _delBtn = null!;
    private Button _undoBtn = null!;
    private Button _redoBtn = null!;
    private Button _snapBtn = null!;
    private Button _saveBtn = null!;

    public override void OnInit()
    {
        Color = Theme.BarBackground;
        SetClipped(true);

        _selectBtn = Add("Select", () => Editor.ActiveToolMode = ToolMode.Select);
        _penBtn = Add("Pen", () => Editor.ActiveToolMode = ToolMode.Pen);
        Add("New", () => Editor.NewShape(), newGroup: true);
        _closeBtn = Add("Close", () => Editor.CloseActiveShape());
        _delBtn = Add("Del", () => Editor.DeleteActiveShape());
        _undoBtn = Add("Undo", () => Editor.Undo(), newGroup: true);
        _redoBtn = Add("Redo", () => Editor.Redo());
        _snapBtn = Add("Snap", () => Editor.SnapEnabled = !Editor.SnapEnabled);
        _saveBtn = Add("Save", () => Editor.Save());
    }

    private Button Add(string label, Action onClick, bool newGroup = false)
    {
        var btn = new Button { LabelText = label, OnClick = onClick, NewGroup = newGroup };
        AddNode(btn);
        _buttons.Add(btn);
        return btn;
    }

    public override void OnUpdate()
    {
        var active = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
        bool hasOpenShape = active is not null && !active.Closed;
        _selectBtn.Selected = Editor.ActiveToolMode == ToolMode.Select;
        _penBtn.Selected = Editor.ActiveToolMode == ToolMode.Pen;
        _closeBtn.Selected = hasOpenShape;
        _closeBtn.Dimmed = !hasOpenShape;
        _delBtn.Dimmed = active is null;
        _undoBtn.Dimmed = !Editor.History.CanUndo;
        _redoBtn.Dimmed = !Editor.History.CanRedo;
        _snapBtn.Selected = Editor.SnapEnabled;
        _saveBtn.Selected = Editor.History.IsDirty;

        float x = Rect.x + Theme.PanelPad;
        float y = Rect.y + (Rect.h - Theme.ButtonHeight) / 2;
        foreach (var btn in _buttons)
        {
            if (btn.NewGroup)
                x += Theme.GroupGap;
            float w = Math.Min(btn.PreferredWidth(), Theme.ButtonMaxWidth);
            btn.SetRect(new IRect { x = (int)x, y = (int)y, w = (int)w, h = Theme.ButtonHeight });
            x += w + Theme.ButtonGap;
        }
    }
}
