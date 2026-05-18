using System.Collections;
using aerisOS.Managers;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace aerisOS.UI
{
    public class AssistantGuideUI : MonoBehaviour
    {
        private const string SpritePathCalm = "Assistants/shime_calm";
        private const string SpritePathFocused = "Assistants/shime_focused";
        private const string SpritePathAlert = "Assistants/shime_alert";
        private const string SpritePathCritical = "Assistants/shime_critical";

        private Text _nameText;
        private Text _moodText;
        private Text _dialogText;
        private Image _avatar;
        private Image _avatarGlow;
        private Coroutine _introRoutine;
        private bool _introPending;
        private int _manualHintIndex;
        private readonly string[] _manualHints =
        {
            "Step 1: open Antivirus and complete the active mini-game objective.",
            "Step 2: after task completion, watch corruption status and finish the next objective.",
            "Step 3: when trace is recovered, open Archive to read the new virus signature.",
            "Tip: you can drag Shime around, then keep cleaning tasks to unlock more story lines."
        };

        public void Build(RectTransform parent, NotificationSystem notify)
        {
            var root = new GameObject("AssistantGuide", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(420f, 140f);
            rt.anchoredPosition = new Vector2(-16f, 14f);

            var bubble = new GameObject("GuideBubble", typeof(RectTransform), typeof(Image));
            bubble.transform.SetParent(root.transform, false);
            var brt = (RectTransform)bubble.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            var bimg = bubble.GetComponent<Image>();
            bimg.sprite = TextureFactory.RoundedGlossy(420, 140, 14,
                Color.white.WithAlpha(0.9f), Color.white.WithAlpha(0.7f), false);
            bimg.raycastTarget = true;

            var hintButton = bubble.AddComponent<Button>();
            hintButton.transition = Selectable.Transition.None;
            hintButton.onClick.AddListener(OnManualHintRequested);

            var avatarFrame = new GameObject("AvatarFrame", typeof(RectTransform), typeof(Image));
            avatarFrame.transform.SetParent(root.transform, false);
            var afrt = (RectTransform)avatarFrame.transform;
            afrt.anchorMin = new Vector2(0f, 0.5f);
            afrt.anchorMax = new Vector2(0f, 0.5f);
            afrt.pivot = new Vector2(0f, 0.5f);
            afrt.sizeDelta = new Vector2(92f, 92f);
            afrt.anchoredPosition = new Vector2(12f, 0f);
            var afimg = avatarFrame.GetComponent<Image>();
            afimg.sprite = TextureFactory.Circle(96, Color.white.WithAlpha(0.36f));
            afimg.raycastTarget = false;

            _avatarGlow = new GameObject("AvatarGlow", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            _avatarGlow.transform.SetParent(avatarFrame.transform, false);
            var agrt = (RectTransform)_avatarGlow.transform;
            agrt.anchorMin = agrt.anchorMax = new Vector2(0.5f, 0.5f);
            agrt.pivot = new Vector2(0.5f, 0.5f);
            agrt.sizeDelta = new Vector2(82f, 82f);
            _avatarGlow.sprite = TextureFactory.Circle(96, new Color(0.3f, 0.95f, 1f, 0.25f));
            _avatarGlow.raycastTarget = false;

            _avatar = new GameObject("AvatarCore", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            _avatar.transform.SetParent(avatarFrame.transform, false);
            var acrt = (RectTransform)_avatar.transform;
            acrt.anchorMin = acrt.anchorMax = new Vector2(0.5f, 0.5f);
            acrt.pivot = new Vector2(0.5f, 0.5f);
            acrt.sizeDelta = new Vector2(64f, 64f);
            _avatar.sprite = LoadMoodSprite(CorruptionStage.Pristine)
                ?? TextureFactory.AppIcon(96, new Color(0.3f, 0.96f, 0.88f), 'S');
            _avatar.raycastTarget = false;

            _nameText = CreateText(root.transform, "Terra Nova", 16, FontStyle.Bold,
                ColorPalette.TextDark, TextAnchor.UpperLeft, new Vector2(108f, -10f), new Vector2(260f, 24f));
            _moodText = CreateText(root.transform, "Mood: Calm", 13, FontStyle.Italic,
                ColorPalette.TextDark.WithAlpha(0.7f), TextAnchor.UpperLeft, new Vector2(108f, -34f), new Vector2(260f, 20f));
            _dialogText = CreateText(root.transform, "Virus alert detected. I am shime and I will guide you through cleanup tasks.", 15, FontStyle.Normal,
                ColorPalette.TextDark.WithAlpha(0.95f), TextAnchor.UpperLeft, new Vector2(108f, -56f), new Vector2(300f, 74f));
            _dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _dialogText.verticalOverflow = VerticalWrapMode.Truncate;

            if (aerisRuntime.Instance != null)
            {
                aerisRuntime.Instance.OnTaskAssigned += OnTaskAssigned;
                aerisRuntime.Instance.OnCorruptionChanged += OnCorruptionChanged;
                aerisRuntime.Instance.OnLoreUnlocked += OnLoreUnlocked;
            }

            _introPending = true;
            TryStartIntro();
        }

        private void OnEnable()
        {
            TryStartIntro();
        }

        private void TryStartIntro()
        {
            if (!_introPending || !isActiveAndEnabled) return;
            if (_introRoutine != null) return;
            _introRoutine = StartCoroutine(IntroSequence());
            _introPending = false;
        }

        private static Text CreateText(Transform parent, string value, int size, FontStyle style,
            Color color, TextAnchor align, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.GetComponent<Text>();
            t.text = value;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        private IEnumerator IntroSequence()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            SetGuideLine("I am Terra. We have a virus in core modules. Start Antivirus now.", CorruptionStage.Pristine);
            yield return new WaitForSecondsRealtime(5f);
            SetGuideLine("Tip: complete tasks in Antivirus to isolate and purge infected fragments.", CorruptionStage.Pristine);
        }

        private void OnManualHintRequested()
        {
            if (_manualHints == null || _manualHints.Length == 0) return;
            string line = _manualHints[_manualHintIndex % _manualHints.Length];
            _manualHintIndex++;
            var stage = aerisRuntime.Instance != null ? aerisRuntime.Instance.Stage : CorruptionStage.Pristine;
            SetGuideLine(line, stage);
        }

        private void OnTaskAssigned(AntivirusTask task)
        {
            if (task == null) return;
            SetGuideLine($"New objective: {task.Title}. I marked likely infected blocks.",
                aerisRuntime.Instance != null ? aerisRuntime.Instance.Stage : CorruptionStage.Pristine);
        }

        private void OnCorruptionChanged(float value, CorruptionStage stage)
        {
            if (stage == CorruptionStage.Pristine) return;

            string line = stage switch
            {
                CorruptionStage.Subtle => "Virus spread increased. Stay focused and keep cleaning modules.",
                CorruptionStage.Distorted => "Critical warning: the virus mutates UI behavior. We need faster response.",
                CorruptionStage.Failing => "Emergency. Core integrity collapsing. Finish tasks immediately.",
                _ => "System status changed."
            };
            SetGuideLine(line, stage);
        }

        private void OnLoreUnlocked(string _)
        {
            SetGuideLine("Recovered trace log. Open Archive to inspect new virus signatures.",
                aerisRuntime.Instance != null ? aerisRuntime.Instance.Stage : CorruptionStage.Pristine);
        }

        private void SetGuideLine(string line, CorruptionStage stage)
        {
            if (_dialogText != null)
            {
                _dialogText.text = line;
            }

            if (_moodText != null)
            {
                _moodText.text = stage switch
                {
                    CorruptionStage.Pristine => "Mood: Calm",
                    CorruptionStage.Subtle => "Mood: Focused",
                    CorruptionStage.Distorted => "Mood: Alert",
                    CorruptionStage.Failing => "Mood: Critical",
                    _ => "Mood: Calm"
                };
            }

            if (_avatarGlow != null)
            {
                _avatarGlow.color = stage switch
                {
                    CorruptionStage.Pristine => new Color(0.3f, 0.95f, 1f, 0.25f),
                    CorruptionStage.Subtle => new Color(0.3f, 0.9f, 0.5f, 0.3f),
                    CorruptionStage.Distorted => new Color(1f, 0.8f, 0.24f, 0.34f),
                    CorruptionStage.Failing => new Color(1f, 0.35f, 0.35f, 0.38f),
                    _ => new Color(0.3f, 0.95f, 1f, 0.25f)
                };
            }

            if (_avatar != null)
            {
                _avatar.sprite = LoadMoodSprite(stage) ?? _avatar.sprite;
            }
        }

        private static Sprite LoadMoodSprite(CorruptionStage stage)
        {
            string path = stage switch
            {
                CorruptionStage.Pristine => SpritePathCalm,
                CorruptionStage.Subtle => SpritePathFocused,
                CorruptionStage.Distorted => SpritePathAlert,
                CorruptionStage.Failing => SpritePathCritical,
                _ => SpritePathCalm
            };
            return Resources.Load<Sprite>(path);
        }

        private void OnDestroy()
        {
            if (_introRoutine != null)
            {
                StopCoroutine(_introRoutine);
            }

            if (aerisRuntime.Instance != null)
            {
                aerisRuntime.Instance.OnTaskAssigned -= OnTaskAssigned;
                aerisRuntime.Instance.OnCorruptionChanged -= OnCorruptionChanged;
                aerisRuntime.Instance.OnLoreUnlocked -= OnLoreUnlocked;
            }
        }
    }
}
