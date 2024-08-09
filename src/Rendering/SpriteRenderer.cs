using System.Numerics;
using Arch.Core;
using OpenTK.Graphics.OpenGL;
using Vector2 = System.Numerics.Vector2;

namespace WaffleEngine;

public static class SpriteRenderer
{
    private static Dictionary<Material, (SpriteMesh mesh, Buffer<(Matrix4x4 transform, Vector2 offset, Vector2 size)> buffer)> _sprite_batches = new();
    private static Dictionary<Material, (SpriteMesh mesh, Buffer<(Matrix4x4 transform, Vector2 offset, Vector2 size)> buffer)> _opaque_sprite_batches = new();

    private static SortByYIncreasing _sort_by_y_increasing = new SortByYIncreasing();
    private class SortByYIncreasing : IComparer<(Matrix4x4 transform, Vector2 offset, Vector2 size)>
    {
        public int Compare((Matrix4x4 transform, Vector2 offset, Vector2 size) a,
            (Matrix4x4 transform, Vector2 offset, Vector2 size) b)
        {
            return (a.transform.M24 < b.transform.M24 ? 1 : 0)
                   - (a.transform.M24 > b.transform.M24 ? 1 : 0);
        }
    }
    
    private static SortByZIncreasing _sort_by_z_increasing = new SortByZIncreasing();
    private class SortByZIncreasing : IComparer<(Matrix4x4 transform, Vector2 offset, Vector2 size)>
    {
        public int Compare((Matrix4x4 transform, Vector2 offset, Vector2 size) a,
            (Matrix4x4 transform, Vector2 offset, Vector2 size) b)
        {
            return (a.transform.M34 > b.transform.M34 ? 1 : 0)
                   - (a.transform.M34 < b.transform.M34 ? 1 : 0);
        }
    }
    
    private static SortByZDecreasing _sort_by_z_decreasing = new SortByZDecreasing();
    private class SortByZDecreasing : IComparer<(Matrix4x4 transform, Vector2 offset, Vector2 size)>
    {
        public int Compare((Matrix4x4 transform, Vector2 offset, Vector2 size) a,
            (Matrix4x4 transform, Vector2 offset, Vector2 size) b)
        {
            return (a.transform.M34 > b.transform.M34 ? 0 : 1)
                   - (a.transform.M34 < b.transform.M34 ? 0 : 1);
        }
    }

    private static void GetSpriteBatches(ref World world)
    {
        var query_desc = new QueryDescription()
            .WithAll<Transform, Sprite>();

        foreach (var batch in _sprite_batches.Values)
        {
            batch.buffer.Clear();
        }

        world.Query(query_desc, (ref Transform transform, ref Sprite sprite) =>
        {
            Material material = sprite.Material;

            if (!_sprite_batches.ContainsKey(material))
            {
                _sprite_batches[material] = (SpriteMesh.Create(material), new());
            }
            
            Matrix4x4 sprite_mat = transform.Matrix;

            sprite_mat.M11 *= (float)material.Texture.Width / sprite.PixelsPerUnit;
            sprite_mat.M22 *= (float)material.Texture.Height / sprite.PixelsPerUnit;
            
            _sprite_batches[material].buffer.Add((sprite_mat, Vector2.Zero, Vector2.One));
        });
    }
    
    private static void GetOpaqueSpriteBatches(ref World world)
    {
        var query_desc = new QueryDescription()
            .WithAll<Transform, SpriteOpaque>();

        foreach (var batch in _opaque_sprite_batches.Values)
        {
            batch.buffer.Clear();
        }

        world.Query(query_desc, (ref Transform transform, ref SpriteOpaque sprite) =>
        {
            Material material = sprite.Material;

            if (!_opaque_sprite_batches.ContainsKey(material))
            {
                _opaque_sprite_batches[material] = (SpriteMesh.Create(material), new());
            }

            Matrix4x4 sprite_mat = transform.Matrix;

            sprite_mat.M11 *= (float)material.Texture.Width / sprite.PixelsPerUnit;
            sprite_mat.M22 *= (float)material.Texture.Height / sprite.PixelsPerUnit;

            _opaque_sprite_batches[material].buffer.Add((sprite_mat, Vector2.Zero, Vector2.One));
        });
    }

    public static void RenderYSorted(ref World world, ref Camera camera)
    {
        GetSpriteBatches(ref world);

        foreach (var batch in _sprite_batches)
        {
            Array.Sort(batch.Value.buffer.Items, _sort_by_y_increasing);
        }

        RenderSprites(ref camera);
    }

    public static void RenderZSorted(ref World world, ref Camera camera)
    {
        GetSpriteBatches(ref world);

        foreach (var batch in _sprite_batches)
        {
            batch.Value.buffer.Sort(_sort_by_z_increasing);
        }

        RenderSprites(ref camera);
    }
    
    public static void RenderUnsorted(ref World world, ref Camera camera)
    {
        GetSpriteBatches(ref world);
        
        RenderSprites(ref camera);
    }

    private static unsafe void RenderSprites(ref Camera camera)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        foreach (var batch in _sprite_batches)
        {
            batch.Value.mesh.Bind();
            
            batch.Key.Enable(ref camera);
            
            batch.Value.mesh.UpdateVertexData(batch.Value.buffer.Items);
            
            GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null, batch.Value.buffer.Count);
            
            batch.Key.Disable();
        }
    }

    public static void RenderOpaques(ref World world, ref Camera camera)
    {
        GetOpaqueSpriteBatches(ref world);
        
        // Front to Back Sorting to avoid overdraw

        // Turns out with 50000 sprites it does more harm then good.
        // Currently CPU Bottlenecked if we get GPU Bottlenecked can change.

        //foreach (var batch in _opaque_sprite_batches)
        //{
        //    batch.Value.buffer.Sort(_sort_by_z_decreasing);
        //}
        
        RenderOpaqueSprites(ref camera);
    }

    private static unsafe void RenderOpaqueSprites(ref Camera camera)
    {
        GL.Enable(EnableCap.DepthTest);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        foreach (var batch in _opaque_sprite_batches)
        {
            batch.Value.mesh.Bind();
            
            batch.Key.Enable(ref camera);
            
            batch.Value.mesh.UpdateVertexData(batch.Value.buffer.Items);
            
            GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null, batch.Value.buffer.Count);
            
            batch.Key.Disable();
        }
        
        GL.Disable(EnableCap.DepthTest);
    }
}
