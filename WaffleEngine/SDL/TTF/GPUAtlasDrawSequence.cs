#region License
/* Copyright (c) 2024-2025 Eduard Gushchin.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 */
#endregion

using System.Runtime.InteropServices;
using WaffleEngine;
using WaffleEngine.Native;

namespace SDL3;

public static partial class TTF
{
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
        public ImageType ImageType;

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
        public ImageType ImageType;
        public NativePtr<GPUAtlasDrawSequence> Next;
    }
}