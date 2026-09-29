using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine;
using WaffleEngine.Vector;

namespace Vector.Editor;

// The document stores Curves directly (anchors at even indices, controls at
// odd indices) and edits them in place — no per-edit rebuild. New segments
// are linear (midpoint controls); smoothing is never assumed.
public enum HandleKind
{
    Anchor,
    Control,
    // Reserved for Phase 2 explicit cubic handles. Kept here so Selection stays stable.
    In,
    Out,
}

public sealed class Shape
{
    public int Id;
    public string Name = "Shape";
    public Color Color = Color.Green;
    public bool Closed = true;
    public Curve Curve = new(new Point(Vector2.Zero));

    public int AnchorCount => Closed ? Curve.Points.Count / 2 : (Curve.Points.Count + 1) / 2;

    public Shape Clone()
    {
        return new Shape
        {
            Id = Id,
            Name = Name,
            Color = Color,
            Closed = Closed,
            Curve = Curve.Clone(),
        };
    }

    // Remove the anchor at pointIndex (must be even). The anchor and one
    // adjacent control are deleted; the remaining control is repositioned to
    // the midpoint of the two surrounding anchors, keeping the segment
    // linear. A trailing anchor drops it plus its control; a leading anchor
    // drops it plus the following control (and a closed loop re-anchors).
    // Returns false when there is nothing valid to remove.
    public bool RemoveAnchor(int pointIndex)
    {
        var pts = Curve.Points;
        if (pointIndex < 0 || pointIndex >= pts.Count || pointIndex % 2 != 0)
            return false;

        if (Closed)
        {
            if (pts.Count / 2 <= 2)
                return false;
            if (pointIndex == 0)
            {
                pts.RemoveRange(0, 2);
                pts[^1] = Midpoint(pts[^2].Position, pts[0].Position);
            }
            else if (pointIndex == pts.Count - 2)
            {
                Vector2 prev = pts[pointIndex - 2].Position;
                Vector2 first = pts[0].Position;
                pts.RemoveAt(pointIndex);
                pts.RemoveAt(pointIndex - 1);
                pts[pointIndex - 1] = Midpoint(prev, first);
            }
            else
            {
                Vector2 prev = pts[pointIndex - 2].Position;
                Vector2 next = pts[pointIndex + 2].Position;
                pts.RemoveAt(pointIndex);
                pts.RemoveAt(pointIndex - 1);
                pts[pointIndex - 1] = Midpoint(prev, next);
            }
        }
        else
        {
            if (pts.Count <= 1)
                return false;
            if (pointIndex == 0 || pointIndex == pts.Count - 1)
            {
                int at = pointIndex == 0 ? 0 : pointIndex - 1;
                pts.RemoveRange(at, 2);
            }
            else
            {
                Vector2 prev = pts[pointIndex - 2].Position;
                Vector2 next = pts[pointIndex + 2].Position;
                pts.RemoveAt(pointIndex);
                pts.RemoveAt(pointIndex - 1);
                pts[pointIndex - 1] = Midpoint(prev, next);
            }
        }

        return true;
    }

    private static Point Midpoint(Vector2 a, Vector2 b) =>
        new(new Vector2((a.x + b.x) / 2f, (a.y + b.y) / 2f));

    // Hit test for whole-curve dragging: closed shapes match their filled
    // area (even-odd over flattened beziers), open shapes match a stroke
    // within tolerance of the flattened path.
    public bool HitTest(Vector2 world, float strokeTol)
    {
        var pts = Curve.Points;
        if (pts.Count == 0)
            return false;
        if (pts.Count == 1)
            return (pts[0].Position - world).Length() < strokeTol;

        const int subdiv = 10;
        int edges = Closed ? pts.Count / 2 : (pts.Count - 1) / 2;
        if (edges <= 0)
        {
            float nearest = strokeTol;
            foreach (var pt in pts)
            {
                float dd = (pt.Position - world).Length();
                if (dd < nearest)
                    nearest = dd;
            }
            return nearest < strokeTol;
        }

        if (Closed)
        {
            // Even-odd containment over the flattened loop.
            bool inside = false;
            Vector2 prev = pts[0].Position;
            for (int e = 0; e < edges; e++)
            {
                for (int s = 1; s <= subdiv; s++)
                {
                    Vector2 cur = SampleEdge(pts, e, (float)s / subdiv);
                    if (RayCrosses(prev, cur, world))
                        inside = !inside;
                    prev = cur;
                }
            }
            if (inside)
                return true;
        }

        float best = strokeTol;
        Vector2 p = pts[0].Position;
        for (int e = 0; e < edges; e++)
        {
            for (int s = 1; s <= subdiv; s++)
            {
                Vector2 c = SampleEdge(pts, e, (float)s / subdiv);
                float d = SegmentDistance(p, c, world);
                if (d < best)
                    best = d;
                p = c;
            }
        }
        return best < strokeTol;
    }

