using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Desktop icon — clicking once selects (highlight), double-click opens the
    /// associated app. Uses a procedural square glossy icon plus a label below.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DesktopIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public AppType App;
        private Image _highlight;

        public static DesktopIcon Create(Transform parent, AppType app, string label, Color tint, char glyph, Sprite sprite = null)
        {
            var go = new GameObject($"Icon_{label}", typeof(RectTransform), typeof(DesktopIcon));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(96, 110);

            var icon = go.GetComponent<DesktopIcon>();
            icon.App = app;

            // Selection highlight (hidden by default).
            var hl = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            hl.transform.SetParent(go.transform, false);
            var hlRT = (RectTransform)hl.transform;
            hlRT.anchorMin = Vector2.zero; hlRT.anchorMax = Vector2.one;
            hlRT.offsetMin = Vector2.zero; hlRT.offsetMax = Vector2.zero;
            var hlImg = hl.GetComponent<Image>();
            hlImg.sprite = TextureFactory.RoundedGlossy(96, 110, 12,
                Color.white.WithAlpha(0.4f), Color.white.WithAlpha(0.1f), false);
            // Must be true — the highlight covers the whole icon footprint and
            // is the only Graphic on this GO, so it has to receive raycasts
            // for IPointerClickHandler to fire.
            hlImg.raycastTarget = true;
            icon._highlight = hlImg;
            hlImg.color = new Color(1, 1, 1, 0.001f); // alpha 0 still raycasts; keep faint to be safe

            // Glossy app icon (top half).
            var imgGO = new GameObject("Img", typeof(RectTransform), typeof(Image));
            imgGO.transform.SetParent(go.transform, false);
            var iRT = (RectTransform)imgGO.transform;
            iRT.anchorMin = iRT.anchorMax = new Vector2(0.5f, 1);
            iRT.pivot = new Vector2(0.5f, 1);
            iRT.sizeDelta = new Vector2(64, 64);
            iRT.anchoredPosition = new Vector2(0, -8);
            imgGO.GetComponent<Image>().sprite = sprite != null ? sprite : TextureFactory.AppIcon(128, tint, glyph);
            imgGO.GetComponent<Image>().raycastTarget = false;

            // Label. Outline gives a 4-sided dark border around white text so
            // it stays readable on top of the bright sky gradient.
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblGO.transform.SetParent(go.transform, false);
            var lRT = (RectTransform)lblGO.transform;
            lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(1, 0);
            lRT.pivot = new Vector2(0.5f, 0);
            lRT.sizeDelta = new Vector2(8, 34);
            lRT.anchoredPosition = new Vector2(0, 2);
            var t = lblGO.GetComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.UpperCenter;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 18;
            t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = lblGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.04f, 0.15f, 0.3f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            return icon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount >= 2)
            {
                AudioManager.Instance?.PlayClick();
                WindowManager.Instance?.OpenWindow(App);
            }
            else
            {
                AudioManager.Instance?.PlayClick();
            }
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (_highlight != null) _highlight.color = new Color(1, 1, 1, 0.35f);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_highlight != null) _highlight.color = new Color(1, 1, 1, 0);
        }
    }
}
