using System;
using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Adaptive host for aeris Antivirus mini-games.
    /// </summary>
    public class AntivirusAppUI : MonoBehaviour
    {
        private Text _taskTitle;
        private Text _hintText;
        private RectTransform _gameHost;
        private GameObject _hintPanel;

        // Разблокированные мини-игры — пополняются по ходу сюжета
        public static readonly System.Collections.Generic.HashSet<string> UnlockedGames
            = new System.Collections.Generic.HashSet<string>();

        public static void UnlockGame(string id) => UnlockedGames.Add(id);

        private AntivirusMiniGameBase _activeGame;
        private bool _completing;

        // ── Режим сценария: только одна задача, без цикла ──────────────────
        private string _lockedTaskId;
        public event Action OnScenarioCleanupCompleted;

        public string LockedTaskId
        {
            get => _lockedTaskId;
            set
            {
                _lockedTaskId = value;
                if (value != null) LoadGameById(value);
            }
        }

        public void Build(RectTransform body)
        {
            BuildHeader(body);
            BuildGameFrame(body);
            ShowNoTasksPlaceholder();
        }

        private void OnDestroy()
        {
            if (_activeGame != null)
                _activeGame.OnCompleted -= OnMiniGameCompleted;
        }

        private void BuildHeader(RectTransform body)
        {
            var topCard = new GameObject("TopCard", typeof(RectTransform), typeof(Image));
            topCard.transform.SetParent(body, false);
            var trt = (RectTransform)topCard.transform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.offsetMin = new Vector2(0, -116);
            trt.offsetMax = new Vector2(0, 0);
            var tImg = topCard.GetComponent<Image>();
            tImg.sprite = TextureFactory.RoundedGlossy(980, 116, 16,
                new Color(0.94f, 0.97f, 1f, 0.98f), new Color(0.86f, 0.93f, 1f, 0.94f), true);

            // левая акцентная полоса
            var accentBar = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            accentBar.transform.SetParent(topCard.transform, false);
            var abrt = (RectTransform)accentBar.transform;
            abrt.anchorMin = new Vector2(0, 0);
            abrt.anchorMax = new Vector2(0, 1);
            abrt.pivot = new Vector2(0, 0.5f);
            abrt.offsetMin = new Vector2(0, 6);
            abrt.offsetMax = new Vector2(6, -6);
            accentBar.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(6, 104, 3,
                ColorPalette.AeroCyan, new Color(0f, 0.55f, 0.85f, 1f), false);

            // название текущей задачи/игры
            _taskTitle = BuildLabel(topCard.transform, "Awaiting task...", 24, FontStyle.Bold,
                new Color(0f, 0.45f, 0.82f, 1f), new Vector2(16, -12), new Vector2(640, 34), TextAnchor.UpperLeft);

            // hint panel
            _hintPanel = new GameObject("HintPanel", typeof(RectTransform), typeof(Image));
            var hintPanel = _hintPanel;
            hintPanel.transform.SetParent(topCard.transform, false);
            var hprt = (RectTransform)hintPanel.transform;
            hprt.anchorMin = new Vector2(0.5f, 0);
            hprt.anchorMax = new Vector2(0.5f, 0);
            hprt.pivot = new Vector2(0.5f, 0);
            hprt.anchoredPosition = new Vector2(0, 8);
            hprt.sizeDelta = new Vector2(520, 38);
            hintPanel.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(520, 38, 8,
                new Color(0.72f, 0.92f, 1f, 0.88f), new Color(0.55f, 0.82f, 1f, 0.72f), false);

            var hintAccent = new GameObject("HintAccent", typeof(RectTransform), typeof(Image));
            hintAccent.transform.SetParent(hintPanel.transform, false);
            var hart = (RectTransform)hintAccent.transform;
            hart.anchorMin = new Vector2(0, 0);
            hart.anchorMax = new Vector2(0, 1);
            hart.pivot = new Vector2(0, 0.5f);
            hart.offsetMin = new Vector2(0, 2);
            hart.offsetMax = new Vector2(4, -2);
            hintAccent.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(4, 34, 2,
                ColorPalette.AeroCyan, ColorPalette.AeroCyan, false);

            BuildLabel(hintPanel.transform, "ℹ", 13, FontStyle.Bold,
                new Color(0f, 0.42f, 0.78f, 1f), new Vector2(8, -10), new Vector2(20, 18), TextAnchor.MiddleCenter);

            _hintText = BuildLabel(hintPanel.transform, "", 15, FontStyle.Bold,
                new Color(0.02f, 0.18f, 0.38f, 0.96f), new Vector2(30, -4), new Vector2(460, 30), TextAnchor.MiddleLeft);
            _hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _hintText.verticalOverflow = VerticalWrapMode.Overflow;
            _hintText.lineSpacing = 1.06f;


        }

        private void BuildGameFrame(RectTransform body)
        {
            var frame = new GameObject("MiniGameFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(body, false);
            var frt = (RectTransform)frame.transform;
            frt.anchorMin = new Vector2(0, 0);
            frt.anchorMax = new Vector2(1, 1);
            frt.offsetMin = new Vector2(0, 0);
            frt.offsetMax = new Vector2(0, -122);

            var frameImg = frame.GetComponent<Image>();
            frameImg.sprite = TextureFactory.RoundedGlossy(980, 500, 14,
                Color.white.WithAlpha(0.92f), Color.white.WithAlpha(0.82f), false);

            _gameHost = new GameObject("GameHost", typeof(RectTransform)).GetComponent<RectTransform>();
            _gameHost.SetParent(frame.transform, false);
            _gameHost.anchorMin = Vector2.zero;
            _gameHost.anchorMax = Vector2.one;
            _gameHost.offsetMin = new Vector2(16, 16);
            _gameHost.offsetMax = new Vector2(-16, -16);
        }

        private static Text BuildLabel(Transform parent, string text, int size, FontStyle style,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor align)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        private void ShowNoTasksPlaceholder()
        {
            if (_gameHost == null) return;
            if (_activeGame != null)
            {
                _activeGame.OnCompleted -= OnMiniGameCompleted;
                Destroy(_activeGame.gameObject);
                _activeGame = null;
            }
            for (int i = _gameHost.childCount - 1; i >= 0; i--)
                Destroy(_gameHost.GetChild(i).gameObject);

            var go = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(_gameHost, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.text      = "No active threats detected.";
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = 22;
            t.color     = new Color(0.2f, 0.6f, 0.4f, 0.7f);
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;

            if (_taskTitle != null) _taskTitle.text = "System clean";
            if (_hintText  != null) _hintText.text  = "";
            if (_hintPanel != null) _hintPanel.SetActive(false);
        }

        // Загрузить мини-игру по id (вызывается из LockedTaskId или после разблокировки)
        public void LoadGameById(string id)
        {
            if (_gameHost == null) return;
            if (_hintPanel != null) _hintPanel.SetActive(true);
            if (_activeGame != null)
            {
                _activeGame.OnCompleted -= OnMiniGameCompleted;
                Destroy(_activeGame.gameObject);
                _activeGame = null;
            }
            for (int i = _gameHost.childCount - 1; i >= 0; i--)
                Destroy(_gameHost.GetChild(i).gameObject);

            var host = new GameObject("MiniGame", typeof(RectTransform));
            host.transform.SetParent(_gameHost, false);
            var hrt = (RectTransform)host.transform;
            hrt.anchorMin = Vector2.zero;
            hrt.anchorMax = Vector2.one;
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;

            _activeGame = id switch
            {
                "clean_bubbles"          => host.AddComponent<BubbleCleanerMiniGame>(),
                "organize_clouds"        => host.AddComponent<CloudSorterMiniGame>(),
                "repair_pixels"          => host.AddComponent<PixelRepairMiniGame>(),
                "garden_stability"       => host.AddComponent<DigitalGardenMiniGame>(),
                "reflection_calibration" => host.AddComponent<ReflectionCalibrationMiniGame>(),
                "popup_panic"            => host.AddComponent<FakePopupPanicMiniGame>(),
                "cursor_dodge"           => host.AddComponent<CursorDodgeMiniGame>(),
                _                        => host.AddComponent<BubbleCleanerMiniGame>(),
            };

            _activeGame.OnCompleted += OnMiniGameCompleted;
            _activeGame.Initialize(_hintText);
            _completing = false;

            if (_taskTitle != null) _taskTitle.text = GetGameTitle(id);
        }

        private static string GetGameTitle(string id) => id switch
        {
            "clean_bubbles"          => "Bubble Cleaner",
            "organize_clouds"        => "Cloud Sorter",
            "repair_pixels"          => "Pixel Repair",
            "garden_stability"       => "Digital Garden",
            "reflection_calibration" => "Reflection Calibration",
            "popup_panic"            => "Popup Panic",
            "cursor_dodge"           => "Cursor Dodge",
            _                        => "Mini-Game",
        };

        private void OnMiniGameCompleted()
        {
            if (_completing) return;
            _completing = true;

            if (_lockedTaskId != null)
            {
                // Разблокируем игру для последующего свободного прохождения
                UnlockGame(_lockedTaskId);
                OnScenarioCleanupCompleted?.Invoke();
                return;
            }

            // Повтор текущей игры из разблокированных
            _completing = false;
        }
    }
}
