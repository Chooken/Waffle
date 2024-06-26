using Flecs.NET.Core;
using Raylib_cs;

namespace WaffleEngine
{
    public static class SpriteRenderer
    {
        public static void Render(Sprite sprite, Camera camera)
        {
            Raylib.BeginMode3D(camera);

            sprite.Draw();

            Raylib.EndMode3D();
        }

        public static void Render(World ecs, Camera camera)
        {
            Routine routine = ecs.Routine<Transform, Sprite>("SpriteRenderer")
                .Iter((Iter iterator, Column<Transform> transforms, Column<Sprite> sprites) =>
                {
                    Raylib.BeginMode3D(camera);

                    foreach (int i in iterator)
                    {
                        sprites[i].Draw(transforms[i]);
                    }
                    
                    Raylib.EndMode3D();
                });
        }
    }
}
