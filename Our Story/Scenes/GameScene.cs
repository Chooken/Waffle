using System.Numerics;
using OurStory.Editor;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;
using WaffleEngine.UI;
using Range = WaffleEngine.UI.Range;
using Vector2 = WaffleEngine.Vector2;
using Vector3 = WaffleEngine.Vector3;

namespace OurStory.Scenes;

public class GameScene : IScene
{
    private TextureEditor editor;
    
    public bool OnSceneLoaded()
    {
        if (!Assets.TryLoadAssetBundle("Core"))
            return false;

        editor = new TextureEditor();
        editor.Start();

        return true;
    }

    public void OnSceneUpdate()
    {
        Update();
        Render();
    }

    private void Update()
    {
        editor.Update();
    }

    private void Render()
    {
        editor.Render();
    }

    public void OnSceneExit()
    {
        
    }
}