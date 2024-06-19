using Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine;

namespace Game.Core
{
    internal class EntryPoint
    {
        private static void Main(string[] args)
        {
            Application.StartingScene(new TestScene());
        }
    }
}