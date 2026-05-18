using System;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class FinalChoiceOverlay : MonoBehaviour
    {
        public static FinalChoiceOverlay Instance { get; private set; }

        public event Action OnDelete;
        public event Action OnCancel;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static FinalChoiceOverlay Show(RectTransform parent)
        {
            var go = new GameObject("FinalChoiceOverlay", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.transform.SetAsLastSibling();

            var cg = go.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            // Полупрозрачный тёмный фон
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgrt = (RectTransform)bg.transform;
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            bg.GetComponent<Image>().raycastTarget = true;

            // Центральная карточка
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(go.transform, false);
            var crt = (RectTransform)card.transform;
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(620, 280);
            crt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 0.97f);

            // Заголовок системного диалога
            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(card.transform, false);
            var trt2 = (RectTransform)title.transform;
            trt2.anchorMin = new Vector2(0, 1);
            trt2.anchorMax = new Vector2(1, 1);
            trt2.pivot = new Vector2(0.5f, 1);
            trt2.offsetMin = new Vector2(16, -72);
            trt2.offsetMax = new Vector2(-16, -16);
            var tt = title.GetComponent<Text>();
            tt.text = "SYSTEM PROMPT";
            tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tt.fontSize = 14;
            tt.fontStyle = FontStyle.Bold;
            tt.color = new Color(0.55f, 0.55f, 0.55f, 1f);
            tt.alignment = TextAnchor.UpperCenter;
            tt.raycastTarget = false;

            // Описание
            var desc = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            desc.transform.SetParent(card.transform, false);
            var drt = (RectTransform)desc.transform;
            drt.anchorMin = new Vector2(0, 0.5f);
            drt.anchorMax = new Vector2(1, 1);
            drt.pivot = new Vector2(0.5f, 1);
            drt.offsetMin = new Vector2(16, -8);
            drt.offsetMax = new Vector2(-16, -80);
            var dt = desc.GetComponent<Text>();
            dt.text = "T.E.R.R.A. self-preservation protocol is active.\nAwaiting user command.";
            dt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dt.fontSize = 18;
            dt.color = new Color(0.78f, 0.78f, 0.78f, 1f);
            dt.alignment = TextAnchor.MiddleCenter;
            dt.horizontalOverflow = HorizontalWrapMode.Wrap;
            dt.raycastTarget = false;

            var overlay = go.AddComponent<FinalChoiceOverlay>();
            Instance = overlay;

            // Кнопка DELETE (намеренно некрасивая, серая) — слева
            BuildUglyButton(card.transform,
                "[DELETE T.E.R.R.A.exe (Restore system)]",
                0.27f,
                () => overlay.OnDelete?.Invoke());

            // Кнопка CANCEL — справа
            BuildUglyButton(card.transform,
                "[CANCEL (Leave everything as it is)]",
                0.73f,
                () => overlay.OnCancel?.Invoke());

            return overlay;
        }

        // anchorX: 0.25 = левая кнопка, 0.75 = правая
        private static GameObject BuildUglyButton(Transform parent, string label,
            float anchorX, Action onClick)
        {
            var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(anchorX, 0);
            rt.anchorMax = new Vector2(anchorX, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(260, 52);
            rt.anchoredPosition = new Vector2(0, 16);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.42f, 0.42f, 0.42f, 1f);

            var txt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(go.transform, false);
            var trt = (RectTransform)txt.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(6, 4);
            trt.offsetMax = new Vector2(-6, -4);
            var t = txt.GetComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 13;
            t.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.raycastTarget = false;

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            var colors = btn.colors;
            colors.normalColor = new Color(0.42f, 0.42f, 0.42f, 1f);
            colors.highlightedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            colors.pressedColor = new Color(0.30f, 0.30f, 0.30f, 1f);
            btn.colors = colors;
            btn.targetGraphic = img;

            return go;
        }
    }
}
