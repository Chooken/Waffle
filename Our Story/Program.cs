// See https://aka.ms/new-console-template for more information

using OurStory;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Text;
using WaffleEngine.UI;

Device.Init();
FontLoader.Init();

if (!WindowManager.TryOpenWindow("Waffle Engine", "waffle-engine-main", 800, 600, out Window? window))
    return;

FontLoader.LoadFont("Fonts/Nunito-Regular.ttf", 26);

UIToplevel toplevel = new UIToplevel((WindowSdl)window);
toplevel.Root = new UIRect()
    .SetWidth(UISize.Percentage(100))
    .SetHeight(UISize.Percentage(100))
    .SetChildAnchor(UIAnchor.Center)
    .AddUIElement(new UIAspectRatio()
        .SetPixelMultiple(new Vector2(256, 256))
        .SetAspectRatio(new Vector2(1, 1))
        .SetWidth(UISize.Percentage(100))
        .SetHeight(UISize.Percentage(100))
        .SetBorderRadius(new Vector4(24, 24, 24, 24), UISizeType.Pixels)
        .SetColor(Color.RGBA255(255, 0, 255, 255))
        .AddUIElement(new UIText()
            .SetText("Hello World."))
    );

Scene start = new Scene();
start.InstantiateEntity().AddComponent(new Camera((WindowSdl)window, 8, 0.1f, 100f));
start.InstantiateEntity().AddComponent(toplevel);

start.AddQuery(new RenderUI());

SceneManager.AddScene(start, "initial");
    
Application.Run();