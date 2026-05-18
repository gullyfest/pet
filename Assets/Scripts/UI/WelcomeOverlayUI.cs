using aerisOS.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class WelcomeOverlayUI : MonoBehaviour
    {
        private CanvasGroup _group;
        private Action _onStart;

        public void Build(RectTransform parent, Action onStart = null)
        {
            _onStart = onStart;
            var root = new GameObject("WelcomeOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var dim = root.GetComponent<Image>();
            dim.sprite = TextureFactory.Solid(new Color(0f, 0f, 0f, 1f));
            dim.color = new Color(0.03f, 0.08f, 0.13f, 0.45f);
            dim.raycastTarget = true;

            _group = root.GetComponent<CanvasGroup>();
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;

            var card = new GameObject("WelcomeCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false);
            var crt = (RectTransform)card.transform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(680f, 380f);
            crt.anchoredPosition = new Vector2(0f, 24f);
            var cimg = card.GetComponent<Image>();
            cimg.sprite = TextureFactory.RoundedGlossy(700, 400, 18,
                new Color(0.86f, 0.96f, 1f, 0.97f),
                new Color(0.72f, 0.9f, 1f, 0.93f), false);
            cimg.raycastTarget = true;

            CreateText(card.transform, "Welcome to aerisOS", 42, FontStyle.Bold,
                ColorPalette.TextDark, TextAnchor.UpperCenter, new Vector2(0f, -30f), new Vector2(620f, 56f));

            CreateText(card.transform,
                "Hi! I am Terra Nova.\n\n" +
                "Clean infected modules in Antivirus,\n" +
                "follow hints in the bottom-right corner,\n" +
                "and open Archive when you discover new virus signatures.",
                22, FontStyle.Normal, ColorPalette.TextDark.WithAlpha(0.94f),
                TextAnchor.UpperCenter, new Vector2(0f, -108f), new Vector2(620f, 176f));

            CreateText(card.transform,
                "Press Start to begin.",
                18, FontStyle.Italic, ColorPalette.TextDark.WithAlpha(0.72f),
                TextAnchor.UpperCenter, new Vector2(0f, -274f), new Vector2(540f, 32f));

            var startButton = aerisButton.Create(card.transform, "Start", new Vector2(250f, 62f), Hide,
                new Color(0.23f, 0.82f, 0.56f), new Color(0.08f, 0.58f, 0.38f));
            var srt = (RectTransform)startButton.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(0f, 22f);
        }

        private static Text CreateText(Transform parent, string value, int size, FontStyle style,
            Color color, TextAnchor align, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.GetComponent<Text>();
            t.text = value;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private void Hide()
        {
            _onStart?.Invoke();
            if (_group != null)
            {
                _group.blocksRaycasts = false;
                _group.interactable = false;
            }
            Destroy(gameObject);
        }
    }
}
