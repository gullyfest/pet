using System.Collections;
using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Floating application window with a draggable title bar, close (×) and
    /// minimize (_) buttons, and a fade-in / fade-out animation on open/close.
    /// The body is filled by Build() with arbitrary content.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class DraggableWindow : MonoBehaviour, IPointerDownHandler
    {
        public string Title = "Window";
        public RectTransform Body { get; private set; }
        public System.Action OnClosed;
        public System.Action<bool> OnMinimizedChanged;
        public System.Action<Vector2> OnResized;
        public bool IsMinimized { get; private set; }
        public bool IsMaximized { get; private set; }
        public Vector2 MinSize = new Vector2(420, 300);
        public float MaxSizeRatio = 0.9f;

        private CanvasGroup _cg;
        private RectTransform _rt;
        private Vector2 _restoreSize;
        private Vector2 _restorePos;

        public static DraggableWindow Create(Transform parent, string title, Vector2 size, Vector2 position)
        {
            var go = new GameObject($"Window_{title}",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(DraggableWindow));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;

            var bg = go.GetComponent<Image>();
            bg.sprite = TextureFactory.RoundedGlossy(
                (int)size.x, (int)size.y, 18,
                Color.white.WithAlpha(0.96f),
                new Color(0.9f, 0.96f, 1f, 0.96f), true);
            bg.type = Image.Type.Simple;

            var win = go.GetComponent<DraggableWindow>();
            win.Title = title;
            win._rt = rt;
            win._cg = go.GetComponent<CanvasGroup>();
            win.BuildChrome(size);
            win.PlayOpen();
            return win;
        }

        private void BuildChrome(Vector2 size)
        {
            // Title bar — top 36 px strip, also handles dragging.
            var bar = new GameObject("TitleBar",
                typeof(RectTransform), typeof(Image), typeof(WindowDragger));
            bar.transform.SetParent(transform, false);
            var barRT = (RectTransform)bar.transform;
            barRT.anchorMin = new Vector2(0, 1);
            barRT.anchorMax = new Vector2(1, 1);
            barRT.pivot = new Vector2(0.5f, 1f);
            barRT.sizeDelta = new Vector2(0, 38);
            barRT.anchoredPosition = Vector2.zero;
            var barImg = bar.GetComponent<Image>();
            barImg.sprite = TextureFactory.RoundedGlossy(
                (int)size.x, 38, 18,
                ColorPalette.AeroCyan,
                new Color(0f, 0.45f, 0.85f, 1f), true);
            bar.GetComponent<WindowDragger>().Target = _rt;

            // Title text.
            var titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(bar.transform, false);
            var trt = (RectTransform)titleGO.transform;
            trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(14, 0); trt.offsetMax = new Vector2(-80, 0);
            var text = titleGO.GetComponent<Text>();
            text.text = Title;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = ColorPalette.TextLight;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            var titleOutline = titleGO.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0.15f, 0.3f, 1f);
            titleOutline.effectDistance = new Vector2(1f, -1f);

            // Minimize button — hides the window via CanvasGroup; the taskbar
            // entry for it remains clickable to restore.
            aerisButton.Create(bar.transform, "_", new Vector2(34, 28), () => SetMinimized(true),
                Color.white.WithAlpha(0.3f), Color.white.WithAlpha(0.1f), fontSize: 20)
                .GetComponent<RectTransform>().With(rt =>
                {
                    rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
                    rt.pivot = new Vector2(1, 0.5f);
                    rt.anchoredPosition = new Vector2(-44, 0);
                });

            // Maximize / restore button.
            aerisButton.Create(bar.transform, "[]", new Vector2(34, 28), ToggleMaximized,
                Color.white.WithAlpha(0.3f), Color.white.WithAlpha(0.1f), fontSize: 16)
                .GetComponent<RectTransform>().With(rt =>
                {
                    rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
                    rt.pivot = new Vector2(1, 0.5f);
                    rt.anchoredPosition = new Vector2(-82, 0);
                });

            // Close button.
            aerisButton.Create(bar.transform, "X", new Vector2(34, 28), Close,
                ColorPalette.AccentPink, new Color(0.7f, 0.1f, 0.4f, 1f), fontSize: 20)
                .GetComponent<RectTransform>().With(rt =>
                {
                    rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
                    rt.pivot = new Vector2(1, 0.5f);
                    rt.anchoredPosition = new Vector2(-6, 0);
                });

            // Body container — sits below the title bar and fills the rest.
            var bodyGO = new GameObject("Body", typeof(RectTransform));
            bodyGO.transform.SetParent(transform, false);
            var brt = (RectTransform)bodyGO.transform;
            brt.anchorMin = new Vector2(0, 0); brt.anchorMax = new Vector2(1, 1);
            brt.offsetMin = new Vector2(10, 10); brt.offsetMax = new Vector2(-10, -42);
            Body = brt;

            // Resize grip in bottom-right corner.
            var grip = new GameObject("ResizeGrip", typeof(RectTransform), typeof(Image), typeof(WindowResizer));
            grip.transform.SetParent(transform, false);
            var grt = (RectTransform)grip.transform;
            grt.anchorMin = new Vector2(1, 0);
            grt.anchorMax = new Vector2(1, 0);
            grt.pivot = new Vector2(1, 0);
            grt.sizeDelta = new Vector2(32, 32);
            grt.anchoredPosition = new Vector2(-4, 4);
            var gimg = grip.GetComponent<Image>();
            gimg.sprite = TextureFactory.RoundedGlossy(28, 28, 8,
                ColorPalette.AeroCyan, new Color(0f, 0.45f, 0.85f, 1f), true);
            gimg.color = new Color(0.35f, 0.75f, 1f, 0.92f);

            var gripIconGO = new GameObject("ResizeIcon", typeof(RectTransform), typeof(Text));
            gripIconGO.transform.SetParent(grip.transform, false);
            var giRT = (RectTransform)gripIconGO.transform;
            giRT.anchorMin = Vector2.zero; giRT.anchorMax = Vector2.one;
            giRT.offsetMin = Vector2.zero; giRT.offsetMax = Vector2.zero;
            var giT = gripIconGO.GetComponent<Text>();
            giT.text = "⤡";
            giT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            giT.fontSize = 18;
            giT.alignment = TextAnchor.MiddleCenter;
            giT.color = Color.white.WithAlpha(0.95f);
            giT.raycastTarget = false;

            var resizer = grip.GetComponent<WindowResizer>();
            resizer.Target = _rt;
            resizer.Owner = this;

            _restoreSize = _rt.sizeDelta;
            _restorePos = _rt.anchoredPosition;
        }

        public void Close()
        {
            StartCoroutine(PlayCloseAnim());
        }

        public void SetMinimized(bool minimized)
        {
            if (IsMinimized == minimized) return;
            IsMinimized = minimized;
            StopCoroutine(nameof(MinimizeAnim));
            StartCoroutine(MinimizeAnim(minimized));
            OnMinimizedChanged?.Invoke(minimized);
        }

        public void ToggleMinimized()
        {
            SetMinimized(!IsMinimized);
            if (!IsMinimized) transform.SetAsLastSibling();
        }

        public void ToggleMaximized()
        {
            var parentRT = transform.parent as RectTransform;
            if (parentRT == null) return;

            if (!IsMaximized)
            {
                _restoreSize = _rt.sizeDelta;
                _restorePos = _rt.anchoredPosition;
                IsMaximized = true;

                Vector2 targetSize = parentRT.rect.size - new Vector2(24f, 24f);
                targetSize *= Mathf.Clamp(MaxSizeRatio, 0.55f, 1f);
                _rt.sizeDelta = new Vector2(
                    Mathf.Max(MinSize.x, targetSize.x),
                    Mathf.Max(MinSize.y, targetSize.y));
                _rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                IsMaximized = false;
                _rt.sizeDelta = _restoreSize;
                _rt.anchoredPosition = _restorePos;
            }

            OnResized?.Invoke(_rt.sizeDelta);
        }

        public void MarkRestoredFromResize()
        {
            IsMaximized = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        private IEnumerator MinimizeAnim(bool minimized)
        {
            float dur = 0.18f, t = 0f;
            float fromAlpha = _cg.alpha;
            float toAlpha = minimized ? 0f : 1f;
            Vector3 fromScale = transform.localScale;
            Vector3 toScale = minimized ? Vector3.one * 0.7f : Vector3.one;
            // While minimized the window must not block clicks/drags.
            _cg.blocksRaycasts = !minimized;
            _cg.interactable = !minimized;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = t / dur;
                _cg.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                transform.localScale = Vector3.Lerp(fromScale, toScale, k);
                yield return null;
            }
            _cg.alpha = toAlpha;
            transform.localScale = toScale;
        }

        private void PlayOpen()
        {
            _cg.alpha = 0f;
            transform.localScale = Vector3.one * 0.85f;
            StartCoroutine(LerpOpen());
        }

        private IEnumerator LerpOpen()
        {
            float dur = 0.22f, t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = t / dur;
                _cg.alpha = k;
                transform.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, k);
                yield return null;
            }
            _cg.alpha = 1f;
            transform.localScale = Vector3.one;
        }

        private IEnumerator PlayCloseAnim()
        {
            float dur = 0.18f, t = 0f;
            Vector3 from = transform.localScale;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = t / dur;
                _cg.alpha = 1f - k;
                transform.localScale = Vector3.Lerp(from, from * 0.9f, k);
                yield return null;
            }
            OnClosed?.Invoke();
            Destroy(gameObject);
        }
    }

    /// <summary>Fluent helper so the chrome layout reads in one expression.</summary>
    internal static class RectTransformFluent
    {
        public static RectTransform With(this RectTransform rt, System.Action<RectTransform> apply)
        {
            apply(rt);
            return rt;
        }
    }

    /// <summary>Drags a target RectTransform when its host is grabbed.</summary>
    internal class WindowDragger : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform Target;
        private Vector2 _startPos;
        private Vector2 _startMouse;

        public void OnBeginDrag(PointerEventData e)
        {
            if (Target == null) return;
            Target.SetAsLastSibling();
            _startPos = Target.anchoredPosition;
            _startMouse = e.position;
        }

        public void OnDrag(PointerEventData e)
        {
            if (Target == null) return;
            Vector2 delta = e.position - _startMouse;
            // e.position is in screen pixels; anchoredPosition is in canvas units.
            // Divide by scaleFactor to match them (fixes "mouse faster than window" bug).
            var canvas = Target.GetComponentInParent<Canvas>();
            if (canvas != null) delta /= canvas.scaleFactor;
            var next = _startPos + delta;

            var parent = Target.parent as RectTransform;
            if (parent != null)
            {
                float halfW = Target.sizeDelta.x * 0.5f;
                float halfH = Target.sizeDelta.y * 0.5f;
                float minX = -parent.rect.width * 0.5f + halfW;
                float maxX = parent.rect.width * 0.5f - halfW;
                float minY = -parent.rect.height * 0.5f + halfH;
                float maxY = parent.rect.height * 0.5f - halfH;
                next.x = Mathf.Clamp(next.x, minX, maxX);
                next.y = Mathf.Clamp(next.y, minY, maxY);
            }

            Target.anchoredPosition = next;
        }
    }

    internal class WindowResizer : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform Target;
        public DraggableWindow Owner;

        private Vector2 _startSize;
        private Vector2 _startMouse;
        private Vector2 _startPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Target == null) return;
            Target.SetAsLastSibling();
            _startSize = Target.sizeDelta;
            _startMouse = eventData.position;
            _startPos = Target.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Target == null) return;
            Vector2 delta = eventData.position - _startMouse;
            var canvas = Target.GetComponentInParent<Canvas>();
            if (canvas != null) delta /= canvas.scaleFactor;

            Vector2 minSize = Owner != null ? Owner.MinSize : new Vector2(420, 300);
            Vector2 maxSize = new Vector2(float.MaxValue, float.MaxValue);
            var parent = Target.parent as RectTransform;
            if (parent != null)
            {
                float ratio = Owner != null ? Mathf.Clamp(Owner.MaxSizeRatio, 0.55f, 1f) : 0.9f;
                maxSize = parent.rect.size * ratio;
            }
            var next = new Vector2(
                Mathf.Clamp(_startSize.x + delta.x, minSize.x, maxSize.x),
                Mathf.Clamp(_startSize.y - delta.y, minSize.y, maxSize.y));

            Target.sizeDelta = next;

            // Pivot = (0.5, 0.5): при изменении size центр остаётся на месте, а угол уходит.
            // Корректируем позицию чтобы левый-верхний угол не смещался.
            Vector2 actualDelta = next - _startSize;
            Target.anchoredPosition = _startPos + new Vector2(actualDelta.x * 0.5f, -actualDelta.y * 0.5f);

            if (Owner != null && Owner.IsMaximized) Owner.MarkRestoredFromResize();
            Owner?.OnResized?.Invoke(next);
        }
    }
}
