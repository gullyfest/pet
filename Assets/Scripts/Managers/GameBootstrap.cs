using aerisOS.UI;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.Managers
{
    /// <summary>
    /// Single entry point that builds the entire UI on scene start. Eliminates
    /// any manual scene setup — open the scene and press Play.
    ///
    /// The [RuntimeInitializeOnLoadMethod] hook spawns the bootstrap GameObject
    /// automatically, so the user does not even need to drag it into the scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            // If a bootstrap already exists in the scene, do nothing.
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            var go = new GameObject("[GameBootstrap]");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            BuildEventSystem();
            BuildAudio();
            BuildMusic();
            BuildUI();
        }

        private static void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            // Pick the input module that matches the project's active input
            // handler (Player Settings → Active Input Handling). The "Both"
            // option also defines ENABLE_INPUT_SYSTEM, so the new module wins.
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void BuildAudio()
        {
            if (AudioManager.Instance != null) return;
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }

        private static void BuildMusic()
        {
            if (MusicPlayer.Instance != null) return;
            var go = new GameObject("MusicPlayer");
            go.AddComponent<MusicPlayer>();
        }

        private void BuildUI()
        {
            // Root canvas — Screen Space Overlay so it scales to window.
            var canvasGO = new GameObject("RootCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            // Pixel-perfect snaps UI graphics to integer pixel boundaries so
            // text/sprites don't get bilinear-blurred when CanvasScaler scales.
            canvas.pixelPerfect = true;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            // Sky gradient background that lives behind everything.
            var bg = new GameObject("SkyBackground", typeof(Image));
            bg.transform.SetParent(canvasGO.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = TextureFactory.VerticalGradient(64, 256,
                ColorPalette.SkyBlueTop, ColorPalette.SkyBlueBottom);
            bgImg.raycastTarget = false;
            Stretch(bg.GetComponent<RectTransform>());

            // Three top-level screens, each with a CanvasGroup for fading.
            var menuRoot    = NewScreen(canvasGO.transform, "Menu");
            var loadingRoot = NewScreen(canvasGO.transform, "Loading");
            var desktopRoot = NewScreen(canvasGO.transform, "Desktop");

            // Fader on top — used by SceneFlowManager for screen transitions.
            var fader = new GameObject("Fader", typeof(Image), typeof(CanvasGroup));
            fader.transform.SetParent(canvasGO.transform, false);
            var faderImg = fader.GetComponent<Image>();
            faderImg.color = new Color(0, 0, 0, 0);
            faderImg.raycastTarget = false;
            Stretch(fader.GetComponent<RectTransform>());
            fader.SetActive(false);

            // Notifications layer (above fader so they survive transitions visually).
            var notifyRoot = new GameObject("Notifications", typeof(RectTransform));
            notifyRoot.transform.SetParent(canvasGO.transform, false);
            Stretch(notifyRoot.GetComponent<RectTransform>());
            var notifySys = notifyRoot.AddComponent<NotificationSystem>();

            // Corruption overlay sits above UI and gradually introduces glitches.
            var overlayRoot = new GameObject("CorruptionOverlay", typeof(RectTransform));
            overlayRoot.transform.SetParent(canvasGO.transform, false);
            Stretch(overlayRoot.GetComponent<RectTransform>());
            var overlay = overlayRoot.AddComponent<CorruptionOverlay>();

            // Build content of each screen.
            var menuUI    = menuRoot.gameObject.AddComponent<MainMenuUI>();
            var loadingUI = loadingRoot.gameObject.AddComponent<LoadingScreen>();
            var desktopUI = desktopRoot.gameObject.AddComponent<DesktopBuilder>();

            // Wire the scene flow manager.
            var flow = canvasGO.AddComponent<SceneFlowManager>();
            flow.Init(
                menuRoot.GetComponent<CanvasGroup>(),
                loadingRoot.GetComponent<CanvasGroup>(),
                desktopRoot.GetComponent<CanvasGroup>(),
                faderImg);

            menuUI.Build();
            loadingUI.Build();
            desktopUI.Build(notifySys);
            notifySys.Init();
            overlay.Build();

            // aeris simulation state: task loop, corruption progression, hidden lore.
            if (aerisRuntime.Instance == null)
            {
                var runtimeGO = new GameObject("aerisRuntime");
                var runtime = runtimeGO.AddComponent<aerisRuntime>();
                runtime.Init(notifySys);
            }
            else
            {
                aerisRuntime.Instance.Init(notifySys);
            }
        }

        private static RectTransform NewScreen(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
