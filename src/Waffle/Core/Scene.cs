namespace WaffleEngine
{
    public abstract class Scene
    {
        public void InternalUpdate()
        {
            // Run game engine interal stuff before 
            // game update.

            Update();
        }

        public abstract void Update();

        public abstract void Render();
    }
}
