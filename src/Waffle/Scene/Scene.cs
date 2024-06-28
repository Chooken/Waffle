using Flecs.NET.Core;

namespace WaffleEngine
{
    public abstract class Scene
    {
        public World World = World.Create();

        public virtual void Init() { }

        public virtual void Deinit() { }
    }
}
