using WaffleEngine;

namespace Vector.Editor.UI;

// The whole editor theme: 8 colors. Everything else is an alias below or a
// WithAlpha derivation at the use site — add a color here only if no
// existing one (possibly alpha-adjusted) does the job. All values are
// display-space (gamma-encoded); Oklch/Oklab statics already arrive that way.
public static class Theme
{
    public static int Generation { get; private set; }
    public static bool IsDark { get; private set; } = true;

    // Runtime scale for curve handles (Settings).
    public static float HandleScale = 1f;

    public static Color Bg;
    public static Color BgLight;
    public static Color Text;
    public static Color TextDim;
    public static Color Border;
    public static Color Accent;
    public static Color AccentLight;
    public static Color Highlight;

    // Fixed universal constants (not theme choices).
    public static readonly Color Scrim = new(0, 0, 0, 0.45f);

    // Curve handles, expressed in the palette so the count stays at 8.
    // Bone joints share AccentLight with the open-start marker; the two
    // never collide because Bone mode hides curve handles.
    public static Color HandleAnchor => TextDim;
    public static Color HandleOpenStart => AccentLight;
    public static Color HandleControl => Accent;
    public static Color HandleBone => AccentLight;
    public static Color HandleSelected => Text;
    public static Color HandleBorder => Text;
    public static Color ChipFallback => TextDim;

    static Theme() => ApplyDark();

    public static void ApplyDark()
    {
        IsDark = true;
        Bg = new Color(0.08f, 0.09f, 0.11f, 1f);
        BgLight = new Color(0.15f, 0.16f, 0.19f, 1f);
        Text = Color.White;
        TextDim = new Color(0.62f, 0.63f, 0.66f, 1f);
        Border = new Color(1f, 1f, 1f, 0.10f);
        Accent = Color.Purple;
        AccentLight = new Color(0.35f, 0.68f, 1f, 1f);
        Highlight = new Color(0.22f, 0.24f, 0.29f, 1f);
        Generation++;
    }

    public static void ApplyLight()
    {
        IsDark = false;
        Bg = new Color(0.93f, 0.93f, 0.94f, 1f);
        BgLight = new Color(1f, 1f, 1f, 1f);
        Text = new Color(0.31f, 0.31f, 0.32f, 1f);
        TextDim = new Color(0.65f, 0.65f, 0.67f, 1f);
        Border = new Color(0f, 0f, 0f, 0.14f);
        Accent = Color.Purple;
        AccentLight = new Color(0.45f, 0.72f, 1f, 1f);
        Highlight = new Color(0.88f, 0.88f, 0.90f, 1f);
        Generation++;
    }

    public const float DisabledAlpha = 0.45f;

    // Layout metrics (pixels).
    public const int CornerRadius = 8;
    public const int ChipRadius = 4;
    public const int ChipSize = 14;
    public const int ChipGap = 8;
    public const float ControlBorderWidth = 1f;
    public const float HandleBorderWidth = 2.5f;
    public const float ChipBorder = 1f;
    public const float SwatchBorder = 1.5f;
    public const int ButtonHeight = 28;
    public const int ButtonMaxWidth = 90;
    public const int ButtonGap = 6;
    public const int GroupGap = 12;
    public const int RowHeight = 30;
    public const int RowGap = 4;
    public const int PanelPad = 12;
    public const int HeaderHeight = 28;
    public const int FooterHeight = 68;
    public const int SliderHeight = 28;
    public const int HandleAnchorSize = 10;
    public const int HandleControlSize = 7;
    public const int HandleSelectedGrow = 2;
    public const float CanvasPad = 10f;
    public const float CanvasBorderWidth = 2f;
    public const int SideWidth = 220;
    public const int InspectorWidth = 240;
    public const int ToolbarHeight = 52;
    public const int StatusHeight = 28;
}
