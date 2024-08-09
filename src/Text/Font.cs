using System.Numerics;

namespace WaffleEngine;

public class Font
{
    public Vector2[] BezierPoints;
    public Dictionary<uint, GlyphData> GlyphDatas = new();
    public int[] GlyphMetaData;

    public GlyphData MissingCharacterData;

    public Font(string file_path)
    {
        List<Vector2> bezier_points = new();
        List<int> meta_data = new();
        
        TrueTypeFontData font_data = TrueTypeFontData.Load(file_path);
        
        float scale = 1.0f / font_data.UnitsPerEm;
        
        List<Vector2[]> missing_contours = CreateContoursWithImpliedPoints(font_data.MissingTrueTypeGlyph, scale);

        var missing_glyph_bounds = GetBounds(font_data.MissingTrueTypeGlyph, font_data);
            
        MissingCharacterData = new()
        {
            Size = missing_glyph_bounds.size,
            ContourDataOffset = meta_data.Count,
            BezierDataOffset = bezier_points.Count,
            NumOfContours = missing_contours.Count
        };

        meta_data.Add(bezier_points.Count);
        meta_data.Add(missing_contours.Count);

        foreach (var contour in missing_contours)
        {
            meta_data.Add(contour.Length - 1);
                
            for (int i = 0; i < contour.Length; i++)
            {
                bezier_points.Add(contour[i] - missing_glyph_bounds.centre);
            }
        }
        
        foreach ((uint character, TrueTypeGlyphData true_type_glyph_data) in font_data.GlypLookup)
        {
            List<Vector2[]> contours = CreateContoursWithImpliedPoints(true_type_glyph_data, scale);

            var glyph_bounds = GetBounds(true_type_glyph_data, font_data);
            
            GlyphData glyph_data = new()
            {
                Size = glyph_bounds.size,
                ContourDataOffset = meta_data.Count,
                BezierDataOffset = bezier_points.Count,
                NumOfContours = contours.Count,
                AdvanceWidth = true_type_glyph_data.AdvanceWidth * scale,
                OffsetX = (true_type_glyph_data.MinX + (float)true_type_glyph_data.Width / 2) * scale,
                OffsetY = (true_type_glyph_data.MinY + (float)true_type_glyph_data.Height / 2) * scale
            };

            GlyphDatas.Add(character, glyph_data);
            meta_data.Add(bezier_points.Count);
            meta_data.Add(contours.Count);

            foreach (var contour in contours)
            {
                meta_data.Add(contour.Length - 1);
                
                for (int i = 0; i < contour.Length; i++)
                {
                    bezier_points.Add(contour[i] - glyph_bounds.centre);
                }
            }
        }

        BezierPoints = bezier_points.ToArray();
        GlyphMetaData = meta_data.ToArray();
    }

    public GlyphData GetGlyphData(uint unicode)
    {
        bool found = GlyphDatas.TryGetValue(unicode, out GlyphData character);
        
        if (!found)
            return MissingCharacterData;

        return character;
    }
    
    public static List<Vector2[]> CreateContoursWithImpliedPoints(TrueTypeGlyphData character, float scale)
    {
        const bool convert_straight_lines_to_bezier = true;

        int start_point_index = 0;
        int contour_count = character.ContourEndIndices.Length;

        List<Vector2[]> contours = new();

        for (int contour_index = 0; contour_index < contour_count; contour_index++)
        {
            int contour_end_index = character.ContourEndIndices[contour_index];
            int num_points_in_contour = contour_end_index - start_point_index + 1;
            Span<Point> contour_points = character.Points.AsSpan(start_point_index, num_points_in_contour);

            List<Vector2> reconstructed_points = new();
            List<Vector2> on_curve_points = new();

            // Get index of first on-curve point (seems to not always be first point for whatever reason)
            int first_on_curve_point_index = 0;
            
            for (int i = 0; i < contour_points.Length; i++)
            {
                if (contour_points[i].OnCurve)
                {
                    first_on_curve_point_index = i;
                    break;
                }
            }

            for (int i = 0; i < contour_points.Length; i++)
            {
                Point curr = contour_points[(i + first_on_curve_point_index + 0) % contour_points.Length];
                Point next = contour_points[(i + first_on_curve_point_index + 1) % contour_points.Length];

                reconstructed_points.Add(new Vector2(curr.X * scale, curr.Y * scale));
                if (curr.OnCurve) on_curve_points.Add(new Vector2(curr.X * scale, curr.Y * scale));
                bool is_consecutive_off_curve_points = !curr.OnCurve && !next.OnCurve;
                bool is_straight_line = curr.OnCurve && next.OnCurve;

                if (is_consecutive_off_curve_points || (is_straight_line && convert_straight_lines_to_bezier))
                {
                    bool on_curve = is_consecutive_off_curve_points;
                    float new_x = (curr.X + next.X) / 2.0f * scale;
                    float new_y = (curr.Y + next.Y) / 2.0f * scale;
                    reconstructed_points.Add(new Vector2(new_x, new_y));
                    if (on_curve) on_curve_points.Add(new Vector2(new_x, new_y));
                }
            }
            reconstructed_points.Add(reconstructed_points[0]);
            reconstructed_points = MakeMonotonic(reconstructed_points);


            contours.Add(reconstructed_points.ToArray());

            start_point_index = contour_end_index + 1;
        }

        return contours;
    }
    
