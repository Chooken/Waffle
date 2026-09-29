namespace Vector.Editor;

// Multi-point selection. Points[0] is the primary (e.g. Delete acts on all,
// Pen/drag gestures anchor on the primary). Shape reorder never invalidates
// entries (shape ids are stable); structural point edits clear them instead.
public struct PointSelection
{
    public int ShapeId;
    public int PointIndex;
    public HandleKind Handle;

    public static PointSelection None => new() { ShapeId = -1, PointIndex = -1, Handle = HandleKind.Anchor };
    public bool HasValue => ShapeId >= 0 && PointIndex >= 0;
}

public sealed class EditorSelection
{
    public int ActiveShapeId = -1;
    public int ActiveBoneId = -1;
    private readonly List<PointSelection> _points = new();

    public IReadOnlyList<PointSelection> Points => _points;
    public PointSelection ActivePoint => _points.Count > 0 ? _points[0] : PointSelection.None;

    public void SelectShape(int shapeId)
    {
        ActiveShapeId = shapeId;
        _points.Clear();
    }

    public void SelectPoint(int shapeId, int pointIndex, HandleKind handle = HandleKind.Anchor)
    {
        ActiveShapeId = shapeId;
        _points.Clear();
        _points.Add(new PointSelection { ShapeId = shapeId, PointIndex = pointIndex, Handle = handle });
    }

    public void AddPoint(int shapeId, int pointIndex, HandleKind handle)
    {
        if (!IsPointActive(shapeId, pointIndex))
        {
            ActiveShapeId = shapeId;
            _points.Add(new PointSelection { ShapeId = shapeId, PointIndex = pointIndex, Handle = handle });
        }
    }

    // Returns false when the toggle removed the point.
    public bool TogglePoint(int shapeId, int pointIndex, HandleKind handle)
    {
        for (int i = 0; i < _points.Count; i++)
        {
            if (_points[i].ShapeId == shapeId && _points[i].PointIndex == pointIndex)
            {
                _points.RemoveAt(i);
                if (_points.Count == 0)
                    ActiveShapeId = -1;
                return false;
            }
        }
        ActiveShapeId = shapeId;
        _points.Add(new PointSelection { ShapeId = shapeId, PointIndex = pointIndex, Handle = handle });
        return true;
    }

    public void ClearPoints() => _points.Clear();

    public void SelectBone(int boneId)
    {
        ActiveBoneId = boneId;
    }

    public void ClearBone()
    {
        ActiveBoneId = -1;
    }

    public void Clear()
    {
        ActiveShapeId = -1;
        ActiveBoneId = -1;
        _points.Clear();
    }

    public bool IsShapeActive(int shapeId) => ActiveShapeId == shapeId;
    public bool IsBoneActive(int boneId) => ActiveBoneId == boneId;

    public bool IsPointActive(int shapeId, int pointIndex)
    {
        foreach (var p in _points)
            if (p.ShapeId == shapeId && p.PointIndex == pointIndex)
                return true;
        return false;
    }
}
