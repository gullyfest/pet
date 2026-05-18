using UnityEngine;

namespace aerisOS.Utils
{
    /// <summary>
    /// Procedural Texture2D / Sprite generators for the aeris Aero UI.
    /// Avoids shipping any image assets — all visuals are generated at runtime.
    /// </summary>
    public static class TextureFactory
    {
        // Cache to avoid rebuilding identical textures every frame.
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> Cache
            = new System.Collections.Generic.Dictionary<string, Sprite>();

        /// <summary>Solid color sprite (1x1 stretched).</summary>
        public static Sprite Solid(Color c)
        {
            string key = $"solid_{ColorUtility.ToHtmlStringRGBA(c)}";
            if (Cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = s;
            return s;
        }

        /// <summary>
        /// Rounded rectangle with a top→bottom vertical gradient and a glossy
        /// highlight band over the top half. Heart of the aeris Aero look.
        /// </summary>
        public static Sprite RoundedGlossy(int w, int h, int radius, Color top, Color bottom, bool gloss = true)
        {
            string key = $"glossy_{w}x{h}_r{radius}_{ColorUtility.ToHtmlStringRGBA(top)}_{ColorUtility.ToHtmlStringRGBA(bottom)}_{gloss}";
            if (Cache.TryGetValue(key, out var s)) return s;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float ty = (float)y / Mathf.Max(1, h - 1);
                Color baseCol = Color.Lerp(bottom, top, ty);

                if (gloss)
                {
                    // Top-half highlight: brighter band that fades downward.
                    if (ty > 0.5f)
                    {
                        float g = (ty - 0.5f) / 0.5f;
                        Color highlight = new Color(1f, 1f, 1f, 0.45f * g);
                        baseCol = AlphaBlend(baseCol, highlight);
                    }
                    // Subtle bottom rim brightness for "wet glass" feel.
                    if (ty < 0.15f)
                    {
                        float g = 1f - (ty / 0.15f);
                        Color glow = new Color(1f, 1f, 1f, 0.18f * g);
                        baseCol = AlphaBlend(baseCol, glow);
                    }
                }

                for (int x = 0; x < w; x++)
                {
                    float a = RoundedAlpha(x, y, w, h, radius);
                    Color c = baseCol;
                    c.a *= a;
                    pixels[y * w + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = s;
            return s;
        }

        /// <summary>Vertical gradient sprite without rounded corners (for backgrounds).</summary>
        public static Sprite VerticalGradient(int w, int h, Color top, Color bottom)
        {
            string key = $"vgrad_{w}x{h}_{ColorUtility.ToHtmlStringRGBA(top)}_{ColorUtility.ToHtmlStringRGBA(bottom)}";
            if (Cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / Mathf.Max(1, h - 1);
                Color c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < w; x++) pixels[y * w + x] = c;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = s;
            return s;
        }

        /// <summary>Filled circle sprite — used for tray dots and bullet indicators.</summary>
        public static Sprite Circle(int diameter, Color color)
        {
            string key = $"circle_{diameter}_{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color[diameter * diameter];
            float r = diameter * 0.5f;
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - r + 0.5f;
                    float dy = y - r + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - dist);
                    Color c = color;
                    c.a *= a;
                    pixels[y * diameter + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = s;
            return s;
        }

        /// <summary>
        /// Stylized "app icon" — a glossy rounded square with a colored letter glyph
        /// drawn directly into the bitmap. Quick-and-dirty but recognizable.
        /// </summary>
        public static Sprite AppIcon(int size, Color tint, char glyph)
        {
            string key = $"appicon_{size}_{ColorUtility.ToHtmlStringRGBA(tint)}_{glyph}";
            if (Cache.TryGetValue(key, out var s)) return s;

            var baseSprite = RoundedGlossy(size, size, size / 5, tint, tint * 0.6f, true);
            var src = baseSprite.texture;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            tex.SetPixels(src.GetPixels());

            // Stamp a chunky letter using a tiny built-in 5x7 bitmap font.
            DrawGlyph(tex, glyph, size / 2, size / 2, Mathf.Max(1, size / 12), Color.white);
            tex.Apply();

            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = s;
            return s;
        }

        // ---- internals ----

        private static float RoundedAlpha(int x, int y, int w, int h, int radius)
        {
            if (radius <= 0) return 1f;
            int rx = -1, ry = -1;
            if (x < radius && y < radius) { rx = radius - x; ry = radius - y; }
            else if (x >= w - radius && y < radius) { rx = x - (w - radius - 1); ry = radius - y; }
            else if (x < radius && y >= h - radius) { rx = radius - x; ry = y - (h - radius - 1); }
            else if (x >= w - radius && y >= h - radius) { rx = x - (w - radius - 1); ry = y - (h - radius - 1); }
            if (rx < 0) return 1f;
            float dist = Mathf.Sqrt(rx * rx + ry * ry);
            return Mathf.Clamp01(radius - dist + 0.5f);
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            float a = src.a + dst.a * (1f - src.a);
            if (a < 0.0001f) return new Color(0, 0, 0, 0);
            float r = (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a;
            float g = (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a;
            float b = (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a;
            return new Color(r, g, b, a);
        }

        // 5x7 minimal glyphs — only the characters we need for app icons.
        private static readonly System.Collections.Generic.Dictionary<char, string[]> Glyphs
            = new System.Collections.Generic.Dictionary<char, string[]>
            {
                { 'C', new[]{"01110","10001","10000","10000","10000","10001","01110"} },
                { 'N', new[]{"10001","11001","10101","10101","10011","10001","10001"} },
                { 'M', new[]{"10001","11011","10101","10101","10001","10001","10001"} },
                { 'A', new[]{"01110","10001","10001","11111","10001","10001","10001"} },
                { 'B', new[]{"11110","10001","10001","11110","10001","10001","11110"} },
                { 'S', new[]{"01111","10000","10000","01110","00001","00001","11110"} },
                { '?', new[]{"01110","10001","00001","00010","00100","00000","00100"} },
            };

        private static void DrawGlyph(Texture2D tex, char glyph, int cx, int cy, int scale, Color col)
        {
            if (!Glyphs.TryGetValue(char.ToUpper(glyph), out var rows)) rows = Glyphs['?'];
            int gw = 5 * scale;
            int gh = 7 * scale;
            int x0 = cx - gw / 2;
            int y0 = cy - gh / 2;
            for (int row = 0; row < 7; row++)
            {
                string r = rows[6 - row];
                for (int col_ = 0; col_ < 5; col_++)
                {
                    if (r[col_] != '1') continue;
                    for (int sy = 0; sy < scale; sy++)
                        for (int sx = 0; sx < scale; sx++)
                        {
                            int px = x0 + col_ * scale + sx;
                            int py = y0 + row * scale + sy;
                            if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                                tex.SetPixel(px, py, col);
                        }
                }
            }
        }
    }
}
