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

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void Render() { }

        public virtual void End() { }
    }
}
