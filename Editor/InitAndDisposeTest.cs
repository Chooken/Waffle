using WaffleEngine;
using WaffleEngine.Serializer;

namespace Editor;

[SerializeDefault]
public struct InitAndDisposeTest
{
    private int i;
    
    public void OnInit()
    {
        i = 10;
        Log.Info("Init Called");
    }

    public void Update()
    {
        i++;
        Log.Info($"Update Called {i}");
    }

    public void Dispose()
    {
        Log.Info("Dispose Called");
    }
}