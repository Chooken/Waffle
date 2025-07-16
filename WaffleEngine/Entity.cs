using System.Diagnostics.CodeAnalysis;

namespace WaffleEngine;

public struct Entity
{
    internal Scene ParentScene;
    internal Arch.Core.Entity ArchEntity;

    public void AddComponent<T>(T component) where T : struct
    {
        ParentScene.AddComponentToEntity(this, component);
    }
}