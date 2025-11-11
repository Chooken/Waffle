using Editor;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Serializer;

Device.Init();

if (!WindowManager.TryOpenWindow("Waffle Engine", "waffle-engine-main", 800, 600, out Window? window))
    return;

Scene scene = new Scene();
scene.InstantiateEntity();
scene.InstantiateEntity();
scene.InstantiateEntity().AddComponent(new Camera((WindowSdl)window, 8, 0.1f, 100f));
scene.InstantiateEntity().AddComponent(new InitAndDisposeTest());

Yaml.TrySerialize("scenes/scene.scene", scene);

if (Yaml.TryDeserialize("scenes/scene", out Scene? scene2))
{
    Log.Error("Failed to deserialize scene.");
    return;
}

Application.Run(scene);