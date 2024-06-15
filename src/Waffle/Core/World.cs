using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public abstract class World
    {
        public void InternalUpdate()
        {
            // Run game engine interal stuff before 
            // game update.

            Update();
        }

        public virtual void Update() { }
    }
}