    // Quadratic edge e of the anchor/control chain. The final edge of a
    // closed loop wraps (last anchor, closing control, first anchor).
    private Vector2 SampleEdge(List<Point> pts, int edge, float t)
    {
        int a, b, c;
        if (Closed && edge == pts.Count / 2 - 1)
        {
            a = pts.Count - 2;
            b = pts.Count - 1;
            c = 0;
        }
        else
        {
            a = edge * 2;
            b = a + 1;
            c = a + 2;
        }
        Vector2 p0 = pts[a].Position;
        Vector2 p1 = pts[b].Position;
        Vector2 p2 = pts[c].Position;
        float u = 1f - t;
        return new Vector2(
            u * u * p0.x + 2f * u * t * p1.x + t * t * p2.x,
            u * u * p0.y + 2f * u * t * p1.y + t * t * p2.y);
    }

    private static bool RayCrosses(Vector2 a, Vector2 b, Vector2 p)
    {
        bool straddles = (a.y > p.y) != (b.y > p.y);
        if (!straddles)
            return false;
        float x = a.x + (p.y - a.y) / (b.y - a.y) * (b.x - a.x);
        return x > p.x;
    }

    private static float SegmentDistance(Vector2 a, Vector2 b, Vector2 p)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-12f)
            return (p - a).Length();
        float t = Math.Clamp(((p.x - a.x) * dx + (p.y - a.y) * dy) / lenSq, 0f, 1f);
        float ex = a.x + dx * t - p.x;
        float ey = a.y + dy * t - p.y;
        return MathF.Sqrt(ex * ex + ey * ey);
    }
}

// Future-proofing hook for bones/animation (interfaces only in Phase 1).
// The document owns the bind pose; a future AnimationSystem will implement
// IVertexDeformer and pass it to the renderer without changing the file format
// beyond the already-reserved `bones` node.
public sealed class Bone
{
    public int Id;
    public string Name = "Bone";
    public int ParentId = -1;
    public Vector2 BindPosition;
}

public interface IVertexDeformer
{
    Vector2 Deform(Vector2 basePos, int shapeId, int pointId);
}

public sealed class NullDeformer : IVertexDeformer
{
    public static readonly NullDeformer Instance = new();
    public Vector2 Deform(Vector2 basePos, int shapeId, int pointId) => basePos;
}

public sealed class VectorAssetDocument : ISerializable, IDeserializable<VectorAssetDocument>
{
    public const int CurrentVersion = 2;

    public IVector2 CanvasSize = new(32, 32);
    public List<Shape> Shapes = new();
    public List<Bone> Bones = new(); // empty in Phase 1, reserved for skeleton
    public int NextShapeId;
    public int NextBoneId;

    public static VectorAssetDocument CreateDefault()
    {
        var doc = new VectorAssetDocument();
        var shape = new Shape { Id = doc.NextShapeId++, Name = "Body", Closed = true };
        shape.Curve = new Curve(new Point(new Vector2(-0.4f, -0.5f)));
        shape.Curve.AddLinear(new Point(new Vector2(-0.50f, -0.25f)));
        shape.Curve.AddLinear(new Point(new Vector2(0.25f, 0.25f)));
        shape.Curve.Close();
        doc.Shapes.Add(shape);
        return doc;
    }

    public VectorAssetDocument Clone()
    {
        var copy = new VectorAssetDocument
        {
            CanvasSize = CanvasSize,
            NextShapeId = NextShapeId,
            NextBoneId = NextBoneId,
        };
        foreach (var s in Shapes)
            copy.Shapes.Add(s.Clone());
        foreach (var b in Bones)
            copy.Bones.Add(new Bone { Id = b.Id, Name = b.Name, ParentId = b.ParentId, BindPosition = b.BindPosition });
        return copy;
    }

