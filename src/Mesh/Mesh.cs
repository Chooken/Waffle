using OpenTK.Graphics.OpenGL;

namespace WaffleEngine
{
    public class Mesh
    {
        protected int _vertex_array_object;
        protected int _vertex_buffer_object;

        public virtual void Bind()
        {
            GL.BindVertexArray(_vertex_array_object);
        }

        public virtual void Unload()
        {
            GL.DeleteVertexArray(_vertex_array_object);
            GL.DeleteBuffer(_vertex_buffer_object);
            
            _vertex_array_object = -1;
            _vertex_buffer_object = -1;
        }

        ~Mesh()
        {
            if (_vertex_array_object == -1)
                return;
            
            Log.Error("Mesh [ID: {0}]: Wasn't unloaded Before the object got destroyed.");
        }
    }
}
