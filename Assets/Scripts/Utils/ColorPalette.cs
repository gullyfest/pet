using UnityEngine;

namespace aerisOS.Utils
{
    /// <summary>
    /// aeris Aero color palette. Bright, glossy, sky-and-water themed colors
    /// inspired by mid-2000s consumer software design (Windows Vista, early iOS,
    /// Sony XMB, Zune). All values are hand-picked for the look.
    /// </summary>
    public static class ColorPalette
    {
        // Sky / desktop background gradient
        public static readonly Color SkyBlueTop    = Hex("#7EC8E3");
        public static readonly Color SkyBlueBottom = Hex("#B0E0F5");

        // Primary accents
        public static readonly Color AeroCyan  = Hex("#00BFFF");
        public static readonly Color AeroGreen = Hex("#00FF7F");

        // Glass / gloss
        public static readonly Color GlossWhite = new Color(1f, 1f, 1f, 0.55f);
        public static readonly Color GlassPanel = new Color(1f, 1f, 1f, 0.35f);

        // Taskbar gradient
        public static readonly Color TaskbarStart = Hex("#1B5FA8");
        public static readonly Color TaskbarEnd   = Hex("#0D3D7A");

        // Text
        public static readonly Color TextDark  = Hex("#0A2540");
        public static readonly Color TextLight = Color.white;

        // Extra accents
        public static readonly Color AccentPink = Hex("#FF69B4");
        public static readonly Color AccentLime = Hex("#7FFF00");

        // Shadow
        public static readonly Color ShadowSoft = new Color(0f, 0f, 0f, 0.25f);

        /// <summary>Convert "#RRGGBB" or "#RRGGBBAA" hex to Color.</summary>
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }

        /// <summary>Return color with replaced alpha (0..1).</summary>
        public static Color WithAlpha(this Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
