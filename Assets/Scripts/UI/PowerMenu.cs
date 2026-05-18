using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Small popup above the Start orb with Shut Down and Restart options.
    /// Clicking outside the panel dismisses it.
    /// </summary>
    public class PowerMenu : MonoBehaviour
    {
        private static PowerMenu _instance;

        /// <summary>Toggle: first call shows the menu, second call hides it.</summary>
        public static void Toggle(Canvas rootCanvas, Vector3 orbWorldPos)
        {
            if (_instance != null) { Destroy(_instance.gameObject); return; }

            // ── Full-screen dismiss overlay (behind the card) ─────────────
            var overlayGO = new GameObject("PowerMenuOverlay",
                typeof(RectTransform), typeof(Image), typeof(PowerMenu));
            overlayGO.transform.SetParent(rootCanvas.transform, false);
            var overlay = (RectTransform)overlayGO.transform;
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.GetComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.001f); // nearly invisible, just catches clicks
            overlayImg.raycastTarget = true;
            _instance = overlayGO.GetComponent<PowerMenu>();

            // ── Card ──────────────────────────────────────────────────────
            var cardGO = new GameObject("PowerCard", typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(overlayGO.transform, false);
            var cardRT = (RectTransform)cardGO.transform;
            cardRT.anchorMin = new Vector2(0, 0);
            cardRT.anchorMax = new Vector2(0, 0);
            cardRT.pivot = new Vector2(0, 0);
            // Position just above the taskbar (64 px) on the left side.
            cardRT.anchoredPosition = new Vector2(12, 68);
            cardRT.sizeDelta = new Vector2(230, 148);

            var cardImg = cardGO.GetComponent<Image>();
            cardImg.sprite = TextureFactory.RoundedGlossy(230, 148, 16,
                ColorPalette.TaskbarStart,
                ColorPalette.TaskbarEnd, true);

            // Title row
            var hdr = new GameObject("Header", typeof(RectTransform), typeof(Text));
            hdr.transform.SetParent(cardGO.transform, false);
            var hrt = (RectTransform)hdr.transform;
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.anchoredPosition = new Vector2(0, -10);
            hrt.sizeDelta = new Vector2(0, 26);
            var ht = hdr.GetComponent<Text>();
            ht.text = "Power Options";
            ht.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ht.fontSize = 17;
            ht.fontStyle = FontStyle.Bold;
            ht.color = Color.white;
            ht.alignment = TextAnchor.MiddleCenter;
            ht.raycastTarget = false;
            var ho = hdr.AddComponent<Outline>();
            ho.effectColor = new Color(0, 0.1f, 0.25f, 1f);
            ho.effectDistance = new Vector2(1f, -1f);

            // Separator line
            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(cardGO.transform, false);
            var srt = (RectTransform)sep.transform;
            srt.anchorMin = new Vector2(0.05f, 1); srt.anchorMax = new Vector2(0.95f, 1);
            srt.pivot = new Vector2(0.5f, 1);
            srt.anchoredPosition = new Vector2(0, -38);
            srt.sizeDelta = new Vector2(0, 2);
            sep.GetComponent<Image>().color = Color.white.WithAlpha(0.3f);

            // Shut Down button
            var shutdown = aerisButton.Create(cardGO.transform, "Shut Down", new Vector2(200, 44),
                DoShutdown, new Color(0.75f, 0.15f, 0.05f), new Color(0.45f, 0.08f, 0.05f));
            var sr1 = (RectTransform)shutdown.transform;
            sr1.anchorMin = sr1.anchorMax = new Vector2(0.5f, 1);
            sr1.pivot = new Vector2(0.5f, 1);
            sr1.anchoredPosition = new Vector2(0, -46);

            // Restart button
            var restart = aerisButton.Create(cardGO.transform, "Restart", new Vector2(200, 44),
                DoRestart, ColorPalette.AeroCyan, new Color(0f, 0.35f, 0.75f));
            var sr2 = (RectTransform)restart.transform;
            sr2.anchorMin = sr2.anchorMax = new Vector2(0.5f, 1);
            sr2.pivot = new Vector2(0.5f, 1);
            sr2.anchoredPosition = new Vector2(0, -96);
        }

        // Clicking the overlay (outside the card) closes the menu.
        private void OnMouseDown() { }

        private void OnEnable() { }

        // IPointerClickHandler on the overlay itself.
        private void Start()
        {
            var img = GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            var go = gameObject.AddComponent<OverlayClickClose>();
            go.Target = this;
        }

        private static void DoShutdown()
        {
            AudioManager.Instance?.PlayClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void DoRestart()
        {
            AudioManager.Instance?.PlaySuccess();
            if (_instance != null) Destroy(_instance.gameObject);
            var r = new GameObject("[RestartEmulator]");
            r.AddComponent<RestartEmulator>();
        }
    }

    // Clicks on the transparent overlay (but NOT on card children) dismiss the menu.
    internal class OverlayClickClose : MonoBehaviour, IPointerClickHandler
    {
        public PowerMenu Target;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (Target != null) Destroy(Target.gameObject);
        }
    }

    /// <summary>
    /// Fake restart: blue BSOD-style screen for 5 s, then closes all windows and
    /// restores music. No actual scene reload.
    /// </summary>
    internal class RestartEmulator : MonoBehaviour
    {
        private void Start() => StartCoroutine(Run());

        private System.Collections.IEnumerator Run()
        {
            // Mute music immediately.
            var mp = Managers.MusicPlayer.Instance;
            float savedVol = mp != null ? mp.Volume : 0f;
            if (mp != null) mp.Volume = 0f;

            // Build blue fullscreen overlay on top of everything.
            var canvas = FindAnyObjectByType<Canvas>();
            GameObject overlay = null;
            Text dotText = null;
            if (canvas != null)
            {
                overlay = new GameObject("RestartOverlay", typeof(RectTransform), typeof(Canvas),
                    typeof(GraphicRaycaster));
                // Use a child canvas so it sorts above everything.
                var oc = overlay.GetComponent<Canvas>();
                oc.overrideSorting = true;
                oc.sortingOrder = 9999;
                overlay.transform.SetParent(canvas.transform, false);
                var rt = (RectTransform)overlay.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                // Blue background.
                var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(overlay.transform, false);
                var brt = (RectTransform)bg.transform;
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
                brt.offsetMin = brt.offsetMax = Vector2.zero;
                bg.GetComponent<Image>().color = new Color(0.0f, 0.18f, 0.56f, 1f); // classic BSOD blue

                // "Restarting" big label.
                AddOverlayText(overlay.transform, "Restarting...", 52, FontStyle.Bold,
                    Color.white, new Vector2(0, 60), new Vector2(800, 70), TextAnchor.MiddleCenter);

                // Subtitle.
                AddOverlayText(overlay.transform, "Please wait while the system restarts.", 22, FontStyle.Normal,
                    new Color(0.75f, 0.88f, 1f, 1f), new Vector2(0, -10), new Vector2(700, 32), TextAnchor.MiddleCenter);

                // Animated dots label — updated in loop below.
                dotText = AddOverlayText(overlay.transform, "", 28, FontStyle.Bold,
                    new Color(0.55f, 0.82f, 1f, 1f), new Vector2(0, -60), new Vector2(400, 36), TextAnchor.MiddleCenter);
            }

            // Animate dots for 5 seconds.
            string[] dots = { ".", ". .", ". . ." };
            float elapsed = 0f;
            float totalDur = 5f;
            int dotIdx = 0;
            float dotTimer = 0f;
            while (elapsed < totalDur)
            {
                elapsed += Time.unscaledDeltaTime;
                dotTimer += Time.unscaledDeltaTime;
                if (dotTimer > 0.55f)
                {
                    dotTimer = 0f;
                    dotIdx = (dotIdx + 1) % dots.Length;
                    if (dotText != null) dotText.text = dots[dotIdx];
                }
                yield return null;
            }

            // Close every open window via Close() so OnClosed fires and
            // WindowManager cleans up both the window and its taskbar entry.
            foreach (var win in FindObjectsByType<DraggableWindow>())
                if (win != null) win.Close();

            // Wait for close animations to finish (0.18 s each).
            yield return new WaitForSecondsRealtime(0.25f);

            // Fade out overlay quickly.
            if (overlay != null)
            {
                var bgImg = overlay.transform.Find("BG")?.GetComponent<Image>();
                for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
                {
                    if (bgImg != null)
                        bgImg.color = new Color(0f, 0.18f, 0.56f, Mathf.Lerp(1f, 0f, t / 0.5f));
                    yield return null;
                }
                Destroy(overlay);
            }

            // Restore music.
            if (mp != null) mp.Volume = savedVol;
            Destroy(gameObject);
        }

        private static Text AddOverlayText(Transform parent, string text, int size, FontStyle style,
            Color color, Vector2 pos, Vector2 sizeDelta, TextAnchor align)
        {
            var go = new GameObject("T", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.fontStyle = style; t.color = color;
            t.alignment = align; t.raycastTarget = false;
            return t;
        }
    }
}
