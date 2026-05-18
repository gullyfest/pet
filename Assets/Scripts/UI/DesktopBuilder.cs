using aerisOS.Narrative;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class DesktopBuilder : MonoBehaviour
    {
        // ─── Скрытые иконки, появляются по сценарию ─────────────────────────
        private GameObject _gameIconGO;
        private GameObject _antivirusIconGO;
        private GameObject _archiveIconGO;
        private GameObject _drawingIconGO;

        // Все иконки (для скрытия в концовке 2)
        private readonly System.Collections.Generic.List<GameObject> _allIcons
            = new System.Collections.Generic.List<GameObject>();

        private DesktopChibiSpawner _chibiSpawner;
        private Image _backgroundTint; // для затемнения в концовке 2

        public ChibiDialogPanel DialogPanel =>
            _chibiSpawner != null ? _chibiSpawner.DialogPanel : null;

        public void RevealGameIcon()
        {
            if (_gameIconGO != null) _gameIconGO.SetActive(true);
        }

        public void RevealAntivirusIcon()
        {
            if (_antivirusIconGO != null) _antivirusIconGO.SetActive(true);
        }

        public void RevealArchiveIcon()
        {
            if (_archiveIconGO != null) _archiveIconGO.SetActive(true);
        }

        public void RevealDrawingIcon()
        {
            if (_drawingIconGO != null) _drawingIconGO.SetActive(true);
        }

        public void HideAllIcons()
        {
            foreach (var icon in _allIcons)
                if (icon != null) icon.SetActive(false);
        }

        // Концовка 1: скрываем только Game и Antivirus
        public void HideGameAndAntivirusIcons()
        {
            if (_gameIconGO != null) _gameIconGO.SetActive(false);
            if (_antivirusIconGO != null) _antivirusIconGO.SetActive(false);
        }

        public void FadeOutChibi(float duration)
        {
            _chibiSpawner?.StartFade(0f, duration);
        }

        public void SetBackgroundBlack()
        {
            if (_backgroundTint != null)
            {
                // Поднимаем поверх всего перед затемнением
                _backgroundTint.transform.SetAsLastSibling();
                _backgroundTint.raycastTarget = true;
                StartCoroutine(FadeTintToBlack());
            }
        }

        private System.Collections.IEnumerator FadeTintToBlack()
        {
            float elapsed = 0f;
            float duration = 2.5f;
            var startColor = _backgroundTint.color;
            var endColor = Color.black;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _backgroundTint.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }
            _backgroundTint.color = endColor;
        }

        public void ShowSystemRestoreDialog(System.Action onOk, RectTransform parent)
        {
            var overlay = new GameObject("RestoreOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            var ort = (RectTransform)overlay.transform;
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero;
            ort.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            overlay.transform.SetAsLastSibling();

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(overlay.transform, false);
            var crt = (RectTransform)card.transform;
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(500, 220);
            crt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);

            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(card.transform, false);
            var trt2 = (RectTransform)title.transform;
            trt2.anchorMin = new Vector2(0, 1); trt2.anchorMax = new Vector2(1, 1);
            trt2.pivot = new Vector2(0.5f, 1);
            trt2.offsetMin = new Vector2(0, -36); trt2.offsetMax = new Vector2(0, -6);
            var tt = title.GetComponent<Text>();
            tt.text = "SYSTEM INFORMATION";
            tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tt.fontSize = 16; tt.fontStyle = FontStyle.Bold;
            tt.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            tt.alignment = TextAnchor.UpperCenter;
            tt.raycastTarget = false;

            var body2 = new GameObject("Body", typeof(RectTransform), typeof(Text));
            body2.transform.SetParent(card.transform, false);
            var brt2 = (RectTransform)body2.transform;
            brt2.anchorMin = new Vector2(0, 0.3f); brt2.anchorMax = new Vector2(1, 1);
            brt2.offsetMin = new Vector2(16, 0); brt2.offsetMax = new Vector2(-16, -44);
            var bt2 = body2.GetComponent<Text>();
            bt2.text = "System successfully restored.\nThreat T.E.R.R.A. completely removed.\n0 errors detected. Your system is operating normally.";
            bt2.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bt2.fontSize = 17;
            bt2.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            bt2.alignment = TextAnchor.MiddleCenter;
            bt2.horizontalOverflow = HorizontalWrapMode.Wrap;
            bt2.raycastTarget = false;

            var okBtn = new GameObject("OK", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
            okBtn.transform.SetParent(card.transform, false);
            var obrt = (RectTransform)okBtn.transform;
            obrt.anchorMin = new Vector2(0.5f, 0); obrt.anchorMax = new Vector2(0.5f, 0);
            obrt.pivot = new Vector2(0.5f, 0);
            obrt.sizeDelta = new Vector2(100, 38);
            obrt.anchoredPosition = new Vector2(0, 16);
            okBtn.GetComponent<Image>().color = new Color(0.72f, 0.72f, 0.72f, 1f);

            var okTxt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            okTxt.transform.SetParent(okBtn.transform, false);
            var otrtt = (RectTransform)okTxt.transform;
            otrtt.anchorMin = Vector2.zero; otrtt.anchorMax = Vector2.one;
            otrtt.offsetMin = Vector2.zero; otrtt.offsetMax = Vector2.zero;
            var ott = okTxt.GetComponent<Text>();
            ott.text = "OK"; ott.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ott.fontSize = 16; ott.color = Color.black; ott.alignment = TextAnchor.MiddleCenter;
            ott.raycastTarget = false;

            okBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Destroy(overlay);
                onOk?.Invoke();
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        public void Build(NotificationSystem notify)
        {
            var rt = (RectTransform)transform;

            // Фоновый тинт (используется для затемнения в концовке 2)
            var tintGO = new GameObject("BgTint", typeof(RectTransform), typeof(Image));
            tintGO.transform.SetParent(rt, false);
            var tintRT = (RectTransform)tintGO.transform;
            tintRT.anchorMin = Vector2.zero;
            tintRT.anchorMax = Vector2.one;
            tintRT.offsetMin = Vector2.zero;
            tintRT.offsetMax = Vector2.zero;
            _backgroundTint = tintGO.GetComponent<Image>();
            _backgroundTint.color = Color.clear;
            _backgroundTint.raycastTarget = false;
            _backgroundTint.sprite = TextureFactory.Solid(Color.black);

            // Декоративные пузыри фона
            CreateBubble(rt, new Vector2(-700, 200),  280, ColorPalette.AeroCyan.WithAlpha(0.25f));
            CreateBubble(rt, new Vector2(620, 280),   220, ColorPalette.GlossWhite.WithAlpha(0.35f));
            CreateBubble(rt, new Vector2(360, -260),  180, ColorPalette.AccentLime.WithAlpha(0.20f));
            CreateBubble(rt, new Vector2(-420, -180), 140, ColorPalette.AccentPink.WithAlpha(0.20f));

            // ── Колонка 1 — 5 базовых иконок (видны с самого начала) ──────────
            var iconCol = new GameObject("IconColumn", typeof(RectTransform));
            iconCol.transform.SetParent(rt, false);
            var iRT = (RectTransform)iconCol.transform;
            iRT.anchorMin = new Vector2(0, 1);
            iRT.anchorMax = new Vector2(0, 1);
            iRT.pivot = new Vector2(0, 1);
            iRT.anchoredPosition = new Vector2(40, -40);
            iRT.sizeDelta = new Vector2(110, 960);

            _allIcons.Add(CreateIconGO(iRT, AppType.MyComputer, "My Computer", ColorPalette.AeroCyan,         'C', 0, "mycomputer"));
            _allIcons.Add(CreateIconGO(iRT, AppType.Notes,      "Notes",       ColorPalette.AccentLime,        'N', 1, "notes"));
            _allIcons.Add(CreateIconGO(iRT, AppType.Music,      "Music",       ColorPalette.AccentPink,        'M', 2, "music"));
            _allIcons.Add(CreateIconGO(iRT, AppType.Browser,    "Browser",     new Color(1f, 0.85f, 0.2f),    'B', 3, "browser"));
            _allIcons.Add(CreateIconGO(iRT, AppType.Settings,   "Settings",    new Color(0.7f, 0.4f, 1f),     'S', 4, "settings"));

            // Антивирус — скрыт, появится в Сцене 5
            _antivirusIconGO = CreateIconGO(iRT, AppType.Antivirus, "Antivirus",
                new Color(0.4f, 0.95f, 0.55f), 'M', 5, "antivirus");
            _antivirusIconGO.SetActive(false);
            _allIcons.Add(_antivirusIconGO);

            // ── Колонка 2 — Game/Archive/Drawing, скрыты до нужных сцен ─────
            var iconCol2 = new GameObject("IconColumn2", typeof(RectTransform));
            iconCol2.transform.SetParent(rt, false);
            var iRT2 = (RectTransform)iconCol2.transform;
            iRT2.anchorMin = new Vector2(0, 1);
            iRT2.anchorMax = new Vector2(0, 1);
            iRT2.pivot = new Vector2(0, 1);
            iRT2.anchoredPosition = new Vector2(155f, -40f);
            iRT2.sizeDelta = new Vector2(110, 960);

            _gameIconGO = CreateIconGO(iRT2, AppType.Game, "Game",
                new Color(1f, 0.65f, 0.85f), 'G', 0, "game");
            _gameIconGO.SetActive(false);
            _allIcons.Add(_gameIconGO);

            _archiveIconGO = CreateIconGO(iRT2, AppType.Archive, "Archive",
                new Color(0.6f, 0.7f, 1f), 'A', 1, "archive");
            _archiveIconGO.SetActive(false);
            _allIcons.Add(_archiveIconGO);

            _drawingIconGO = CreateIconGO(iRT2, AppType.Drawing, "Drawing",
                new Color(1f, 0.8f, 0.6f), 'D', 2, "drawing");
            _drawingIconGO.SetActive(false);
            _allIcons.Add(_drawingIconGO);

            // Декоративные чиби
            var chibiLayer = new GameObject("ChibiLayer", typeof(RectTransform));
            chibiLayer.transform.SetParent(rt, false);
            var chRT = (RectTransform)chibiLayer.transform;
            chRT.anchorMin = Vector2.zero;
            chRT.anchorMax = Vector2.one;
            chRT.offsetMin = new Vector2(120f, 64f);
            chRT.offsetMax = new Vector2(-10f, -10f);
            _chibiSpawner = chibiLayer.AddComponent<DesktopChibiSpawner>();
            _chibiSpawner.Build(chRT);

            // Слой окон
            var windowLayer = new GameObject("WindowLayer", typeof(RectTransform));
            windowLayer.transform.SetParent(rt, false);
            var wlRT = (RectTransform)windowLayer.transform;
            wlRT.anchorMin = Vector2.zero;
            wlRT.anchorMax = Vector2.one;
            wlRT.offsetMin = new Vector2(0, 64);
            wlRT.offsetMax = Vector2.zero;
            windowLayer.transform.SetAsLastSibling();

            // Таскбар
            var panelGO = new GameObject("SystemPanel", typeof(RectTransform), typeof(SystemPanel));
            panelGO.transform.SetParent(rt, false);
            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panel = panelGO.GetComponent<SystemPanel>();
            panel.Build(rt);
            panelGO.transform.SetAsLastSibling();

            // Менеджер окон
            var wm = gameObject.AddComponent<WindowManager>();
            wm.Init(wlRT, notify, panel);

            // Нарратив: Init сейчас, Start() ScenarioDirector запустит его когда Desktop станет активным
            var director = gameObject.AddComponent<ScenarioDirector>();
            _chibiSpawner.StartIntroDrop();
            director.Init(this);
        }

        // ─── Создать иконку (возвращает родительский GO иконки) ─────────────
        private static GameObject CreateIconGO(Transform parent, AppType app, string label,
            Color tint, char glyph, int index, string iconName = null)
        {
            Sprite sprite = null;
            if (iconName != null)
            {
                sprite = Resources.Load<Sprite>($"icons/{iconName}");
                if (sprite == null)
                {
                    var all = Resources.LoadAll<Sprite>($"icons/{iconName}");
                    if (all.Length > 0) sprite = all[0];
                }
            }
            var icon = DesktopIcon.Create(parent, app, label, tint, glyph, sprite);
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(0, -index * 120f);
            return icon.gameObject;
        }

        private static void CreateIcon(Transform parent, AppType app, string label,
            Color tint, char glyph, int index, string iconName = null)
        {
            CreateIconGO(parent, app, label, tint, glyph, index, iconName);
        }

        private static void CreateBubble(Transform parent, Vector2 pos, int size, Color color)
        {
            var b = new GameObject("Bubble", typeof(RectTransform), typeof(Image));
            b.transform.SetParent(parent, false);
            var rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img = b.GetComponent<Image>();
            img.sprite = TextureFactory.Circle(Mathf.Min(256, size), color);
            img.raycastTarget = false;
        }
    }
}
