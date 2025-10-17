namespace WaffleEngine;

public static class SceneManager
{
    private static Dictionary<string, IScene> _activeScenes = new Dictionary<string, IScene>();
    
    public static void UpdateScenes()
    {
        foreach (var scene in _activeScenes.Values)
        {
            scene.OnSceneUpdate();
        }
    }

    public static bool AddScene(IScene scene, string name)
    {
        if (!scene.OnSceneLoaded())
        {
            WLog.Warning($"Failed to load scene: {name}");
            return false;
        }
        
        _activeScenes.Add(name, scene);
        return true;
    }
    
    public static void RemoveScene(string name)
    {
        if (!_activeScenes.TryGetValue(name, out var scene))
            return;
        
        scene.OnSceneExit();
        
        _activeScenes.Remove(name);
    }

    public static bool SetScene(IScene newScene, string name)
    {
        if (!newScene.OnSceneLoaded())
        {
            WLog.Warning($"Failed to load scene: {name}");
            return false;
        }
        
        foreach (var scene in _activeScenes.Values)
        {
            scene.OnSceneExit();
        }
        
        _activeScenes.Clear();
        _activeScenes.Add(name, newScene);
        return true;
    }

    public static void CleanUp()
    {
        foreach (var scene in _activeScenes.Values)
        {
            scene.OnSceneExit();
        }
        
        _activeScenes.Clear();
    }
}