namespace WaffleEngine;

public interface IScene
{
    /// <summary>
    /// Gets called when the scene is loaded.
    /// </summary>
    /// <returns>If true the scene was loaded correctly.</returns>
    public bool OnSceneLoaded();

    /// <summary>
    /// Gets called when the scene is updated every frame.
    /// </summary>
    public void OnSceneUpdate();

    /// <summary>
    /// Gets called when exiting current scene.
    /// </summary>
    public void OnSceneExit();
}