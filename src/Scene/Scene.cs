using Arch.Core;

namespace WaffleEngine
{
    public abstract class Scene
    {
        public World World = World.Create();

        public virtual void Init() { }

        public virtual void Update() { }

        public virtual void Deinit() 
        {
            World.Destroy(World);
        }

        public delegate void SystemDelegate<T1>(ref T1 t1);
        public delegate void SystemDelegate<T1, T2>(ref T1 t1, ref T2 t2);
        public delegate void SystemDelegate<T1, T2, T3>(ref T1 t1, ref T2 t2, ref T3 t3);

        public void RunSystem<T1>(SystemDelegate<T1> system)
        {
            var query = new QueryDescription()
                .WithAll<T1>();

            World.Query(query, (ref T1 t1) =>
            {
                system.Invoke(ref t1);
            });
        }
        
        public void RunSystem<T1, T2>(SystemDelegate<T1, T2> system)
        {
            var query = new QueryDescription()
                .WithAll<T1, T2>();

            World.Query(query, (ref T1 t1, ref T2 t2) =>
            {
                system.Invoke(ref t1, ref t2);
            });
        }
        
        public void RunSystem<T1, T2, T3>(SystemDelegate<T1, T2, T3> system)
        {
            var query = new QueryDescription()
                .WithAll<T1, T2, T3>();

            World.Query(query, (ref T1 t1, ref T2 t2, ref T3 t3) =>
            {
                system.Invoke(ref t1, ref t2, ref t3);
            });
        }
    }
}
