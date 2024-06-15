using Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public class Application
    {
        private static Application? Instance;

        private World _GameWorld;

        public Application(World game_world)
        {
            _GameWorld = game_world;
        }

        public static void StartWorld(World game_world)
        {
            if (Instance != null)
                return;

            // Creates new Application
            // 
            // Will eventually add a World to the app where
            // Waffle can call Game World code.

            Instance = new Application(game_world);

            Instance.Init();

            Instance.Run();
        }

        public void Exit()
        {
            // Run Exit code.
        }

        public void Init()
        {
            // Init Window and stuff
        }

        public void Run()
        {
            while (true)
            {
                _GameWorld.InternalUpdate();
            }
        }
    }
}
