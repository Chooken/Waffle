using Flecs.NET.Core;
using System;
using System.Dynamic;
using System.IO;
using System.Numerics;
using WaffleEngine;

namespace Game.Core
{
    public class TestScene : Scene
    {
        int targetFPS = 60;
        //Camera camera = new Camera(0, 0, 0);
        //Sprite player = new Sprite("core", "Character");
        //Mesh mesh = new Mesh();
        //Raylib_cs.Model model;

        //public void Start()
        //{
        //    List<Vector2> vertices = new List<Vector2>();
        //    vertices.Add(new Vector2(0f, 0f));
        //    vertices.Add(new Vector2(0.5f, 0.4f));
        //    vertices.Add(new Vector2(0.4f, 0.5f));
        //    vertices.Add(new Vector2(-0.4f, 0.5f));
        //    vertices.Add(new Vector2(-0.5f, 0.4f));
        //    vertices.Add(new Vector2(-0.5f, -0.4f));
        //    vertices.Add(new Vector2(-0.4f, -0.5f));
        //    vertices.Add(new Vector2(0.4f, -0.5f));
        //    vertices.Add(new Vector2(0.5f, -0.4f));
        //    mesh.TriangulatePoly(vertices);

        //    mesh.UploadMesh();

        //    model = Raylib_cs.Raylib.LoadModelFromMesh(mesh);

        //    //var mesh = Raylib_cs.Raylib.GenMeshPoly(6, 1);

        //    //model = Raylib_cs.Raylib.LoadModelFromMesh(mesh);
        //}

        //public void End()
        //{
        //    Raylib_cs.Raylib.UnloadModel(model);
        //}

        //public void Render()
        //{
        //    //SpriteRenderer.Render(player, camera);

        //    Raylib_cs.Raylib.BeginMode3D(camera);

        //    Raylib_cs.Raylib.DrawModelEx(model, Vector3.Zero, Vector3.UnitY, 0f, Vector3.One, Raylib_cs.Color.RayWhite);

        //    Raylib_cs.Raylib.DrawGrid(10, 1);
        //    mesh.DebugMesh();

        //    Raylib_cs.Raylib.EndMode3D();

        //    Raylib_cs.Raylib.DrawFPS(10,10);
        //}

        public struct Character
        {
            public Vector3 Origin;
            public float Multiplier;
        }

        public override void Init()
        {
            this.World.Set(new Camera(0, 0, 0));

            Sprite sprite = new Sprite("core", "Character");

            Random random = new Random();

            for (int i = 0; i < 200; i++)
            {
                this.World.Entity()
                    .Set(new Transform())
                    .Set(sprite)
                    .Set(new Character
                    {
                        Origin = new Vector3(random.NextSingle() * 14f - 7, random.NextSingle() * 8f - 4, random.NextSingle() * -1f),
                        Multiplier = random.NextSingle() * 2 - 1f
                    });
            }

            this.World.Routine<Transform, Character>()
                .Each((ref Transform transform, ref Character character) =>
                {
                    transform.Position = new Vector3(
                        character.Origin.X + (float)Math.Sin((DateTime.Now - DateTime.MinValue).TotalSeconds * character.Multiplier),
                        character.Origin.Y + (float)Math.Cos((DateTime.Now - DateTime.MinValue).TotalSeconds * character.Multiplier),
                        character.Origin.Z
                    );
                });

            SpriteRenderer.Render(ref this.World);
        }

        public override void Update()
        {
            // Tests Uncapped Framerate.
            if (Keyboard.IsPressed(Keycode.F))
                Window.SetTargetFPS(targetFPS ^= 60);

            //// Tests Resizing.
            if (Keyboard.IsPressed(Keycode.R))
                Window.Resize(1500, 540);
        }

        public override void Deinit()
        {
            this.World.Dispose();
        }
    }
}
