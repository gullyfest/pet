using System.Collections.Generic;
using aerisOS.Managers;
using UnityEngine;

namespace aerisOS.UI
{
    /// <summary>
    /// Manages toast notifications. Newer toasts push older ones downward by
    /// offsetting their anchor positions. Plays a notify sound for each push.
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }
        private readonly List<NotificationToast> _alive = new List<NotificationToast>();
        private const float ToastHeight = 84f;

        public void Init()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            _alive.Clear();
            if (Instance == this) Instance = null;
        }

        public void Push(string message)
        {
            // Remove destroyed toasts BEFORE iterating to avoid MissingReferenceException.
            _alive.RemoveAll(x => x == null);
            for (int i = 0; i < _alive.Count; i++)
            {
                var rt = (RectTransform)_alive[i].transform;
                rt.anchoredPosition += new Vector2(0, -ToastHeight);
            }

            var toast = NotificationToast.Create(transform, message);
            _alive.Add(toast);
            AudioManager.Instance?.PlayNotify();

            // Cap visible toasts to avoid stacking forever.
            while (_alive.Count > 4)
            {
                if (_alive[0] != null) Destroy(_alive[0].gameObject);
                _alive.RemoveAt(0);
            }
        }
    }
}
