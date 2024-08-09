
namespace WaffleEngine;

// Sebastian Legues FontData with some minor preference tweaks.
public class TrueTypeFontData
{
    public TrueTypeGlyphData[] Glyphs { get; private set; }
    public TrueTypeGlyphData MissingTrueTypeGlyph;
    public int UnitsPerEm;

    public Dictionary<uint, TrueTypeGlyphData> GlypLookup;

    public static TrueTypeFontData Load(string file_path)
    {
        return FontParser.Parse(file_path);
    }
    
    public TrueTypeFontData(TrueTypeGlyphData[] glyphs, int units_per_em)
    {
        Glyphs = glyphs;
        UnitsPerEm = units_per_em;
        GlypLookup = new();

        foreach (TrueTypeGlyphData c in glyphs)
        {
            if (c == null) continue;
            GlypLookup.Add(c.UnicodeValue, c);
            if (c.GlyphIndex == 0) MissingTrueTypeGlyph = c;
        }

        if (MissingTrueTypeGlyph == null) throw new System.Exception("No missing character glyph provided!");
    }

    public bool TryGetGlyph(uint unicode, out TrueTypeGlyphData character)
    {
        bool found = GlypLookup.TryGetValue(unicode, out character);
        if (!found)
        {
            character = MissingTrueTypeGlyph;
        }
        return found;
    }
}

public class TrueTypeGlyphData
{
    public uint UnicodeValue;
    public uint GlyphIndex;
    public Point[] Points;
    public int[] ContourEndIndices;
    public int AdvanceWidth;
    public int LeftSideBearing;

    public int MinX;
    public int MaxX;
    public int MinY;
    public int MaxY;

    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;

}

public struct Point
{
    public int X;
    public int Y;
    public bool OnCurve;

    public Point(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public Point(int x, int y, bool on_curve)
    {
        X = x;
        Y = y;
        OnCurve = on_curve;
    }
}