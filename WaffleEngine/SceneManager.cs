using System.Diagnostics.CodeAnalysis;

namespace WaffleEngine;

public static class SceneManager
{
    private static Dictionary<string, Scene> _activeScenes = new Dictionary<string, Scene>();

    public static void RunActiveSceneQueries()
    {
        foreach (var scene in _activeScenes.Values)
        {
            scene.RunQueries();
        }
    }

    public static void AddScene(Scene scene, string name)
    {
        scene.Init();
        _activeScenes.Add(name, scene);
    }
    
    public static void RemoveScene(string name)
    {
        if (!_activeScenes.TryGetValue(name, out var scene))
            return;
        
        scene.Dispose();
        
        _activeScenes.Remove(name);
    }

    public static void SetScene(Scene scene, string name)
    {
        _activeScenes.Clear();
        _activeScenes.Add(name, scene);
    }

    public static void CleanUp()
    {
        foreach (var scene in _activeScenes.Values)
        {
            scene.Dispose();
        }
        _activeScenes.Clear();
    }
}