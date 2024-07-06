using Arch.Core;
using System;
using System.Dynamic;
using System.IO;
using System.Numerics;
using WaffleEngine;
using WaterTrans.GlyphLoader;
using Mesh = WaffleEngine.Mesh;
using Transform = WaffleEngine.Transform;

namespace Game.Core
{
    public class TestScene : Scene
    {
        private int _targetFPS = 60;
        private Camera _camera = new Camera(0, 0, 0);
        private UIManager _ui_manager = new UIManager();

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
            _camera = new Camera(0,0,0);

            Sprite sprite = new Sprite("core", "Character");

            Random random = new Random();

            for (int i = 0; i < 500; i++)
            {
                this.World.Create(
                    new Transform(), 
                    sprite, 
                    new Character
                    {
                        Origin = new Vector3(random.NextSingle() * 14f - 7, random.NextSingle() * 8f - 4, random.NextSingle() * -1f),
                        Multiplier = random.NextSingle() * 2 - 1f
                    }
                );
            }
            
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

            Typeface tf;

            using (var fontStream = System.IO.File.OpenRead("/assets/Quicksand-Regular.ttf"))
            {
                tf = new Typeface(fontStream);
            }
            
            double font_size = 20;
            string text = "hey";

            double x = 0;

            foreach (char character in text)
            {
                ushort glyphIndex = tf.CharacterToGlyphMap[character];

                var geometry = tf.GetGlyphOutline(glyphIndex, font_size);
                
                double advanceWidth = tf.AdvanceWidths[glyphIndex] * font_size;

                this.World.Create(
                    Mesh.TriangulateGlyph(geometry.Figures), 
                    new Transform { Position = new Vector3((float)x, 0, 0)}
                );
                
                x += advanceWidth;
            }

            this.World.Create(Mesh.TriangulatePoly(vertices));
        }

        public override void Update()
        {
            // Tests Uncapped Framerate.
            if (Keyboard.IsPressed(Keycode.F))
                Window.SetTargetFPS(_targetFPS ^= 60);

            //// Tests Resizing.
            if (Keyboard.IsPressed(Keycode.R))
                Window.Resize(1500, 540);

            var query = new QueryDescription()
                .WithAll<Transform, Character>();

            World.Query(query, (ref Transform transform, ref Character character) =>
            {
                transform.Position = new Vector3(
                    character.Origin.X + MathF.Sin(Time.TimeSinceStart * character.Multiplier),
                    character.Origin.Y + MathF.Cos(Time.TimeSinceStart * character.Multiplier),
                    character.Origin.Z
                );
            });

            SpriteRenderer.RenderYSorted(ref this.World, ref _camera);
            
            var query_2 = new QueryDescription()
                .WithAll<Mesh>();
            
            Raylib_cs.Raylib.BeginMode3D(_camera);
            
            World.Query(query_2, (ref Mesh mesh) =>
            {
                Raylib_cs.Raylib.DrawModelEx(mesh, Vector3.Zero, Vector3.UnitY, 0f, Vector3.One, Raylib_cs.Color.RayWhite);
                mesh.DebugMesh();
            });
            
            Raylib_cs.Raylib.EndMode3D();

            _ui_manager.RenderUI();
        }

        public override void Deinit()
        {
            var query = new QueryDescription()
                .WithAll<Mesh>();
            
            World.Query(query, (ref Mesh mesh) =>
            {
                mesh.DestroyMesh();
            });
            
            base.Deinit();
        }
    }
}
