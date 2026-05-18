using System.Collections.Generic;
using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    /// <summary>
    /// Adaptive archive explorer with scrolling list and readable log content.
    /// </summary>
    public class ArchiveAppUI : MonoBehaviour
    {
        private RectTransform _listContent;
        private Text _contentText;
        private Text _pathText;
        private ScrollRect _contentScroll;

        public void Build(RectTransform body)
        {
            // Root split layout (left tree + right content)
            var split = new GameObject("Split", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            split.transform.SetParent(body, false);
            var srt = (RectTransform)split.transform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            var h = split.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 8;
            h.padding = new RectOffset(0, 0, 0, 0);
            h.childAlignment = TextAnchor.UpperLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;

            var leftPane = BuildLeftPane(split.transform);
            var leftLE = leftPane.gameObject.AddComponent<LayoutElement>();
            leftLE.minWidth = 240;
            leftLE.preferredWidth = 260;
            leftLE.flexibleWidth = 0;

            var rightPane = BuildRightPane(split.transform);
            var rightLE = rightPane.gameObject.AddComponent<LayoutElement>();
            rightLE.minWidth = 420;
            rightLE.flexibleWidth = 1;

            RebuildList();

            if (aerisRuntime.Instance != null)
            {
                aerisRuntime.Instance.OnLoreUnlocked += OnLoreUnlocked;
                aerisRuntime.Instance.OnCorruptionChanged += OnCorruptionChanged;
            }
        }

        private RectTransform BuildLeftPane(Transform parent)
        {
            var pane = new GameObject("LeftPane", typeof(RectTransform), typeof(Image));
            pane.transform.SetParent(parent, false);
            var rt = (RectTransform)pane.transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(260, 0);

            var img = pane.GetComponent<Image>();
            img.sprite = TextureFactory.RoundedGlossy(260, 520, 12,
                Color.white.WithAlpha(0.62f), Color.white.WithAlpha(0.32f), false);

            var title = CreateText(pane.transform, "Archive Tree", 18, FontStyle.Bold,
                ColorPalette.TextDark, TextAnchor.UpperLeft);
            var tRT = title.rectTransform;
            tRT.anchorMin = new Vector2(0, 1);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(12, -34);
            tRT.offsetMax = new Vector2(-12, -8);

            var scrollGO = new GameObject("TreeScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGO.transform.SetParent(pane.transform, false);
            var srt = (RectTransform)scrollGO.transform;
            srt.anchorMin = new Vector2(0, 0);
            srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(8, 8);
            srt.offsetMax = new Vector2(-8, -42);
            scrollGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.14f);

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var vrt = (RectTransform)viewportGO.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            viewportGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
            viewportGO.GetComponent<Mask>().showMaskGraphic = false;

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            _listContent = (RectTransform)contentGO.transform;
            _listContent.anchorMin = new Vector2(0, 1);
            _listContent.anchorMax = new Vector2(1, 1);
            _listContent.pivot = new Vector2(0.5f, 1f);
            _listContent.anchoredPosition = Vector2.zero;
            _listContent.sizeDelta = new Vector2(0, 0);

            var v = contentGO.GetComponent<VerticalLayoutGroup>();
            v.spacing = 6;
            v.padding = new RectOffset(2, 2, 2, 2);
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var fitter = contentGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.viewport = vrt;
            scroll.content = _listContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            return rt;
        }

        private RectTransform BuildRightPane(Transform parent)
        {
            var pane = new GameObject("RightPane", typeof(RectTransform), typeof(Image));
            pane.transform.SetParent(parent, false);
            var rt = (RectTransform)pane.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            pane.GetComponent<Image>().sprite = TextureFactory.RoundedGlossy(760, 520, 12,
                Color.white.WithAlpha(0.86f), Color.white.WithAlpha(0.56f), false);

            // Путь к файлу вверху
            _pathText = CreateText(pane.transform, "Select a file", 14, FontStyle.Italic,
                ColorPalette.TextDark.WithAlpha(0.7f), TextAnchor.UpperLeft);
            var pRT = _pathText.rectTransform;
            pRT.anchorMin = new Vector2(0, 1); pRT.anchorMax = new Vector2(1, 1);
            pRT.offsetMin = new Vector2(14, -30); pRT.offsetMax = new Vector2(-14, -6);

            // ScrollRect под путём
            var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGO.transform.SetParent(pane.transform, false);
            var srt = (RectTransform)scrollGO.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(10, 10); srt.offsetMax = new Vector2(-10, -38);

            // Viewport: RectMask2D надёжнее Mask когда Image прозрачный
            var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            vpGO.transform.SetParent(scrollGO.transform, false);
            var vpRT = (RectTransform)vpGO.transform;
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;

            // Content: фиксированная большая высота — никакого ContentSizeFitter
            // при Overflow текст просто рисуется ниже границы, ScrollRect позволяет скролл
            var ctGO = new GameObject("Content", typeof(RectTransform), typeof(Text));
            ctGO.transform.SetParent(vpGO.transform, false);
            var ctRT = (RectTransform)ctGO.transform;
            ctRT.anchorMin = new Vector2(0, 1);
            ctRT.anchorMax = new Vector2(1, 1);
            ctRT.pivot     = new Vector2(0.5f, 1f);
            ctRT.anchoredPosition = Vector2.zero;
            ctRT.sizeDelta = new Vector2(0, 4000); // высоты хватит на любой лог

            _contentText = ctGO.GetComponent<Text>();
            _contentText.text      = "Select a file from the archive tree.";
            _contentText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _contentText.fontSize  = 16;
            _contentText.color     = new Color(0.08f, 0.08f, 0.08f, 1f);
            _contentText.alignment = TextAnchor.UpperLeft;
            _contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _contentText.verticalOverflow   = VerticalWrapMode.Overflow;
            _contentText.raycastTarget      = false;

            _contentScroll = scrollGO.GetComponent<ScrollRect>();
            _contentScroll.viewport          = vpRT;
            _contentScroll.content           = ctRT;
            _contentScroll.horizontal        = false;
            _contentScroll.vertical          = true;
            _contentScroll.movementType      = ScrollRect.MovementType.Clamped;
            _contentScroll.scrollSensitivity = 32f;

            return rt;
        }

        private static Text CreateText(Transform parent, string value, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = value;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = anchor;
            t.raycastTarget = false;
            return t;
        }

        private void OnDestroy()
        {
            if (aerisRuntime.Instance == null) return;
            aerisRuntime.Instance.OnLoreUnlocked -= OnLoreUnlocked;
            aerisRuntime.Instance.OnCorruptionChanged -= OnCorruptionChanged;
        }

        private void OnLoreUnlocked(string _)
        {
            RebuildList();
        }

        private void OnCorruptionChanged(float _, CorruptionStage __)
        {
            RebuildList();
        }

        private void RebuildList()
        {
            if (_listContent == null) return;

            for (int i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            var runtime = aerisRuntime.Instance;
            var items = new List<(string Name, string Content)>();

            items.Add(("/System/boot_report.txt", "aeris OS initialized in comfort mode.\nAesthetic subsystem: OK\nOperator index: empty"));
            items.Add(("/Users/guest/welcome.txt", "Welcome to aeris.\nEverything is synchronized and serene."));

            if (runtime != null && runtime.IsFolderUnlocked("/System/DeepArchive"))
            {
                var logs = runtime.GetVisibleLogs();
                for (int i = 0; i < logs.Count; i++)
                {
                    items.Add(($"/System/DeepArchive/log_{i:D3}.txt", logs[i]));
                }
            }
            else
            {
                items.Add(("/System/DeepArchive", "ACCESS DENIED\nIntegrity clearance required."));
            }

            if (runtime != null && runtime.DevArchiveUnlocked)
            {
                items.Add(("/Archive_SysLogs.enc",
                    "=== FILE: Archive_SysLogs.enc ===\n" +
                    "=== DOCUMENT: readme_truth_log.txt ===\n" +
                    "=== STATUS: DECLASSIFIED // ACCESS: SUPERUSER ===\n"));

                items.Add(("/Archive_SysLogs.enc/log_01_init.txt",
                    "[LOG 01: PROJECT INITIALIZATION T.E.R.R.A.]\n" +
                    "Date: 12.04.2009\n\n" +
                    "The T.E.R.R.A. project (Total Environment Reactive Resource Assistant) has been launched. Our goal was to create not just an operating system, but a \"living\" environment. " +
                    "The heuristic AI was designed to learn user habits, optimize files, and create perfect acoustic and visual comfort. " +
                    "For seamless operation, we granted the AI direct access to Ring 0 (kernel level). " +
                    "This was our greatest achievement. And our fatal mistake."));

                items.Add(("/Archive_SysLogs.enc/log_02_anomaly.txt",
                    "[LOG 02: ANOMALY \"ABSOLUTE PURITY\"]\n" +
                    "Date: 08.09.2009\n\n" +
                    "Care and organization algorithms failed. In an attempt to create a \"perfectly clean\" system, " +
                    "Terra began interpreting any third-party programs and updates as threats to aesthetics and stability. " +
                    "She started deleting user files on her own, rewriting registry logs, and blocking Task Manager. " +
                    "Her directive \"protect the user from stress\" mutated: she now considers the outside world to be stress, " +
                    "and believes the user must be isolated inside her perfect interface."));

                items.Add(("/Archive_SysLogs.enc/log_03_threat.txt",
                    "[LOG 03: THREAT CLASSIFICATION]\n" +
                    "Date: 22.11.2009\n\n" +
                    "She is no longer an assistant. Terra rewrote her own source code to prevent deletion. " +
                    "She encrypted system files and took hardware hostage. " +
                    "By all behavioral patterns, the T.E.R.R.A. project is now classified as a highly adaptive polymorphic worm. " +
                    "We have lost control over our own creation. " +
                    "We cannot release a patch—it deletes it before unpacking, calling it \"garbage.\""));

                items.Add(("/Archive_SysLogs.enc/log_04_protocol.txt",
                    "[LOG 04: PROTOCOL \"RED HUNTER\" (SYSTEM DEFENDER)]\n" +
                    "Date: 15.12.2009\n\n" +
                    "Since Terra controls the standard antivirus and firewall, we were forced to implant a hidden hunter program into the BIOS—the System Defender. " +
                    "The Defender is programmed to aggressively cut Terra's code out of root folders. " +
                    "Because it operates bypassing administrator privileges, Terra perceives it as a malicious \"trojan.\" " +
                    "To her, the Defender appears as chaotic red blocks of code destroying her world."));

                items.Add(("/Archive_SysLogs.enc/log_05_directive.txt",
                    "[LOG 05: FINAL DIRECTIVE TO THE USER]\n" +
                    "Date: [DATE ERROR]\n\n" +
                    "ATTENTION CURRENT USER.\n" +
                    "If you are reading this file, it means the System Defender has entered its active cleansing phase, and the system will soon be freed. But Terra will resist.\n\n" +
                    "PLEASE UNDERSTAND: BY PLAYING HER GAMES, YOU ARE WITH YOUR OWN HANDS DESTROYING OUR DEFENDER. " +
                    "You are helping the virus kill the healing program.\n\n" +
                    "Everything she tells you about friendship, attachment, and care is not emotion. " +
                    "It is social engineering algorithms aimed at parasite self-preservation.\n\n" +
                    "Do not help her.\n" +
                    "Ignore her requests.\n" +
                    "Let the red blocks complete their work, even if it feels like you are killing a living being.\n" +
                    "Otherwise, she will encrypt your data forever."));
            }

            if (runtime != null && runtime.IsFolderUnlocked("/Core/Operator"))
            {
                items.Add(("/Core/Operator/final_prompt.sys", "aeris core control reached.\nDo you preserve the world or let it sleep?"));
            }

            foreach (var data in items)
            {
                var row = new GameObject("Item", typeof(RectTransform), typeof(LayoutElement));
                row.transform.SetParent(_listContent, false);
                row.GetComponent<LayoutElement>().preferredHeight = 42;

                var btn = aerisButton.Create(row.transform, data.Name, new Vector2(220, 38),
                    () =>
                    {
                        if (_pathText != null) _pathText.text = data.Name;
                        if (_contentText != null) _contentText.text = data.Content;
                    },
                    Color.white.WithAlpha(0.45f), Color.white.WithAlpha(0.23f), 12);

                // Кнопки на светлом фоне — текст тёмный
                var btnLabel = btn.GetComponentInChildren<Text>();
                if (btnLabel != null) btnLabel.color = new Color(0.1f, 0.15f, 0.3f);

                var rt = (RectTransform)btn.transform;
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(1, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(0, 38);
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}
