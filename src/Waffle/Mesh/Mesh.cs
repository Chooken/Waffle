using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public class Mesh
    {
        private Raylib_cs.Mesh _mesh;

        public Mesh TriangulatePoly(List<Vector2> outer, params List<Vector2>[] inners)
        {
            List<Triangle> triangles = BowyerWatson.Triangulate(ref outer);

            Log.Info("{0}", triangles.Count);

            _mesh = new(outer.Count, triangles.Count);
            _mesh.AllocVertices();
            _mesh.AllocIndices();

            Span<Vector3> vertices = _mesh.VerticesAs<Vector3>();
            Span<ushort> indices = _mesh.IndicesAs<ushort>();

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

            return this;
        }

        public void UploadMesh()
        {
            Raylib.UploadMesh(ref _mesh, false);
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
    }
}