    public Shape? FindShape(int shapeId)
    {
        foreach (var s in Shapes)
            if (s.Id == shapeId)
                return s;
        return null;
    }

    // ---- YAML persistence (VYaml low-level, forward-compatible: unknown keys skipped) ----

    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        emitter.WriteString("version");
        emitter.WriteInt32(CurrentVersion);

        emitter.WriteString("canvas");
        emitter.BeginSequence(SequenceStyle.Flow);
        emitter.WriteInt32(CanvasSize.x);
        emitter.WriteInt32(CanvasSize.y);
        emitter.EndSequence();

        emitter.WriteString("nextIds");
        emitter.BeginSequence(SequenceStyle.Flow);
        emitter.WriteInt32(NextShapeId);
        emitter.WriteInt32(NextBoneId);
        emitter.EndSequence();

        emitter.WriteString("shapes");
        emitter.BeginSequence();
        foreach (var s in Shapes)
        {
            emitter.BeginMapping();
            emitter.WriteString("id");
            emitter.WriteInt32(s.Id);
            emitter.WriteString("name");
            emitter.WriteString(s.Name);
            emitter.WriteString("color");
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(s.Color.r);
            emitter.WriteFloat(s.Color.g);
            emitter.WriteFloat(s.Color.b);
            emitter.WriteFloat(s.Color.a);
            emitter.EndSequence();
            emitter.WriteString("closed");
            emitter.WriteBool(s.Closed);
            // Raw curve points, interleaved anchor/control (even = anchor).
            emitter.WriteString("points");
            emitter.BeginSequence(SequenceStyle.Flow);
            foreach (var p in s.Curve.Points)
            {
                emitter.WriteFloat(p.Position.x);
                emitter.WriteFloat(p.Position.y);
            }
            emitter.EndSequence();
            emitter.EndMapping();
        }
        emitter.EndSequence();

