using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Clickable wallpaper tile inside the Browser window. On click it finds
    /// the SkyBackground Image in the root canvas and swaps its gradient sprite,
    /// effectively changing the desktop background live.
    /// </summary>
    public class WallpaperTile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public int WpIndex;
        public NotificationSystem Notify;

        private Image _img;
        private Vector3 _baseScale;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _baseScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale * 1.06f, 0.1f));
        }

        public void OnPointerExit(PointerEventData e)
        {
            StopAllCoroutines();
            StartCoroutine(LerpScale(_baseScale, 0.1f));
        }

        public void OnPointerClick(PointerEventData e)
        {
            var wallpapers = WindowManager.Wallpapers;
            if (WpIndex < 0 || WpIndex >= wallpapers.Length) return;

            var wp = wallpapers[WpIndex];

            // Find the sky background Image by name and replace its sprite.
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var bgTransform = canvas.transform.Find("SkyBackground");
                if (bgTransform != null)
                {
                    var bg = bgTransform.GetComponent<Image>();
                    if (bg != null)
                        bg.sprite = TextureFactory.VerticalGradient(64, 256, wp.Top, wp.Bot);
                }
            }

            AudioManager.Instance?.PlaySuccess();
            Notify?.Push($"Wallpaper: {wp.Name}");
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
