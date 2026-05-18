using System;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class BSODScreen : MonoBehaviour
    {
        // ── Режим 1: полноэкранный синий фон (за диалогом) ───────────────────
        public static GameObject ShowBackground(RectTransform parent)
        {
            var go = new GameObject("BSODBackground", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0.55f, 1f);
            go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        // ── Режим 2: попап с ошибкой и кнопкой OK (поверх всего) ─────────────
        public static void ShowFatalErrorPopup(RectTransform parent, Action onOk)
        {
            var overlay = new GameObject("BSODPopupOverlay", typeof(RectTransform), typeof(CanvasGroup));
            overlay.transform.SetParent(parent, false);
            var ort = (RectTransform)overlay.transform;
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero;
            ort.offsetMax = Vector2.zero;
            overlay.transform.SetAsLastSibling();
            overlay.GetComponent<CanvasGroup>().blocksRaycasts = true;

            // ── Серое окно
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(overlay.transform, false);
            var crt = (RectTransform)card.transform;
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot     = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(580, 340);
            crt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = new Color(0.78f, 0.78f, 0.78f, 1f);

            // ── Синяя шапка (внутри карточки)
            var header = new GameObject("Header", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(card.transform, false);
            var hrt = (RectTransform)header.transform;
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot     = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(0f, 44f);
            hrt.anchoredPosition = Vector2.zero;
            header.GetComponent<Image>().color = new Color(0f, 0f, 0.55f, 1f);

            var hTxtGO = new GameObject("HTitle", typeof(RectTransform), typeof(Text));
            hTxtGO.transform.SetParent(header.transform, false);
            var hTxtRT = (RectTransform)hTxtGO.transform;
            hTxtRT.anchorMin = Vector2.zero; hTxtRT.anchorMax = Vector2.one;
            hTxtRT.offsetMin = new Vector2(12f, 0f); hTxtRT.offsetMax = new Vector2(-12f, 0f);
            var hTxt = hTxtGO.GetComponent<Text>();
            hTxt.text      = "FATAL SYSTEM ERROR";
            hTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.fontSize  = 18; hTxt.fontStyle = FontStyle.Bold;
            hTxt.color     = Color.white;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.raycastTarget = false;

            // ── Основной текст (от низа шапки до верха кнопки)
            const string msg =
                "STOP: 0x000000DEAD\n" +
                "(T.E.R.R.A._OVERWRITE_COMPLETE)\n\n" +
                "Device locked.\n" +
                "Directory encrypted.\n" +
                "You will like it here.";

            var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(Text));
            bodyGO.transform.SetParent(card.transform, false);
            var bodyRT = (RectTransform)bodyGO.transform;
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(20f, 62f);   // снизу 62 = кнопка + отступ
            bodyRT.offsetMax = new Vector2(-20f, -52f); // сверху 52 = шапка + отступ
            var bodyTxt = bodyGO.GetComponent<Text>();
            bodyTxt.text      = msg;
            bodyTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyTxt.fontSize  = 17;
            bodyTxt.color     = new Color(0.06f, 0.06f, 0.06f, 1f);
            bodyTxt.alignment = TextAnchor.MiddleCenter;
            bodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            bodyTxt.raycastTarget      = false;

            // ── Кнопка OK
            var btn = new GameObject("OK", typeof(RectTransform), typeof(Image), typeof(Button));
            btn.transform.SetParent(card.transform, false);
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = new Vector2(0.5f, 0f);
            brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot     = new Vector2(0.5f, 0f);
            brt.sizeDelta = new Vector2(100f, 36f);
            brt.anchoredPosition = new Vector2(0f, 18f);
            btn.GetComponent<Image>().color = new Color(0.62f, 0.62f, 0.62f, 1f);

            var btnLblGO = new GameObject("Lbl", typeof(RectTransform), typeof(Text));
            btnLblGO.transform.SetParent(btn.transform, false);
            var blRT = (RectTransform)btnLblGO.transform;
            blRT.anchorMin = Vector2.zero; blRT.anchorMax = Vector2.one;
            blRT.offsetMin = Vector2.zero; blRT.offsetMax = Vector2.zero;
            var blTxt = btnLblGO.GetComponent<Text>();
            blTxt.text      = "OK";
            blTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            blTxt.fontSize  = 16; blTxt.color = Color.black;
            blTxt.alignment = TextAnchor.MiddleCenter;
            blTxt.raycastTarget = false;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Destroy(overlay);
                onOk?.Invoke();
            });
        }

        private static void AddLabel(Transform parent, string text, int size, FontStyle style,
            Color color, Vector2 offsetMin, Vector2 offsetMax, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("Lbl", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.fontStyle = style; t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
        }
    }
}
