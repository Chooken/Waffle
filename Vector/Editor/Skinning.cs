using WaffleEngine;

namespace Vector.Editor;

// Rigid 2D pose math for the bone system. Pure functions (no document
// access) so rig behavior is headless-testable. Angles are degrees.
//
// Storage split: Bone carries hierarchical LOCAL pose (current) plus a
// snapshotted BIND world pose. Skinning maps bind->current per bound bone:
//   world = parentWorld o local        (posing, hierarchical)
//   p'    = currentWorld * bindWorld^-1 * bindPos   (linear blend, 1 bone)
public struct RigidPose
{
    public Vector2 Position;
    public float AngleDeg;

    public RigidPose(Vector2 position, float angleDeg)
    {
        Position = position;
        AngleDeg = angleDeg;
    }
}

public static class Skinning
{
    private const float Deg2Rad = MathF.PI / 180f;

    public static Vector2 Rotate(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Deg2Rad;
        float c = MathF.Cos(rad);
        float s = MathF.Sin(rad);
        return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
    }

    public static RigidPose Compose(RigidPose parent, RigidPose local) =>
        new(Rotate(local.Position, parent.AngleDeg) + parent.Position,
            parent.AngleDeg + local.AngleDeg);

    public static RigidPose Inverse(RigidPose world) =>
        new(Rotate(-world.Position, -world.AngleDeg), -world.AngleDeg);

    public static RigidPose RelativeTo(RigidPose world, RigidPose parentWorld) =>
        Compose(Inverse(parentWorld), world);

    // World poses for a bone list. Unknown parents degrade to roots.
    public static Dictionary<int, RigidPose> ComputeWorlds(IReadOnlyList<Bone> bones)
    {
        var byId = new Dictionary<int, Bone>();
        foreach (var bone in bones)
            byId[bone.Id] = bone;

        var worlds = new Dictionary<int, RigidPose>();
        foreach (var bone in bones)
            Resolve(bone, byId, worlds);
        return worlds;
    }

    private static RigidPose Resolve(
        Bone bone, Dictionary<int, Bone> byId, Dictionary<int, RigidPose> worlds)
    {
        if (worlds.TryGetValue(bone.Id, out var cached))
            return cached;

        var local = new RigidPose(bone.LocalPosition, bone.LocalAngle);
        RigidPose world = local;
        if (bone.ParentId >= 0
            && byId.TryGetValue(bone.ParentId, out var parent)
            && parent.Id != bone.Id)
        {
            world = Compose(Resolve(parent, byId, worlds), local);
        }
        worlds[bone.Id] = world;
        return world;
    }

    public static Vector2 DeformPoint(Vector2 bindPos, RigidPose bindWorld, RigidPose currentWorld)
    {
        Vector2 centered = bindPos - bindWorld.Position;
        Vector2 unposed = Rotate(centered, -bindWorld.AngleDeg);
        Vector2 reposed = Rotate(unposed, currentWorld.AngleDeg);
        return reposed + currentWorld.Position;
    }

    public static int NearestBone(Vector2 point, IReadOnlyList<Bone> bones,
        IReadOnlyDictionary<int, RigidPose> worlds)
    {
        int best = -1;
        float bestDistSq = float.MaxValue;
        foreach (var bone in bones)
        {
            if (!worlds.TryGetValue(bone.Id, out var world))
                continue;
            float dx = point.x - world.Position.x;
            float dy = point.y - world.Position.y;
            float d = dx * dx + dy * dy;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                best = bone.Id;
            }
        }
        return best;
    }
}
