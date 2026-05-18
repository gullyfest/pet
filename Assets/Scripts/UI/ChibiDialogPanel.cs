using aerisOS.Managers;
using aerisOS.Narrative;
using aerisOS.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public class ChibiDialogPanel : MonoBehaviour, IPointerClickHandler
    {
        private static readonly string[] MoodPaths =
        {
            "Sprites/calm",
            "Sprites/happy",
            "Sprites/sad",
            "Sprites/surprised",
            "Sprites/angry",
        };

        // ── UI refs ─────────────────────────────────────────────────────────────
        private Image      _charImage;
        private Text       _dialogText;
        private Text       _arrowText;
        private GameObject _boxGo;

        // ── Script state ────────────────────────────────────────────────────────
        private DialogLine[] _lines;
        private int          _lineIndex;
        private Action       _onComplete;

        private bool         _waitingForInput; // заблокирован на NameInput / ColorPicker
        private bool         _waitingForEvent; // заблокирован на WaitForEvent

        // ── Тайпрайтер ──────────────────────────────────────────────────────────
        private bool      _isTyping;
        private Coroutine _typeRoutine;
        private string    _fullText;
        private float     _ctrlTimer;

        // ── Блокировка ввода (концовка 2) ────────────────────────────────────
        private bool _inputLocked;

        /// <summary>Блокирует/разблокирует любой ввод игрока для прокрутки диалога.</summary>
        public void LockInput(bool locked) => _inputLocked = locked;

        /// <summary>Запускает глитч-эффекты: текст уходит вправо, шейк окна, мигание цвета.</summary>
        public void StartGlitch()
        {
            if (_dialogText != null)
                _dialogText.horizontalOverflow = HorizontalWrapMode.Overflow;
            StartCoroutine(GlitchRoutine());
        }

        private IEnumerator GlitchRoutine()
        {
            var boxRt = (RectTransform)_boxGo.transform;
            var dtRt  = (RectTransform)_dialogText.transform;
            var origBoxPos   = boxRt.anchoredPosition;
            var origOffsetMin = dtRt.offsetMin;
            var origOffsetMax = dtRt.offsetMax;
            var origColor    = _dialogText.color;
            var rng = new System.Random();
            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime;

                // Диалоговое окно дрожит, амплитуда нарастает
                float shakeAmt = Mathf.Lerp(3f, 22f, elapsed / 10f);
                boxRt.anchoredPosition = origBoxPos + new Vector2(
                    UnityEngine.Random.Range(-shakeAmt, shakeAmt),
                    UnityEngine.Random.Range(-shakeAmt * 0.35f, shakeAmt * 0.35f)
                );

                // Текст уезжает вправо за пределы экрана
                float drift = Mathf.Lerp(0f, -700f, Mathf.Clamp01(elapsed / 7f));
                dtRt.offsetMin = new Vector2(origOffsetMin.x + drift, origOffsetMin.y);
                dtRt.offsetMax = new Vector2(origOffsetMax.x + drift, origOffsetMax.y);

                // Случайные вспышки красного цвета текста
                _dialogText.color = rng.NextDouble() < 0.18f
                    ? new Color(1f, (float)rng.NextDouble() * 0.2f, (float)rng.NextDouble() * 0.2f)
                    : origColor;

                yield return null;
            }
        }

        // ── Inline widgets (создаются на нужной строке, удаляются при переходе) ─
        private GameObject _nameInputGO;
        private GameObject _colorPickerGO;
        private GameObject _choiceGO;

        private bool _built;

        // цвета для 6 вариантов выбора
        private static readonly (string Label, Color Value)[] ColorOptions =
        {
            ("Blue",   new Color(0.20f, 0.60f, 1.00f)),
            ("Pink",   new Color(1.00f, 0.42f, 0.70f)),
            ("Lime",   new Color(0.40f, 0.90f, 0.20f)),
            ("Purple", new Color(0.65f, 0.30f, 1.00f)),
            ("Orange", new Color(1.00f, 0.60f, 0.10f)),
            ("Cyan",   new Color(0.00f, 0.85f, 1.00f)),
        };

        // ════════════════════════════════════════════════════════════════════════
        // Build — вызывается один раз при создании
        // ════════════════════════════════════════════════════════════════════════
        public void Build(RectTransform bounds)
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Невидимый полноэкранный перехватчик кликов
            var bg = gameObject.AddComponent<Image>();
            bg.color = Color.clear;
            bg.raycastTarget = true;

            // ── Портрет персонажа ─────────────────────────────────────────────
            var maskGo = new GameObject("CharMask", typeof(RectTransform), typeof(RectMask2D));
            maskGo.transform.SetParent(transform, false);
            var maskRt = (RectTransform)maskGo.transform;
            maskRt.anchorMin = new Vector2(0.76f, 0f);
            maskRt.anchorMax = new Vector2(0.76f, 0f);
            maskRt.pivot     = new Vector2(0.5f, 0f);
            maskRt.anchoredPosition = new Vector2(0f, 64f);
            maskRt.sizeDelta = new Vector2(720f, 864f);

            var charGo = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            charGo.transform.SetParent(maskGo.transform, false);
            var charRt = (RectTransform)charGo.transform;
            charRt.anchorMin = new Vector2(0f, 1f);
            charRt.anchorMax = new Vector2(1f, 1f);
            charRt.pivot     = new Vector2(0.5f, 1f);
            charRt.sizeDelta = new Vector2(0f, 1080f);
            charRt.anchoredPosition = Vector2.zero;
            _charImage = charGo.GetComponent<Image>();
            _charImage.preserveAspect = true;
            _charImage.raycastTarget  = false;

            // ── Диалоговое окно ───────────────────────────────────────────────
            _boxGo = new GameObject("DialogBox", typeof(RectTransform), typeof(Image));
            _boxGo.transform.SetParent(transform, false);
            var boxRt = (RectTransform)_boxGo.transform;
            boxRt.anchorMin = new Vector2(0f, 0f);
            boxRt.anchorMax = new Vector2(1f, 0f);
            boxRt.pivot     = new Vector2(0.5f, 0f);
            boxRt.sizeDelta = new Vector2(-360f, 300f);
            boxRt.anchoredPosition = new Vector2(0f, 72f);
            var boxImg = _boxGo.GetComponent<Image>();
            boxImg.sprite = TextureFactory.RoundedGlossy(760, 190, 20,
                new Color(0.05f, 0.07f, 0.20f, 0.94f),
                new Color(0.07f, 0.09f, 0.26f, 0.94f), false);
            boxImg.raycastTarget = false;

            // Плашка с именем
            var namePlate = new GameObject("NamePlate", typeof(RectTransform), typeof(Image));
            namePlate.transform.SetParent(_boxGo.transform, false);
            var npRt = (RectTransform)namePlate.transform;
            npRt.anchorMin = new Vector2(0f, 1f);
            npRt.anchorMax = new Vector2(0f, 1f);
            npRt.pivot     = new Vector2(0f, 0f);
            npRt.sizeDelta = new Vector2(360f, 72f);
            npRt.anchoredPosition = new Vector2(28f, 0f);
            var npImg = namePlate.GetComponent<Image>();
            npImg.sprite = TextureFactory.RoundedGlossy(180, 36, 10,
                new Color(0.2f, 0.6f, 1f, 0.95f), new Color(0.1f, 0.45f, 0.9f, 0.9f), false);

            // регистрируем плашку в ColorAccentSystem
            ColorAccentSystem.Register(c =>
            {
                if (npImg != null)
                    npImg.sprite = TextureFactory.RoundedGlossy(180, 36, 10,
                        new Color(c.r * 0.9f, c.g * 0.9f, c.b * 0.9f, 0.95f),
                        new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 0.90f), false);
            });

            var nameTextGo = new GameObject("NameText", typeof(RectTransform), typeof(Text));
            nameTextGo.transform.SetParent(namePlate.transform, false);
            var ntRt = (RectTransform)nameTextGo.transform;
            ntRt.anchorMin = Vector2.zero;
            ntRt.anchorMax = Vector2.one;
            ntRt.offsetMin = new Vector2(8f, 2f);
            ntRt.offsetMax = new Vector2(-8f, -2f);
            var nt = nameTextGo.GetComponent<Text>();
            nt.text      = "Terra";
            nt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nt.fontSize  = 40;
            nt.fontStyle = FontStyle.Bold;
            nt.color     = Color.white;
            nt.alignment = TextAnchor.MiddleCenter;
            nt.raycastTarget = false;

            // Текст диалога
            var dtGo = new GameObject("DialogText", typeof(RectTransform), typeof(Text));
            dtGo.transform.SetParent(_boxGo.transform, false);
            var dtRt = (RectTransform)dtGo.transform;
            dtRt.anchorMin = new Vector2(0f, 0f);
            dtRt.anchorMax = new Vector2(1f, 1f);
            dtRt.offsetMin = new Vector2(28f, 62f);   // оставляем 62px снизу под виджеты
            dtRt.offsetMax = new Vector2(-28f, -42f);
            _dialogText = dtGo.GetComponent<Text>();
            _dialogText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _dialogText.fontSize  = 38;
            _dialogText.color     = new Color(0.92f, 0.96f, 1f);
            _dialogText.alignment = TextAnchor.UpperLeft;
            _dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _dialogText.verticalOverflow   = VerticalWrapMode.Truncate;
            _dialogText.raycastTarget = false;

            // Стрелка "продолжить"
            var arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Text));
            arrowGo.transform.SetParent(_boxGo.transform, false);
            var arrowRt = (RectTransform)arrowGo.transform;
            arrowRt.anchorMin = new Vector2(1f, 0f);
            arrowRt.anchorMax = new Vector2(1f, 0f);
            arrowRt.pivot     = new Vector2(1f, 0f);
            arrowRt.sizeDelta = new Vector2(44f, 44f);
            arrowRt.anchoredPosition = new Vector2(-20f, 14f);
            _arrowText = arrowGo.GetComponent<Text>();
            _arrowText.text      = "▶";
            _arrowText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _arrowText.fontSize  = 22;
            _arrowText.color     = new Color(0.4f, 0.9f, 1f, 0.9f);
            _arrowText.alignment = TextAnchor.MiddleCenter;
            _arrowText.raycastTarget = false;

            _built = true;
            gameObject.SetActive(false);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Публичное API
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Запускает скрипт диалога. onComplete вызывается когда последняя строка пройдена.</summary>
        public void Play(DialogLine[] lines, Action onComplete)
        {
            if (!_built) return;
            _lines      = lines;
            _onComplete = onComplete;
            _lineIndex  = 0;
            _waitingForInput = false;
            _waitingForEvent = false;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            ApplyLine();
        }

        /// <summary>Разблокирует паузу WaitForEvent (вызывается извне).</summary>
        public void Resume()
        {
            if (!_waitingForEvent) return;
            _waitingForEvent = false;
            RefreshArrow();
        }

        // ── Старый Show() — оставлен для совместимости с правым кликом по чиби ─
        public void Show()
        {
            if (!_built) return;
            _lineIndex = 0;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            if (_lines != null && _lines.Length > 0)
                ApplyLine();
            else
                gameObject.SetActive(false);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Обработка кликов и клавиш
        // ════════════════════════════════════════════════════════════════════════
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inputLocked) return;
            if (_waitingForInput || _waitingForEvent) return;
            if (_isTyping) { CompleteTyping(); return; }
            AdvanceLine();
        }

        private void Update()
        {
            if (_lines == null || !gameObject.activeSelf) return;
            if (_inputLocked) return;
            if (_waitingForInput) return; // нельзя скипать выбор / ввод имени

#if ENABLE_INPUT_SYSTEM
            bool ctrlHeld = UnityEngine.InputSystem.Keyboard.current != null &&
                            (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed ||
                             UnityEngine.InputSystem.Keyboard.current.rightCtrlKey.isPressed);
#else
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
            if (ctrlHeld)
            {
                _ctrlTimer += Time.deltaTime;
                if (_ctrlTimer >= 0.02f)
                {
                    _ctrlTimer = 0f;
                    if (_isTyping)
                        CompleteTyping();
                    else if (!_waitingForEvent)
                        AdvanceLine();
                }
            }
            else
            {
                _ctrlTimer = 0f;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Внутренняя логика
        // ════════════════════════════════════════════════════════════════════════
        private void AdvanceLine()
        {
            _lineIndex++;
            if (_lineIndex >= _lines.Length)
            {
                gameObject.SetActive(false);
                _onComplete?.Invoke();
                return;
            }
            ApplyLine();
        }

        private void ApplyLine()
        {
            // Останавливаем предыдущий тайпрайтер
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; _isTyping = false; }

            DestroyInlineWidgets();

            var line = _lines[_lineIndex];
            line.OnReach?.Invoke();

            // Подстановка имени
            string text = line.Text.Replace("{name}", PlayerState.Name);
            _fullText = text;
            StartTypewriter(text);

            // Спрайт настроения
            var sp = Resources.Load<Sprite>(MoodPaths[(int)line.Mood]);
            if (sp != null && _charImage != null) _charImage.sprite = sp;

            _waitingForInput = false;
            _waitingForEvent = false;

            switch (line.PauseType)
            {
                case DialogPauseType.NameInput:
                    _waitingForInput = true;
                    SpawnNameInput();
                    break;

                case DialogPauseType.ColorPicker:
                    _waitingForInput = true;
                    SpawnColorPicker();
                    break;

                case DialogPauseType.Choice:
                    _waitingForInput = true;
                    SpawnChoiceButtons(line.Choices);
                    break;

                case DialogPauseType.WaitForEvent:
                    _waitingForEvent = true;
                    break;
            }

            RefreshArrow();
        }

        private void RefreshArrow()
        {
            if (_arrowText == null) return;
            bool hide    = _waitingForInput || _waitingForEvent || _isTyping;
            bool isLast  = (_lineIndex >= _lines.Length - 1);
            _arrowText.color = hide    ? Color.clear
                             : isLast  ? new Color(1f, 0.5f, 0.5f, 0.9f)
                                       : new Color(0.4f, 0.9f, 1f, 0.9f);
        }

        private void StartTypewriter(string text)
        {
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }
            _typeRoutine = StartCoroutine(TypeText(text));
        }

        private IEnumerator TypeText(string text)
        {
            _isTyping = true;
            if (_dialogText != null) _dialogText.text = "";
            float delay = 1f / 30f;
            for (int i = 0; i < text.Length; i++)
            {
                if (_dialogText != null) _dialogText.text = text.Substring(0, i + 1);
                if ((i + 1) % 3 == 0)
                    AudioManager.Instance?.PlayTyping();
                yield return new WaitForSeconds(delay);
            }
            _isTyping = false;
            _typeRoutine = null;
            RefreshArrow();

            // AutoAdvance: автопереход без клика (используется для заблокированных строк)
            if (_lineIndex < _lines.Length && _lines[_lineIndex].AutoAdvance)
            {
                yield return new WaitForSeconds(0.4f);
                AdvanceLine();
            }
        }

        private void CompleteTyping()
        {
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }
            _isTyping = false;
            if (_dialogText != null) _dialogText.text = _fullText;
            RefreshArrow();
        }

        private void DestroyInlineWidgets()
        {
            if (_nameInputGO   != null) { Destroy(_nameInputGO);   _nameInputGO   = null; }
            if (_colorPickerGO != null) { Destroy(_colorPickerGO); _colorPickerGO = null; }
            if (_choiceGO      != null) { Destroy(_choiceGO);      _choiceGO      = null; }
        }

        // ─── Виджет ввода имени ───────────────────────────────────────────────
        private void SpawnNameInput()
        {
            _nameInputGO = new GameObject("NameInputWidget", typeof(RectTransform));
            _nameInputGO.transform.SetParent(_boxGo.transform, false);
            var wrt = (RectTransform)_nameInputGO.transform;
            wrt.anchorMin = new Vector2(0f, 0f);
            wrt.anchorMax = new Vector2(1f, 0f);
            wrt.pivot     = new Vector2(0.5f, 0f);
            wrt.offsetMin = new Vector2(28f, 8f);
            wrt.offsetMax = new Vector2(-28f, 8f);
            wrt.sizeDelta = new Vector2(-56f, 52f);

            // Фон поля ввода
            var fieldGo = new GameObject("Field", typeof(RectTransform), typeof(Image), typeof(InputField));
            fieldGo.transform.SetParent(_nameInputGO.transform, false);
            var frt = (RectTransform)fieldGo.transform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(1f, 1f);
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = new Vector2(-120f, 0f);
            var fImg = fieldGo.GetComponent<Image>();
            fImg.sprite = TextureFactory.RoundedGlossy(400, 44, 8,
                Color.white.WithAlpha(0.9f), Color.white.WithAlpha(0.7f), false);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(fieldGo.transform, false);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10f, 4f);
            trt.offsetMax = new Vector2(-10f, -4f);
            var txt = textGo.GetComponent<Text>();
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = 26;
            txt.color     = new Color(0.1f, 0.15f, 0.35f);
            txt.alignment = TextAnchor.MiddleLeft;
            txt.supportRichText = false;

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(fieldGo.transform, false);
            var prt = (RectTransform)placeholderGo.transform;
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(10f, 4f);
            prt.offsetMax = new Vector2(-10f, -4f);
            var ph = placeholderGo.GetComponent<Text>();
            ph.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ph.fontSize  = 24;
            ph.fontStyle = FontStyle.Italic;
            ph.color     = new Color(0.5f, 0.55f, 0.7f, 0.7f);
            ph.text      = "Enter your name...";
            ph.alignment = TextAnchor.MiddleLeft;

            var field = fieldGo.GetComponent<InputField>();
            field.textComponent  = txt;
            field.placeholder    = ph;
            field.lineType       = InputField.LineType.SingleLine;
            field.characterLimit = 20;
            field.Select();
            field.ActivateInputField();

            // Кнопка подтверждения
            var confirmBtn = aerisButton.Create(_nameInputGO.transform, "OK", new Vector2(108f, 52f),
                () => OnNameConfirmed(field),
                new Color(0.2f, 0.75f, 0.45f), new Color(0.05f, 0.52f, 0.28f));
            var brt = (RectTransform)confirmBtn.transform;
            brt.anchorMin = new Vector2(1f, 0f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot     = new Vector2(1f, 0.5f);
            brt.offsetMin = new Vector2(-108f, 0f);
            brt.offsetMax = Vector2.zero;

            // Подтверждение по Enter (onEndEdit срабатывает при нажатии Enter)
            field.onEndEdit.AddListener(val => OnNameConfirmed(field));
        }

        private void OnNameConfirmed(InputField field)
        {
            if (!_waitingForInput) return;
            string name = field != null ? field.text.Trim() : "";
            if (string.IsNullOrEmpty(name)) return; // не переходим при пустом имени
            PlayerState.Name = name;
            _waitingForInput = false;
            AdvanceLine();
        }

        // ─── Виджет выбора цвета ─────────────────────────────────────────────
        private void SpawnColorPicker()
        {
            _colorPickerGO = new GameObject("ColorPickerWidget", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            _colorPickerGO.transform.SetParent(_boxGo.transform, false);
            var wrt = (RectTransform)_colorPickerGO.transform;
            wrt.anchorMin = new Vector2(0f, 0f);
            wrt.anchorMax = new Vector2(1f, 0f);
            wrt.pivot     = new Vector2(0.5f, 0f);
            wrt.offsetMin = new Vector2(28f, 8f);
            wrt.offsetMax = new Vector2(-28f, 8f);
            wrt.sizeDelta = new Vector2(-56f, 52f);

            var hlg = _colorPickerGO.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing           = 8f;
            hlg.childAlignment    = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;

            foreach (var (label, color) in ColorOptions)
            {
                Color captured = color;
                var btn = aerisButton.Create(_colorPickerGO.transform, label,
                    new Vector2(80f, 48f),
                    () => OnColorPicked(captured),
                    color, new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f));
            }
        }

        private void OnColorPicked(Color color)
        {
            if (!_waitingForInput) return;
            ColorAccentSystem.Apply(color);
            _waitingForInput = false;
            AdvanceLine();
        }

        // ─── Виджет выбора варианта ответа ──────────────────────────────────
        private void SpawnChoiceButtons(ChoiceOption[] choices)
        {
            if (choices == null || choices.Length == 0) { _waitingForInput = false; AdvanceLine(); return; }

            // Контейнер со вертикальной раскладкой
            _choiceGO = new GameObject("ChoiceWidget", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _choiceGO.transform.SetParent(_boxGo.transform, false);
            var wrt = (RectTransform)_choiceGO.transform;
            wrt.anchorMin = new Vector2(0f, 0f);
            wrt.anchorMax = new Vector2(1f, 0f);
            wrt.pivot     = new Vector2(0.5f, 0f);
            wrt.offsetMin = new Vector2(28f, 8f);
            wrt.offsetMax = new Vector2(-28f, 8f);
            wrt.sizeDelta = new Vector2(-56f, choices.Length * 46f + (choices.Length - 1) * 6f);

            var vlg = _choiceGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing           = 6f;
            vlg.childAlignment    = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight     = false;

            Color btnTop = new Color(0.10f, 0.45f, 0.85f, 1f);
            Color btnBot = new Color(0.05f, 0.28f, 0.60f, 1f);

            foreach (var opt in choices)
            {
                var captured = opt;
                var btn = aerisButton.Create(_choiceGO.transform, captured.Label,
                    new Vector2(0f, 44f),
                    () => OnChoicePicked(captured),
                    btnTop, btnBot, 22);
                var brt = (RectTransform)btn.transform;
                brt.sizeDelta = new Vector2(0f, 44f);
            }
        }

        private void OnChoicePicked(ChoiceOption choice)
        {
            if (!_waitingForInput) return;
            _waitingForInput = false;

            // Вставляем строки ответа перед оставшимися строками
            if (choice.Response != null && choice.Response.Length > 0)
            {
                var after = new System.Collections.Generic.List<DialogLine>();
                after.AddRange(choice.Response);
                // добавляем строки которые шли бы после текущей
                for (int i = _lineIndex + 1; i < _lines.Length; i++)
                    after.Add(_lines[i]);
                _lines     = after.ToArray();
                _lineIndex = -1; // AdvanceLine сделает +1 → 0
            }

            AdvanceLine();
        }
    }
}
