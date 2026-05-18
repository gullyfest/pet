using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Glossy aeris Aero-style button with hover scale-up, color tint shift
    /// on press, and a click sound. Builds its own visuals — caller only sets
    /// the label and the click handler.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class aerisButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Color TopColor    = ColorPalette.AeroCyan;
        public Color BottomColor = new Color(0f, 0.45f, 0.9f, 1f);
        public string Label      = "Button";
        public System.Action OnClick;

        private Image _bg;
        private Text _text;
        private bool _isPressed;
        private Vector3 _baseScale;

        public static aerisButton Create(Transform parent, string label, Vector2 size, System.Action onClick,
            Color? topColor = null, Color? bottomColor = null, int fontSize = 28)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(aerisButton));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            var btn = go.GetComponent<aerisButton>();
            btn.Label = label;
            btn.OnClick = onClick;
            if (topColor.HasValue) btn.TopColor = topColor.Value;
            if (bottomColor.HasValue) btn.BottomColor = bottomColor.Value;

            btn._bg = go.GetComponent<Image>();
            btn._bg.sprite = TextureFactory.RoundedGlossy(
                Mathf.Max(64, (int)size.x),
                Mathf.Max(40, (int)size.y),
                Mathf.Min(32, (int)size.y / 3),
                btn.TopColor, btn.BottomColor, true);
            btn._bg.type = Image.Type.Simple;

            // Label child.
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var textRT = (RectTransform)textGO.transform;
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            btn._text = textGO.GetComponent<Text>();
            btn._text.text = label;
            btn._text.alignment = TextAnchor.MiddleCenter;
            btn._text.color = ColorPalette.TextLight;
            btn._text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btn._text.fontSize = fontSize;
            btn._text.fontStyle = FontStyle.Bold;
            btn._text.raycastTarget = false;
            // Soft drop shadow effect for readability against glassy bg.
            var shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = ColorPalette.ShadowSoft;
            shadow.effectDistance = new Vector2(1f, -1f);

            btn._baseScale = go.transform.localScale;
            return btn;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isPressed) StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale * 1.05f, 0.12f));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale, 0.12f));
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale * 0.96f, 0.07f));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;
            StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale * 1.05f, 0.07f));
            AudioManager.Instance?.PlayClick();
            OnClick?.Invoke();
        }

        private System.Collections.IEnumerator LerpScale(Vector3 target, float dur)
        {
            Vector3 from = transform.localScale;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(from, target, t / dur);
                yield return null;
            }
            transform.localScale = target;
        }
    }
}
