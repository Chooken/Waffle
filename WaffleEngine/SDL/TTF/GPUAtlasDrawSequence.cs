using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Native;

namespace WaffleEngine.SDLExtra.TTF;

[StructLayout(LayoutKind.Sequential)]
public struct GPUAtlasDrawSequence
{
    /// <summary>
    /// Texture atlas that stores the glyphs
    /// </summary>
    public IntPtr AtlasTexture;
        
    /// <summary>
    /// An array of vertex positions
    /// </summary>
    public NativePtr<Vector2> XY;
        
    /// <summary>
    /// An array of normalized texture coordinates for each vertex
    /// </summary>
    public NativePtr<Vector2> UV;
        
    /// <summary>
    /// Number of vertices
    /// </summary>
    public int NumVertices;
        
    /// <summary>
    /// An array of indices into the 'vertices' arrays
    /// </summary>
    public NativePtr<int> Indices;
        
    /// <summary>
    /// Number of indices
    /// </summary>
    public int NumIndices;

    /// <summary>
    /// The image type of this draw sequence
    /// </summary>
    public SDL3.TTF.ImageType ImageType;

    /// <summary>
    /// The next sequence (will be NULL in case of the last sequence) 
    /// </summary>
    public NativePtr<GPUAtlasDrawSequence> Next;

    public GPUAtlasDrawSequenceFormatted AsFormatted()
    {
        return new GPUAtlasDrawSequenceFormatted()
        {
            AtlasTexture = this.AtlasTexture,
            Vertices = new NativeArray<Vector2>(XY, (uint)NumVertices),
            UVs = new NativeArray<Vector2>(UV, (uint)NumVertices),
            Indices = new NativeArray<int>(Indices, (uint)NumIndices),
            ImageType = this.ImageType,
            Next = this.Next,
        };
    }
}

public struct GPUAtlasDrawSequenceFormatted
{
    public IntPtr AtlasTexture;
    public NativeArray<Vector2> Vertices;
    public NativeArray<Vector2> UVs;
    public NativeArray<int> Indices;
    public SDL3.TTF.ImageType ImageType;
    public NativePtr<GPUAtlasDrawSequence> Next;
}