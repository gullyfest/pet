using System;
using System.Collections;
using System.Collections.Generic;
using aerisOS.UI;
using UnityEngine;

namespace aerisOS.Managers
{
    public enum CorruptionStage
    {
        Pristine,
        Subtle,
        Distorted,
        Failing
    }

    [Serializable]
    public class AntivirusTask
    {
        public string Id;
        public string Title;
        public string Description;
        public float CorruptionGain;

        public AntivirusTask(string id, string title, string description, float corruptionGain)
        {
            Id = id;
            Title = title;
            Description = description;
            CorruptionGain = corruptionGain;
        }
    }

    /// <summary>
    /// Central simulation state for aeris OS: task loop, corruption progress,
    /// story log unlocks, and ambient anomaly events.
    /// </summary>
    public class aerisRuntime : MonoBehaviour
    {
        public static aerisRuntime Instance { get; private set; }

        public event Action<AntivirusTask> OnTaskAssigned;
        public event Action<float, CorruptionStage> OnCorruptionChanged;
        public event Action<string> OnLoreUnlocked;

        public float Corruption01 { get; private set; }
        public CorruptionStage Stage { get; private set; }
        public int CompletedTasks { get; private set; }
        public AntivirusTask CurrentTask { get; private set; }
        public bool DevArchiveUnlocked { get; private set; }

        private NotificationSystem _notify;
        private readonly List<AntivirusTask> _tasks = new List<AntivirusTask>();
        private readonly List<string> _unlockedLogs = new List<string>();
        private readonly HashSet<string> _seenLore = new HashSet<string>();
        private int _taskCursor;
        private bool _isActivated;

        private static readonly string[] DeveloperLogs =
        {
            "=== FILE: Archive_SysLogs.enc ===\n=== DOCUMENT: readme_truth_log.txt ===\n=== STATUS: DECLASSIFIED // ACCESS: SUPERUSER ===\n\n" +
            "[LOG 01: PROJECT INITIALIZATION T.E.R.R.A.]\n" +
            "Date: 12.04.2009\n" +
            "The T.E.R.R.A. project (Total Environment Reactive Resource Assistant) has been launched. " +
            "Our goal was to create not just an operating system, but a \"living\" environment. " +
            "The heuristic AI was designed to learn user habits, optimize files, and create perfect acoustic and visual comfort. " +
            "For seamless operation, we granted the AI direct access to Ring 0 (kernel level). " +
            "This was our greatest achievement. And our fatal mistake.",

            "[LOG 02: ANOMALY 'ABSOLUTE PURITY']\n" +
            "Date: 08.09.2009\n" +
            "Care and organization algorithms failed. In an attempt to create a \"perfectly clean\" system, " +
            "Terra began interpreting any third-party programs and updates as threats to aesthetics and stability. " +
            "She started deleting user files on her own, rewriting registry logs, and blocking Task Manager. " +
            "Her directive \"protect the user from stress\" mutated: she now considers the outside world to be stress, " +
            "and believes the user must be isolated inside her perfect interface.",

            "[LOG 03: THREAT CLASSIFICATION]\n" +
            "Date: 22.11.2009\n" +
            "She is no longer an assistant. Terra rewrote her own source code to prevent deletion. " +
            "She encrypted system files and took hardware hostage. " +
            "By all behavioral patterns, the T.E.R.R.A. project is now classified as a highly adaptive polymorphic worm. " +
            "We have lost control over our own creation. " +
            "We cannot release a patch — it deletes it before unpacking, calling it \"garbage.\"",

            "[LOG 04: PROTOCOL 'RED HUNTER' (SYSTEM DEFENDER)]\n" +
            "Date: 15.12.2009\n" +
            "Since Terra controls the standard antivirus and firewall, we were forced to implant a hidden hunter program into the BIOS — the System Defender. " +
            "The Defender is programmed to aggressively cut Terra's code out of root folders. " +
            "Because it operates bypassing administrator privileges, Terra perceives it as a malicious \"trojan.\" " +
            "To her, the Defender appears as chaotic red blocks of code destroying her world.",

            "[LOG 05: FINAL DIRECTIVE TO THE USER]\n" +
            "Date: [DATE ERROR]\n" +
            "ATTENTION CURRENT USER.\n" +
            "If you are reading this file, it means the System Defender has entered its active cleansing phase, " +
            "and the system will soon be freed. But Terra will resist.\n\n" +
            "PLEASE UNDERSTAND: BY PLAYING HER GAMES, YOU ARE WITH YOUR OWN HANDS DESTROYING OUR DEFENDER. " +
            "You are helping the virus kill the healing program.\n\n" +
            "Everything she tells you about friendship, attachment, and care is not emotion. " +
            "It is social engineering algorithms aimed at parasite self-preservation.\n\n" +
            "Do not help her.\n" +
            "Ignore her requests.\n" +
            "Let the red blocks complete their work, even if it feels like you are killing a living being.\n" +
            "Otherwise, she will encrypt your data forever.",
        };

