using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine.Text.HarfBuzz;

namespace WaffleEngine.MSDF;

public class MsdfFont : IDisposable
{
    private HBBlob _blob;
    private HBFace _face;
    private HBFont _font;

    private MsdfFontData _data;
    private Texture _msdf_texture;

    private TextMesh _text_mesh;
    private DefaultSpriteMaterial _sprite_material;

    private Buffer<(Matrix4x4 transform, Vector2 offset, Vector2 size)> _instance_buffer = new();

    public MsdfFont(string path)
    {
        string json = File.ReadAllText($"{path}.json");

        _data = MsdfFontData.FromJson(json);

        _msdf_texture = new Texture($"{path}.png");
        _msdf_texture.SetLinear();

        Shader shader = Shader.Get("core", "msdf_text");
        _sprite_material = new DefaultSpriteMaterial(shader, _msdf_texture);
        _text_mesh = TextMesh.Create(_sprite_material);

        // 3. Create Font
        var font_bytes = File.ReadAllBytes($"{path}.ttf");
        _blob = new HBBlob(font_bytes);
        _face = new HBFace(_blob, 0);
        _font = new HBFont(_face);
        _font.Scale = (32 * 72, 32 * 72);
    }

    public void Render(HBBuffer buffer, Camera camera)
    {
        _instance_buffer.Clear();

        Span<GlyphInfo> glyph_infos = buffer.GlyphInfos;
        Span<GlyphPosition> glyph_positions = buffer.GlyphPositions;

        float cursor_x = 0;
        float cursor_y = 0;

        for (int glyph = 0; glyph < glyph_infos.Length; glyph++)
        {
            if (_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds != null)
            {
                Matrix4x4 transform = Matrix4x4.Identity;

                transform.M11 = ((float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Right - (float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Left);
                transform.M22 = ((float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Top - (float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Bottom);

                transform.M14 = cursor_x + glyph_positions[glyph].XOffset / (32 * 72) + (float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Left;
                transform.M24 = cursor_y + glyph_positions[glyph].YOffset / (32 * 72) + (float)_data.Glyphs[glyph_infos[glyph].Codepoint].PlaneBounds.Bottom;

                Vector2 offset = new Vector2(
                    (float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Left / _msdf_texture.Width,
                    (float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Bottom / _msdf_texture.Height
                );

                Vector2 size = new Vector2(
                    ((float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Right - (float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Left) / _msdf_texture.Width,
                    ((float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Top - (float)_data.Glyphs[glyph_infos[glyph].Codepoint].AtlasBounds.Bottom) / _msdf_texture.Height
                );

                _instance_buffer.Add((transform, offset, size));
            }

            cursor_x += (float)glyph_positions[glyph].XAdvance / (32 * 72);
            cursor_y += (float)glyph_positions[glyph].YAdvance / (32 * 72);
        }

        _text_mesh.UpdateVertexData(_instance_buffer.Items);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _text_mesh.Bind();

        _sprite_material.Enable(camera);

        _text_mesh.UpdateVertexData(_instance_buffer.Items);

        unsafe
        {
            GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null, _instance_buffer.Count);
        }

        _sprite_material.Disable();
    }

    public static implicit operator HBFont(MsdfFont font) => font._font;

    public void Dispose()
    {
        _font.Dispose();
        _face.Dispose();
        _blob.Dispose();
    }
}