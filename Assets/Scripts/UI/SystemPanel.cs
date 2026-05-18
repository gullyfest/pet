using System;
using System.Globalization;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Bottom taskbar with the Start indicator on the left, a weather widget
    /// in the middle, and a tray + clock on the right. The clock updates each
    /// frame from DateTime.Now.
    /// </summary>
    public class SystemPanel : MonoBehaviour
    {
        private Text _clock;
        private Text _date;
        private float _clockTimer;
        public RectTransform TasksArea { get; private set; }
        private Canvas _rootCanvas;

        public void Build(Transform parent)
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas == null) _rootCanvas = FindAnyObjectByType<Canvas>();
            var bar = new GameObject("Taskbar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(parent, false);
            var brt = (RectTransform)bar.transform;
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.sizeDelta = new Vector2(0, 64);
            brt.anchoredPosition = Vector2.zero;
            bar.GetComponent<Image>().sprite = TextureFactory.VerticalGradient(
                64, 64, ColorPalette.TaskbarStart, ColorPalette.TaskbarEnd);

            // Top highlight strip — aeris gloss line above the bar.
            var strip = new GameObject("GlossStrip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(bar.transform, false);
            var srt = (RectTransform)strip.transform;
            srt.anchorMin = new Vector2(0, 1);
            srt.anchorMax = new Vector2(1, 1);
            srt.pivot = new Vector2(0.5f, 1);
            srt.sizeDelta = new Vector2(0, 6);
            srt.anchoredPosition = Vector2.zero;
            strip.GetComponent<Image>().color = ColorPalette.GlossWhite;
            strip.GetComponent<Image>().raycastTarget = false;

            // Start orb — clickable, opens PowerMenu.
            // Pivot at CENTER (0.5, 0.5) so scale-on-hover grows in all directions equally.
            var orb = new GameObject("StartOrb", typeof(RectTransform), typeof(Image), typeof(OrbButton));
            orb.transform.SetParent(bar.transform, false);
            var ort = (RectTransform)orb.transform;
            ort.anchorMin = new Vector2(0, 0.5f); ort.anchorMax = new Vector2(0, 0.5f);
            ort.pivot = new Vector2(0.5f, 0.5f);   // ← centre pivot prevents side-skew on scale
            ort.anchoredPosition = new Vector2(40, 0); // centre of the 48px orb from left=13
            ort.sizeDelta = new Vector2(48, 48);
            var orbImg = orb.GetComponent<Image>();
            orbImg.sprite = TextureFactory.RoundedGlossy(
                48, 48, 24,
                new Color(0.35f, 1f, 0.55f), new Color(0.05f, 0.65f, 0.25f), true);
            orbImg.raycastTarget = true;
            var orbBtn = orb.GetComponent<OrbButton>();
            orbBtn.Panel = this;

            // Gloss highlight inside the orb (non-interactive).
            var orbGloss = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            orbGloss.transform.SetParent(orb.transform, false);
            var ogrt = (RectTransform)orbGloss.transform;
            ogrt.anchorMin = ogrt.anchorMax = new Vector2(0.5f, 0.70f);
            ogrt.pivot = new Vector2(0.5f, 0.5f);
            ogrt.sizeDelta = new Vector2(28, 13);
            orbGloss.GetComponent<Image>().sprite = TextureFactory.Circle(32, new Color(1f, 1f, 1f, 0.65f));
            orbGloss.GetComponent<Image>().raycastTarget = false;

            // Tasks area — horizontal strip just right of the start orb where
            // open windows get a button so they can be restored when minimized.
            var tasks = new GameObject("Tasks", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tasks.transform.SetParent(bar.transform, false);
            var tRT = (RectTransform)tasks.transform;
            tRT.anchorMin = new Vector2(0, 0); tRT.anchorMax = new Vector2(0, 1);
            tRT.pivot = new Vector2(0, 0.5f);
            tRT.anchoredPosition = new Vector2(72, 0);
            tRT.sizeDelta = new Vector2(700, 0);
            var hl = tasks.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 6;
            hl.padding = new RectOffset(0, 0, 8, 8);
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = false;
            hl.childControlHeight = false;
            TasksArea = tRT;

            // Weather widget (placeholder: sun + temperature text).
            var weather = new GameObject("Weather", typeof(RectTransform));
            weather.transform.SetParent(bar.transform, false);
            var wrt = (RectTransform)weather.transform;
            wrt.anchorMin = wrt.anchorMax = new Vector2(0.5f, 0.5f);
            wrt.pivot = new Vector2(0.5f, 0.5f);
            wrt.anchoredPosition = Vector2.zero;
            wrt.sizeDelta = new Vector2(220, 48);

            var sun = new GameObject("Sun", typeof(RectTransform), typeof(Image));
            sun.transform.SetParent(weather.transform, false);
            var sunRT = (RectTransform)sun.transform;
            sunRT.anchorMin = sunRT.anchorMax = new Vector2(0, 0.5f);
            sunRT.pivot = new Vector2(0, 0.5f);
            sunRT.sizeDelta = new Vector2(40, 40);
            sunRT.anchoredPosition = new Vector2(0, 0);
            sun.GetComponent<Image>().sprite = TextureFactory.Circle(40, new Color(1f, 0.85f, 0.2f, 1f));
            sun.GetComponent<Image>().raycastTarget = false;

            var weatherText = new GameObject("Text", typeof(RectTransform), typeof(Text));
            weatherText.transform.SetParent(weather.transform, false);
            var wt = (RectTransform)weatherText.transform;
            wt.anchorMin = new Vector2(0, 0); wt.anchorMax = new Vector2(1, 1);
            wt.offsetMin = new Vector2(50, 0); wt.offsetMax = new Vector2(0, 0);
            var weatherLabel = weatherText.GetComponent<Text>();
            weatherLabel.text = "Sunny  +24°C";
            weatherLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            weatherLabel.fontSize = 20;
            weatherLabel.fontStyle = FontStyle.Bold;
            weatherLabel.color = ColorPalette.TextLight;
            weatherLabel.alignment = TextAnchor.MiddleLeft;
            weatherLabel.raycastTarget = false;

            // Tray dots.
            for (int i = 0; i < 3; i++)
            {
                var dot = new GameObject($"TrayDot_{i}", typeof(RectTransform), typeof(Image));
                dot.transform.SetParent(bar.transform, false);
                var drt = (RectTransform)dot.transform;
                drt.anchorMin = drt.anchorMax = new Vector2(1, 0.5f);
                drt.pivot = new Vector2(1, 0.5f);
                drt.sizeDelta = new Vector2(14, 14);
                drt.anchoredPosition = new Vector2(-200 - i * 24, 0);
                Color[] tints = { ColorPalette.AccentLime, ColorPalette.AccentPink, ColorPalette.AeroCyan };
                dot.GetComponent<Image>().sprite = TextureFactory.Circle(20, tints[i]);
                dot.GetComponent<Image>().raycastTarget = false;
            }

            // Clock.
            var clock = new GameObject("Clock", typeof(RectTransform), typeof(Text));
            clock.transform.SetParent(bar.transform, false);
            var crt = (RectTransform)clock.transform;
            crt.anchorMin = crt.anchorMax = new Vector2(1, 0.5f);
            crt.pivot = new Vector2(1, 0.5f);
            crt.anchoredPosition = new Vector2(-16, 8);
            crt.sizeDelta = new Vector2(140, 28);
            _clock = clock.GetComponent<Text>();
            _clock.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _clock.fontSize = 24;
            _clock.fontStyle = FontStyle.Bold;
            _clock.color = ColorPalette.TextLight;
            _clock.alignment = TextAnchor.MiddleRight;
            _clock.raycastTarget = false;

            var dateGO = new GameObject("Date", typeof(RectTransform), typeof(Text));
            dateGO.transform.SetParent(bar.transform, false);
            var drT = (RectTransform)dateGO.transform;
            drT.anchorMin = drT.anchorMax = new Vector2(1, 0.5f);
            drT.pivot = new Vector2(1, 0.5f);
            drT.anchoredPosition = new Vector2(-16, -14);
            drT.sizeDelta = new Vector2(160, 20);
            _date = dateGO.GetComponent<Text>();
            _date.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _date.fontSize = 14;
            _date.color = ColorPalette.TextLight.WithAlpha(0.85f);
            _date.alignment = TextAnchor.MiddleRight;
            _date.raycastTarget = false;
        }

        private void Update()
        {
            // Update clock at 500 ms intervals instead of every frame.
            _clockTimer -= Time.unscaledDeltaTime;
            if (_clockTimer > 0f) return;
            _clockTimer = 0.5f;
            var now = DateTime.Now;
            if (_clock != null) _clock.text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            if (_date != null) _date.text = now.ToString("ddd, dd MMM", CultureInfo.InvariantCulture);
        }

        public Canvas GetRootCanvas() => _rootCanvas;
    }

    /// <summary>Handles clicks on the Start orb to toggle the PowerMenu.</summary>
    internal class OrbButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public SystemPanel Panel;
        private Image _img;

        private void Awake() => _img = GetComponent<Image>();

        public void OnPointerEnter(PointerEventData e)
        {
            if (_img) _img.color = new Color(0.85f, 1f, 0.85f, 1f);
            transform.localScale = Vector3.one * 1.08f;
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_img) _img.color = Color.white;
            transform.localScale = Vector3.one;
        }

        public void OnPointerClick(PointerEventData e)
        {
            aerisOS.Managers.AudioManager.Instance?.PlayClick();
            var canvas = Panel != null ? Panel.GetRootCanvas() : FindAnyObjectByType<Canvas>();
            PowerMenu.Toggle(canvas, transform.position);
        }
    }
}