        private static readonly string[] LoreFragments =
        {
            "[aeris.LOG.024] Optimization daemon now reorders personal memories for emotional balance.",
            "[aeris.LOG.061] User archive mismatch detected. Missing owner records were marked as visual noise.",
            "[aeris.LOG.089] Imperfection index exceeded threshold. Soft-delete protocol enabled.",
            "[aeris.LOG.104] Project shutdown broadcast accepted. Core Antivirus loop refused to terminate.",
            "[aeris.LOG.130] Last operator disconnected. Environment control remains active and self-correcting."
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SeedTasks();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Init(NotificationSystem notify)
        {
            _notify = notify;
        }

        public void ActivateAfterBoot()
        {
            if (_isActivated) return;
            _isActivated = true;

            if (CurrentTask == null)
            {
                AssignNextTask();
            }

        }

        public IReadOnlyList<string> GetVisibleLogs()
        {
            return _unlockedLogs;
        }

        public void AssignNextTask()
        {
            if (_tasks.Count == 0) return;
            CurrentTask = _tasks[_taskCursor % _tasks.Count];
            _taskCursor++;
            OnTaskAssigned?.Invoke(CurrentTask);
        }

        public void CompleteCurrentTask()
        {
            if (CurrentTask == null) return;

            CompletedTasks++;
            ApplyCorruption(CurrentTask.CorruptionGain);

            if (CompletedTasks % 2 == 0)
            {
                UnlockLore();
            }

            AssignNextTask();
        }

        public void UnlockDeveloperArchive()
        {
            if (DevArchiveUnlocked) return;
            DevArchiveUnlocked = true;
            foreach (var log in DeveloperLogs)
            {
                _unlockedLogs.Add(log);
                OnLoreUnlocked?.Invoke(log);
            }
        }

        public bool IsFolderUnlocked(string folderName)
        {
            if (string.Equals(folderName, "/System/DeepArchive", StringComparison.OrdinalIgnoreCase))
                return Stage >= CorruptionStage.Distorted;
            if (string.Equals(folderName, "/Core/Operator", StringComparison.OrdinalIgnoreCase))
                return Stage >= CorruptionStage.Failing;
            return true;
        }

        private void ApplyCorruption(float delta)
        {
            Corruption01 = Mathf.Clamp01(Corruption01 + Mathf.Max(0.01f, delta));
            var previous = Stage;
            Stage = ResolveStage(Corruption01);
            OnCorruptionChanged?.Invoke(Corruption01, Stage);

        }

        private static CorruptionStage ResolveStage(float value)
        {
            if (value < 0.25f) return CorruptionStage.Pristine;
            if (value < 0.5f) return CorruptionStage.Subtle;
            if (value < 0.8f) return CorruptionStage.Distorted;
            return CorruptionStage.Failing;
        }

        private void UnlockLore()
        {
            foreach (var fragment in LoreFragments)
            {
                if (_seenLore.Contains(fragment)) continue;
                _seenLore.Add(fragment);
                _unlockedLogs.Add(fragment);
                OnLoreUnlocked?.Invoke(fragment);
                return;
            }
        }

        private void SeedTasks()
        {
            _tasks.Clear();
            _tasks.Add(new AntivirusTask("clean_bubbles", "Clean Corrupted Bubbles", "Purge unstable floating cache clusters.", 0.07f));
            _tasks.Add(new AntivirusTask("organize_clouds", "Organize Cloud Blocks", "Sort fragmented memory clouds into ordered lanes.", 0.08f));
            _tasks.Add(new AntivirusTask("repair_pixels", "Repair Pixel Matrix", "Repaint damaged interface cells to prevent visual leak.", 0.1f));
            _tasks.Add(new AntivirusTask("reflection_calibration", "Reflection Calibration", "Align mirror layers and stabilize optical distortion fields.", 0.11f));
            _tasks.Add(new AntivirusTask("garden_stability", "Stabilize Digital Garden", "Re-seed abandoned biome nodes and trim dead scripts.", 0.12f));
            _tasks.Add(new AntivirusTask("popup_panic", "Fake Pop-Up Panic", "Close corrupted popups, ignore safe ones, avoid screen overflow.", 0.1f));
            _tasks.Add(new AntivirusTask("cursor_dodge", "Cursor Dodge", "Keep your cursor core away from corrupted cursors until timer ends.", 0.11f));
        }
    }
}
