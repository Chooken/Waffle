using System;
using System.IO;
using System.Numerics;
using WaffleEngine;

namespace Game.Core
{
    public class TestScene : Scene
    {
        int targetFPS = 60;
        Camera camera = new Camera(0, 0, 0);
        Sprite player = new Sprite("core", "Character");
        Mesh mesh = new Mesh();
        Raylib_cs.Model model;

        public void Start()
        {
            List<Vector2> vertices = new List<Vector2>();
            vertices.Add(new Vector2(0f, 0f));
            vertices.Add(new Vector2(0.5f, 0.4f));
            vertices.Add(new Vector2(0.4f, 0.5f));
            vertices.Add(new Vector2(-0.4f, 0.5f));
            vertices.Add(new Vector2(-0.5f, 0.4f));
            vertices.Add(new Vector2(-0.5f, -0.4f));
            vertices.Add(new Vector2(-0.4f, -0.5f));
            vertices.Add(new Vector2(0.4f, -0.5f));
            vertices.Add(new Vector2(0.5f, -0.4f));
            mesh.TriangulatePoly(vertices);

            mesh.UploadMesh();

            model = Raylib_cs.Raylib.LoadModelFromMesh(mesh);

            //var mesh = Raylib_cs.Raylib.GenMeshPoly(6, 1);

            //model = Raylib_cs.Raylib.LoadModelFromMesh(mesh);
        }

        public void End()
        {
            Raylib_cs.Raylib.UnloadModel(model);
        }

        public void Update()
        {
            //Log.Error("AHHAHAH");
            //Log.Info("Key W: {0}", Keyboard.IsDown(Keycode.W));
            //Log.Info("Mouse Left: {0}", Mouse.IsLeft());

            // Tests Uncapped Framerate.
            if (Keyboard.IsPressed(Keycode.F))
                Window.SetTargetFPS(targetFPS ^= 60);

            // Tests Resizing.
            if (Keyboard.IsPressed(Keycode.R))
                Window.Resize(1500, 540);

            Vector2 moveVec = new Vector2(0, 0);

            if (Keyboard.IsDown(Keycode.A))
                moveVec.X -= 1;
            if (Keyboard.IsDown(Keycode.D))
                moveVec.X += 1;


            if (Keyboard.IsDown(Keycode.W))
                moveVec.Y += 1;
            if (Keyboard.IsDown(Keycode.S))
                moveVec.Y -= 1;

            if (Keyboard.IsDown(Keycode.LeftShift))
                moveVec *= 5f;

            camera.Move(moveVec.X * Raylib_cs.Raylib.GetFrameTime(), moveVec.Y * Raylib_cs.Raylib.GetFrameTime());
        }

        public void Render()
        {
            //SpriteRenderer.Render(player, camera);

            Raylib_cs.Raylib.BeginMode3D(camera);

            Raylib_cs.Raylib.DrawModelEx(model, Vector3.Zero, Vector3.UnitY, 0f, Vector3.One, Raylib_cs.Color.RayWhite);

            Raylib_cs.Raylib.DrawGrid(10, 1);
            mesh.DebugMesh();

            Raylib_cs.Raylib.EndMode3D();

            Raylib_cs.Raylib.DrawFPS(10,10);
        }

        public override void Init()
        {
            
        }
    }
}
