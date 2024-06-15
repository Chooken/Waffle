using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine;

namespace Game.Core
{
    public class GameWorld : World
    {
        private static void Main(string[] args)
        {
            Application.StartWorld(new GameWorld());
        }

        public override void Update()
        {
            Log.Error("AHHAHAH");
        }
    }
}
