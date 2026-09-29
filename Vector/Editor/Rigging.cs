namespace Vector.Editor;

// Document-level rig operations shared by the bone tool and the toolbar.
// Each is a single undoable step.
public static class Rigging
{
    public static void DeleteBone(VectorAssetDocument doc, EditorHistory history,
        EditorSelection selection, int boneId)
    {
        var bone = doc.FindBone(boneId);
        if (bone is null)
            return;
        history.Checkpoint(doc);
        foreach (var child in doc.Bones)
        {
            if (child.ParentId == boneId)
                child.ParentId = bone.ParentId;
        }
        foreach (var shape in doc.Shapes)
        {
            for (int i = 0; i < shape.BoneBinding.Count && i < shape.Curve.Points.Count; i++)
            {
                if (shape.BoneBinding[i] == boneId)
                {
                    shape.BoneBinding[i] = -1;
                    shape.BindPose[i] = shape.Curve.Points[i].Position;
                }
            }
        }
        doc.Bones.Remove(bone);
        if (selection.ActiveBoneId == boneId)
            selection.ClearBone();
    }

    public static void AutoBindAll(VectorAssetDocument doc, EditorHistory history)
    {
        history.Checkpoint(doc);
        var worlds = Skinning.ComputeWorlds(doc.Bones);
        foreach (var shape in doc.Shapes)
            shape.AutoBind(doc.Bones, worlds);
    }
}
