using WaffleEngine;
using WaffleEngine.Serializer;

namespace Editor;

[SerializeDefault]
public struct InitAndDisposeTest : ISceneInit, ISceneUpdate, IDisposable
{
    public void OnInit()
    {
        Log.Info("Init Called", "InitAndDisposeTest");
    }

    public void Update()
    {
        Log.Info("Update Called", "InitAndDisposeTest");
    }

    public void Dispose()
    {
        Log.Info("Dispose Called", "InitAndDisposeTest");
    }
}