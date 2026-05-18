using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class MusicPlayerUI : MonoBehaviour
    {
        private Text _trackTitle;
        private Text _trackArtist;
        private Text _playBtnLabel;
        private RectTransform _progressFill;
        private float _progressMaxW;
        private Text[] _trackListItems;

        public void Build(RectTransform body)
        {
            float y = -8f;

            // ── Art (smaller to save vertical space) ───────────────────────
            Place(MakeArt(), body, 100, 100, 0, y); y -= 108f;

            // ── Title / Artist ─────────────────────────────────────────────
            _trackTitle = MakeLabel(body, "Loading…", 20, FontStyle.Bold, ColorPalette.TextDark, 0, y, 460, 24);
            y -= 26f;
            _trackArtist = MakeLabel(body, "", 14, FontStyle.Normal, ColorPalette.TextDark.WithAlpha(0.65f), 0, y, 460, 18);
            y -= 22f;

            // ── Progress bar ──────────────────────────────────────────────
            _progressMaxW = 440f;
            var pbBG = MakeImage(body, 0, y, _progressMaxW, 16,
                new Color(0.08f, 0.12f, 0.22f, 1f));
            var pfRT = MakeImage(pbBG, 3, 0, 6, -4,
                new Color(0f, 0.88f, 1f, 1f),
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0, 1),
                pivot: new Vector2(0, 0.5f));
            pfRT.GetComponent<Image>().sprite = null;
            _progressFill = pfRT;
            y -= 22f;

            // ── Transport: |<  <<  [PLAY]  >>  >| ─────────────────────────
            var rowGO = new GameObject("Transport", typeof(RectTransform));
            rowGO.transform.SetParent(body, false);
            var rowRT = (RectTransform)rowGO.transform;
            rowRT.anchorMin = rowRT.anchorMax = new Vector2(0.5f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0, y);
            rowRT.sizeDelta = new Vector2(460, 50);
            y -= 56f;

            MakeTransportBtn(rowGO.transform, "|<", -190, 48, 46, () => MusicPlayer.Instance?.Prev());
            MakeTransportBtn(rowGO.transform, "<<", -115, 48, 46, () =>
            {
                var s = MusicPlayer.Instance?.GetComponent<AudioSource>();
                if (s) s.time = Mathf.Max(0, s.time - 5f);
            });
            var playGO = aerisButton.Create(rowGO.transform, "II", new Vector2(64, 50),
                () => MusicPlayer.Instance?.Pause(),
                ColorPalette.AccentLime, new Color(0.08f, 0.5f, 0.12f), 24);
            var pRT = (RectTransform)playGO.transform;
            pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
            pRT.pivot = new Vector2(0.5f, 0.5f);
            pRT.anchoredPosition = new Vector2(0, 0);
            _playBtnLabel = playGO.transform.Find("Label")?.GetComponent<Text>();
            MakeTransportBtn(rowGO.transform, ">>", 115, 48, 46, () =>
            {
                var s = MusicPlayer.Instance?.GetComponent<AudioSource>();
                if (s && s.clip) s.time = Mathf.Min(s.clip.length - 0.1f, s.time + 5f);
            });
            MakeTransportBtn(rowGO.transform, ">|", 190, 48, 46, () => MusicPlayer.Instance?.Next());

            // ── Volume bar ─────────────────────────────────────────────────
            MakeLabel(body, "Volume", 12, FontStyle.Normal, ColorPalette.TextDark.WithAlpha(0.55f), 0, y, 460, 16);
            y -= 18f;
            float volW = 380f;
            var volBG = MakeImage(body, 0, y, volW, 12, new Color(0.08f, 0.12f, 0.22f, 1f));
            volBG.GetComponent<Image>().raycastTarget = true;
            var vSlider = volBG.gameObject.AddComponent<VolumeSliderBar>();
            vSlider.BarWidth = volW;
            var vFill = MakeImage(volBG, 2, 0, 4, -2,
                ColorPalette.AccentLime,
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0, 1),
                pivot: new Vector2(0, 0.5f));
            vFill.GetComponent<Image>().sprite = null;
            vSlider.Fill = vFill;
            float initVol = MusicPlayer.Instance != null ? MusicPlayer.Instance.Volume : 0.28f;
            vFill.sizeDelta = new Vector2(Mathf.Max(4, volW * initVol - 4), -2);
            y -= 18f;

            // ── Playlist label — centered, black ───────────────────────────
            y -= 4f;
            var plLbl = new GameObject("PlaylistLbl", typeof(RectTransform), typeof(Text));
            plLbl.transform.SetParent(body, false);
            var plRT = (RectTransform)plLbl.transform;
            plRT.anchorMin = new Vector2(0.5f, 1); plRT.anchorMax = new Vector2(0.5f, 1);
            plRT.pivot = new Vector2(0.5f, 1);
            plRT.anchoredPosition = new Vector2(0, y);
            plRT.sizeDelta = new Vector2(440, 20);
            var plT = plLbl.GetComponent<Text>();
            plT.text = "Playlist";
            plT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            plT.fontSize = 15; plT.fontStyle = FontStyle.Bold;
            plT.color = ColorPalette.TextDark;  // full black
            plT.alignment = TextAnchor.MiddleCenter;
            plT.raycastTarget = false;
            y -= 24f;

            // ── Scrollable playlist + scrollbar ────────────────────────────
            const float rowH = 38f;
            const float rowGap = 5f;
            const int visibleRows = 3;
            float scrollH = visibleRows * rowH + (visibleRows - 1) * rowGap;
            const float sbW = 10f; // scrollbar width

            int count = MusicPlayer.TrackMeta.Length;
            float contentH = count * rowH + (count - 1) * rowGap;

            // Outer container holds scroll area + scrollbar side by side
            var outerGO = new GameObject("PlaylistOuter", typeof(RectTransform));
            outerGO.transform.SetParent(body, false);
            var oRT = (RectTransform)outerGO.transform;
            oRT.anchorMin = oRT.anchorMax = new Vector2(0.5f, 1f);
            oRT.pivot = new Vector2(0.5f, 1f);
            oRT.anchoredPosition = new Vector2(0, y);
            oRT.sizeDelta = new Vector2(440, scrollH);

            // Viewport (scroll area, leaves room for scrollbar)
            var scrollGO = new GameObject("PlaylistScroll", typeof(RectTransform), typeof(UnityEngine.UI.ScrollRect));
            scrollGO.transform.SetParent(outerGO.transform, false);
            var sRT = (RectTransform)scrollGO.transform;
            sRT.anchorMin = Vector2.zero; sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero; sRT.offsetMax = new Vector2(-(sbW + 4f), 0);
            scrollGO.AddComponent<UnityEngine.UI.RectMask2D>();

            var sr = scrollGO.GetComponent<UnityEngine.UI.ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            // snap per row: one row height + gap
            sr.scrollSensitivity = rowH + rowGap;
            sr.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            sr.inertia = false; // disable inertia for snap-per-row feel

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(scrollGO.transform, false);
            var cRT = (RectTransform)contentGO.transform;
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1f);
            cRT.anchoredPosition = Vector2.zero;
            cRT.sizeDelta = new Vector2(0, contentH);
            sr.content = cRT;

            // Scrollbar on the right
            var sbGO = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            sbGO.transform.SetParent(outerGO.transform, false);
            var sbRT = (RectTransform)sbGO.transform;
            sbRT.anchorMin = new Vector2(1, 0); sbRT.anchorMax = new Vector2(1, 1);
            sbRT.pivot = new Vector2(1, 0.5f);
            sbRT.anchoredPosition = Vector2.zero;
            sbRT.sizeDelta = new Vector2(sbW, 0);
            sbGO.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.35f, 0.5f);

            // Scrollbar handle
            var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGO.transform.SetParent(sbGO.transform, false);
            var handleRT = (RectTransform)handleGO.transform;
            handleRT.anchorMin = Vector2.zero; handleRT.anchorMax = Vector2.one;
            handleRT.offsetMin = new Vector2(2, 2); handleRT.offsetMax = new Vector2(-2, -2);
            handleGO.GetComponent<Image>().color = new Color(0f, 0.75f, 1f, 0.9f);
            handleGO.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(8, 30, 4,
                new Color(0f, 0.85f, 1f), new Color(0f, 0.5f, 0.9f), true);

            var sb = sbGO.GetComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.targetGraphic = handleGO.GetComponent<Image>();
            sb.handleRect = handleRT;

            sr.verticalScrollbar = sb;
            sr.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            _trackListItems = new Text[count];
            float ry = 0f;
            for (int i = 0; i < count; i++)
            {
                int idx = i;
                var meta = MusicPlayer.TrackMeta[i];

                var rowBG = new GameObject($"Track_{i}", typeof(RectTransform), typeof(Image));
                rowBG.transform.SetParent(contentGO.transform, false);
                var rRT = (RectTransform)rowBG.transform;
                rRT.anchorMin = new Vector2(0, 1); rRT.anchorMax = new Vector2(1, 1);
                rRT.pivot = new Vector2(0.5f, 1f);
                rRT.anchoredPosition = new Vector2(0, -ry);
                rRT.sizeDelta = new Vector2(0, rowH);
                rowBG.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(
                    420, (int)rowH, 8, Color.white.WithAlpha(0.45f), Color.white.WithAlpha(0.2f), false);
                rowBG.GetComponent<Image>().raycastTarget = true;
                rowBG.AddComponent<TrackRowButton>().Index = idx;

                var lbl = new GameObject("L", typeof(RectTransform), typeof(Text));
                lbl.transform.SetParent(rowBG.transform, false);
                var lRT = (RectTransform)lbl.transform;
                lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
                lRT.offsetMin = new Vector2(12, 0); lRT.offsetMax = new Vector2(-12, 0);
                var lt = lbl.GetComponent<Text>();
                lt.text = $"{i + 1}.  {meta.Title}  —  {meta.Artist}";
                lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lt.fontSize = 14; lt.fontStyle = FontStyle.Bold;
                lt.color = ColorPalette.TextDark;
                lt.alignment = TextAnchor.MiddleLeft;
                lt.raycastTarget = false;
                lbl.AddComponent<Outline>().effectColor = new Color(0, 0.1f, 0.3f, 0.2f);
                _trackListItems[i] = lt;
                ry += rowH + rowGap;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static GameObject MakeArt()
        {
            var go = new GameObject("Art", typeof(RectTransform), typeof(Image));
            go.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(
                120, 120, 18, ColorPalette.AeroCyan, new Color(0f, 0.4f, 0.9f), true);
            go.GetComponent<Image>().raycastTarget = false;

            var note = new GameObject("N", typeof(RectTransform), typeof(Text));
            note.transform.SetParent(go.transform, false);
            var nrt = (RectTransform)note.transform;
            nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            var nt = note.GetComponent<Text>();
            nt.text = "J"; nt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nt.fontSize = 64; nt.fontStyle = FontStyle.Bold;
            nt.color = Color.white.WithAlpha(0.7f);
            nt.alignment = TextAnchor.MiddleCenter; nt.raycastTarget = false;
            return go;
        }

        private static void Place(GameObject go, RectTransform parent, float w, float h, float x, float y)
        {
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static RectTransform MakeImage(Component parent, float x, float y, float w, float h,
            Color color,
            Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null)
        {
            var go = new GameObject("Img", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin ?? new Vector2(0.5f, 1f);
            rt.anchorMax = anchorMax ?? new Vector2(0.5f, 1f);
            rt.pivot = pivot ?? new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.sprite = null;
            img.raycastTarget = false;
            return rt;
        }

        private static Text MakeLabel(RectTransform parent, string text, int size, FontStyle style,
            Color color, float x, float y, float w, float h)
        {
            var go = new GameObject("T", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            var t = go.GetComponent<Text>();
            t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.fontStyle = style; t.color = color;
            t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
            return t;
        }

        private static void MakeTransportBtn(Transform parent, string lbl, float x, float w, float h,
            System.Action onClick)
        {
            var btn = aerisButton.Create(parent, lbl, new Vector2(w, h), onClick,
                ColorPalette.AeroCyan, new Color(0f, 0.38f, 0.82f), 17);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0);
        }

        private void Update()
        {
            var mp = MusicPlayer.Instance;
            if (mp == null) return;

            var meta = MusicPlayer.TrackMeta[mp.CurrentIndex];
            if (_trackTitle != null) _trackTitle.text = meta.Title;
            if (_trackArtist != null) _trackArtist.text = meta.Artist;
            if (_playBtnLabel != null) _playBtnLabel.text = mp.IsPlaying ? "II" : ">";

            if (_progressFill != null)
                _progressFill.sizeDelta = new Vector2(
                    Mathf.Max(4f, (_progressMaxW - 6f) * mp.Progress), -4f);

            if (_trackListItems != null)
                for (int i = 0; i < _trackListItems.Length; i++)
                    if (_trackListItems[i] != null)
                        _trackListItems[i].color = i == mp.CurrentIndex
                            ? ColorPalette.AeroCyan : ColorPalette.TextDark;
        }
    }

    internal class VolumeSliderBar : MonoBehaviour,
        UnityEngine.EventSystems.IPointerClickHandler,
        UnityEngine.EventSystems.IDragHandler
    {
        public float BarWidth = 380f;
        public RectTransform Fill;

        private void SetVolume(float screenX)
        {
            var rt = (RectTransform)transform;
            var canvas = rt.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, new Vector2(screenX, 0), canvas?.worldCamera, out var local);
            float t = Mathf.Clamp01((local.x + BarWidth * 0.5f) / BarWidth);
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Volume = t;
            if (Fill != null) Fill.sizeDelta = new Vector2(Mathf.Max(4, BarWidth * t - 4), Fill.sizeDelta.y);
        }

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e) => SetVolume(e.position.x);
        public void OnDrag(UnityEngine.EventSystems.PointerEventData e) => SetVolume(e.position.x);
    }

    internal class TrackRowButton : MonoBehaviour,
        UnityEngine.EventSystems.IPointerClickHandler,
        UnityEngine.EventSystems.IPointerEnterHandler,
        UnityEngine.EventSystems.IPointerExitHandler
    {
        public int Index;
        private Image _bg;
        private void Awake() => _bg = GetComponent<Image>();
        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) => _bg.color = new Color(0.7f, 0.95f, 1f, 1f);
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e) => _bg.color = Color.white;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e)
        {
            AudioManager.Instance?.PlayClick();
            MusicPlayer.Instance?.SetTrack(Index);
        }
    }
}
