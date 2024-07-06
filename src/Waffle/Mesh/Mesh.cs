using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WaterTrans.GlyphLoader.Geometry;

namespace WaffleEngine
{
    public class Mesh
    {
        private Raylib_cs.Mesh _mesh;
        private Raylib_cs.Model _model;

        public static Mesh TriangulatePoly(List<Vector2> outer, params List<Vector2>[] inners)
        {
            Mesh mesh = new Mesh();
            
            List<Triangle> triangles = BowyerWatson.Triangulate(ref outer);

            Log.Info("{0}", triangles.Count);

            mesh._mesh = new Raylib_cs.Mesh (outer.Count, triangles.Count);
            mesh._mesh.AllocVertices();
            mesh._mesh.AllocIndices();

            Span<Vector3> vertices = mesh._mesh.VerticesAs<Vector3>();
            Span<ushort> indices = mesh._mesh.IndicesAs<ushort>();

            for (int i = 0; i < outer.Count; i++)
            {
                vertices[i] = new Vector3(outer[i].X, outer[i].Y, 0);
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                indices[i * 3 + 0] = (ushort)triangles[i].A;
                indices[i * 3 + 1] = (ushort)triangles[i].B;
                indices[i * 3 + 2] = (ushort)triangles[i].C;
            }
            
            Raylib.UploadMesh(ref mesh._mesh, false);
            
            mesh._model = Raylib.LoadModelFromMesh(mesh._mesh);

            return mesh;
        }
        
        public static Mesh TriangulateGlyph(PathFigureCollection figures)
        {
            Mesh mesh = new Mesh();
            
            List<Triangle> triangles = BowyerWatson.Triangulate(ref figures);

            Log.Info("{0}", triangles.Count);

            mesh._mesh = new Raylib_cs.Mesh (outer.Count, triangles.Count);
            mesh._mesh.AllocVertices();
            mesh._mesh.AllocIndices();

            Span<Vector3> vertices = mesh._mesh.VerticesAs<Vector3>();
            Span<ushort> indices = mesh._mesh.IndicesAs<ushort>();

            for (int i = 0; i < outer.Count; i++)
            {
                vertices[i] = new Vector3(outer[i].X, outer[i].Y, 0);
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                indices[i * 3 + 0] = (ushort)triangles[i].A;
                indices[i * 3 + 1] = (ushort)triangles[i].B;
                indices[i * 3 + 2] = (ushort)triangles[i].C;
            }
            
            Raylib.UploadMesh(ref mesh._mesh, false);
            
            mesh._model = Raylib.LoadModelFromMesh(mesh._mesh);

            return mesh;
        }

        public void DestroyMesh()
        {
            Raylib.UnloadModel(_model);
        }

        public void DebugMesh()
        {
            Span<Vector3> vertices = _mesh.VerticesAs<Vector3>();
            Span<ushort> indices = _mesh.IndicesAs<ushort>();

            foreach (Vector3 vert in vertices)
            {
                Raylib.DrawPoint3D(vert, Color.Pink);
            }

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Raylib.DrawLine3D(vertices[indices[i * 3]], vertices[indices[i * 3 + 1]], Color.Red);
                Raylib.DrawLine3D(vertices[indices[i * 3 + 1]], vertices[indices[i * 3 + 2]], Color.Red);
                Raylib.DrawLine3D(vertices[indices[i * 3 + 2]], vertices[indices[i * 3]], Color.Red);
            }
        }

        public static implicit operator Raylib_cs.Mesh(Mesh mesh) => mesh._mesh;
        public static implicit operator Raylib_cs.Model(Mesh mesh) => mesh._model;
    }
}
