using WaffleEngine;
using WaffleEngine.Serializer;

namespace Editor;

[SerializeDefault]
public struct InitAndDisposeTest : ISceneInit, IDisposable
{
    public void OnInit()
    {
        Log.Info("Init Called", "InitAndDisposeTest");
    }

    public void Dispose()
    {
        Log.Info("Dispose Called", "InitAndDisposeTest");
    }
}