    public static (Vector2 centre, Vector2 size) GetBounds(TrueTypeGlyphData character, TrueTypeFontData font_data)
    {
        const float antiAliasPadding = 0.005f;
        float scale = 1f / font_data.UnitsPerEm;

        float left = character.MinX * scale;
        float right = character.MaxX * scale;
        float top = character.MaxY * scale;
        float bottom = character.MinY * scale;

        Vector2 centre = new Vector2(left + right, top + bottom) / 2;
        Vector2 size = new Vector2(right - left, top - bottom) + Vector2.One * antiAliasPadding;
        return (centre, size);
    }
    
    public static List<Vector2> MakeMonotonic(List<Vector2> original)
    {
        List<Vector2> monotonic = new(original.Count);
        monotonic.Add(original[0]);

        for (int i = 0; i < original.Count - 1; i += 2)
        {
            Vector2 p0 = original[i];
            Vector2 p1 = original[i + 1];
            Vector2 p2 = original[i + 2];

            if ((p1.Y < MathF.Min(p0.Y, p2.Y) || p1.Y > MathF.Max(p0.Y, p2.Y)))
            {
                var split = SplitAtTurningPointY(p0, p1, p2);
                monotonic.Add(split.a1);
                monotonic.Add(split.a2);
                monotonic.Add(split.b1);
                monotonic.Add(split.b2);
            }
            else
            {
                monotonic.Add(p1);
                monotonic.Add(p2);
            }
        }
        return monotonic;
    }
    
    static (Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) SplitAtTurningPointY(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 a = p0 - 2 * p1 + p2;
        Vector2 b = 2 * (p1 - p0);
        Vector2 c = p0;

        // Calculate turning point by setting gradient.y to 0: 2at + b = 0; therefore t = -b / 2a
        float turning_point_t = -b.Y / (2 * a.Y);
        Vector2 turning_point = a * turning_point_t * turning_point_t + b * turning_point_t + c;

        // Calculate the new p1 point for curveA with points: p0, p1A, turningPoint
        // This is done by saying that p0 + gradient(t=0) * ? = p1A = (p1A.x, turningPoint.y)
        // Solve for lambda using the known turningPoint.y, and then solve for p1A.x
        float lambda_a = (turning_point.Y - p0.Y) / b.Y;
        float p1_a_x = p0.X + b.X * lambda_a;

        // Calculate the new p1 point for curveB with points: turningPoint, p1B, p2
        // This is done by saying that p2 + gradient(t=1) * ? = p1B = (p1B.x, turningPoint.y)
        // Solve for lambda using the known turningPoint.y, and then solve for p1B.x
        float lambda_b = (turning_point.Y - p2.Y) / (2 * a.Y + b.Y);
        float p1_b_x = p2.X + (2 * a.X + b.X) * lambda_b;

        return (new Vector2(p1_a_x, turning_point.Y), turning_point, new Vector2(p1_b_x, turning_point.Y), p2);
    }
}

public struct GlyphData
{
    public float AdvanceWidth;
    public int NumOfContours;
    public int ContourDataOffset;
    public int BezierDataOffset;
    public Vector2 Size;
    public float OffsetX;
    public float OffsetY;
}