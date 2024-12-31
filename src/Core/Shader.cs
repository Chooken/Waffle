using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class Shader : IDisposable
{
    private int ID;

    private Dictionary<string, int> _cached_attributes = new();
    private Dictionary<string, int> _cached_uniforms = new();

    private bool _value_disposed = false;

    public static Shader ShaderFromPath(string vertex_path, string fragment_path)
    {
        string vertex_shader_source = File.ReadAllText(vertex_path);
        string fragment_shader_source = File.ReadAllText(fragment_path);

        return new Shader(vertex_shader_source, fragment_shader_source);
    }

    public Shader(string vertex_shader_source, string fragment_shader_source)
    {
        ID = GL.CreateProgram();

        int vertex_shader_id = GL.CreateShader(ShaderType.VertexShader);
        int fragment_shader_id = GL.CreateShader(ShaderType.FragmentShader);
        
        GL.ShaderSource(vertex_shader_id, vertex_shader_source);
        GL.ShaderSource(fragment_shader_id, fragment_shader_source);
        
        GL.CompileShader(vertex_shader_id);
        GL.CompileShader(fragment_shader_id);

        int success = 0;

        GL.GetShaderi(vertex_shader_id, ShaderParameterName.CompileStatus, ref success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(vertex_shader_id, out string info);
            Log.Error("Shader Error [Vertex]: {0}", info);
        }
        
        GL.GetShaderi(fragment_shader_id, ShaderParameterName.CompileStatus, ref success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(fragment_shader_id, out string info);
            Log.Error("Shader Error [Fragment]: {0}", info);
        }
        
        GL.AttachShader(ID, vertex_shader_id);
        GL.AttachShader(ID, fragment_shader_id);
        
        GL.LinkProgram(ID);
        
        GL.GetProgrami(ID, ProgramProperty.LinkStatus, ref success);
        if (success == 0)
        {
            GL.GetProgramInfoLog(ID, out string info);
            Log.Error("Shader Link Error: {0}", info);
        }
        else Log.Info("Loaded Shader ID: {0}", ID);

        GL.DetachShader(ID, vertex_shader_id);
        GL.DetachShader(ID, fragment_shader_id);
        
        GL.DeleteShader(vertex_shader_id);
        GL.DeleteShader(fragment_shader_id);
    }

    public void ReloadShader(string vertex_shader_source, string fragment_shader_source)
    {
        int new_ID = GL.CreateProgram();

        int vertex_shader_id = GL.CreateShader(ShaderType.VertexShader);
        int fragment_shader_id = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(vertex_shader_id, vertex_shader_source);
        GL.ShaderSource(fragment_shader_id, fragment_shader_source);

        GL.CompileShader(vertex_shader_id);
        GL.CompileShader(fragment_shader_id);

        int success = 0;

        GL.GetShaderi(vertex_shader_id, ShaderParameterName.CompileStatus, ref success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(vertex_shader_id, out string info);
            Log.Error("Shader Error [Vertex]: {0}", info);
        }

        GL.GetShaderi(fragment_shader_id, ShaderParameterName.CompileStatus, ref success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(fragment_shader_id, out string info);
            Log.Error("Shader Error [Fragment]: {0}", info);
        }

        GL.AttachShader(new_ID, vertex_shader_id);
        GL.AttachShader(new_ID, fragment_shader_id);

        GL.LinkProgram(new_ID);

        GL.GetProgrami(new_ID, ProgramProperty.LinkStatus, ref success);
        if (success == 0)
        {
            GL.GetProgramInfoLog(new_ID, out string info);
            Log.Error("Shader Link Error: {0}", info);
        } else Log.Info("Remapped Shader ID: {0} to {1}", ID, new_ID);

        GL.DetachShader(new_ID, vertex_shader_id);
        GL.DetachShader(new_ID, fragment_shader_id);

        GL.DeleteShader(vertex_shader_id);
        GL.DeleteShader(fragment_shader_id);

        GL.DeleteProgram(ID);

        ID = new_ID;
    }

    public void Enable()
    {
        GL.UseProgram(ID);
    }

    public void Disable()
    {
        GL.UseProgram(0);
    }

    public int GetAttribLocation(string attrib_name)
    {
        if (!_cached_attributes.ContainsKey(attrib_name))
            _cached_attributes[attrib_name] = GL.GetAttribLocation(ID, attrib_name);

        return _cached_attributes[attrib_name];
    } 

    public int GetUniformLocation(string uniform_name)
    {
        if (!_cached_uniforms.ContainsKey(uniform_name))
            _cached_uniforms[uniform_name] = GL.GetUniformLocation(ID, uniform_name);

        return _cached_uniforms[uniform_name];
    } 

    public void Dispose()
    {
        if (_value_disposed)
            return;
        
        _value_disposed = true;
        
        GL.DeleteProgram(ID);

        Log.Info("Unloaded Shader ID: {0}", ID);

        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        if (_value_disposed)
            return;
        
        Log.Error("GPU Resource leak on Shader ID {0}", ID);
    }

    public static Shader Get(string folder, string name)
    {
        return AssetLoader.GetShader(folder, name);
    }
}