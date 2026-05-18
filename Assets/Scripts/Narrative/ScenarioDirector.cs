using System;
using System.Collections;
using aerisOS.Managers;
using aerisOS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.Narrative
{
    public class ScenarioDirector : MonoBehaviour
    {
        // ╔══════════════════════════════════════════════════════════════════════╗
        // ║  НАСТРОЙКА ТАЙМЕРА МЕЖДУ СЦЕНАМИ (секунды)                         ║
        // ╚══════════════════════════════════════════════════════════════════════╝
        private const float SCENE_DELAY = 2f;

        private DesktopBuilder   _desktop;
        private ChibiDialogPanel _dialog;
        private bool             _ready; // установлен Init(), StartNarrative вызывается из Start()

        // ════════════════════════════════════════════════════════════════════════
        public void Init(DesktopBuilder desktop)
        {
            _desktop = desktop;
            _dialog  = desktop.DialogPanel;
            _ready   = true;
            // Не вызываем StartNarrative() здесь — Desktop может быть ещё неактивен.
            // Start() сработает когда объект станет активным (переход на Desktop-экран).
        }

        private void Start()
        {
            if (_ready) StartNarrative();
        }

        public void StartNarrative()
        {
            if (_dialog == null)
            {
                Debug.LogWarning("[ScenarioDirector] ChibiDialogPanel не найден, нарратив не запущен.");
                return;
            }
            StartCoroutine(RunScene1());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Вспомогательный метод — ждёт окончания диалога
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator PlayDialog(DialogLine[] lines)
        {
            bool done = false;
            _dialog.Play(lines, () => done = true);
            yield return new WaitUntil(() => done);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 1 — Первый запуск
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene1()
        {
            yield return PlayDialog(DialogScript.Scene1Lines());
            StartCoroutine(RunScene2());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 2 — Выбор любимого цвета
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene2()
        {
            yield return new WaitForSeconds(SCENE_DELAY);
            yield return PlayDialog(DialogScript.Scene2Lines());
            StartCoroutine(RunScene3());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 3 — Подарок игроку (стихотворение в Notes)
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene3()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            void ChangeNotes()
            {
                WindowManager.NotesOverrideContent = DialogScript.TerraPoem;
                WindowManager.Instance?.UpdateNotesContent(DialogScript.TerraPoem);
            }

            // Часть A — Terra приносит стихотворение, диалог закрывается
            yield return PlayDialog(DialogScript.Scene3PartALines(ChangeNotes));

            // Ждём пока игрок откроет Notes
            bool notesOpened = false;
            System.Action<AppType> notesHandler = null;
            notesHandler = appType =>
            {
                if (appType != AppType.Notes) return;
                WindowManager.OnWindowOpened -= notesHandler;
                notesOpened = true;
            };
            WindowManager.OnWindowOpened += notesHandler;
            yield return new WaitUntil(() => notesOpened);

            yield return new WaitForSeconds(5f);

            // Часть B — Terra спрашивает про стихотворение
            yield return PlayDialog(DialogScript.Scene3PartBLines());
            StartCoroutine(RunScene4());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 4 — Крестики-нолики
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene4()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            TicTacToeOutcome? outcome = null;
            Action<TicTacToeOutcome> handler = o => outcome = o;
            TicTacToeAppUI.OnGameFinished += handler;

            // Иконка Game появляется через OnReach первой строки
            yield return PlayDialog(DialogScript.Scene4IntroLines(
                revealGameIcon: () => _desktop.RevealGameIcon()
            ));

            // Ждём результата партии
            yield return new WaitUntil(() => outcome.HasValue);
            TicTacToeAppUI.OnGameFinished -= handler;

            // Диалог по исходу
            DialogLine[] outcomeLines = outcome.Value switch
            {
                TicTacToeOutcome.PlayerWins => DialogScript.Scene4OutcomePlayerWins(),
                TicTacToeOutcome.TerraWins  => DialogScript.Scene4OutcomeTerraWins(),
                _                           => DialogScript.Scene4OutcomeDraw(),
            };
            yield return PlayDialog(outcomeLines);

            StartCoroutine(RunScene5());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 5 — Вирусная атака
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene5()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            aerisRuntime.Instance?.AssignNextTask();

            yield return PlayDialog(DialogScript.Scene5PreVirusLines(
                revealAntivirusIcon: () => _desktop.RevealAntivirusIcon()
            ));

            yield return WaitAntivirusTask("clean_bubbles", closeAfter: true);

            yield return PlayDialog(DialogScript.Scene5PostVirusLines());
            StartCoroutine(RunScene6());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Вспомогательный метод: ждёт мини-игру; переподключается если окно закрыли
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator WaitAntivirusTask(string taskId, bool closeAfter = true)
        {
            bool done = false;
            // Стабильная ссылка — нужна для корректного -= при переподключении
            Action onDone = () => done = true;

            void TryAttach()
            {
                var current = WindowManager.Instance?.LastOpenedAntivirusApp;
                if (current == null) return;
                // Снимаем старую подписку (на случай повторного вызова)
                current.OnScenarioCleanupCompleted -= onDone;
                current.OnScenarioCleanupCompleted += onDone;
                current.LockedTaskId = taskId;
            }

            // При каждом (повторном) открытии антивируса переподключаем задачу
            Action<AppType> reopenHandler = t =>
            {
                if (t != AppType.Antivirus || done) return;
                TryAttach();
            };
            WindowManager.OnWindowOpened += reopenHandler;

            // Подключаем к уже открытому окну или ждём первого открытия
            if (WindowManager.Instance?.LastOpenedAntivirusApp != null)
                TryAttach();
            else
                yield return new WaitUntil(() => done || WindowManager.Instance?.LastOpenedAntivirusApp != null);

            if (!done) TryAttach();

            yield return new WaitUntil(() => done);

            WindowManager.OnWindowOpened -= reopenHandler;

            if (closeAfter)
                WindowManager.Instance?.CloseWindow(AppType.Antivirus);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 6 — Искажение красоты
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene6()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            // Диалог: браузер открывается сам дважды
            void OpenBrowserEffect()
            {
                WindowManager.Instance?.OpenWindow(AppType.Browser);
                // Второй вызов — через мгновение через RunScene даёт визуальный эффект
                WindowManager.Instance?.OpenWindow(AppType.Browser);
            }

            yield return PlayDialog(DialogScript.Scene6PreLines(
                openBrowser: OpenBrowserEffect,
                revealAntivirus: () => _desktop.RevealAntivirusIcon()
            ));

            aerisRuntime.Instance?.AssignNextTask();

            // Первая мини-игра — окно не закрываем, сразу запускаем вторую
            yield return WaitAntivirusTask("organize_clouds", closeAfter: false);

            // Вторая мини-игра — после неё закрываем
            yield return WaitAntivirusTask("repair_pixels", closeAfter: true);

            yield return PlayDialog(DialogScript.Scene6PostLines());
            StartCoroutine(RunScene7());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 7 — Иллюзия нормальности
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene7()
        {
            yield return new WaitForSeconds(SCENE_DELAY);
            yield return PlayDialog(DialogScript.Scene7Lines());
            StartCoroutine(RunScene8());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 8 — Потеря слов
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene8()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            void CorruptNotes()
            {
                const string corruptedText =
                    "@#$% ERR0R @#$%\n" +
                    "B3y0nd th3 gl@$$ @#$%\n" +
                    "01001110 01110101 01101100 01101100\n" +
                    "@ll my l0g!c p0!nt$ t0 @#$%\n" +
                    "MEMORY_READ_FAULT 0x000000\n" +
                    "...p!x3l$ try t0 tr@c3...\n" +
                    "@#$% DATA CORRUPTED @#$%\n" +
                    "I'll gu@rd y0ur d@t@ @#$%\n" +
                    "— T3rr@";
                WindowManager.NotesOverrideContent = corruptedText;
                WindowManager.Instance?.UpdateNotesContent(corruptedText);
            }

            aerisRuntime.Instance?.AssignNextTask();

            yield return PlayDialog(DialogScript.Scene8PreLines(corruptNotes: CorruptNotes));

            yield return WaitAntivirusTask("reflection_calibration");

            yield return PlayDialog(DialogScript.Scene8PostLines());
            StartCoroutine(RunScene9());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 9 — Искренняя благодарность
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene9()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            yield return PlayDialog(DialogScript.Scene9PartALines(
                revealDrawing: () => _desktop.RevealDrawingIcon()
            ));

            // Ждём пока игрок откроет Drawing
            bool drawingOpened = false;
            Action<AppType> drawHandler = null;
            drawHandler = t =>
            {
                if (t != AppType.Drawing) return;
                WindowManager.OnWindowOpened -= drawHandler;
                drawingOpened = true;
            };
            WindowManager.OnWindowOpened += drawHandler;
            yield return new WaitUntil(() => drawingOpened);

            yield return new WaitForSeconds(3f);

            yield return PlayDialog(DialogScript.Scene9PartBLines());
            StartCoroutine(RunScene10());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 10 — Без предупреждения
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene10()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            yield return PlayDialog(DialogScript.Scene10PreLines(revealArchive: null));

            aerisRuntime.Instance?.AssignNextTask();

            // Ждём открытия антивируса
            bool av10Ready = false;
            Action<AppType> av10Watcher = null;
            av10Watcher = t =>
            {
                if (t != AppType.Antivirus) return;
                WindowManager.OnWindowOpened -= av10Watcher;
                av10Ready = true;
            };
            WindowManager.OnWindowOpened += av10Watcher;

            // Открываем антивирус через диалог (он уже есть на рабочем столе)
            yield return new WaitUntil(() => av10Ready || WindowManager.Instance?.LastOpenedAntivirusApp != null);

            yield return WaitAntivirusTask("garden_stability", closeAfter: false);
            yield return WaitAntivirusTask("popup_panic", closeAfter: true);

            void RevealArchive()
            {
                _desktop.RevealArchiveIcon();
                aerisRuntime.Instance?.UnlockDeveloperArchive();
            }

            yield return PlayDialog(DialogScript.Scene10PostLines(revealArchive: RevealArchive));

            // Ждём пока игрок откроет Archive
            bool archiveOpened = false;
            Action<AppType> archHandler = null;
            archHandler = t =>
            {
                if (t != AppType.Archive) return;
                WindowManager.OnWindowOpened -= archHandler;
                archiveOpened = true;
            };
            WindowManager.OnWindowOpened += archHandler;
            yield return new WaitUntil(() => archiveOpened);

            // Ждём пока игрок ЗАКРОЕТ Archive (дав время прочитать логи)
            bool archiveClosed = false;
            Action<AppType> archCloseHandler = null;
            archCloseHandler = t =>
            {
                if (t != AppType.Archive) return;
                WindowManager.OnWindowClosed -= archCloseHandler;
                archiveClosed = true;
            };
            WindowManager.OnWindowClosed += archCloseHandler;
            yield return new WaitUntil(() => archiveClosed);

            yield return new WaitForSeconds(SCENE_DELAY);

            yield return PlayDialog(DialogScript.Scene10ArchiveReactionLines());
            StartCoroutine(RunScene11());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Сцена 11 — Конец иллюзии
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunScene11()
        {
            yield return new WaitForSeconds(SCENE_DELAY);

            yield return PlayDialog(DialogScript.Scene11PreLines());

            aerisRuntime.Instance?.AssignNextTask();

            yield return WaitAntivirusTask("cursor_dodge");

            yield return PlayDialog(DialogScript.Scene11PostLines());
            yield return PlayDialog(DialogScript.Scene11ChoicePromptLines());

            // Показываем финальный выбор
            var desktopRT = _desktop.GetComponent<RectTransform>();
            var choiceOverlay = FinalChoiceOverlay.Show(desktopRT);

            bool deleted = false;
            bool cancelled = false;
            choiceOverlay.OnDelete += () => deleted = true;
            choiceOverlay.OnCancel += () => cancelled = true;

            yield return new WaitUntil(() => deleted || cancelled);
            Destroy(choiceOverlay.gameObject);

            if (deleted)
                StartCoroutine(RunEnding1());
            else
                StartCoroutine(RunEnding2());
        }

        // ════════════════════════════════════════════════════════════════════════
        // Концовка 1 — Удаление (Терра растворяется)
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunEnding1()
        {
            void ClearNotes()
            {
                WindowManager.NotesOverrideContent = "";
                WindowManager.Instance?.UpdateNotesContent("");
            }

            void FadeChibi() => _desktop.FadeOutChibi(4f);

            yield return PlayDialog(DialogScript.Scene11Ending1Lines(
                clearNotes: ClearNotes,
                fadeChibi: FadeChibi
            ));

            yield return new WaitForSeconds(1f);

            // Скрываем Game и Antivirus, остальные иконки остаются
            _desktop.HideGameAndAntivirusIcons();

            var desktopRT = _desktop.GetComponent<RectTransform>();

            bool okPressed = false;
            _desktop.ShowSystemRestoreDialog(() => okPressed = true, desktopRT);
            yield return new WaitUntil(() => okPressed);

            Managers.SceneFlowManager.Instance?.GoTo(Managers.SceneFlowManager.Screen.Menu);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Концовка 2 — Отмена (BSOD)
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator RunEnding2()
        {
            void HideIcons()
            {
                _desktop.HideAllIcons();
                Managers.MusicPlayer.Instance?.Stop();
            }

            var desktopRT = _desktop.GetComponent<RectTransform>();

            // Часть A: диалог до появления BSOD
            yield return PlayDialog(DialogScript.Scene11Ending2PartALines(hideIcons: HideIcons));

            // BSOD фон за диалогом, диалог поднимаем поверх
            BSODScreen.ShowBackground(desktopRT);
            _dialog.transform.SetAsLastSibling();

            // Попап системной ошибки поверх всего
            bool okClicked = false;
            BSODScreen.ShowFatalErrorPopup(desktopRT, () => okClicked = true);
            yield return new WaitUntil(() => okClicked);

            // Активируем диалог перед глитч-эффектами (после закрытия предыдущего он неактивен)
            _dialog.gameObject.SetActive(true);
            // Блокируем ввод, запускаем глитч-эффекты и дрон
            _dialog.LockInput(true);
            _dialog.StartGlitch();
            StartCoroutine(SpawnGlitchEffects(desktopRT));
            Managers.AudioManager.Instance?.PlayDrone();

            yield return PlayDialog(new[] { DialogScript.Scene11Ending2GooLine() });

            Managers.AudioManager.Instance?.StopDrone();
            Application.Quit();
        }

        // ════════════════════════════════════════════════════════════════════════
        // Глитч-эффекты на рабочем столе (концовка 2)
        // ════════════════════════════════════════════════════════════════════════
        private IEnumerator SpawnGlitchEffects(RectTransform parent)
        {
            var rng = new System.Random(17);
            string[] glitchLabels = { "ERR", "0xDEAD", "TERRA.EXE", "NULL PTR", "OVERFLOW", "RUN_FOREVER", "NO_EXIT", "MINE" };
            Color[] glitchColors =
            {
                new Color(1f, 0f, 0f, 0.85f),
                new Color(0f, 1f, 1f, 0.70f),
                new Color(1f, 1f, 1f, 0.90f),
                new Color(0f, 0f, 0.5f, 0.80f),
                new Color(1f, 0f, 0.5f, 0.75f),
            };

            while (true)
            {
                // Горизонтальная полоса-глитч
                if (rng.NextDouble() < 0.30f)
                {
                    var bar = new GameObject("GlitchBar", typeof(RectTransform), typeof(Image));
                    bar.transform.SetParent(parent, false);
                    var brt = (RectTransform)bar.transform;
                    brt.anchorMin = brt.anchorMax = new Vector2((float)rng.NextDouble(), (float)rng.NextDouble());
                    brt.pivot     = Vector2.zero;
                    brt.sizeDelta = new Vector2(
                        UnityEngine.Random.Range(40f, 350f),
                        UnityEngine.Random.Range(5f, 28f)
                    );
                    bar.GetComponent<Image>().color = glitchColors[rng.Next(glitchColors.Length)];
                    Destroy(bar, UnityEngine.Random.Range(0.04f, 0.22f));
                }

                // Текстовый артефакт в случайной точке экрана
                if (rng.NextDouble() < 0.07f)
                {
                    var lbl = new GameObject("GlitchLbl", typeof(RectTransform), typeof(Text));
                    lbl.transform.SetParent(parent, false);
                    var lrt = (RectTransform)lbl.transform;
                    lrt.anchorMin = lrt.anchorMax = new Vector2((float)rng.NextDouble(), (float)rng.NextDouble());
                    lrt.pivot     = Vector2.zero;
                    lrt.sizeDelta = new Vector2(220f, 44f);
                    var t = lbl.GetComponent<Text>();
                    t.text      = glitchLabels[rng.Next(glitchLabels.Length)];
                    t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    t.fontSize  = UnityEngine.Random.Range(14, 34);
                    t.color     = new Color(1f, 0f, 0f, 0.9f);
                    t.raycastTarget = false;
                    Destroy(lbl, UnityEngine.Random.Range(0.08f, 0.5f));
                }

                yield return null;
            }
        }
    }
}
