using WaffleEngine;

namespace Vector.Editor.UI;

// Single source of truth for editor look-and-feel. All values are
// display-space (gamma-encoded) colors — Oklch/Oklab statics already arrive
// that way, raw floats here were picked on screen. Nothing outside this
// file should hardcode a UI color or metric.
public static class Theme
{
    // Window background behind everything.
    public static readonly Color SceneBackground = new(0.08f, 0.09f, 0.11f, 1f);

    // Panels.
    public static readonly Color BarBackground = new(0.11f, 0.12f, 0.14f, 1f);
    public static readonly Color PanelBackground = new(0.13f, 0.14f, 0.17f, 1f);

    // Buttons / rows.
    public static readonly Color ControlNormal = new(0.16f, 0.17f, 0.20f, 1f);
    public static readonly Color ControlHover = new(0.24f, 0.26f, 0.31f, 1f);
    public static readonly Color ControlSelected = new(0.20f, 0.36f, 0.52f, 1f);
    public static readonly Color ControlBorderColor = new(0.55f, 0.75f, 1f, 1f);
    public static readonly Color RowHover = new(0.22f, 0.24f, 0.29f, 1f);
    public static readonly Color RowNormal = new(0.17f, 0.18f, 0.22f, 1f);

    // Slider track.
    public static readonly Color TrackBackground = new(0.10f, 0.11f, 0.13f, 1f);
    public static readonly Color TrackRest = new(0.25f, 0.27f, 0.32f, 1f);
    public static readonly Color TrackFill = new(0.35f, 0.58f, 0.88f, 1f);

    // Canvas viewport.
    public static readonly Color CanvasBackdrop = new(0.07f, 0.08f, 0.10f, 1f);
    public static readonly Color CanvasBorder = new(0.55f, 0.75f, 1f, 1f);

    // Curve handles (all display-ready Oklch statics — no conversion).
    public static readonly Color HandleAnchor = Color.Gray;
    public static readonly Color HandleOpenStart = Color.Lime;
    public static readonly Color HandleControl = Color.Orange;
    public static readonly Color HandleSelected = Color.White;
    public static readonly Color HandleBorderColor = Color.White;

    public static readonly Color Text = Color.White;
    public static readonly Color ChipFallback = Color.Gray;

    public const float DisabledAlpha = 0.45f;

    // Layout metrics (pixels).
    public const int CornerRadius = 6;
    public const int ChipRadius = 4;
    public const float ControlBorderWidth = 2f;
    public const float HandleBorderWidth = 2.5f;
    public const float ChipBorder = 1f;
    public const float SwatchBorder = 1.5f;
    public const int ButtonHeight = 28;
    public const int ButtonMaxWidth = 90;
    public const int ButtonGap = 6;
    public const int GroupGap = 12;
    public const int RowHeight = 30;
    public const int RowGap = 4;
    public const int PanelPad = 6;
    public const int HeaderHeight = 28;
    public const int FooterHeight = 34;
    public const int SliderHeight = 28;
    public const int HandleAnchorSize = 10;
    public const int HandleControlSize = 7;
    public const int HandleSelectedGrow = 2;
    public const float CanvasPad = 10f;
    public const float CanvasBorderWidth = 2f;
    public const int SideWidth = 220;
    public const int ToolbarHeight = 38;
}
