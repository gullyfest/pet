using System.Collections;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Loading screen with a glossy progress bar that animates from 0 to 100 %
    /// over a configurable duration, then invokes a callback. Mostly cosmetic
    /// — there are no real assets to load — but it sells the OS-startup feel.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private Image _fill;
        private Text _label;
        private RectTransform _fillRect;
        private float _maxWidth;

        public void Build()
        {
            var rt = (RectTransform)transform;

            // Centered title.
            var title = new GameObject("LoadingTitle", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(rt, false);
            var trt = (RectTransform)title.transform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0, 80);
            trt.sizeDelta = new Vector2(800, 80);
            var tt = title.GetComponent<Text>();
            tt.text = "Loading aerisOS...";
            tt.alignment = TextAnchor.MiddleCenter;
            tt.color = ColorPalette.TextLight;
            tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tt.fontSize = 56;
            tt.fontStyle = FontStyle.Bold;
            tt.raycastTarget = false;
            var sh = title.AddComponent<Shadow>();
            sh.effectColor = ColorPalette.TextDark.WithAlpha(0.6f);
            sh.effectDistance = new Vector2(2, -2);

            // Bar background.
            var barBG = new GameObject("BarBG", typeof(RectTransform), typeof(Image));
            barBG.transform.SetParent(rt, false);
            var bgRT = (RectTransform)barBG.transform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0.5f, 0.5f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.anchoredPosition = new Vector2(0, -20);
            bgRT.sizeDelta = new Vector2(680, 36);
            barBG.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(680, 36, 16,
                Color.white.WithAlpha(0.4f), Color.white.WithAlpha(0.15f), false);
            barBG.GetComponent<Image>().raycastTarget = false;

            // Bar fill — sized via a child whose width we animate.
            var fillContainer = new GameObject("FillContainer", typeof(RectTransform));
            fillContainer.transform.SetParent(barBG.transform, false);
            var fcRT = (RectTransform)fillContainer.transform;
            fcRT.anchorMin = new Vector2(0, 0); fcRT.anchorMax = new Vector2(1, 1);
            fcRT.offsetMin = new Vector2(4, 4); fcRT.offsetMax = new Vector2(-4, -4);

            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(fillContainer.transform, false);
            _fillRect = (RectTransform)fillGO.transform;
            _fillRect.anchorMin = new Vector2(0, 0); _fillRect.anchorMax = new Vector2(0, 1);
            _fillRect.pivot = new Vector2(0, 0.5f);
            _fillRect.anchoredPosition = new Vector2(0, 0);
            _maxWidth = 672f;
            _fillRect.sizeDelta = new Vector2(0, 28);
            _fill = fillGO.GetComponent<Image>();
            _fill.sprite = TextureFactory.RoundedGlossy(672, 28, 12,
                ColorPalette.AccentLime, new Color(0.2f, 0.7f, 0.2f), true);
            _fill.raycastTarget = false;

            // Percent label.
            var labelGO = new GameObject("PercentLabel", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(rt, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = new Vector2(0, -70);
            lrt.sizeDelta = new Vector2(400, 30);
            _label = labelGO.GetComponent<Text>();
            _label.text = "0%";
            _label.alignment = TextAnchor.MiddleCenter;
            _label.color = ColorPalette.TextLight;
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize = 22;
            _label.fontStyle = FontStyle.Bold;
            _label.raycastTarget = false;
        }

        public void Run(float duration, System.Action onDone)
        {
            StopAllCoroutines();
            StartCoroutine(RunInternal(duration, onDone));
        }

        private IEnumerator RunInternal(float duration, System.Action onDone)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (_fillRect != null) _fillRect.sizeDelta = new Vector2(_maxWidth * k, _fillRect.sizeDelta.y);
                if (_label != null) _label.text = Mathf.RoundToInt(k * 100f) + "%";
                yield return null;
            }
            onDone?.Invoke();
        }
    }
}
