namespace WaffleEngine.Text;

public enum HintingFlags
{
    Invalid = -1,
        
    /// <summary>
    /// Normal hinting applies standard grid-fitting.
    /// </summary>
    Normal = 0,
        
    /// <summary>
    /// Light hinting applies subtle adjustments to improve rendering.
    /// </summary>
    Light,
        
    /// <summary>
    /// Monochrome hinting adjusts the font for better rendering at lower resolutions.
    /// </summary>
    Mono,
        
    /// <summary>
    /// No hinting, the font is rendered without any grid-fitting.
    /// </summary>
    None,
        
    /// <summary>
    /// Light hinting with subpixel rendering for more precise font edges.
    /// </summary>
    LightSubpixel,
}