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

        public static void Render()
        {

        }
    }
}
