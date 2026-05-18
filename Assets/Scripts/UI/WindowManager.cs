using aerisOS.Managers;
using aerisOS.Narrative;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public enum AppType { MyComputer, Notes, Music, Browser, Settings, Antivirus, Archive, Game, Drawing }

    /// <summary>
    /// Spawns DraggableWindow instances pre-populated with content for each
    /// virtual application. One window per app type at a time (re-clicking the
    /// icon brings the existing window forward).
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        public static WindowManager Instance { get; private set; }

        // Нарративные хуки
        public static string NotesOverrideContent = null;
        public static event System.Action<AppType> OnWindowOpened;
        public static event System.Action<AppType> OnWindowClosed;
        public AntivirusAppUI LastOpenedAntivirusApp { get; private set; }

        private RectTransform _layer;
        private NotificationSystem _notify;
        private SystemPanel _panel;
        private readonly System.Collections.Generic.Dictionary<AppType, DraggableWindow> _open
            = new System.Collections.Generic.Dictionary<AppType, DraggableWindow>();
        private readonly System.Collections.Generic.Dictionary<AppType, TaskbarEntry> _entries
            = new System.Collections.Generic.Dictionary<AppType, TaskbarEntry>();

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Закрывает окно указанного приложения (если открыто).</summary>
        public void CloseWindow(AppType type)
        {
            if (_open.TryGetValue(type, out var w) && w != null)
                w.Close();
        }

        /// <summary>Обновляет текст Notes если окно уже открыто.</summary>
        public void UpdateNotesContent(string text)
        {
            if (_open.TryGetValue(AppType.Notes, out var w) && w != null)
            {
                var field = w.Body.GetComponentInChildren<InputField>();
                if (field != null) field.text = text;
            }
        }

        public void Init(RectTransform windowLayer, NotificationSystem notify, SystemPanel panel)
        {
            Instance = this;
            _layer = windowLayer;
            _notify = notify;
            _panel = panel;
        }

        public void OpenWindow(AppType type)
        {
            if (_open.TryGetValue(type, out var existing) && existing != null)
            {
                // Re-clicking the icon restores a minimized window, otherwise
                // just brings it to front.
                if (existing.IsMinimized) existing.SetMinimized(false);
                existing.transform.SetAsLastSibling();
                if (_entries.TryGetValue(type, out var e) && e != null) e.Refresh();
                return;
            }

            string title = type switch
            {
                AppType.MyComputer => "My Computer",
                AppType.Notes      => "Notes",
                AppType.Music      => "Music Player",
                AppType.Browser    => "Web Browser",
                AppType.Settings   => "Settings",
                AppType.Antivirus=> "aeris Antivirus",
                AppType.Archive    => "aeris Archive",
                AppType.Game       => "Game",
                AppType.Drawing    => "Gift from Terra",
                _ => "App"
            };

            // Per-app window sizes.
            Vector2 size = type switch
            {
                AppType.Settings  => new Vector2(660, 520),
                AppType.Music     => new Vector2(520, 500),
                AppType.Browser   => new Vector2(620, 460),
                AppType.Notes     => new Vector2(520, 360),
                AppType.Antivirus => new Vector2(920, 620),
                AppType.Archive   => new Vector2(760, 460),
                AppType.Game      => new Vector2(520, 640),
                AppType.Drawing   => new Vector2(520, 520),
                _                 => new Vector2(580, 380),
            };
            Vector2 pos = new Vector2(Random.Range(-180f, 180f), Random.Range(-60f, 60f));
            var window = DraggableWindow.Create(_layer, title, size, pos);
            if (type == AppType.Antivirus) window.MinSize = new Vector2(720, 500);
            window.transform.SetAsLastSibling();

            FillBody(type, window);
            OnWindowOpened?.Invoke(type);
            window.OnClosed = () =>
            {
                _open.Remove(type);
                if (_entries.TryGetValue(type, out var e) && e != null) Destroy(e.gameObject);
                _entries.Remove(type);
                OnWindowClosed?.Invoke(type);
            };
            _open[type] = window;

            // Spawn a taskbar entry for it.
            if (_panel != null && _panel.TasksArea != null)
            {
                var entry = TaskbarEntry.Create(_panel.TasksArea, window);
                _entries[type] = entry;
            }

            AudioManager.Instance?.PlaySuccess();
        }

        // --- per-app body content ---

        private void FillBody(AppType type, DraggableWindow window)
        {
            switch (type)
            {
                case AppType.MyComputer: BuildMyComputer(window.Body); break;
                case AppType.Notes:      BuildNotes(window.Body); break;
                case AppType.Music:      BuildMusic(window.Body); break;
                case AppType.Browser:    BuildBrowser(window.Body); break;
                case AppType.Settings:   BuildSettings(window.Body); break;
                case AppType.Antivirus:BuildAntivirus(window.Body); break;
                case AppType.Archive:    BuildArchive(window.Body); break;
                case AppType.Game:       BuildGame(window.Body); break;
                case AppType.Drawing:    BuildDrawing(window.Body); break;
            }
        }

        private static Text AddText(Transform parent, string text, int size, TextAnchor align,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin ?? new Vector2(0, 1);
            rt.anchorMax = anchorMax ?? new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.text = text;
            t.alignment = align;
            t.color = color;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.raycastTarget = false;
            return t;
        }

        private void BuildMyComputer(RectTransform body)
        {
            AddText(body, "System Information", 22, TextAnchor.UpperLeft, ColorPalette.TextDark,
                new Vector2(8, -8), new Vector2(400, 30));
            AddText(body, $"OS: aerisOS 1.0\nCPU: Synthetic\nRAM: {SystemInfo.systemMemorySize} MB\nGPU: {SystemInfo.graphicsDeviceName}\nResolution: {Screen.width}x{Screen.height}\nUnity: {Application.unityVersion}",
                16, TextAnchor.UpperLeft, ColorPalette.TextDark,
                new Vector2(8, -44), new Vector2(540, 280));
        }

        private void BuildNotes(RectTransform body)
        {
            var go = new GameObject("Notes", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(body, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8, 8); rt.offsetMax = new Vector2(-8, -8);
            var img = go.GetComponent<Image>();
            img.sprite = TextureFactory.RoundedGlossy(560, 320, 12,
                Color.white.WithAlpha(0.85f), Color.white.WithAlpha(0.7f), false);

            // Inner text component.
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var trt = (RectTransform)textGO.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(12, 8); trt.offsetMax = new Vector2(-12, -8);
            var text = textGO.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = ColorPalette.TextDark;
            text.supportRichText = false;
            text.alignment = TextAnchor.UpperLeft;

            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.lineType = InputField.LineType.MultiLineNewline;
            input.text = NotesOverrideContent ?? "Welcome to Notes.\nType anything you like.\n\n— aerisOS";
        }

        private void BuildMusic(RectTransform body)
        {
            // Delegate entirely to the live-updating MusicPlayerUI component.
            var ui = body.gameObject.AddComponent<MusicPlayerUI>();
            ui.Build(body);
        }

        // ── Wallpaper definitions: name, top colour, bottom colour, emoji tag ─
        public static readonly (string Name, Color Top, Color Bot, string Tag)[] Wallpapers =
        {
            ("Aqua Sky",    ColorPalette.Hex("#48C6EF"), ColorPalette.Hex("#B8EEFF"), "Aero Classic"),
            ("Lime Garden", ColorPalette.Hex("#56C86A"), ColorPalette.Hex("#B8F0C8"), "Nature"),
            ("Sunset Glow", ColorPalette.Hex("#FF6B6B"), ColorPalette.Hex("#FFD4A8"), "Warm"),
            ("Deep Ocean",  ColorPalette.Hex("#1A5276"), ColorPalette.Hex("#2E86C1"), "Dark"),
            ("Lavender",    ColorPalette.Hex("#8E44AD"), ColorPalette.Hex("#D7BDE2"), "Dream"),
            ("Rose Gold",   ColorPalette.Hex("#C0392B"), ColorPalette.Hex("#F1948A"), "Elegant"),
            ("Mint Fresh",  ColorPalette.Hex("#1ABC9C"), ColorPalette.Hex("#A8E6CF"), "Fresh"),
            ("Twilight",    ColorPalette.Hex("#2C3E50"), ColorPalette.Hex("#3498DB"), "Night"),
        };

        private void BuildBrowser(RectTransform body)
        {
            // ── Header ──────────────────────────────────────────────────────
            var hdr = AddText(body, "Wallpaper Gallery", 26, TextAnchor.UpperCenter,
                ColorPalette.TextDark, new Vector2(0, -10), new Vector2(540, 34),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hdr.rectTransform.pivot = new Vector2(0.5f, 1);
            hdr.fontStyle = FontStyle.Bold;

            var sub = AddText(body, "Select a theme to change your desktop wallpaper",
                14, TextAnchor.UpperCenter, ColorPalette.TextDark.WithAlpha(0.6f),
                new Vector2(0, -46), new Vector2(520, 20),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            sub.rectTransform.pivot = new Vector2(0.5f, 1);

            // ── 4×2 tile grid ────────────────────────────────────────────
            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            grid.transform.SetParent(body, false);
            var gRT = (RectTransform)grid.transform;
            gRT.anchorMin = gRT.anchorMax = new Vector2(0.5f, 1);
            gRT.pivot = new Vector2(0.5f, 1);
            gRT.anchoredPosition = new Vector2(0, -72);
            gRT.sizeDelta = new Vector2(552, 280);
            var gl = grid.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(126, 118);
            gl.spacing = new Vector2(10, 10);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 4;
            gl.childAlignment = TextAnchor.UpperCenter;

            for (int i = 0; i < Wallpapers.Length; i++)
            {
                int idx = i;
                var wp = Wallpapers[i];

                // Outer card with slight drop-shadow feel (darker border)
                var card = new GameObject($"Card_{i}", typeof(RectTransform), typeof(Image));
                card.transform.SetParent(grid.transform, false);
                card.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(
                    130, 122, 14,
                    new Color(wp.Top.r * 0.6f, wp.Top.g * 0.6f, wp.Top.b * 0.6f),
                    new Color(wp.Bot.r * 0.6f, wp.Bot.g * 0.6f, wp.Bot.b * 0.6f), false);
                card.GetComponent<Image>().raycastTarget = false;

                // Inner tile (gradient preview), slightly inset
                var tile = new GameObject("Tile", typeof(RectTransform), typeof(Image), typeof(WallpaperTile));
                tile.transform.SetParent(card.transform, false);
                var tRT = (RectTransform)tile.transform;
                tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
                tRT.offsetMin = new Vector2(3, 24); tRT.offsetMax = new Vector2(-3, -3);
                tile.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(120, 84, 10, wp.Top, wp.Bot, true);
                tile.GetComponent<Image>().raycastTarget = true;
                var wt = tile.GetComponent<WallpaperTile>();
                wt.WpIndex = idx;
                wt.Notify = _notify;

                // Name label at bottom of card
                var lbl = new GameObject("Lbl", typeof(RectTransform), typeof(Text));
                lbl.transform.SetParent(card.transform, false);
                var lRT = (RectTransform)lbl.transform;
                lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(1, 0);
                lRT.pivot = new Vector2(0.5f, 0);
                lRT.sizeDelta = new Vector2(0, 22);
                lRT.anchoredPosition = new Vector2(0, 2);
                var lt = lbl.GetComponent<Text>();
                lt.text = wp.Name;
                lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lt.fontSize = 13; lt.fontStyle = FontStyle.Bold;
                lt.color = Color.white;
                lt.alignment = TextAnchor.MiddleCenter;
                lt.raycastTarget = false;
                var lo = lbl.AddComponent<Outline>();
                lo.effectColor = new Color(0, 0, 0, 0.7f);
                lo.effectDistance = new Vector2(1f, -1f);

                // Tag badge top-right
                var tag = new GameObject("Tag", typeof(RectTransform), typeof(Text));
                tag.transform.SetParent(card.transform, false);
                var tagRT = (RectTransform)tag.transform;
                tagRT.anchorMin = new Vector2(0, 1); tagRT.anchorMax = new Vector2(1, 1);
                tagRT.pivot = new Vector2(0.5f, 1);
                tagRT.sizeDelta = new Vector2(0, 20);
                tagRT.anchoredPosition = new Vector2(0, -2);
                var tagt = tag.GetComponent<Text>();
                tagt.text = wp.Tag;
                tagt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                tagt.fontSize = 11; tagt.fontStyle = FontStyle.Normal;
                tagt.color = Color.white.WithAlpha(0.85f);
                tagt.alignment = TextAnchor.MiddleCenter;
                tagt.raycastTarget = false;
            }

            // Footer hint
            var foot = AddText(body, "Click any wallpaper to apply",
                13, TextAnchor.LowerCenter, ColorPalette.TextDark.WithAlpha(0.5f),
                new Vector2(0, 6), new Vector2(520, 18),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            foot.rectTransform.pivot = new Vector2(0.5f, 0);
        }

        private void BuildSettings(RectTransform body)
        {
            float rowY = -10f;
            const float rowStep = 68f;

            AddText(body, "SFX Volume", 20, TextAnchor.UpperLeft, ColorPalette.TextDark,
                new Vector2(8, rowY), new Vector2(300, 26));
            rowY -= 36f;
            PlaceSlider(body, new Vector2(8, rowY), new Vector2(420, 28),
                AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 0.6f,
                v => { if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = v; });
            rowY -= rowStep;

            AddText(body, "Music Volume", 20, TextAnchor.UpperLeft, ColorPalette.TextDark,
                new Vector2(8, rowY), new Vector2(300, 26));
            rowY -= 36f;
            PlaceSlider(body, new Vector2(8, rowY), new Vector2(420, 28),
                aerisOS.Managers.MusicPlayer.Instance != null
                    ? aerisOS.Managers.MusicPlayer.Instance.Volume : 0.3f,
                v => { if (aerisOS.Managers.MusicPlayer.Instance != null) aerisOS.Managers.MusicPlayer.Instance.Volume = v; });
            rowY -= rowStep;

            // Test sound buttons.
            var btnClick = aerisButton.Create(body, "Test Click", new Vector2(155, 44),
                () => AudioManager.Instance?.PlayClick());
            var bCRT = (RectTransform)btnClick.transform;
            bCRT.anchorMin = bCRT.anchorMax = new Vector2(0, 1);
            bCRT.pivot = new Vector2(0, 1);
            bCRT.anchoredPosition = new Vector2(8, rowY);

            var btnNotify = aerisButton.Create(body, "Test Notify", new Vector2(155, 44),
                () => AudioManager.Instance?.PlayNotify());
            var bNRT = (RectTransform)btnNotify.transform;
            bNRT.anchorMin = bNRT.anchorMax = new Vector2(0, 1);
            bNRT.pivot = new Vector2(0, 1);
            bNRT.anchoredPosition = new Vector2(172, rowY);

            // Back-to-menu button.
            var back = aerisButton.Create(body, "Back to Menu", new Vector2(220, 50),
                () => SceneFlowManager.Instance?.GoTo(SceneFlowManager.Screen.Menu),
                ColorPalette.AccentPink, new Color(0.6f, 0.1f, 0.4f));
            var bRT = (RectTransform)back.transform;
            bRT.anchorMin = bRT.anchorMax = new Vector2(0.5f, 0);
            bRT.pivot = new Vector2(0.5f, 0);
            bRT.anchoredPosition = new Vector2(0, 14);
        }

        private void BuildAntivirus(RectTransform body)
        {
            var app = body.gameObject.AddComponent<AntivirusAppUI>();
            LastOpenedAntivirusApp = app;
            app.Build(body);
        }

        private static void BuildArchive(RectTransform body)
        {
            var app = body.gameObject.AddComponent<ArchiveAppUI>();
            app.Build(body);
        }

        private static void BuildGame(RectTransform body)
        {
            var app = body.gameObject.AddComponent<TicTacToeAppUI>();
            app.Build(body);
        }

        private void BuildDrawing(RectTransform body)
        {
            var bg = new GameObject("DrawingBG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(body, false);
            var bgrt = (RectTransform)bg.transform;
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 1f);
            bg.GetComponent<Image>().raycastTarget = false;

            var imgGO = new GameObject("GiftImg", typeof(RectTransform), typeof(Image));
            imgGO.transform.SetParent(body, false);
            var irt = (RectTransform)imgGO.transform;
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(8, 8);
            irt.offsetMax = new Vector2(-8, -8);
            var imgComp = imgGO.GetComponent<Image>();
            imgComp.raycastTarget = false;
            imgComp.preserveAspect = true;

            var tex = Resources.Load<Texture2D>("Gift/gift");
            if (tex != null)
                imgComp.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
        }

        // Builds a complete styled Slider and places it at anchoredPos inside parent.
        private static void PlaceSlider(RectTransform parent, Vector2 anchoredPos,
            Vector2 size, float initial, UnityEngine.Events.UnityAction<float> onChange)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            BuildSlider(go, initial, onChange);
        }

        private static void BuildSlider(GameObject sliderGO,
            float initial, UnityEngine.Events.UnityAction<float> onChange)
        {
            var slider = sliderGO.GetComponent<Slider>();
            int w = (int)((RectTransform)sliderGO.transform).sizeDelta.x;
            int h = (int)((RectTransform)sliderGO.transform).sizeDelta.y;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGO.transform, false);
            var bgRT = (RectTransform)bg.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(w, h, h / 2,
                Color.white.WithAlpha(0.5f), Color.white.WithAlpha(0.3f), false);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
            var faRT = (RectTransform)fillArea.transform;
            faRT.anchorMin = new Vector2(0, 0); faRT.anchorMax = new Vector2(1, 1);
            faRT.offsetMin = new Vector2(6, 4); faRT.offsetMax = new Vector2(-6, -4);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRT = (RectTransform)fill.transform;
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(
                w - 12, h - 8, (h - 8) / 2, ColorPalette.AeroCyan, new Color(0f, 0.5f, 0.9f), true);
            slider.fillRect = fillRT;

            var handleArea = new GameObject("HandleSlideArea", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGO.transform, false);
            var haRT = (RectTransform)handleArea.transform;
            haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(10, 0); haRT.offsetMax = new Vector2(-10, 0);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRT = (RectTransform)handle.transform;
            hRT.sizeDelta = new Vector2(h + 8, h + 8);
            handle.GetComponent<Image>().sprite = TextureFactory.Circle(40, Color.white);
            slider.handleRect = hRT;
            slider.targetGraphic = handle.GetComponent<Image>();

            slider.minValue = 0f; slider.maxValue = 1f; slider.value = initial;
            slider.onValueChanged.AddListener(onChange);
        }
    }
}