        emitter.WriteString("bones");
        emitter.BeginSequence();
        foreach (var b in Bones)
        {
            emitter.BeginMapping();
            emitter.WriteString("id");
            emitter.WriteInt32(b.Id);
            emitter.WriteString("name");
            emitter.WriteString(b.Name);
            emitter.WriteString("parent");
            emitter.WriteInt32(b.ParentId);
            emitter.WriteString("pos");
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(b.BindPosition.x);
            emitter.WriteFloat(b.BindPosition.y);
            emitter.EndSequence();
            emitter.EndMapping();
        }
        emitter.EndSequence();

        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out VectorAssetDocument doc)
    {
        doc = new VectorAssetDocument();
        try
        {
            parser.SkipAfter(ParseEventType.DocumentStart);
            if (parser.CurrentEventType != ParseEventType.MappingStart)
                return false;
            parser.Read(); // enter root mapping

            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                if (parser.CurrentEventType != ParseEventType.Scalar)
                {
                    parser.SkipCurrentNode();
                    continue;
                }

                string rootKey = parser.ReadScalarAsString() ?? string.Empty;
                switch (rootKey)
                {
                    case "canvas":
                        doc.CanvasSize = ReadIVector2(ref parser, doc.CanvasSize);
                        break;
                    case "nextIds":
                        var ids = ReadIntList(ref parser);
                        if (ids.Count >= 3)
                        {
                            // v1 layout: [shape, point, bone]
                            doc.NextShapeId = ids[0];
                            doc.NextBoneId = ids[2];
                        }
                        else if (ids.Count == 2)
                        {
                            doc.NextShapeId = ids[0];
                            doc.NextBoneId = ids[1];
                        }
                        else if (ids.Count == 1)
                        {
                            doc.NextShapeId = ids[0];
                        }
                        break;
                    case "shapes":
                        doc.Shapes = ReadShapes(ref parser);
                        break;
                    case "bones":
                        doc.Bones = ReadBones(ref parser);
                        break;
                    default:
                        // version + any future animation keys: skip for forward compat
                        parser.SkipCurrentNode();
                        break;
                }
            }

            doc.EnsureIds();
            return true;
        }
        catch
        {
            doc = new VectorAssetDocument();
            return false;
        }
    }

    private void EnsureIds()
    {
        foreach (var s in Shapes)
        {
            if (s.Id >= NextShapeId) NextShapeId = s.Id + 1;
        }
        foreach (var b in Bones)
            if (b.Id >= NextBoneId) NextBoneId = b.Id + 1;
    }

    private static IVector2 ReadIVector2(ref YamlParser parser, IVector2 fallback)
    {
        var list = ReadIntList(ref parser);
        return list.Count >= 2 ? new IVector2(list[0], list[1]) : fallback;
    }

    private static List<int> ReadIntList(ref YamlParser parser)
    {
        var result = new List<int>();
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            parser.SkipCurrentNode();
            return result;
        }
        parser.Read(); // enter sequence
        while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
        {
            if (parser.CurrentEventType == ParseEventType.Scalar)
            {
                if (parser.TryGetScalarAsInt32(out int v))
                    result.Add(v);
                parser.Read();
            }
            else
            {
                parser.SkipCurrentNode();
            }
        }
        parser.Read(); // skip SequenceEnd
        return result;
    }

    private static List<float> ReadFloatList(ref YamlParser parser)
    {
        var result = new List<float>();
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            parser.SkipCurrentNode();
            return result;
        }
        parser.Read();
        while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
        {
            if (parser.CurrentEventType == ParseEventType.Scalar)
            {
                if (parser.TryGetScalarAsFloat(out float v))
                    result.Add(v);
                parser.Read();
            }
            else
            {
                parser.SkipCurrentNode();
            }
        }
        parser.Read();
        return result;
    }

    private static List<Shape> ReadShapes(ref YamlParser parser)
    {
        var shapes = new List<Shape>();
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            parser.SkipCurrentNode();
            return shapes;
        }
        parser.Read();
        while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
        {
            if (parser.CurrentEventType != ParseEventType.MappingStart)
            {
                parser.SkipCurrentNode();
                continue;
            }
            shapes.Add(ReadShape(ref parser));
        }
        parser.Read();
        return shapes;
    }

    private static Shape ReadShape(ref YamlParser parser)
    {
        var shape = new Shape();
        bool legacyColors = false;
        bool hasColor = false;
        parser.Read(); // enter shape mapping
            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                if (parser.CurrentEventType != ParseEventType.Scalar)
                {
                    parser.SkipCurrentNode();
                    continue;
                }
                string akey = parser.ReadScalarAsString() ?? string.Empty;
                switch (akey)
            {
                case "id":
                    shape.Id = ReadIntScalar(ref parser);
                    break;
                case "name":
                    shape.Name = ReadStringScalar(ref parser, "Shape");
                    break;
                case "color":
                    hasColor = true;
                    var c = ReadFloatList(ref parser);
                    if (c.Count >= 4) shape.Color = new Color(c[0], c[1], c[2], c[3]);
                    else if (c.Count >= 3) shape.Color = new Color(c[0], c[1], c[2], 1f);
                    break;
                case "closed":
                    shape.Closed = ReadBoolScalar(ref parser, true);
                    break;
                case "points":
                    ReadRawPoints(ref parser, shape);
                    break;
                case "anchors":
                    // v1 files: migrate once via the global smoother. Their
                    // colors were stored linear; encode to display gamma so
                    // old assets render exactly as before.
                    legacyColors = true;
                    var legacy = ReadLegacyAnchors(ref parser);
                    if (legacy.Count > 0)
                    {
                        var positions = new Vector2[legacy.Count];
                        var smooth = new bool[legacy.Count];
                        for (int i = 0; i < legacy.Count; i++)
                        {
                            positions[i] = legacy[i].Position;
                            smooth[i] = legacy[i].Smooth;
                        }
                        shape.Curve = Curve.FromAnchors(positions, smooth, shape.Closed);
                    }
                    break;
                default:
                    parser.SkipCurrentNode();
                    break;
            }
        }
        if (legacyColors && hasColor)
            shape.Color = shape.Color.ToGamma();
        parser.Read(); // skip MappingEnd
        return shape;
    }

    // v2: flat [x, y, x, y, ...] interleaved anchor/control points.
    private static void ReadRawPoints(ref YamlParser parser, Shape shape)
    {
        var floats = ReadFloatList(ref parser);
        if (floats.Count < 2)
            return;
        var curve = new Curve(new Point(new Vector2(floats[0], floats[1])));
        for (int i = 2; i + 1 < floats.Count; i += 2)
            curve.Points.Add(new Point(new Vector2(floats[i], floats[i + 1])));
        shape.Curve = curve;
    }

    private struct LegacyAnchor
    {
        public Vector2 Position;
        public bool Smooth;
    }

    private static List<LegacyAnchor> ReadLegacyAnchors(ref YamlParser parser)
    {
        var anchors = new List<LegacyAnchor>();
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            parser.SkipCurrentNode();
            return anchors;
        }
        parser.Read();
        while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
        {
            if (parser.CurrentEventType != ParseEventType.MappingStart)
            {
                parser.SkipCurrentNode();
                continue;
            }
            parser.Read();
            var anchor = new LegacyAnchor { Smooth = true };
            Vector2 pos = Vector2.Zero;
            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                if (parser.CurrentEventType != ParseEventType.Scalar)
                {
                    parser.SkipCurrentNode();
                    continue;
                }
                string anchorKey = parser.ReadScalarAsString() ?? string.Empty;
                switch (anchorKey)
                {
                    case "id": ReadIntScalar(ref parser); break;
                    case "pos":
                        var f = ReadFloatList(ref parser);
                        if (f.Count >= 2) pos = new Vector2(f[0], f[1]);
                        break;
                    case "smooth": anchor.Smooth = ReadBoolScalar(ref parser, true); break;
                    default: parser.SkipCurrentNode(); break;
                }
            }
            anchor.Position = pos;
            anchors.Add(anchor);
            parser.Read(); // skip MappingEnd
        }
        parser.Read(); // skip SequenceEnd
        return anchors;
    }

    private static List<Bone> ReadBones(ref YamlParser parser)
    {
        var bones = new List<Bone>();
        if (parser.CurrentEventType != ParseEventType.SequenceStart)
        {
            parser.SkipCurrentNode();
            return bones;
        }
        parser.Read();
        while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
        {
            if (parser.CurrentEventType != ParseEventType.MappingStart)
            {
                parser.SkipCurrentNode();
                continue;
            }
            parser.Read();
            var bone = new Bone();
            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                if (parser.CurrentEventType != ParseEventType.Scalar)
                {
                    parser.SkipCurrentNode();
                    continue;
                }
                string bkey = parser.ReadScalarAsString() ?? string.Empty;
                switch (bkey)
                {
                    case "id": bone.Id = ReadIntScalar(ref parser); break;
                    case "name": bone.Name = ReadStringScalar(ref parser, "Bone"); break;
                    case "parent": bone.ParentId = ReadIntScalar(ref parser); break;
                    case "pos":
                        var f = ReadFloatList(ref parser);
                        if (f.Count >= 2) bone.BindPosition = new Vector2(f[0], f[1]);
                        break;
                    default: parser.SkipCurrentNode(); break;
                }
            }
            bones.Add(bone);
            parser.Read();
        }
        parser.Read();
        return bones;
    }

    private static int ReadIntScalar(ref YamlParser parser)
    {
        if (parser.CurrentEventType != ParseEventType.Scalar)
        {
            parser.SkipCurrentNode();
            return 0;
        }
        int v = parser.GetScalarAsInt32();
        parser.Read();
        return v;
    }

    private static string ReadStringScalar(ref YamlParser parser, string fallback)
    {
        if (parser.CurrentEventType != ParseEventType.Scalar)
        {
            parser.SkipCurrentNode();
            return fallback;
        }
        string? s = parser.GetScalarAsString();
        parser.Read();
        return s ?? fallback;
    }

    private static bool ReadBoolScalar(ref YamlParser parser, bool fallback)
    {
        if (parser.CurrentEventType != ParseEventType.Scalar)
        {
            parser.SkipCurrentNode();
            return fallback;
        }
        if (parser.TryGetScalarAsBool(out bool v))
        {
            parser.Read();
            return v;
        }
        parser.Read();
        return fallback;
    }
}
