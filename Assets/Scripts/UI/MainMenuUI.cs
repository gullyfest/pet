using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// aeris Aero main menu: title, glassy bubble, three primary buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        public void Build()
        {
            var rt = (RectTransform)transform;

            // Big glossy "bubble" centered on screen — the visual anchor.
            var bubble = new GameObject("Bubble", typeof(RectTransform), typeof(Image));
            bubble.transform.SetParent(rt, false);
            var brt = (RectTransform)bubble.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(600, 600);
            brt.anchoredPosition = new Vector2(420, 0);
            bubble.GetComponent<Image>().sprite = TextureFactory.Circle(512, ColorPalette.AeroCyan.WithAlpha(0.35f));
            bubble.GetComponent<Image>().raycastTarget = false;

            // Inner gloss bubble.
            var bubble2 = new GameObject("BubbleGloss", typeof(RectTransform), typeof(Image));
            bubble2.transform.SetParent(bubble.transform, false);
            var b2rt = (RectTransform)bubble2.transform;
            b2rt.anchorMin = b2rt.anchorMax = new Vector2(0.5f, 0.5f);
            b2rt.pivot = new Vector2(0.5f, 0.5f);
            b2rt.sizeDelta = new Vector2(360, 360);
            b2rt.anchoredPosition = new Vector2(-40, 80);
            bubble2.GetComponent<Image>().sprite = TextureFactory.Circle(360, ColorPalette.GlossWhite);
            bubble2.GetComponent<Image>().raycastTarget = false;

            // Title.
            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(rt, false);
            var trt = (RectTransform)title.transform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(0, 1);
            trt.pivot = new Vector2(0, 1);
            trt.anchoredPosition = new Vector2(120, -120);
            trt.sizeDelta = new Vector2(900, 110);
            var titleText = title.GetComponent<Text>();
            titleText.text = "aeris OS";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 86;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = ColorPalette.TextLight;
            titleText.alignment = TextAnchor.UpperLeft;
            titleText.raycastTarget = false;
            var sh = title.AddComponent<Shadow>();
            sh.effectColor = ColorPalette.TextDark.WithAlpha(0.6f);
            sh.effectDistance = new Vector2(2, -3);

            // Subtitle.
            var subtitle = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
            subtitle.transform.SetParent(rt, false);
            var srt = (RectTransform)subtitle.transform;
            srt.anchorMin = new Vector2(0, 1);
            srt.anchorMax = new Vector2(0, 1);
            srt.pivot = new Vector2(0, 1);
            srt.anchoredPosition = new Vector2(124, -210);
            srt.sizeDelta = new Vector2(900, 40);
            var sub = subtitle.GetComponent<Text>();
            sub.text = "A beautiful future still running after everyone left.";
            sub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sub.fontSize = 22;
            sub.fontStyle = FontStyle.Italic;
            sub.color = ColorPalette.TextDark.WithAlpha(0.85f);
            sub.alignment = TextAnchor.UpperLeft;
            sub.raycastTarget = false;

            // Buttons column.
            var col = new GameObject("Buttons", typeof(RectTransform));
            col.transform.SetParent(rt, false);
            var crt = (RectTransform)col.transform;
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(0, 0);
            crt.pivot = new Vector2(0, 0);
            crt.anchoredPosition = new Vector2(140, 240);
            crt.sizeDelta = new Vector2(360, 360);

            CreateMenuButton(crt, "Start",  0,  OnStart);
            CreateMenuButton(crt, "Exit",   1,  OnExit);

            MusicPlayer.Instance?.RequestPlay();
        }

        private void CreateMenuButton(Transform parent, string label, int index, System.Action onClick)
        {
            var btn = aerisButton.Create(parent, label, new Vector2(360, 80), onClick);
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = new Vector2(0, 1);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(0, 1);
            brt.anchoredPosition = new Vector2(0, -index * 100f);
        }

        private void OnStart()
        {
            MusicPlayer.Instance?.Stop();
            SceneFlowManager.Instance?.GoTo(SceneFlowManager.Screen.Loading, 0.35f, () =>
            {
                var ls = FindAnyObjectByType<LoadingScreen>();
                ls?.Run(2.5f, () =>
                {
                    SceneFlowManager.Instance?.GoTo(SceneFlowManager.Screen.Desktop, 0.35f, () =>
                    {
                        aerisRuntime.Instance?.ActivateAfterBoot();
                    });
                });
            });
        }

        private void OnSettings()
        {
            // Settings is a window opened on the desktop. Jump straight there.
            SceneFlowManager.Instance?.GoTo(SceneFlowManager.Screen.Loading, 0.35f, () =>
            {
                var ls = FindAnyObjectByType<LoadingScreen>();
                ls?.Run(1.2f, () =>
                {
                    SceneFlowManager.Instance?.GoTo(SceneFlowManager.Screen.Desktop, 0.35f, () =>
                    {
                        aerisRuntime.Instance?.ActivateAfterBoot();
                        WindowManager.Instance?.OpenWindow(AppType.Settings);
                    });
                });
            });
        }

        private void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
