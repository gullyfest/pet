using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Fullscreen visual layer that introduces subtle glitch/noise as aeris
    /// corruption advances. Designed to stay atmospheric, not jumpscare-heavy.
    /// </summary>
    public class CorruptionOverlay : MonoBehaviour
    {
        private CanvasGroup _group;
        private Image _tint;
        private RectTransform _scanline;
        private float _strength;

        public void Build()
        {
            _group = gameObject.AddComponent<CanvasGroup>();

            var tintGO = new GameObject("Tint", typeof(RectTransform), typeof(Image));
            tintGO.transform.SetParent(transform, false);
            var trt = (RectTransform)tintGO.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            _tint = tintGO.GetComponent<Image>();
            _tint.sprite = TextureFactory.Solid(new Color(0.08f, 0.2f, 0.35f, 1f));
            _tint.color = new Color(1f, 1f, 1f, 0f);
            _tint.raycastTarget = false;

            var scanGO = new GameObject("Scanline", typeof(RectTransform), typeof(Image));
            scanGO.transform.SetParent(transform, false);
            _scanline = (RectTransform)scanGO.transform;
            _scanline.anchorMin = new Vector2(0, 1);
            _scanline.anchorMax = new Vector2(1, 1);
            _scanline.pivot = new Vector2(0.5f, 1f);
            _scanline.sizeDelta = new Vector2(0, 4);
            var sImg = scanGO.GetComponent<Image>();
            sImg.sprite = TextureFactory.Solid(new Color(0.55f, 1f, 1f, 1f));
            sImg.color = new Color(1f, 1f, 1f, 0f);
            sImg.raycastTarget = false;

            _group.alpha = 0f;

            if (aerisRuntime.Instance != null)
            {
                aerisRuntime.Instance.OnCorruptionChanged += OnCorruptionChanged;
            }
        }

        private void OnDestroy()
        {
            if (aerisRuntime.Instance != null)
            {
                aerisRuntime.Instance.OnCorruptionChanged -= OnCorruptionChanged;
            }
        }

        private void OnCorruptionChanged(float value, CorruptionStage _)
        {
            _strength = value;
        }

        private void Update()
        {
            if (_group == null || _tint == null || _scanline == null) return;

            _group.alpha = Mathf.Lerp(_group.alpha, _strength, Time.unscaledDeltaTime * 2f);

            float baseAlpha = Mathf.Lerp(0f, 0.3f, _strength);
            float pulse = Mathf.PerlinNoise(Time.unscaledTime * 3.5f, 0f) * 0.08f;
            _tint.color = new Color(1f, 1f, 1f, baseAlpha + pulse);

            float y = Mathf.Repeat(Time.unscaledTime * Mathf.Lerp(60f, 260f, _strength), Screen.height + 20f);
            _scanline.anchoredPosition = new Vector2(0f, -y);
            var s = _scanline.GetComponent<Image>();
            if (s != null)
            {
                s.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.02f, 0.23f, _strength));
            }

            if (_strength > 0.45f)
            {
                float jitter = (_strength - 0.45f) * 4f;
                transform.localPosition = new Vector3(
                    Random.Range(-jitter, jitter),
                    Random.Range(-jitter, jitter),
                    0f);
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }
    }
}
