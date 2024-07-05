using Arch.Core;
using Raylib_cs;
using System.Numerics;

namespace WaffleEngine
{
    public static unsafe class SpriteRenderer
    {
        private static List<(Transform transform, Sprite sprite)> list = new();

        public static void RenderYSorted(ref World world, ref Camera camera)
        {
            var query_desc = new QueryDescription()
                .WithAll<Transform, Sprite>();

            list.Clear();

            world.Query(query_desc, (ref Transform transform, ref Sprite sprite) =>
            {
                list.Add((transform, sprite));
            });

            list.Sort((a, b) =>
            {
                return (a.transform.Position.Y < b.transform.Position.Y ? 1 : 0)
                - (a.transform.Position.Y > b.transform.Position.Y ? 1 : 0);
            });

            RenderList(ref camera);
        }

        public static void RenderZSorted(ref World world, ref Camera camera)
        {
            var query_desc = new QueryDescription()
                .WithAll<Transform, Sprite>();

            list.Clear();

            world.Query(query_desc, (ref Transform transform, ref Sprite sprite) =>
            {
                list.Add((transform, sprite));
            });

            list.Sort((a, b) =>
            {
                return (a.transform.Position.Z > b.transform.Position.Z ? 1 : 0)
                - (a.transform.Position.Z < b.transform.Position.Z ? 1 : 0);
            });

            RenderList(ref camera);
        }

        private static void RenderList(ref Camera camera)
        {
            Raylib.BeginMode3D(camera);

            foreach ((Transform transform, Sprite sprite) in list)
            {
                Texture2D texture = sprite.GetTexture();

                float local_width = (float)texture.Width / sprite.PixelsPerUnit;
                float local_height = (float)texture.Height / sprite.PixelsPerUnit;

                Raylib.DrawTexturePro(
                    texture,
                    new Rectangle(0, 0, -texture.Width, -texture.Height),
                    new Rectangle(local_width * 0.5f, -local_height * 0.5f, -local_width, local_height),
                    new Vector2(-transform.Position.X, -transform.Position.Y),
                    transform.Rotation.Z,
                    Color.White
                );
            }

            Raylib.EndMode3D();
        }

        public static void RenderUnsorted(ref World world, ref Camera camera)
        {
            var query_desc = new QueryDescription()
                .WithAll<Transform, Sprite>();

            Raylib.BeginMode3D(camera);

            world.Query(query_desc, (ref Transform transform, ref Sprite sprite) =>
            {
                Texture2D texture = sprite.GetTexture();

                float local_width = (float)texture.Width / sprite.PixelsPerUnit;
                float local_height = (float)texture.Height / sprite.PixelsPerUnit;

                Raylib.DrawTexturePro(
                    texture,
                    new Rectangle(0, 0, -texture.Width, -texture.Height),
                    new Rectangle(local_width * 0.5f, -local_height * 0.5f, -local_width, local_height),
                    new Vector2(-transform.Position.X, -transform.Position.Y),
                    transform.Rotation.Z,
                    Color.White
                );
            });

            Raylib.EndMode3D();
        }
    }
}
