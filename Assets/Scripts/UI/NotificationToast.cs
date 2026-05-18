using System.Collections;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// One toast bubble — fades in, holds for a few seconds, fades out, destroys
    /// itself. Spawned and queued by NotificationSystem.
    /// </summary>
    public class NotificationToast : MonoBehaviour
    {
        public static NotificationToast Create(Transform parent, string message)
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image), typeof(NotificationToast));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(360, 72);
            rt.anchoredPosition = new Vector2(-24, -24);

            var img = go.GetComponent<Image>();
            img.sprite = TextureFactory.RoundedGlossy(360, 72, 16,
                ColorPalette.AeroCyan, new Color(0f, 0.4f, 0.8f), true);

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var trt = (RectTransform)textGO.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16, 8); trt.offsetMax = new Vector2(-16, -8);
            var t = textGO.GetComponent<Text>();
            t.text = message;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 18;
            t.fontStyle = FontStyle.Bold;
            t.color = ColorPalette.TextLight;
            t.alignment = TextAnchor.MiddleLeft;
            t.raycastTarget = false;
            var sh = textGO.AddComponent<Shadow>();
            sh.effectColor = ColorPalette.ShadowSoft;
            sh.effectDistance = new Vector2(1, -1);

            var toast = go.GetComponent<NotificationToast>();
            toast.StartCoroutine(toast.PlayLifecycle());
            return toast;
        }

        private IEnumerator PlayLifecycle()
        {
            var cg = GetComponent<CanvasGroup>();
            var rt = (RectTransform)transform;
            Vector2 startPos = rt.anchoredPosition + new Vector2(80, 0);
            Vector2 endPos = rt.anchoredPosition;
            rt.anchoredPosition = startPos;

            cg.alpha = 0f;
            float dur = 0.35f, t = 0f;
            while (t < dur) { t += Time.unscaledDeltaTime; float k = t / dur; cg.alpha = k; rt.anchoredPosition = Vector2.Lerp(startPos, endPos, k); yield return null; }
            cg.alpha = 1f; rt.anchoredPosition = endPos;

            yield return new WaitForSecondsRealtime(2.4f);

            t = 0f;
            while (t < dur) { t += Time.unscaledDeltaTime; cg.alpha = 1f - (t / dur); yield return null; }
            Destroy(gameObject);
        }
    }
}
