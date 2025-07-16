using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Editor;
using Editor.Shaders;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Serializer;

Device.Init();
ShaderCompiler.Init();
ShaderCompiler.CompileAllShaders();

if (!WindowManager.TryOpenWindow("Waffle Engine", "waffle-engine-main", 800, 600, out Window? window))
    return;

Scene scene = new Scene();
scene.InstantiateEntity();
scene.InstantiateEntity();
scene.InstantiateEntity().AddComponent(new Camera((WindowSdl)window, 8, 0.1f, 100f));
scene.InstantiateEntity().AddComponent(new InitAndDisposeTest());
scene.AddQuery(new TestQuery());
scene.AddQuery(new RenderQuery());

SceneManager.AddScene(scene, "initial");

SceneSerializer.TrySerialize("scenes/scene", scene);
SceneSerializer.TryDeserialize("scenes/scene", out var scene2);

ShaderSerializer.TryDeserialize("compiled-shaders/BuiltinShaders-triangle", out var shader);
Application.Run();