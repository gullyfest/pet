using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// One button on the taskbar that represents an open window. Clicking it
    /// toggles the window between visible and minimized, and brings it to
    /// front when restoring. Highlights itself when its window is active.
    /// </summary>
    public class TaskbarEntry : MonoBehaviour, IPointerClickHandler
    {
        public DraggableWindow Window { get; private set; }
        private Image _bg;
        private Text _label;

        public static TaskbarEntry Create(Transform parent, DraggableWindow window)
        {
            var go = new GameObject($"Task_{window.Title}",
                typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(TaskbarEntry));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(170, 44);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 170;
            le.preferredHeight = 44;
            le.minWidth = 120;

            var entry = go.GetComponent<TaskbarEntry>();
            entry.Window = window;
            entry._bg = go.GetComponent<Image>();
            entry._bg.sprite = TextureFactory.RoundedGlossy(170, 44, 8,
                Color.white.WithAlpha(0.35f), Color.white.WithAlpha(0.1f), true);
            entry._bg.raycastTarget = true;

            // Label.
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblGO.transform.SetParent(go.transform, false);
            var lRT = (RectTransform)lblGO.transform;
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(10, 0); lRT.offsetMax = new Vector2(-10, 0);
            entry._label = lblGO.GetComponent<Text>();
            entry._label.text = window.Title;
            entry._label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry._label.fontSize = 16;
            entry._label.fontStyle = FontStyle.Bold;
            entry._label.color = Color.white;
            entry._label.alignment = TextAnchor.MiddleLeft;
            entry._label.raycastTarget = false;
            entry._label.horizontalOverflow = HorizontalWrapMode.Overflow;
            var outline = lblGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.1f, 0.25f, 1f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            // React to window state.
            window.OnMinimizedChanged = m => entry.Refresh();
            entry.Refresh();
            return entry;
        }

        public void Refresh()
        {
            if (Window == null) return;
            // Active windows are tinted brighter; minimized are darker.
            _bg.color = Window.IsMinimized
                ? new Color(1f, 1f, 1f, 0.7f)
                : new Color(0.7f, 0.95f, 1f, 1f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Window == null) return;
            AudioManager.Instance?.PlayClick();
            Window.ToggleMinimized();
            Refresh();
        }
    }
}
