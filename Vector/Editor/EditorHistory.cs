namespace Vector.Editor;

// Snapshot-based undo/redo (Memento). Deliberate tradeoff for Phase 1:
// assets are tiny (dozens of points), so cloning the document per stroke is
// cheap, reviewable, and impossible to desync. Phase 2 can replace the inside
// with fine-grained commands without changing the public API.
public sealed class EditorHistory
{
    private readonly List<VectorAssetDocument> _undo = new();
    private readonly List<VectorAssetDocument> _redo = new();
    private const int Limit = 100;

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public bool IsDirty { get; private set; }

    public void Checkpoint(VectorAssetDocument doc)
    {
        _undo.Add(doc.Clone());
        if (_undo.Count > Limit)
            _undo.RemoveAt(0);
        _redo.Clear();
        IsDirty = true;
    }

    public bool TryUndo(VectorAssetDocument current, out VectorAssetDocument restored)
    {
        restored = current;
        if (_undo.Count == 0)
            return false;
        _redo.Add(current.Clone());
        restored = _undo[^1].Clone();
        _undo.RemoveAt(_undo.Count - 1);
        IsDirty = true;
        return true;
    }

    public bool TryRedo(VectorAssetDocument current, out VectorAssetDocument restored)
    {
        restored = current;
        if (_redo.Count == 0)
            return false;
        _undo.Add(current.Clone());
        restored = _redo[^1].Clone();
        _redo.RemoveAt(_redo.Count - 1);
        IsDirty = true;
        return true;
    }

    public void MarkSaved() => IsDirty = false;
    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
        IsDirty = false;
    }
}
