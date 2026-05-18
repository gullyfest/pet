using System.Collections.Generic;
using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    internal abstract class AntivirusMiniGameBase : MonoBehaviour
    {
        public event System.Action OnCompleted;

        protected RectTransform Root;
        protected Text Hint;
        protected bool IsDone;
        protected Text RuleText;

        public void Initialize(Text hint)
        {
            Root = (RectTransform)transform;
            Hint = hint;
            Build();
        }

        protected abstract void Build();

        protected void Finish(string message)
        {
            if (IsDone) return;
            IsDone = true;
            if (Hint != null) Hint.text = message;
            OnCompleted?.Invoke();
        }

        protected void BuildRuleCard(string _, string rules) { }

        protected static Text MakeText(Transform parent, string text, int size, FontStyle style,
            TextAnchor align, Color color, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        protected static Image MakePanel(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color top, Color bottom, bool gloss = false)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var img = go.GetComponent<Image>();
            img.sprite = TextureFactory.RoundedGlossy(720, 420, 14, top, bottom, gloss);
            return img;
        }

        protected static RectTransform MakeClipArea(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject("ClipArea", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.001f);
            img.raycastTarget = false;
            return rt;
        }

        protected static void FitTitleToParent(Text title, RectTransform parent, int max, int min)
        {
            if (title == null || parent == null) return;
            float w = Mathf.Max(240f, parent.rect.width);
            float scale = Mathf.InverseLerp(240f, 920f, w);
            title.fontSize = Mathf.RoundToInt(Mathf.Lerp(min, max, scale));
        }
    }

    internal class BubbleCleanerMiniGame : AntivirusMiniGameBase
    {
        private readonly List<MovingBubble> _bubbles = new List<MovingBubble>();
        private RectTransform _playArea;
        private Text _title;
        private int _corruptedLeft;
        private int _mistakes;

        protected override void Build()
        {
            MakePanel(transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.28f, 0.52f, 0.78f, 0.88f), new Color(0.12f, 0.32f, 0.62f, 0.88f), true);

            _playArea = new GameObject("PlayArea", typeof(RectTransform)).GetComponent<RectTransform>();
            _playArea.SetParent(transform, false);
            _playArea.anchorMin = new Vector2(0.02f, 0.06f);
            _playArea.anchorMax = new Vector2(0.98f, 0.9f);
            _playArea.offsetMin = Vector2.zero;
            _playArea.offsetMax = Vector2.zero;

            _title = MakeText(transform, "Bubble Cleaner", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildRuleCard("How to play",
                "Click only glitched bubbles. Avoid clear bubbles.\n2 mistakes trigger full field reset.");

            SpawnBubbles();
            Hint.text = "Catch corrupted bubbles with visual glitches.";
        }

        private void SpawnBubbles()
        {
            _bubbles.Clear();
            _corruptedLeft = 0;

            int total = 18;
            int corruptedTarget = 9;
            if (aerisRuntime.Instance != null && aerisRuntime.Instance.Stage >= CorruptionStage.Distorted)
            {
                total = 22;
                corruptedTarget = 12;
            }

            for (int i = 0; i < total; i++)
            {
                bool corrupted = i < corruptedTarget;
                if (corrupted) _corruptedLeft++;

                var go = new GameObject("Bubble", typeof(RectTransform), typeof(Image), typeof(BubbleClickTarget));
                go.transform.SetParent(_playArea, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                float sizeN = Random.Range(0.08f, 0.14f);
                var img = go.GetComponent<Image>();
                img.sprite = TextureFactory.Circle(128,
                    corrupted ? new Color(0.05f, 0.55f, 1f, 0.96f) : new Color(0.96f, 0.98f, 1f, 0.82f));

                var model = new MovingBubble
                {
                    Rect = rt,
                    Image = img,
                    NormalizedPos = new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.15f, 0.85f)),
                    Velocity = Random.insideUnitCircle.normalized * Random.Range(0.12f, 0.22f),
                    SizeN = sizeN,
                    Corrupted = corrupted,
                    Phase = Random.Range(0f, 6.2f)
                };

                var click = go.GetComponent<BubbleClickTarget>();
                click.OnClicked = () => OnBubbleClicked(model);
                _bubbles.Add(model);
            }
        }

        private void OnBubbleClicked(MovingBubble bubble)
        {
            if (IsDone) return;

            if (bubble.Corrupted)
            {
                bubble.Corrupted = false;
                bubble.Image.color = new Color(0.96f, 0.98f, 1f, 0.82f);
                bubble.Rect.localScale = Vector3.one;
                bubble.Velocity *= 0.55f;
                _corruptedLeft--;
                Hint.text = _corruptedLeft > 0
                    ? $"Corrupted bubbles left: {_corruptedLeft}"
                    : "Bubble field stabilized.";

                if (_corruptedLeft <= 0)
                {
                    Finish("Bubble Cleaner complete.");
                }
            }
            else
            {
                _mistakes++;
                bubble.Image.color = new Color(1f, 0.78f, 0.78f, 0.5f);
                Hint.text = $"Wrong bubble. Mistakes: {_mistakes}/2";
                if (_mistakes >= 2)
                {
                    Hint.text = "Too many mistakes. Bubble field reinitialized.";
                    for (int i = _playArea.childCount - 1; i >= 0; i--) Destroy(_playArea.GetChild(i).gameObject);
                    SpawnBubbles();
                    _mistakes = 0;
                }
            }
        }

        private void Update()
        {
            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            if (_playArea == null || _bubbles.Count == 0) return;
            float w = Mathf.Max(200f, _playArea.rect.width);
            float h = Mathf.Max(120f, _playArea.rect.height);

            foreach (var bubble in _bubbles)
            {
                bubble.NormalizedPos += bubble.Velocity * Time.unscaledDeltaTime;
                if (bubble.NormalizedPos.x < 0.05f || bubble.NormalizedPos.x > 0.95f)
                    bubble.Velocity.x *= -1f;
                if (bubble.NormalizedPos.y < 0.08f || bubble.NormalizedPos.y > 0.92f)
                    bubble.Velocity.y *= -1f;

                bubble.NormalizedPos.x = Mathf.Clamp(bubble.NormalizedPos.x, 0.05f, 0.95f);
                bubble.NormalizedPos.y = Mathf.Clamp(bubble.NormalizedPos.y, 0.08f, 0.92f);

                float size = Mathf.Min(w, h) * bubble.SizeN;
                bubble.Rect.sizeDelta = new Vector2(size, size);

                Vector2 pos = new Vector2((bubble.NormalizedPos.x - 0.5f) * w, (bubble.NormalizedPos.y - 0.5f) * h);
                float bob = Mathf.Sin(Time.unscaledTime * 2f + bubble.Phase) * 6f;
                bubble.Rect.anchoredPosition = pos + new Vector2(0f, bob);

                if (bubble.Corrupted)
                {
                    float g = Mathf.PerlinNoise(Time.unscaledTime * 8f, bubble.Phase);
                    // резкое мерцание между синим и бирюзово-белым
                    bubble.Image.color = g > 0.65f
                        ? new Color(0.7f, 0.95f, 1f, 1f)
                        : new Color(0.05f, 0.45f + g * 0.3f, 1f, 0.96f);
                    // лёгкое масштабирование — видно что пузырь "дышит" не так
                    bubble.Rect.localScale = Vector3.one * (0.9f + Mathf.Sin(Time.unscaledTime * 12f + bubble.Phase) * 0.08f);
                }
                else
                {
                    bubble.Rect.localScale = Vector3.one;
                }
            }
        }

        private class MovingBubble
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 NormalizedPos;
            public Vector2 Velocity;
            public float SizeN;
            public bool Corrupted;
            public float Phase;
        }
    }

    internal class CloudSorterMiniGame : AntivirusMiniGameBase
    {
        private static readonly string[] BinNames   = { "Blue", "White", "Pink" };
        private static readonly Color[]  BinTop     = { new Color(0.38f,0.62f,1f,0.92f), new Color(0.82f,0.86f,0.92f,0.92f), new Color(1f,0.6f,0.78f,0.92f) };
        private static readonly Color[]  BinBot     = { new Color(0.22f,0.44f,0.92f,0.92f), new Color(0.62f,0.66f,0.74f,0.92f), new Color(0.92f,0.38f,0.62f,0.92f) };
        private static readonly Color[]  BinText    = { Color.white, new Color(0.1f,0.16f,0.28f,1f), Color.white };
        private static readonly Color[]  CloudColor = { new Color(0.36f,0.62f,1f,0.96f), new Color(0.88f,0.91f,0.96f,0.96f), new Color(1f,0.62f,0.8f,0.96f) };

        private RectTransform[] _binRects;
        private RectTransform   _playArea;
        private Text            _title;
        private Text            _counterText;

        private readonly List<FallingEntry> _active = new List<FallingEntry>();
        private int   _sorted;
        private int   _missed;
        private float _spawnTimer;
        private const int TargetSorted = 12;
        private const int MaxMissed    = 3;

        private class FallingEntry
        {
            public RectTransform  Rect;
            public CloudDragObject Drag;
            public int  Lane;
            public float NormX;   // 0..1 горизонтально
            public float NormY;   // 0=top  1=bottom
        }

        protected override void Build()
        {
            MakePanel(transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.78f, 0.92f, 1f, 0.55f), new Color(0.55f, 0.78f, 1f, 0.32f), true);

            _title = MakeText(transform, "Cloud Sorter", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildRuleCard("How to play",
                "Drag falling clouds to the matching color zone before they fall off.\n3 missed = restart.");

            // зона падения облаков
            _playArea = new GameObject("PlayArea", typeof(RectTransform)).GetComponent<RectTransform>();
            _playArea.SetParent(transform, false);
            _playArea.anchorMin = new Vector2(0.02f, 0.28f);
            _playArea.anchorMax = new Vector2(0.98f, 0.94f);
            _playArea.offsetMin = _playArea.offsetMax = Vector2.zero;

            // бины внизу
            _binRects = new RectTransform[3];
            for (int i = 0; i < 3; i++)
            {
                var panel = MakePanel(transform,
                    new Vector2(0.03f + i * 0.325f, 0.04f), new Vector2(0.32f + i * 0.325f, 0.26f),
                    Vector2.zero, Vector2.zero, BinTop[i], BinBot[i], true);
                _binRects[i] = panel.rectTransform;
                MakeText(panel.transform, BinNames[i], 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                    BinText[i], Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            // счётчик прогресса
            var cPanel = MakePanel(transform, new Vector2(0.72f, 0.88f), new Vector2(0.98f, 0.98f),
                Vector2.zero, Vector2.zero, new Color(0.08f,0.18f,0.38f,0.85f), new Color(0.04f,0.1f,0.24f,0.85f), false);
            MakeText(cPanel.transform, "SORTED", 10, FontStyle.Bold, TextAnchor.UpperCenter,
                new Color(0.55f,0.82f,1f,0.85f), new Vector2(0,1), new Vector2(1,1), new Vector2(0,-3), new Vector2(0,-3));
            _counterText = MakeText(cPanel.transform, $"0 / {TargetSorted}", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                Color.white, Vector2.zero, Vector2.one, new Vector2(0,0), new Vector2(0,-14));

            _spawnTimer = 0f;
            Hint.text = $"Sort {TargetSorted} clouds. Don't let them fall!";
        }

        private void SpawnCloud()
        {
            int lane = Random.Range(0, 3);
            var color = CloudColor[lane];

            var go = new GameObject("FallingCloud", typeof(RectTransform), typeof(Image), typeof(CloudDragObject));
            go.transform.SetParent(_playArea, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            float bs = 80f;
            rt.sizeDelta = new Vector2(bs * 2.2f, bs * 1.4f);

            go.GetComponent<Image>().color = new Color(0,0,0,0); // невидимый, только raycast
            BuildCloudPuffs(go.transform, bs, color);

            float normX = Random.Range(0.12f, 0.88f);
            var entry = new FallingEntry { Rect = rt, Drag = go.GetComponent<CloudDragObject>(), Lane = lane, NormX = normX, NormY = 0f };

            entry.Drag.OnDropped = pos => OnDrop(entry, pos);
            _active.Add(entry);
        }

        private static void BuildCloudPuffs(Transform parent, float bs, Color color)
        {
            (Vector2 pos, float s)[] puffs =
            {
                (new Vector2(-bs*0.48f, -bs*0.05f), 0.78f),
                (new Vector2(0f,         bs*0.08f),  1.0f),
                (new Vector2( bs*0.48f, -bs*0.05f), 0.78f),
                (new Vector2(-bs*0.22f,  bs*0.42f), 0.62f),
                (new Vector2( bs*0.22f,  bs*0.42f), 0.62f),
            };
            foreach (var (pos, s) in puffs)
            {
                var g = new GameObject("Puff", typeof(RectTransform), typeof(Image));
                g.transform.SetParent(parent, false);
                var prt = (RectTransform)g.transform;
                prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.pivot = new Vector2(0.5f, 0.5f);
                prt.anchoredPosition = pos;
                float d = bs * s;
                prt.sizeDelta = new Vector2(d, d);
                var img = g.GetComponent<Image>();
                img.sprite = TextureFactory.Circle(64, color);
                img.raycastTarget = false;
            }
        }

        private void OnDrop(FallingEntry entry, Vector2 screenPos)
        {
            if (!_active.Contains(entry)) return;

            for (int i = 0; i < _binRects.Length; i++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_binRects[i], screenPos, null)) continue;
                if (i == entry.Lane)
                {
                    _sorted++;
                    if (_counterText != null) _counterText.text = $"{_sorted} / {TargetSorted}";
                    _active.Remove(entry);
                    Destroy(entry.Rect.gameObject);
                    Hint.text = $"Sorted: {_sorted}/{TargetSorted}  Missed: {_missed}/{MaxMissed}";
                    if (_sorted >= TargetSorted) Finish("Cloud Sort complete.");
                }
                else
                {
                    Hint.text = "Wrong zone — cloud continues falling!";
                }
                return;
            }
            // промах мимо бина — облако продолжает падать
        }

        private void Update()
        {
            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            if (IsDone) return;

            _spawnTimer -= Time.unscaledDeltaTime;
            if (_spawnTimer <= 0f && _active.Count < 3)
            {
                SpawnCloud();
                _spawnTimer = Random.Range(0.9f, 1.6f);
            }

            if (_playArea == null || _playArea.rect.width < 1f) return;
            float aW = _playArea.rect.width;
            float aH = _playArea.rect.height;
            float speed = 0.076f + _sorted * 0.004f; // ускоряется по мере прогресса

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var c = _active[i];
                if (c.Rect == null) { _active.RemoveAt(i); continue; }

                if (!c.Drag.IsDragging)
                {
                    c.NormY += Time.unscaledDeltaTime * speed;
                    float x = (c.NormX - 0.5f) * aW;
                    float y = (0.5f - c.NormY) * aH;
                    c.Rect.anchoredPosition = new Vector2(x, y);
                }

                if (c.NormY > 1.08f)
                {
                    _missed++;
                    Destroy(c.Rect.gameObject);
                    _active.RemoveAt(i);
                    if (_missed >= MaxMissed)
                    {
                        ResetGame();
                    }
                    else
                    {
                        Hint.text = $"Cloud missed! {_missed}/{MaxMissed} allowed.";
                    }
                }
            }
        }

        private void ResetGame()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Rect != null) Destroy(_active[i].Rect.gameObject);
            }
            _active.Clear();
            _sorted = 0;
            _missed = 0;
            if (_counterText != null) _counterText.text = $"0 / {TargetSorted}";
            _spawnTimer = 0.5f;
            Hint.text = "Too many missed! Restarting...";
        }
    }

    internal class PixelRepairMiniGame : AntivirusMiniGameBase
    {
        private readonly List<PixelCell> _cells = new List<PixelCell>();
        private GridLayoutGroup _layout;
        private RectTransform _grid;
        private RectTransform _gridClip;
        private RectTransform _scanButtonRT;
        private Text _title;
        private int _broken;
        private int _hiddenBroken;
        private int _scanCharges;
        private int _repairStreak;
        private int _lastColumns;
        private float _spreadTimer = 5f;

        protected override void Build()
        {
            MakePanel(transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.85f, 0.94f, 1f, 0.52f), new Color(0.62f, 0.78f, 0.96f, 0.35f), false);

            _title = MakeText(transform, "Pixel Repair", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildRuleCard("How to play",
                "Repair red pixels. Hidden faults are masked in blue.\nUse Scan Sweep to reveal hidden faults and clear all damage.");

            _gridClip = MakeClipArea(transform, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.78f),
                Vector2.zero, Vector2.zero);

            var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGO.transform.SetParent(_gridClip, false);
            _grid = (RectTransform)gridGO.transform;
            _grid.anchorMin = Vector2.zero;
            _grid.anchorMax = Vector2.one;
            _grid.offsetMin = Vector2.zero;
            _grid.offsetMax = Vector2.zero;

            _layout = gridGO.GetComponent<GridLayoutGroup>();
            _layout.spacing = new Vector2(4f, 4f);
            _layout.padding = new RectOffset(4, 4, 4, 4);
            _layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _layout.constraintCount = 12;
            _layout.childAlignment = TextAnchor.UpperCenter;

            var scanBtn = aerisButton.Create(transform, "Scan Sweep", new Vector2(170, 40), TriggerScanSweep,
                ColorPalette.AeroCyan, new Color(0f, 0.45f, 0.85f), 18);
            _scanButtonRT = (RectTransform)scanBtn.transform;
            _scanButtonRT.anchorMin = _scanButtonRT.anchorMax = new Vector2(1f, 1f);
            _scanButtonRT.pivot = new Vector2(1f, 1f);
            _scanButtonRT.anchoredPosition = new Vector2(-16f, -8f);
            _scanButtonRT.SetAsLastSibling();

            BuildCells();
            Hint.text = $"Repair fractured pixels: {_broken} remaining.";
        }

        private void BuildCells()
        {
            _cells.Clear();
            _broken = 0;
            _hiddenBroken = 0;
            _repairStreak = 0;
            _scanCharges = 3;

            const int count = 72; // 12×6 — всегда помещается на экран
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Cell", typeof(RectTransform), typeof(Image), typeof(PixelCell));
                go.transform.SetParent(_grid, false);
                var img = go.GetComponent<Image>();

                bool broken = Random.value > 0.58f;
                bool hiddenBroken = broken && Random.value > 0.52f;
                if (broken) _broken++;
                if (hiddenBroken) _hiddenBroken++;

                var cell = go.GetComponent<PixelCell>();
                cell.Initialize(this, img, broken, hiddenBroken);
                _cells.Add(cell);
            }

            Hint.text = $"Repair fractured pixels: {_broken} remaining. Hidden faults: {_hiddenBroken}. Sweeps: {_scanCharges}";
        }

        public void Repair(PixelCell cell)
        {
            if (cell == null || !cell.Broken || IsDone) return;

            if (cell.IsHiddenBroken)
            {
                Hint.text = "This fault is masked. Use Scan Sweep first.";
                _repairStreak = 0;
                return;
            }

            cell.SetBroken(false);
            _broken--;
            _repairStreak++;
            if (_repairStreak >= 5)
            {
                _repairStreak = 0;
                _scanCharges++;
                Hint.text = $"Repair streak bonus. Extra sweep granted. Sweeps: {_scanCharges}";
            }
            else
            {
                Hint.text = _broken > 0
                    ? $"Repair fractured pixels: {_broken} remaining. Hidden faults: {_hiddenBroken}. Sweeps: {_scanCharges}"
                    : "Pixel matrix stabilized.";
            }

            if (_broken <= 0)
            {
                Finish("Pixel Repair complete.");
            }
        }

        private void TriggerScanSweep()
        {
            if (IsDone) return;
            if (_scanCharges <= 0)
            {
                Hint.text = "No sweeps left. Build a repair streak to regain scans.";
                return;
            }

            int revealed = 0;
            int start = Random.Range(0, Mathf.Max(1, _cells.Count));
            for (int i = 0; i < _cells.Count && revealed < 4; i++)
            {
                int idx = (start + i) % _cells.Count;
                if (_cells[idx].RevealHiddenFault())
                {
                    revealed++;
                }
            }

            _scanCharges--;
            _repairStreak = 0;
            if (revealed > 0)
            {
                _hiddenBroken = Mathf.Max(0, _hiddenBroken - revealed);
                Hint.text = $"Scan Sweep revealed {revealed} hidden faults. Hidden left: {_hiddenBroken}. Sweeps: {_scanCharges}";
            }
            else
            {
                Hint.text = $"Scan Sweep found no hidden faults. Sweeps: {_scanCharges}";
            }
        }

        private void SpreadCorruption()
        {
            var sources = new List<int>();
            for (int i = 0; i < _cells.Count; i++)
                if (_cells[i].Broken) sources.Add(i);

            foreach (int idx in sources)
            {
                foreach (int n in GetNeighborIndices(idx))
                {
                    if (!_cells[n].Broken && Random.value < 0.17f)
                    {
                        _cells[n].SetBroken(true, false);
                        _broken++;
                    }
                }
            }
            Hint.text = $"Corruption spreading! Fractured pixels: {_broken}. Sweeps: {_scanCharges}";
        }

        private static List<int> GetNeighborIndices(int idx)
        {
            const int cols = 12, rows = 6;
            var result = new List<int>(4);
            int c = idx % cols, r = idx / cols;
            if (c > 0) result.Add(idx - 1);
            if (c < cols - 1) result.Add(idx + 1);
            if (r > 0) result.Add(idx - cols);
            if (r < rows - 1) result.Add(idx + cols);
            return result;
        }

        private void Update()
        {
            if (_grid == null || _layout == null) return;

            FitTitleToParent(_title, transform as RectTransform, 24, 18);

            if (!IsDone)
            {
                _spreadTimer -= Time.unscaledDeltaTime;
                if (_spreadTimer <= 0f)
                {
                    SpreadCorruption();
                    // интервал сжимается при большом заражении
                    _spreadTimer = Mathf.Lerp(5f, 2.5f, Mathf.Clamp01(_broken / 40f));
                }
            }
            var root = transform as RectTransform;
            if (root != null && _scanButtonRT != null)
            {
                float wRoot = Mathf.Max(360f, root.rect.width);
                float scaleRoot = Mathf.InverseLerp(360f, 900f, wRoot);
                _scanButtonRT.sizeDelta = new Vector2(
                    Mathf.Lerp(130f, 170f, scaleRoot),
                    Mathf.Lerp(32f, 40f, scaleRoot));
            }

            float width = Mathf.Max(220f, _grid.rect.width - _layout.padding.left - _layout.padding.right);
            float height = Mathf.Max(100f, _grid.rect.height - _layout.padding.top - _layout.padding.bottom);
            const int cols = 12;
            const int rows = 6;
            _layout.constraintCount = cols;
            float cellW = (width  - _layout.spacing.x * (cols - 1)) / cols;
            float cellH = (height - _layout.spacing.y * (rows - 1)) / rows;
            float cell = Mathf.Clamp(Mathf.Min(cellW, cellH), 18f, 72f);
            _layout.cellSize = new Vector2(cell, cell);
        }
    }

    internal class DigitalGardenMiniGame : AntivirusMiniGameBase
    {
        private float _water = 0.4f;
        private float _light = 0.5f;
        private int _rootsLeft = 7;
        private int _weedsLeft = 5;
        private Image _waterFill;
        private Image _lightFill;
        private RectTransform _waterBar;
        private RectTransform _lightBar;
        private Text _status;
        private RectTransform _gardenStage;
        private RectTransform _gardenClip;
        private Image _stabilityAura;
        private Image _plantCore;
        private Text _title;
        private readonly List<Image> _petals = new List<Image>();
        private readonly List<RectTransform> _roots = new List<RectTransform>();
        private readonly List<RectTransform> _weeds = new List<RectTransform>();
        private float _eventTimer = 9f;

        protected override void Build()
        {
            MakePanel(transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.82f, 0.96f, 0.85f, 0.58f), new Color(0.58f, 0.82f, 0.65f, 0.38f), true);

            _title = MakeText(transform, "Digital Garden", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildGauge(new Vector2(0.04f, 0.73f), "Water", out _waterFill, out _waterBar,
                () => _water = Mathf.Clamp01(_water + 0.07f), () => _water = Mathf.Clamp01(_water - 0.07f));
            BuildGauge(new Vector2(0.52f, 0.73f), "Light", out _lightFill, out _lightBar,
                () => _light = Mathf.Clamp01(_light + 0.07f), () => _light = Mathf.Clamp01(_light - 0.07f));

            _status = MakeText(transform, "", 15, FontStyle.Bold, TextAnchor.MiddleLeft,
                ColorPalette.TextDark, new Vector2(0.04f, 0.64f), new Vector2(0.96f, 0.72f), Vector2.zero, Vector2.zero);
            _status.horizontalOverflow = HorizontalWrapMode.Wrap;
            _status.verticalOverflow = VerticalWrapMode.Overflow;
            _status.lineSpacing = 1.02f;

            _gardenClip = MakeClipArea(transform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.68f),
                Vector2.zero, Vector2.zero);

            _gardenStage = MakePanel(_gardenClip, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(1f, 1f, 1f, 0.44f), new Color(0.78f, 0.95f, 0.82f, 0.22f), false).rectTransform;

            _stabilityAura = MakePanel(_gardenStage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                new Color(0.55f, 1f, 0.68f, 0.24f), new Color(0.32f, 0.82f, 0.58f, 0.1f), true);
            _stabilityAura.rectTransform.sizeDelta = new Vector2(270f, 270f);

            _plantCore = MakePanel(_gardenStage, new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f),
                Vector2.zero, Vector2.zero,
                new Color(0.3f, 0.75f, 0.38f, 0.98f), new Color(0.18f, 0.52f, 0.24f, 0.98f), true);
            _plantCore.rectTransform.sizeDelta = new Vector2(110f, 120f);

            for (int i = 0; i < 7; i++)
            {
                var leaf = MakePanel(_gardenStage, new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f),
                    Vector2.zero, Vector2.zero,
                    new Color(0.5f, 0.92f, 0.56f, 0.95f), new Color(0.22f, 0.65f, 0.3f, 0.95f), true);
                leaf.rectTransform.sizeDelta = new Vector2(56f, 84f);
                _petals.Add(leaf);
            }

            for (int i = 0; i < 5; i++)
            {
                var weed = new GameObject("Weed", typeof(RectTransform), typeof(Image), typeof(SimplePointerButton));
                weed.transform.SetParent(_gardenStage, false);
                var wrt = (RectTransform)weed.transform;
                wrt.anchorMin = wrt.anchorMax = i switch
                {
                    0 => new Vector2(0.14f, 0.22f),
                    1 => new Vector2(0.32f, 0.18f),
                    2 => new Vector2(0.68f, 0.18f),
                    3 => new Vector2(0.84f, 0.22f),
                    _ => new Vector2(0.5f, 0.16f)
                };
                wrt.pivot = new Vector2(0.5f, 0.5f);
                wrt.sizeDelta = new Vector2(54f, 54f);
                var wimg = weed.GetComponent<Image>();
                wimg.sprite = TextureFactory.Circle(64, new Color(0.18f, 0.75f, 0.22f, 0.95f));
                var wbtn = weed.GetComponent<SimplePointerButton>();
                wbtn.ClickAction = () =>
                {
                    if (IsDone || !weed.activeSelf) return;
                    weed.SetActive(false);
                    _weedsLeft = Mathf.Max(0, _weedsLeft - 1);
                    UpdateStatus();
                    CheckDone();
                };
                _weeds.Add(wrt);
            }

            for (int i = 0; i < 7; i++)
            {
                var root = new GameObject("Root", typeof(RectTransform), typeof(Image), typeof(SimplePointerButton));
                root.transform.SetParent(_gardenStage, false);
                var rt = (RectTransform)root.transform;
                rt.anchorMin = rt.anchorMax = i switch
                {
                    0 => new Vector2(0.12f, 0.08f),
                    1 => new Vector2(0.32f, 0.08f),
                    2 => new Vector2(0.52f, 0.08f),
                    3 => new Vector2(0.72f, 0.08f),
                    4 => new Vector2(0.2f, 0.2f),
                    5 => new Vector2(0.5f, 0.2f),
                    _ => new Vector2(0.8f, 0.2f)
                };
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(92, 42);
                var img = root.GetComponent<Image>();
                img.sprite = TextureFactory.RoundedGlossy(92, 42, 10,
                    new Color(0.47f, 0.24f, 0.14f, 0.95f), new Color(0.33f, 0.15f, 0.1f, 0.95f), false);

                var btn = root.GetComponent<SimplePointerButton>();
                btn.ClickAction = () =>
                {
                    if (IsDone || !root.activeSelf) return;
                    root.SetActive(false);
                    _rootsLeft--;
                    UpdateStatus();
                    CheckDone();
                };
                _roots.Add(rt);
            }

            UpdateStatus();
            Hint.text = "Remove Roots & Weeds, then balance Water/Light. Watch for random events!";
        }

        private void BuildGauge(Vector2 anchor, string label, out Image fill, out RectTransform barRect, System.Action plus, System.Action minus)
        {
            var panel = MakePanel(transform, anchor, anchor + new Vector2(0.42f, 0.2f), Vector2.zero, Vector2.zero,
                Color.white.WithAlpha(0.45f), Color.white.WithAlpha(0.2f));

            MakeText(panel.transform, label, 18, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -8), new Vector2(-12, -8));

            var bar = MakePanel(panel.transform, new Vector2(0.04f, 0.26f), new Vector2(0.82f, 0.58f),
                Vector2.zero, Vector2.zero, new Color(0.2f, 0.32f, 0.45f, 0.9f), new Color(0.08f, 0.18f, 0.3f, 0.9f));
            barRect = bar.rectTransform;

            fill = MakePanel(bar.transform, new Vector2(0, 0), new Vector2(0.5f, 1f),
                Vector2.zero, Vector2.zero, ColorPalette.AccentLime, new Color(0.2f, 0.65f, 0.28f), true);
            fill.rectTransform.pivot = new Vector2(0, 0.5f);
            fill.rectTransform.anchorMin = new Vector2(0, 0);
            fill.rectTransform.anchorMax = new Vector2(0, 1);

            var plusBtn = aerisButton.Create(panel.transform, "+", new Vector2(44, 32), plus,
                ColorPalette.AeroCyan, new Color(0f, 0.45f, 0.85f), 22);
            var prt = (RectTransform)plusBtn.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.91f, 0.64f);
            prt.pivot = new Vector2(0.5f, 0.5f);

            var minusBtn = aerisButton.Create(panel.transform, "-", new Vector2(44, 32), minus,
                ColorPalette.AccentPink, new Color(0.7f, 0.2f, 0.44f), 22);
            var mrt = (RectTransform)minusBtn.transform;
            mrt.anchorMin = mrt.anchorMax = new Vector2(0.91f, 0.28f);
            mrt.pivot = new Vector2(0.5f, 0.5f);
        }

        private void UpdateStatus()
        {
            float waterOk = Mathf.Abs(_water - 0.62f);
            float lightOk = Mathf.Abs(_light - 0.58f);
            bool balanced = waterOk < 0.1f && lightOk < 0.1f;

            string wState = _water < 0.35f ? "LOW!" : _water > 0.85f ? "HIGH" : "OK";
            string lState = _light < 0.35f ? "LOW!" : _light > 0.85f ? "HIGH" : "OK";
            _status.text = $"Roots: {_rootsLeft}  Weeds: {_weedsLeft}  Balance: {(balanced ? "STABLE ✓" : "Unstable")}" +
                $"\nWater: {wState}   Light: {lState}";

            if (_rootsLeft <= 0 && _weedsLeft <= 0 && !balanced && Hint != null)
                Hint.text = "Garden cleared! Now balance Water and Light to finish.";
            _waterFill.rectTransform.sizeDelta = new Vector2(0f, 0f);
            _lightFill.rectTransform.sizeDelta = new Vector2(0f, 0f);
            float waterWidth = _waterBar != null ? Mathf.Max(40f, _waterBar.rect.width) : 230f;
            float lightWidth = _lightBar != null ? Mathf.Max(40f, _lightBar.rect.width) : 230f;
            _waterFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, waterWidth * _water);
            _lightFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lightWidth * _light);
            // цвет заливки: зелёный=OK, жёлтый=немного off, красный=критично
            _waterFill.color = _water < 0.25f || _water > 0.9f ? new Color(1f, 0.22f, 0.18f, 1f)
                             : Mathf.Abs(_water - 0.62f) < 0.1f ? ColorPalette.AccentLime
                             : new Color(1f, 0.82f, 0.1f, 1f);
            _lightFill.color = _light < 0.25f || _light > 0.9f ? new Color(1f, 0.22f, 0.18f, 1f)
                             : Mathf.Abs(_light - 0.58f) < 0.1f ? ColorPalette.AccentLime
                             : new Color(1f, 0.82f, 0.1f, 1f);

            if (_plantCore != null)
            {
                float harmony = 1f - Mathf.Clamp01((waterOk + lightOk) * 2.2f);
                float pulse = 0.94f + Mathf.Sin(Time.unscaledTime * 2.8f) * 0.06f;
                _plantCore.rectTransform.localScale = Vector3.one * (0.88f + harmony * 0.26f) * pulse;
                _plantCore.color = Color.Lerp(
                    new Color(0.44f, 0.52f, 0.2f, 0.95f),
                    new Color(0.22f, 0.8f, 0.36f, 0.98f),
                    harmony);
            }

            if (_stabilityAura != null)
            {
                float harmony = 1f - Mathf.Clamp01((waterOk + lightOk) * 2.2f);
                _stabilityAura.color = new Color(0.45f, 0.95f, 0.62f, 0.12f + harmony * 0.32f);
            }

            for (int i = 0; i < _petals.Count; i++)
            {
                var leaf = _petals[i];
                if (leaf == null) continue;

                float a = (Mathf.PI * 2f / Mathf.Max(1, _petals.Count)) * i + Time.unscaledTime * 0.35f;
                float radius = 78f + Mathf.Sin(Time.unscaledTime * 1.8f + i) * 8f;
                var lrt = leaf.rectTransform;
                lrt.anchoredPosition = new Vector2(Mathf.Cos(a) * radius, 24f + Mathf.Sin(a) * 26f);
                lrt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 3f + i) * 18f);
                leaf.color = balanced
                    ? new Color(0.52f, 0.96f, 0.58f, 0.92f)
                    : new Color(0.62f, 0.78f, 0.44f, 0.88f);
            }
        }

        private void CheckDone()
        {
            bool balanced = Mathf.Abs(_water - 0.62f) < 0.1f && Mathf.Abs(_light - 0.58f) < 0.1f;
            if (_rootsLeft <= 0 && _weedsLeft <= 0 && balanced)
            {
                Finish("Digital Garden stabilized.");
            }
        }

        private void Update()
        {
            if (IsDone) return;

            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            var root = transform as RectTransform;
            if (root != null)
            {
                float w = Mathf.Max(340f, root.rect.width);
                float scale = Mathf.InverseLerp(340f, 900f, w);
                _status.fontSize = Mathf.RoundToInt(Mathf.Lerp(11f, 15f, scale));
                _gardenStage.localScale = Vector3.one * Mathf.Lerp(0.72f, 1f, scale);
            }

            _water = Mathf.Clamp01(_water - Time.unscaledDeltaTime * 0.03f);
            _light = Mathf.Clamp01(_light + Mathf.Sin(Time.unscaledTime * 0.8f) * 0.0012f);

            _eventTimer -= Time.unscaledDeltaTime;
            if (_eventTimer <= 0f)
            {
                // случайное событие: утечка воды или всплеск света
                if (Random.value > 0.5f)
                {
                    _water = Mathf.Clamp01(_water - 0.14f);
                    Hint.text = "Water leak detected! Restore water level.";
                }
                else
                {
                    _light = Mathf.Clamp01(_light + 0.16f);
                    Hint.text = "Light surge! Reduce light level.";
                }
                _eventTimer = Random.Range(8f, 13f);
            }

            UpdateStatus();
            CheckDone();
        }
    }

    internal class ReflectionCalibrationMiniGame : AntivirusMiniGameBase
    {
        private readonly float[] _values  = new float[4];
        private readonly float[] _targets = new float[4];
        private readonly Image[] _fillBars = new Image[4];
        private readonly Image[] _colPanels = new Image[4];
        private Text _title;

        private static readonly Color[] ChannelColors =
        {
            new Color(0f,    0.82f, 1f,    1f),
            new Color(0.38f, 1f,    0.58f, 1f),
            new Color(1f,    0.82f, 0f,    1f),
            new Color(1f,    0.42f, 0.78f, 1f),
        };

        protected override void Build()
        {
            MakePanel(transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.06f, 0.13f, 0.28f, 0.97f), new Color(0.03f, 0.07f, 0.18f, 0.97f), false);

            _title = MakeText(transform, "Signal Calibration", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -10), new Vector2(-14, -10));

            MakeText(transform, "Move each bar into its glowing target band using + / −", 13, FontStyle.Normal,
                TextAnchor.UpperCenter, new Color(0.55f, 0.82f, 1f, 0.82f),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -42), new Vector2(0, -42));

            BuildRuleCard("How to play",
                "Use + / − to move each bar into its glowing target zone. Bars start above or below target.\nAll 4 channels must lock green simultaneously.");

            for (int i = 0; i < 4; i++)
            {
                _targets[i] = Random.Range(0.3f, 0.85f);
                // начальное значение гарантированно далеко от цели (чтобы требовались и + и −)
                do { _values[i] = Random.value; }
                while (Mathf.Abs(_values[i] - _targets[i]) < 0.22f);
                BuildColumn(i);
            }

            Hint.text = "Align all 4 signal bars to their target zones.";
        }

        private void BuildColumn(int idx)
        {
            var accent = ChannelColors[idx];
            float xMin = 0.05f + idx * 0.238f;
            float xMax = xMin + 0.205f;

            var colImg = MakePanel(transform, new Vector2(xMin, 0.08f), new Vector2(xMax, 0.92f),
                Vector2.zero, Vector2.zero,
                new Color(0.08f, 0.18f, 0.35f, 0.92f), new Color(0.04f, 0.1f, 0.22f, 0.92f), false);
            _colPanels[idx] = colImg;

            MakeText(colImg.transform, $"CH {idx + 1}", 13, FontStyle.Bold, TextAnchor.UpperCenter,
                new Color(accent.r, accent.g, accent.b, 0.9f),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -5), new Vector2(0, -5));

            // вертикальный трек
            var track = MakePanel(colImg.transform, new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.84f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 1f, 1f, 0.06f), new Color(1f, 1f, 1f, 0.03f), false);

            // целевая зона (светящаяся полоса)
            float tMin = _targets[idx] - 0.08f;
            float tMax = _targets[idx] + 0.08f;
            MakePanel(track.transform, new Vector2(0f, tMin), new Vector2(1f, tMax),
                Vector2.zero, Vector2.zero,
                new Color(accent.r, accent.g, accent.b, 0.32f),
                new Color(accent.r, accent.g, accent.b, 0.14f), false);

            // белая линия по центру цели
            MakePanel(track.transform,
                new Vector2(0f, _targets[idx] - 0.015f), new Vector2(1f, _targets[idx] + 0.015f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 1f, 1f, 0.65f), new Color(1f, 1f, 1f, 0.65f), false);

            // заливка бара (растёт снизу)
            var fill = MakePanel(track.transform, new Vector2(0.08f, 0f), new Vector2(0.92f, _values[idx]),
                Vector2.zero, Vector2.zero, accent, accent * 0.6f, true);
            _fillBars[idx] = fill;

            int capturedIdx = idx;
            var plusBtn = aerisButton.Create(colImg.transform, "+", new Vector2(46, 34),
                () => _values[capturedIdx] = Mathf.Clamp01(_values[capturedIdx] + 0.05f),
                accent, accent * 0.5f, 22);
            var prt = (RectTransform)plusBtn.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.93f);
            prt.pivot = new Vector2(0.5f, 0.5f);

            var minusBtn = aerisButton.Create(colImg.transform, "−", new Vector2(46, 34),
                () => _values[capturedIdx] = Mathf.Clamp01(_values[capturedIdx] - 0.05f),
                new Color(accent.r * 0.6f, accent.g * 0.6f, accent.b * 0.6f, 1f),
                new Color(accent.r * 0.35f, accent.g * 0.35f, accent.b * 0.35f, 1f), 22);
            var mrt = (RectTransform)minusBtn.transform;
            mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0.07f);
            mrt.pivot = new Vector2(0.5f, 0.5f);
        }

        private void Update()
        {
            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            if (IsDone) return;

            int locked = 0;
            for (int i = 0; i < 4; i++)
            {
                if (_fillBars[i] == null) continue;

                // двигаем верхнюю границу бара
                var rt = _fillBars[i].rectTransform;
                rt.anchorMax = new Vector2(rt.anchorMax.x, _values[i]);

                bool near = Mathf.Abs(_values[i] - _targets[i]) < 0.08f;
                if (near) locked++;

                // яркость бара: тусклый пока не попал в зону
                _fillBars[i].color = near ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.6f);

                // подсветка колонки при попадании
                if (_colPanels[i] != null)
                {
                    var accent = ChannelColors[i];
                    var targetCol = near
                        ? new Color(accent.r * 0.22f, accent.g * 0.22f, accent.b * 0.22f, 0.95f)
                        : new Color(0.08f, 0.18f, 0.35f, 0.92f);
                    _colPanels[i].color = Color.Lerp(_colPanels[i].color, targetCol, Time.unscaledDeltaTime * 5f);
                }
            }

            if (locked >= 4) { Finish("Signal Calibration complete."); return; }
            Hint.text = locked > 0
                ? $"Channels locked: {locked}/4 — adjust the rest."
                : "Raise each bar to align with its glowing target band.";
        }
    }

    internal class FakePopupPanicMiniGame : AntivirusMiniGameBase
    {
        private readonly List<PopupEntry> _entries = new List<PopupEntry>();
        private RectTransform _spawnRoot;
        private Text _title;
        private Text _progressLabel;
        private int _cleared;
        private int _wrong;
        private float _spawnTimer;

        protected override void Build()
        {
            MakePanel(transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.89f, 0.95f, 1f, 0.58f), new Color(0.66f, 0.79f, 0.97f, 0.35f), true);

            _title = MakeText(transform, "Fake Pop-Up Panic", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildRuleCard("How to play",
                "Close only CORRUPTED (red ⚠) popups. Keep SAFE (blue ✓) ones.\nIf 12+ windows open at once, system resets.");

            // прогресс-счётчик
            var progPanel = MakePanel(transform, new Vector2(1, 1), new Vector2(1, 1),
                Vector2.zero, Vector2.zero,
                new Color(0.08f, 0.18f, 0.38f, 0.88f), new Color(0.04f, 0.1f, 0.24f, 0.88f), false);
            progPanel.rectTransform.anchorMin = new Vector2(0.72f, 0.86f);
            progPanel.rectTransform.anchorMax = new Vector2(0.98f, 0.98f);
            progPanel.rectTransform.offsetMin = Vector2.zero;
            progPanel.rectTransform.offsetMax = Vector2.zero;
            MakeText(progPanel.transform, "CLOSED", 11, FontStyle.Bold, TextAnchor.UpperCenter,
                new Color(0.55f, 0.82f, 1f, 0.85f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -4), new Vector2(0, -4));
            _progressLabel = MakeText(progPanel.transform, "0 / 6", 22, FontStyle.Bold, TextAnchor.MiddleCenter,
                Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, -14));

            _spawnRoot = new GameObject("SpawnRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            _spawnRoot.SetParent(transform, false);
            _spawnRoot.anchorMin = new Vector2(0.02f, 0.04f);
            _spawnRoot.anchorMax = new Vector2(0.98f, 0.84f);
            _spawnRoot.offsetMin = Vector2.zero;
            _spawnRoot.offsetMax = Vector2.zero;

            Hint.text = "Close 6 corrupted popups (⚠) to stabilize shell.";
        }

        private void Update()
        {
            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            if (IsDone) return;

            _spawnTimer -= Time.unscaledDeltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnPopup();
                _spawnTimer = Mathf.Lerp(1.2f, 0.6f, Mathf.Clamp01(_cleared / 6f));
            }

            if (_entries.Count > 11)
            {
                ResetBoard("Overflow! Too many windows. Panic reset.");
            }
        }

        private void SpawnPopup()
        {
            var go = new GameObject("Popup", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_spawnRoot, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            float w = Random.Range(190f, 280f);
            float h = Random.Range(100f, 146f);
            float x = Random.Range(0f, Mathf.Max(1f, _spawnRoot.rect.width - w));
            float y = -Random.Range(0f, Mathf.Max(1f, _spawnRoot.rect.height - h));
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);

            // гарантируем что corrupted появляются достаточно часто
            int corruptedOnScreen = 0;
            foreach (var e in _entries) if (e.Corrupted) corruptedOnScreen++;
            bool corrupted = corruptedOnScreen == 0 || Random.value > 0.35f;
            var img = go.GetComponent<Image>();
            img.sprite = TextureFactory.RoundedGlossy((int)w, (int)h, 12,
                corrupted ? new Color(1f, 0.8f, 0.86f, 0.95f) : new Color(0.86f, 0.95f, 1f, 0.95f),
                corrupted ? new Color(0.82f, 0.54f, 0.68f, 0.95f) : new Color(0.56f, 0.75f, 0.96f, 0.95f), true);

            // иконка: ⚠ для corrupted, ✓ для safe
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var irt = (RectTransform)iconGo.transform;
            irt.anchorMin = irt.anchorMax = new Vector2(0, 1);
            irt.pivot = new Vector2(0, 1);
            irt.anchoredPosition = new Vector2(8, -8);
            irt.sizeDelta = new Vector2(22, 22);
            iconGo.GetComponent<Image>().sprite = TextureFactory.Circle(32,
                corrupted ? new Color(1f, 0.22f, 0.18f, 1f) : new Color(0.18f, 0.55f, 1f, 1f));
            iconGo.GetComponent<Image>().raycastTarget = false;
            MakeText(iconGo.transform, corrupted ? "!" : "✓", 13, FontStyle.Bold, TextAnchor.MiddleCenter,
                Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).raycastTarget = false;

            MakeText(go.transform, corrupted ? "System Alert" : "System Info", 13, FontStyle.Bold,
                TextAnchor.UpperLeft, corrupted ? new Color(0.7f, 0.1f, 0.1f, 1f) : new Color(0.08f, 0.28f, 0.65f, 1f),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(34, -10), new Vector2(-46, -10));

            MakeText(go.transform, corrupted ? "Integrity mismatch detected." : "Background sync successful.",
                11, FontStyle.Normal, TextAnchor.UpperLeft, ColorPalette.TextDark.WithAlpha(0.82f),
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 12), new Vector2(-10, -28));

            var closeColor = corrupted ? new Color(1f, 0.3f, 0.3f, 0.85f) : new Color(0.5f, 0.72f, 1f, 0.7f);
            var closeBtn = aerisButton.Create(go.transform, "✕", new Vector2(32, 26), () => { },
                closeColor, closeColor * 0.7f, 15);
            var crt = (RectTransform)closeBtn.transform;
            crt.anchorMin = crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(1, 1);
            crt.anchoredPosition = new Vector2(-6, -6);

            var entry = new PopupEntry { Root = go, Corrupted = corrupted };
            _entries.Add(entry);

            var click = closeBtn.gameObject.AddComponent<SimplePointerButton>();
            click.ClickAction = () => OnClose(entry);

            // corrupted всегда дрейфуют — это делает их более зловещими
            if (corrupted) go.AddComponent<DriftingPopup>();
        }

        private void OnClose(PopupEntry entry)
        {
            if (entry == null || entry.Root == null) return;

            _entries.Remove(entry);
            Destroy(entry.Root);

            if (entry.Corrupted)
            {
                _cleared++;
                if (_progressLabel != null) _progressLabel.text = $"{_cleared} / 6";
                Hint.text = $"Corrupted closed: {_cleared}/6";
                if (_cleared >= 6) { Finish("Fake Pop-Up Panic complete."); }
            }
            else
            {
                _wrong++;
                Hint.text = $"Safe popup closed by mistake: {_wrong}/3";
                if (_wrong >= 3) { ResetBoard("Too many safe popups closed. System reset."); }
            }
        }

        private void ResetBoard(string message)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Root != null) Destroy(_entries[i].Root);
            }
            _entries.Clear();
            _wrong = 0;
            Hint.text = $"{message} Progress: {_cleared}/6 saved.";
        }

        private class PopupEntry
        {
            public GameObject Root;
            public bool Corrupted;
        }
    }

    internal class CursorDodgeMiniGame : AntivirusMiniGameBase
    {
        private RectTransform _arena;
        private RectTransform _player;
        private RectTransform _playerRing;
        private Image _timerFill;
        private Text _title;
        private readonly List<RectTransform> _enemies = new List<RectTransform>();
        private readonly List<RectTransform> _enemyCores = new List<RectTransform>();
        private readonly List<Vector2> _enemyVel = new List<Vector2>();
        private readonly List<bool> _enemyIsChaser = new List<bool>();
        private readonly List<RectTransform> _mines = new List<RectTransform>();
        private readonly List<(RectTransform cx, RectTransform cy)> _mineCrosses = new List<(RectTransform, RectTransform)>();
        private readonly List<float> _minePhase = new List<float>();
        private bool _hasControl;
        private float _timer = 18f;

        protected override void Build()
        {
            MakePanel(transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.9f, 0.97f, 1f, 0.55f), new Color(0.64f, 0.84f, 0.98f, 0.35f), true);

            _title = MakeText(transform, "Cursor Dodge", 24, FontStyle.Bold, TextAnchor.UpperLeft,
                ColorPalette.TextDark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -40), new Vector2(-14, -8));

            BuildRuleCard("How to play",
                "Move your cursor core inside arena. Survive 18 seconds.\nRed hunters track you. Purple mines drift and punish wide movement.");

            // таймер-бар сверху
            var timerBg = MakePanel(transform, new Vector2(0.08f, 0.83f), new Vector2(0.92f, 0.87f),
                Vector2.zero, Vector2.zero, new Color(0.1f, 0.15f, 0.28f, 0.85f), new Color(0.06f, 0.1f, 0.2f, 0.85f), false);
            _timerFill = MakePanel(timerBg.transform, new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero, new Color(0.22f, 0.92f, 0.58f, 1f), new Color(0.1f, 0.72f, 0.38f, 1f), true);

            _arena = MakePanel(transform, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.82f),
                Vector2.zero, Vector2.zero,
                new Color(0.06f, 0.12f, 0.26f, 0.95f), new Color(0.03f, 0.07f, 0.16f, 0.95f), false).rectTransform;

            var tracker = _arena.gameObject.AddComponent<CursorArenaTracker>();
            tracker.OnMoved = SetPlayerPos;
            tracker.OnEntered = () => _hasControl = true;
            tracker.OnExited = () => _hasControl = false;

            // игрок: зелёный кружок + белое кольцо прицел
            _player = MakeCursor(_arena, new Color(0.22f, 0.95f, 0.58f, 1f), 22f);
            var ring = MakeCursor(_arena, new Color(1f, 1f, 1f, 0.55f), 34f);
            ring.SetSiblingIndex(0); // кольцо за игроком
            _playerRing = ring;
            _player.anchoredPosition = Vector2.zero;

            int enemyCount = 10;
            if (aerisRuntime.Instance != null && aerisRuntime.Instance.Stage >= CorruptionStage.Distorted)
                enemyCount = 14;

            for (int i = 0; i < enemyCount; i++)
            {
                bool chaser = i % 3 == 0;
                float size = chaser ? 26f : 20f;
                var eColor = chaser ? new Color(1f, 0.18f, 0.28f, 1f) : new Color(1f, 0.5f, 0.22f, 1f);
                var e = MakeCursor(_arena, eColor, size);
                // тёмное ядро внутри врага
                var core = MakeCursor(_arena, new Color(0.25f, 0f, 0f, 0.85f), size * 0.42f);
                core.SetSiblingIndex(e.GetSiblingIndex() + 1);
                _enemyCores.Add(core);
                e.anchoredPosition = Random.insideUnitCircle * 140f;
                _enemies.Add(e);
                _enemyVel.Add(Random.insideUnitCircle.normalized * Random.Range(110f, 190f));
                _enemyIsChaser.Add(chaser);
            }

            int mineCount = 6;
            if (aerisRuntime.Instance != null && aerisRuntime.Instance.Stage >= CorruptionStage.Distorted)
                mineCount = 8;
            for (int i = 0; i < mineCount; i++)
            {
                var m = MakeCursor(_arena, new Color(0.58f, 0.28f, 1f, 0.92f), 20f);
                // крест внутри мины
                var cx = MakeCursor(_arena, new Color(1f, 1f, 1f, 0.55f), 3f);
                cx.sizeDelta = new Vector2(14f, 3f);
                cx.SetSiblingIndex(m.GetSiblingIndex() + 1);
                var cy = MakeCursor(_arena, new Color(1f, 1f, 1f, 0.55f), 3f);
                cy.sizeDelta = new Vector2(3f, 14f);
                cy.SetSiblingIndex(m.GetSiblingIndex() + 2);
                _mineCrosses.Add((cx, cy));
                m.anchoredPosition = Random.insideUnitCircle * 170f;
                _mines.Add(m);
                _minePhase.Add(Random.Range(0f, 6.28f));
            }

            Hint.text = "Enter arena and survive red hunters + drifting mines.";
        }

        private static RectTransform MakeCursor(Transform parent, Color color, float size)
        {
            var go = new GameObject("Cursor", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            img.sprite = TextureFactory.Circle(64, color);
            return rt;
        }

        private void SetPlayerPos(Vector2 localPos)
        {
            if (_player == null || _arena == null) return;
            float halfW = _arena.rect.width * 0.5f - 12f;
            float halfH = _arena.rect.height * 0.5f - 12f;
            _player.anchoredPosition = new Vector2(
                Mathf.Clamp(localPos.x, -halfW, halfW),
                Mathf.Clamp(localPos.y, -halfH, halfH));
        }

        private void Update()
        {
            FitTitleToParent(_title, transform as RectTransform, 24, 18);
            if (IsDone || _arena == null || _player == null) return;

            float dt = Time.unscaledDeltaTime;
            float halfW = _arena.rect.width * 0.5f;
            float halfH = _arena.rect.height * 0.5f;
            float pressure = Mathf.Clamp01(1f - (_timer / 18f));
            float speedBoost = 1f + pressure * 1.4f;

            for (int i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e == null) continue;

                Vector2 p = e.anchoredPosition;
                var vel = _enemyVel[i];
                if (_enemyIsChaser[i] && _player != null)
                {
                    Vector2 toPlayer = (_player.anchoredPosition - p);
                    if (toPlayer.sqrMagnitude > 0.001f)
                    {
                        vel = Vector2.Lerp(vel, toPlayer.normalized * Mathf.Max(120f, vel.magnitude), dt * 2.8f);
                    }
                }

                p += vel * dt * speedBoost;

                if (p.x < -halfW + 14f || p.x > halfW - 14f) vel.x *= -1f;
                if (p.y < -halfH + 14f || p.y > halfH - 14f) vel.y *= -1f;
                _enemyVel[i] = vel;

                p.x = Mathf.Clamp(p.x, -halfW + 14f, halfW - 14f);
                p.y = Mathf.Clamp(p.y, -halfH + 14f, halfH - 14f);
                e.anchoredPosition = p;

                // синхронизируем тёмное ядро
                if (i < _enemyCores.Count && _enemyCores[i] != null)
                    _enemyCores[i].anchoredPosition = p;

                float hitRadius = _enemyIsChaser[i] ? 28f : 24f;
                if (Vector2.Distance(p, _player.anchoredPosition) < hitRadius)
                {
                    _timer = 18f;
                    _player.anchoredPosition = Vector2.zero;
                    if (_playerRing != null) _playerRing.anchoredPosition = Vector2.zero;
                    Hint.text = "Corrupted cursor collision! Timer reset.";
                    return;
                }
            }

            for (int i = 0; i < _mines.Count; i++)
            {
                var mine = _mines[i];
                if (mine == null) continue;

                float phase = _minePhase[i];
                var offset = new Vector2(
                    Mathf.Sin(Time.unscaledTime * 1.8f + phase) * 90f,
                    Mathf.Cos(Time.unscaledTime * 1.4f + phase) * 62f);
                mine.anchoredPosition = Vector2.Lerp(mine.anchoredPosition, offset, dt * (1.2f + pressure * 1.6f));
                mine.localScale = Vector3.one * (0.85f + Mathf.Sin(Time.unscaledTime * 6f + phase) * 0.12f);
                if (i < _mineCrosses.Count)
                {
                    _mineCrosses[i].cx.anchoredPosition = mine.anchoredPosition;
                    _mineCrosses[i].cy.anchoredPosition = mine.anchoredPosition;
                }

                if (Vector2.Distance(mine.anchoredPosition, _player.anchoredPosition) < 20f)
                {
                    _timer = 18f;
                    _player.anchoredPosition = Vector2.zero;
                    if (_playerRing != null) _playerRing.anchoredPosition = Vector2.zero;
                    Hint.text = "Mine contact detected! Timer reset.";
                    return;
                }
            }

            if (!_hasControl)
            {
                Hint.text = "Move pointer inside arena to control your cursor core.";
                return;
            }

            _timer -= dt;

            // таймер-бар: зелёный → жёлтый → красный по мере уменьшения
            if (_timerFill != null)
            {
                float t = _timer / 18f;
                var rt = _timerFill.rectTransform;
                rt.anchorMax = new Vector2(t, 1f);
                _timerFill.color = Color.Lerp(new Color(1f, 0.22f, 0.22f, 1f), new Color(0.22f, 0.92f, 0.58f, 1f), t);
            }

            // кольцо игрока следует за ним
            if (_playerRing != null && _player != null)
                _playerRing.anchoredPosition = _player.anchoredPosition;

            Hint.text = $"Survive: {Mathf.CeilToInt(_timer)}s  |  Threat: {Mathf.RoundToInt((1f + pressure) * 100f)}%";
            if (_timer <= 0f) { Finish("Cursor Dodge complete."); }
        }
    }

    internal class BubbleClickTarget : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayClick();
            OnClicked?.Invoke();
        }
    }

    internal class CloudDragObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public System.Action<Vector2> OnDropped;
        public bool IsDragging { get; private set; }

        private RectTransform _rt;
        private RectTransform _parentRt;
        private Camera _eventCam;
        private CanvasGroup _cg;
        private Vector2 _start;
        private Vector2 _dragOffset;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            _cg = gameObject.AddComponent<CanvasGroup>();
            // parent ещё не установлен в Awake — захватываем в OnBeginDrag
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _parentRt = _rt.parent as RectTransform; // parent уже установлен к моменту первого drag
            IsDragging = true;
            _start = _rt.anchoredPosition;
            _cg.blocksRaycasts = false;
            _eventCam = eventData.pressEventCamera;

            // запоминаем смещение от точки клика до центра облака
            if (_parentRt != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRt, eventData.position, _eventCam, out var clickLocal))
            {
                _dragOffset = _rt.anchoredPosition - clickLocal;
            }
            else
            {
                _dragOffset = Vector2.zero;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_parentRt == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRt, eventData.position, _eventCam, out var localPos))
            {
                _rt.anchoredPosition = localPos + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
            _cg.blocksRaycasts = true;
            OnDropped?.Invoke(eventData.position);
        }

        public void ResetToStart()
        {
            _rt.anchoredPosition = _start;
        }
    }

    internal class PixelCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        private PixelRepairMiniGame _owner;
        private Image _image;
        private bool _hiddenBroken;

        public bool Broken { get; private set; }
        public bool IsHiddenBroken => Broken && _hiddenBroken;

        public void Initialize(PixelRepairMiniGame owner, Image image, bool broken, bool hiddenBroken)
        {
            _owner = owner;
            _image = image;
            SetBroken(broken, hiddenBroken);
        }

        public void SetBroken(bool broken, bool hiddenBroken = false)
        {
            Broken = broken;
            _hiddenBroken = broken && hiddenBroken;
            RefreshVisual();
        }

        public bool RevealHiddenFault()
        {
            if (!Broken || !_hiddenBroken) return false;
            _hiddenBroken = false;
            RefreshVisual();
            return true;
        }

        private void RefreshVisual()
        {
            if (_image == null) return;
            if (!Broken)
            {
                _image.color = new Color(0.55f, 0.88f, 1f, 1f);
                return;
            }

            _image.color = _hiddenBroken
                ? new Color(0.46f, 0.68f, 1f, 1f)
                : new Color(1f, 0.45f, 0.45f, 1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _owner?.Repair(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Compatible with both old and new Input System: if there is an
            // active pressed pointer while entering the cell, treat it as paint-drag.
            if (eventData != null
                && eventData.button == PointerEventData.InputButton.Left
                && (eventData.pointerPress != null || eventData.dragging))
            {
                _owner?.Repair(this);
            }
        }
    }

    internal class SimplePointerButton : MonoBehaviour, IPointerClickHandler
    {
        public System.Action ClickAction;

        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayClick();
            ClickAction?.Invoke();
        }
    }

    internal class CursorArenaTracker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public System.Action<Vector2> OnMoved;
        public System.Action OnEntered;
        public System.Action OnExited;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEntered?.Invoke();
            OnPointerMove(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExited?.Invoke();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            var rt = transform as RectTransform;
            if (rt == null || eventData == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var local);
            OnMoved?.Invoke(local);
        }
    }

    internal class DriftingPopup : MonoBehaviour
    {
        private RectTransform _rt;
        private Vector2 _base;
        private float _phase;

        private void Awake()
        {
            _rt = transform as RectTransform;
            if (_rt != null) _base = _rt.anchoredPosition;
            _phase = Random.Range(0f, 6.28f);
        }

        private void Update()
        {
            if (_rt == null) return;
            _rt.anchoredPosition = _base + new Vector2(
                Mathf.Sin(Time.unscaledTime * 1.1f + _phase) * 6f,
                Mathf.Cos(Time.unscaledTime * 0.9f + _phase) * 4f);
        }
    }
}
