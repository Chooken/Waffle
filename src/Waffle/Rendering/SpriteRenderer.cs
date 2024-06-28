using Flecs.NET.Core;
using Flecs.NET.Utilities;
using Raylib_cs;
using System.Numerics;

namespace WaffleEngine
{
    public static unsafe class SpriteRenderer
    {
        public static void Render(ref World world)
        {
            Routine routine = world.Routine<Transform, Sprite>("Sprite Renderer")
                .Kind(Ecs.OnStore)
                .OrderBy<Transform>(CompareTransforms)
                .Iter((Iter iterator, Column<Transform> transforms, Column<Sprite> sprites) =>
                {
                    Raylib.BeginMode3D(iterator.World().Get<Camera>());

                    foreach (int i in iterator)
                    {
                        Texture2D texture = sprites[i].GetTexture();

                        float local_width = (float)texture.Width / sprites[i].PixelsPerUnit;
                        float local_height = (float)texture.Height / sprites[i].PixelsPerUnit;

                        Raylib.DrawTexturePro(
                            texture,
                            new Rectangle(0, 0, -texture.Width, -texture.Height),
                            new Rectangle(local_width * 0.5f, -local_height * 0.5f, -local_width, local_height),
                            new Vector2(-transforms[i].Position.X, -transforms[i].Position.Y),
                            transforms[i].Rotation.Z,
                            Color.White
                        );
                    }

                    Raylib.EndMode3D();
                });
        }

        private static int CompareTransforms(ulong e1, void* t1, ulong e2, void* t2)
        {
            Transform* transform1 = (Transform*)t1;
            Transform* transform2 = (Transform*)t2;

            return Macros.Bool(transform1->Position.Z > transform2->Position.Z) - Macros.Bool(transform1->Position.Z < transform2->Position.Z);
        }
    }
}
