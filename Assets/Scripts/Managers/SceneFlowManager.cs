using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.Managers
{
    /// <summary>
    /// Coordinates transitions between top-level UI screens (Menu, Loading, Desktop).
    /// All screens live as children of the same root canvas; this manager only
    /// activates one at a time and runs a CanvasGroup fade between them.
    /// </summary>
    public class SceneFlowManager : MonoBehaviour
    {
        public static SceneFlowManager Instance { get; private set; }

        public enum Screen { Menu, Loading, Desktop }

        private CanvasGroup _menu;
        private CanvasGroup _loading;
        private CanvasGroup _desktop;
        private Image _fader;

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Init(CanvasGroup menu, CanvasGroup loading, CanvasGroup desktop, Image fader)
        {
            Instance = this;
            _menu = menu;
            _loading = loading;
            _desktop = desktop;
            _fader = fader;
            ShowImmediate(Screen.Menu);
        }

        public void ShowImmediate(Screen s)
        {
            SetGroup(_menu,    s == Screen.Menu);
            SetGroup(_loading, s == Screen.Loading);
            SetGroup(_desktop, s == Screen.Desktop);
        }

        public void GoTo(Screen s, float fadeDuration = 0.4f, Action onArrived = null)
        {
            StartCoroutine(FadeTo(s, fadeDuration, onArrived));
        }

        private IEnumerator FadeTo(Screen target, float dur, Action onArrived)
        {
            // Fade to black via the global fader image.
            yield return Fade(0f, 1f, dur);
            ShowImmediate(target);
            yield return Fade(1f, 0f, dur);
            onArrived?.Invoke();
        }

        private IEnumerator Fade(float from, float to, float dur)
        {
            if (_fader == null) yield break;
            _fader.gameObject.SetActive(true);
            var c = _fader.color; c.a = from; _fader.color = c;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, t / dur);
                _fader.color = c;
                yield return null;
            }
            c.a = to; _fader.color = c;
            if (to <= 0.01f) _fader.gameObject.SetActive(false);
        }

        private static void SetGroup(CanvasGroup g, bool active)
        {
            if (g == null) return;
            g.alpha = active ? 1f : 0f;
            g.interactable = active;
            g.blocksRaycasts = active;
            g.gameObject.SetActive(active);
        }
    }
}
