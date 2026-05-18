using System.Linq;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aerisOS.UI
{
    // Состояния из actions.xml (только те, чьи кадры доступны: shime1-10, shime18-19)
    internal enum ChibiState
    {
        Idle,       // Stand:     shime1          (Duration 250)
        Walk,       // Walk:      1→2→1→3         (Duration 6/75 с)
        Run,        // Run:       1→2→1→3 быстрее (Duration 2/75 с)
        Falling,    // Falling:   shime4           (Duration 250)
        Bouncing,   // Bouncing:  18→19            (Duration 4/75 с)
        Dragging,   // Pinched:   5-10 позиционно  (Duration 5/75 с)
        Resisting,  // Resisting: 5→6→…→1→…       (Duration 5/75 с)
    }

    // Тайминги кадров — Duration из actions.xml (1 тик = 40 мс, стандарт Shimeji-EE)
    internal static class ShimejiTiming
    {
        public const float Walk     = 6f   * 0.04f;  // 0.240 с/кадр
        public const float Run      = 2f   * 0.04f;  // 0.080 с/кадр
        public const float Bounce   = 4f   * 0.04f;  // 0.160 с/кадр
        public const float Drag     = 5f   * 0.04f;  // 0.200 с/кадр
        public const float Idle     = 250f * 0.04f;  // 10.0 с (практически статика)
    }

    public class DesktopChibiSpawner : MonoBehaviour
    {
        private ChibiActor _actor;

        public ChibiDialogPanel DialogPanel => _actor?.GetDialogPanel();

        public void Build(RectTransform bounds)
        {
            _actor = ChibiFactory.Spawn(bounds, "Terra", 'T',
                new Color(0.75f, 0.95f, 1f, 0.95f), new Vector2(-120f, -170f));
        }

        public void StartIntroDrop() => _actor?.BeginIntroDrop();

        public void StartFade(float targetAlpha, float duration)
        {
            StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        private System.Collections.IEnumerator FadeRoutine(float target, float duration)
        {
            if (_actor == null) yield break;
            var cg = _actor.GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            float start = cg.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            cg.alpha = target;
        }
    }

    internal static class ChibiFactory
    {
        // Масштаб не применяется — спрайты отображаются в оригинальном размере
        private const float VisualScale = 1.0f;

        private static Sprite S(int n) =>
            Resources.Load<Sprite>($"Chibis/Shime/shime{n}");

        private static Sprite[] F(params int[] idx) =>
            idx.Select(S).Where(s => s != null).ToArray();

        public static ChibiActor Spawn(RectTransform bounds, string name,
            char glyph, Color tint, Vector2 startPos)
        {
            // Кадры согласно actions.xml ───────────────────────────────
            var walkFrames   = F(1, 2, 1, 3);          // Action "Walk"
            var runFrames    = F(1, 2, 1, 3);          // Action "Run" (те же, быстрее)
            var idleFrames   = F(1);                   // Action "Stand"
            var fallFrames   = F(4);                   // Action "Falling"
            var bounceFrames = F(19, 18, 1);           // приземление: почти упала → плюхнулась → встала

            // Action "Resisting": shime5,6 × повторы, прерванные shime1 (упрощено)
            var resistFrames = F(5,6,5,6,1, 5,6,5,6, 5,6,5,6,1);

            // Action "Pinched": спрайты по смещению [дальний-право … центр … дальний-лево]
            // FootX < cursor-50 → shime9, < cursor-30 → shime7, < cursor → shime5,
            // ≈cursor → shime1, > cursor+10 → shime6, > cursor+30 → shime8, > cursor+50 → shime10
            var pinchedSprites = new[] { S(9), S(7), S(5), S(1), S(6), S(8), S(10) };

            // GameObject ──────────────────────────────────────────────
            var go = new GameObject($"Chibi_{name}",
                typeof(RectTransform), typeof(CanvasGroup), typeof(ChibiActor));
            go.transform.SetParent(bounds, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(252f, 324f);
            rt.anchoredPosition = startPos;

            // Shadow
            var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadow.transform.SetParent(go.transform, false);
            var srt = (RectTransform)shadow.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(168f, 48f);
            srt.anchoredPosition = new Vector2(0f, 24f);
            var simg = shadow.GetComponent<Image>();
            simg.sprite = TextureFactory.Circle(64, new Color(0f, 0f, 0f, 0.16f));
            simg.raycastTarget = false;

            // Body
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(go.transform, false);
            var brt = (RectTransform)body.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(66f, 66f);
            var bimg = body.GetComponent<Image>();
            if (walkFrames.Length > 0)
            {
                bimg.sprite = walkFrames[0];
                bimg.SetNativeSize();
            }
            else
            {
                bimg.sprite = TextureFactory.AppIcon(96, tint, glyph);
            }
            bimg.raycastTarget = true;

            rt.sizeDelta = new Vector2(
                Mathf.Max(72f, brt.sizeDelta.x + 16f),
                Mathf.Max(92f, brt.sizeDelta.y + 32f));

            // Диалоговая панель крепится к рабочему столу (полный экран, минуя смещение ChibiLayer)
            var desktopRt = bounds.parent as RectTransform ?? bounds;
            var dialogGo = new GameObject("ChibiDialog", typeof(RectTransform), typeof(ChibiDialogPanel));
            dialogGo.transform.SetParent(desktopRt, false);
            var dialogPanel = dialogGo.GetComponent<ChibiDialogPanel>();
            dialogPanel.Build(desktopRt);

            var actor = go.GetComponent<ChibiActor>();
            actor.Initialize(bounds, startPos, bimg, simg,
                walkFrames, runFrames, idleFrames, fallFrames, bounceFrames,
                resistFrames, pinchedSprites, dialogPanel);
            return actor;
        }
    }

    internal class ChibiActor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private RectTransform _rt;
        private RectTransform _bounds;
        private Image _shadow;
        private Image _body;
        private ChibiDialogPanel _dialogPanel;

        private Sprite[] _walkFrames;
        private Sprite[] _runFrames;
        private Sprite[] _idleFrames;
        private Sprite[] _fallFrames;
        private Sprite[] _bounceFrames;
        private Sprite[] _resistFrames;
        private Sprite[] _pinchedSprites; // [0]=shime9 … [3]=shime1 … [6]=shime10

        private ChibiState _state;
        private float _phase;
        private float _animTimer;
        private int   _frameIndex;
        private float _stateTimer;

        private float   _fallVelocity;
        private float   _moveSpeedX;    // текущая скорость ходьбы/бега
        private float   _targetX;       // целевой X для Walk/Run
        private bool    _facingRight;
        private float   _groundY;

        private bool _introWaiting;
        private bool _introDrop;

        public ChibiDialogPanel GetDialogPanel() => _dialogPanel;

        // Drag
        private float _dragDeltaX;  // смещение курсора за кадр при drag
        private float _lastDragLocalX;

        // Таймеры поведений
        private float _idleDuration;

        public void Initialize(RectTransform bounds, Vector2 home, Image body, Image shadow,
            Sprite[] walkFrames, Sprite[] runFrames, Sprite[] idleFrames,
            Sprite[] fallFrames, Sprite[] bounceFrames,
            Sprite[] resistFrames, Sprite[] pinchedSprites, ChibiDialogPanel dialogPanel)
        {
            _rt = transform as RectTransform;
            _bounds = bounds;
            _body = body;
            _shadow = shadow;
            _walkFrames    = walkFrames;
            _runFrames     = runFrames;
            _idleFrames    = idleFrames;
            _fallFrames    = fallFrames;
            _bounceFrames  = bounceFrames;
            _resistFrames  = resistFrames;
            _pinchedSprites = pinchedSprites;

            _dialogPanel  = dialogPanel;
            _phase        = Random.Range(0f, 6.28f);
            _idleDuration = Random.Range(0.8f, 2f);

            // Сразу ставим персонажа на пол — без intro-падения
            // groundY = низ bounds + 4px (отступ для тени); bounds уже обрезан под taskbar
            float halfH  = bounds.rect.height * 0.5f - 56f;
            _groundY     = -(bounds.rect.height * 0.5f) + 4f;
            float floorY = Mathf.Clamp(_groundY, -halfH, halfH);
            _rt.anchoredPosition = new Vector2(home.x, floorY);

            _state        = ChibiState.Idle;
            _introWaiting = false;
            _introDrop    = false;
            _fallVelocity = 0f;
        }

        public void BeginIntroDrop()
        {
            if (!_introWaiting) return;
            _introWaiting = false;
            EnterFalling(-60f, introDrop: true);
        }

        private void Update()
        {
            if (_rt == null || _bounds == null) return;

            float dt    = Time.unscaledDeltaTime;
            float halfW = _bounds.rect.width  * 0.5f - 46f;
            float halfH = _bounds.rect.height * 0.5f - 56f;

            if (_groundY < -9000f)
                _groundY = -(_bounds.rect.height * 0.5f) + 4f;
            float floorY = Mathf.Clamp(_groundY, -halfH, halfH);

            if (_introWaiting)
            {
                _rt.anchoredPosition = new Vector2(0f, halfH);
                PlayAnimation(_idleFrames, ShimejiTiming.Idle);
                transform.localRotation = Quaternion.identity;
                transform.localScale    = Vector3.one;
                return;
            }

            switch (_state)
            {
                case ChibiState.Idle:      TickIdle(dt, floorY);                 break;
                case ChibiState.Walk:      TickWalk(dt, halfW, floorY);          break;
                case ChibiState.Run:       TickRun(dt, halfW, floorY);           break;
                case ChibiState.Falling:   TickFalling(dt, halfW, halfH, floorY); break;
                case ChibiState.Bouncing:  TickBouncing(dt, floorY);             break;
                case ChibiState.Dragging:  TickDragging();                       break;
                case ChibiState.Resisting: TickResisting(dt, floorY);            break;
            }

            TickShadow(floorY, halfH);
        }

        // ── Stand / Idle ──────────────────────────────────────────────
        private void TickIdle(float dt, float floorY)
        {
            SnapFloor(floorY);
            PlayAnimation(_idleFrames, ShimejiTiming.Idle);
            ApplyBob(idlePulse: true);

            _stateTimer += dt;
            if (_stateTimer >= _idleDuration)
                PickNextGroundBehavior();
        }

        // ── Walk ──────────────────────────────────────────────────────
        private void TickWalk(float dt, float halfW, float floorY)
        {
            AdvanceToTarget(dt, halfW, floorY, _moveSpeedX);
            PlayAnimation(_walkFrames, ShimejiTiming.Walk);
            ApplyBob(idlePulse: false);
        }

        // ── Run ───────────────────────────────────────────────────────
        private void TickRun(float dt, float halfW, float floorY)
        {
            AdvanceToTarget(dt, halfW, floorY, _moveSpeedX);
            PlayAnimation(_runFrames, ShimejiTiming.Run);
            ApplyBob(idlePulse: false);
        }

        // ── Falling ───────────────────────────────────────────────────
        private void TickFalling(float dt, float halfW, float halfH, float floorY)
        {
            var p = _rt.anchoredPosition;
            _fallVelocity -= 2300f * dt;
            p.y += _fallVelocity * dt;

            if (!_introDrop)
            {
                p.x += _moveSpeedX * dt * 0.25f;
                UpdateFacing(_moveSpeedX);
            }
            else
            {
                p.x = 0f;
            }

            if (p.y <= floorY)
            {
                p.y = floorY;
                float impact = Mathf.Abs(_fallVelocity);
                if (impact > 240f)
                {
                    // Высокий отскок: Bouncing → потом Stand
                    _fallVelocity = 0f;
                    EnterBouncing();
                }
                else
                {
                    _fallVelocity = 0f;
                    _introDrop = false;
                    EnterIdle();
                }
            }

            p.x = Mathf.Clamp(p.x, -halfW, halfW);
            p.y = Mathf.Clamp(p.y, floorY, halfH);
            _rt.anchoredPosition = p;

            PlayAnimation(_fallFrames, 0.14f);
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
        }

        // ── Bouncing (приземление): shime19 → shime18 → shime1, один раз ─
        private void TickBouncing(float dt, float floorY)
        {
            SnapFloor(floorY);
            PlayAnimation(_bounceFrames, ShimejiTiming.Bounce);
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;

            _stateTimer += dt;
            float bounceDur = _bounceFrames.Length > 0
                ? _bounceFrames.Length * ShimejiTiming.Bounce
                : 0.48f;

            if (_stateTimer >= bounceDur)
            {
                _introDrop = false;
                EnterIdle();
            }
        }

        // ── Pinched (перетаскивание) ──────────────────────────────────
        // Смещение определяется скоростью перетаскивания курсора (аналог FootX vs cursor.x)
        private void TickDragging()
        {
            if (_body == null) return;

            // Затухание: без движения курсора delta стремится к 0 → возврат к shime1
            _dragDeltaX = Mathf.Lerp(_dragDeltaX, 0f, Time.unscaledDeltaTime * 5f);

            // diff > 0: курсор движется вправо → нога "тянется" влево → shime5/7/9
            // diff < 0: курсор движется влево  → нога "тянется" вправо → shime6/8/10
            float diff = _dragDeltaX;

            Sprite sp;
            if      (diff  >  18f) sp = _pinchedSprites[0]; // shime9
            else if (diff  >   8f) sp = _pinchedSprites[1]; // shime7
            else if (diff  >   2f) sp = _pinchedSprites[2]; // shime5
            else if (diff  <  -18f) sp = _pinchedSprites[6]; // shime10
            else if (diff  <   -8f) sp = _pinchedSprites[5]; // shime8
            else if (diff  <   -2f) sp = _pinchedSprites[4]; // shime6
            else                   sp = _pinchedSprites[3]; // shime1

            if (sp != null && _body.sprite != sp)
            {
                _body.sprite = sp;
                _body.SetNativeSize();
            }

            float sway = Mathf.Sin(Time.unscaledTime * 1.5f + _phase) * 2.1f;
            transform.localRotation = Quaternion.Euler(0f, 0f, sway);
            transform.localScale    = Vector3.one * (1.003f + Mathf.Sin(Time.unscaledTime * 1.4f + _phase) * 0.008f);
        }

        // ── Resisting (после отпускания drag, перед падением) ─────────
        // behaviors.xml: Dragged → Pinched → Resisting (Loop)
        // При отпускании — Thrown/Fall
        // Здесь Resisting играет кратко на полу, если брошен без momentum
        private void TickResisting(float dt, float floorY)
        {
            SnapFloor(floorY);
            PlayAnimation(_resistFrames, ShimejiTiming.Drag);
            ApplyBob(idlePulse: false);

            _stateTimer += dt;
            // Длина Resisting из actions.xml ≈ 30 кадров × 5/75 = 2.0 с
            if (_stateTimer >= 2f)
                EnterIdle();
        }

        // ── State transitions ─────────────────────────────────────────

        private void EnterIdle()
        {
            _state        = ChibiState.Idle;
            _stateTimer   = 0f;
            _frameIndex   = 0;
            _animTimer    = 0f;
            _idleDuration = Random.Range(0.6f, 2.5f);
            transform.localScale    = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void EnterBouncing()
        {
            _state      = ChibiState.Bouncing;
            _stateTimer = 0f;
            _frameIndex = 0;
            _animTimer  = 0f;
            // Показываем первый кадр сразу — PlayAnimation делает +1 до отображения,
            // из-за чего frames[0] иначе пропускается
            if (_bounceFrames != null && _bounceFrames.Length > 0 && _body != null)
            {
                _body.sprite = _bounceFrames[0];
                _body.SetNativeSize();
            }
        }

        private void EnterFalling(float initialVY, bool introDrop = false)
        {
            _state        = ChibiState.Falling;
            _fallVelocity = initialVY;
            _introDrop    = introDrop;
            _stateTimer   = 0f;
            _frameIndex   = 0;
            _animTimer    = 0f;
        }

        // Выбор следующего поведения на полу (из behaviors.xml: Floor condition block)
        // StandUp Freq 200, WalkAlongWorkAreaFloor Freq 100, RunAlongWorkAreaFloor Freq 100
        private void PickNextGroundBehavior()
        {
            float halfW = _bounds.rect.width * 0.5f - 46f;
            float roll  = Random.value;

            if (roll < 0.5f)
            {
                // StandUp (ещё один цикл Idle)
                EnterIdle();
                return;
            }

            bool isRun = roll < 0.75f; // Run ~25%, Walk ~25%
            float speed = isRun ? Random.Range(45f, 80f) : Random.Range(18f, 36f);
            float dir   = Random.value < 0.5f ? 1f : -1f;

            _moveSpeedX = dir * speed;
            _targetX    = Mathf.Clamp(
                _rt.anchoredPosition.x + dir * Random.Range(80f, halfW * 0.85f),
                -halfW, halfW);

            _state      = isRun ? ChibiState.Run : ChibiState.Walk;
            _stateTimer = 0f;
            _frameIndex = 0;
            _animTimer  = 0f;
        }

        // ── Helpers ───────────────────────────────────────────────────

        // Движение к _targetX; при достижении → Idle
        private void AdvanceToTarget(float dt, float halfW, float floorY, float speed)
        {
            var p   = _rt.anchoredPosition;
            float dir = Mathf.Sign(_targetX - p.x);
            p.x += dir * Mathf.Abs(speed) * dt;

            UpdateFacing(dir);

            bool arrived = dir > 0f ? p.x >= _targetX : p.x <= _targetX;
            bool hitWall = p.x <= -halfW || p.x >= halfW;

            if (arrived || hitWall)
            {
                p.x = Mathf.Clamp(p.x, -halfW, halfW);
                p.y = floorY;
                _rt.anchoredPosition = p;
                EnterIdle();
                return;
            }

            p.x = Mathf.Clamp(p.x, -halfW, halfW);
            p.y = floorY;
            _rt.anchoredPosition = p;
        }

        private void SnapFloor(float floorY)
        {
            var p = _rt.anchoredPosition;
            p.y = floorY;
            _rt.anchoredPosition = p;
        }

        private void PlayAnimation(Sprite[] frames, float frameTime)
        {
            if (_body == null || frames == null || frames.Length == 0) return;
            if (frames.Length == 1)
            {
                if (_body.sprite != frames[0])
                {
                    _body.sprite = frames[0];
                    _body.SetNativeSize();
                }
                return;
            }
            _animTimer += Time.unscaledDeltaTime;
            if (_animTimer < frameTime) return;
            _animTimer  = 0f;
            _frameIndex = (_frameIndex + 1) % frames.Length;
            var next = frames[_frameIndex];
            if (_body.sprite != next)
            {
                _body.sprite = next;
                _body.SetNativeSize();
            }
        }

        private void ApplyBob(bool idlePulse)
        {
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
        }

        private void UpdateFacing(float dirX)
        {
            if (_body == null || Mathf.Abs(dirX) < 0.01f) return;
            _facingRight = dirX > 0f;
            var s  = _body.rectTransform.localScale;
            float ax = Mathf.Max(0.001f, Mathf.Abs(s.x));
            _body.rectTransform.localScale = new Vector3(_facingRight ? -ax : ax, 1f, 1f);
        }

        private void TickShadow(float floorY, float halfH)
        {
            if (_shadow == null) return;
            if (_state == ChibiState.Falling)
            {
                float norm = Mathf.InverseLerp(floorY, halfH, _rt.anchoredPosition.y);
                _shadow.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.24f, 0.08f, norm));
                _shadow.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(174f, 108f, norm), 42f);
            }
            else if (_state == ChibiState.Dragging)
            {
                _shadow.color = new Color(0f, 0f, 0f, 0.1f);
                _shadow.rectTransform.sizeDelta = new Vector2(132f, 36f);
            }
            else
            {
                _shadow.color = new Color(0f, 0f, 0f, 0.16f);
                _shadow.rectTransform.sizeDelta = new Vector2(168f, 48f);
            }
        }

        // ── Drag handlers ─────────────────────────────────────────────

        public void OnBeginDrag(PointerEventData eventData)
        {
            _state        = ChibiState.Dragging;
            _fallVelocity = 0f;
            _introDrop    = false;
            _dragDeltaX   = 0f;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _bounds, eventData.position, eventData.pressEventCamera, out var local))
            {
                _lastDragLocalX = local.x;
            }

            // Оригинальное направление при подъёме — всегда влево
            _facingRight = false;
            if (_body != null)
            {
                var s  = _body.rectTransform.localScale;
                float ax = Mathf.Max(0.001f, Mathf.Abs(s.x));
                _body.rectTransform.localScale = new Vector3(ax, s.y, s.z);
            }

            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rt == null || _bounds == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _bounds, eventData.position, eventData.pressEventCamera, out var local))
                return;

            // Скорость курсора по X (в пикселях UI за кадр)
            _dragDeltaX     = local.x - _lastDragLocalX;
            _lastDragLocalX = local.x;

            float halfW = _bounds.rect.width  * 0.5f - 42f;
            float halfH = _bounds.rect.height * 0.5f - 52f;
            _rt.anchoredPosition = new Vector2(
                Mathf.Clamp(local.x, -halfW, halfW),
                Mathf.Clamp(local.y, -halfH, halfH));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Thrown: падение с начальной горизонтальной скоростью из lastDragDelta
            // behaviors.xml "Thrown": Falling InitialVX=cursor.dx, InitialVY=cursor.dy
            _moveSpeedX   = _dragDeltaX / Mathf.Max(Time.unscaledDeltaTime, 0.0001f) * 0.15f;
            EnterFalling(-80f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Правый клик открывает диалоговое окно в стиле визуальной новеллы
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (_state == ChibiState.Dragging) return;
            _dialogPanel?.Show();
        }
    }
}
