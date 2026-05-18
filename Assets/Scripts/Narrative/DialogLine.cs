using System;
using UnityEngine;

namespace aerisOS.Narrative
{
    public enum DialogMood
    {
        Calm     = 0,
        Happy    = 1,
        Sad      = 2,
        Surprised = 3,
        Angry    = 4,
    }

    public enum DialogPauseType
    {
        None,
        NameInput,
        ColorPicker,
        WaitForEvent,
        Choice,
    }

    public class ChoiceOption
    {
        public string       Label;
        public DialogLine[] Response;
    }

    public class DialogLine
    {
        public string         Text;
        public DialogMood     Mood;
        public DialogPauseType PauseType;
        public Action         OnReach;
        public ChoiceOption[] Choices;    // только для PauseType.Choice
        public bool           AutoAdvance; // если true — сам переходит после тайпрайтера (без клика)

        public static DialogLine Say(string text, DialogMood mood = DialogMood.Calm, Action onReach = null)
            => new DialogLine { Text = text, Mood = mood, OnReach = onReach };

        // Строка, которая автоматически переходит вперёд после окончания тайпрайтера
        public static DialogLine AutoSay(string text, DialogMood mood = DialogMood.Calm, Action onReach = null)
            => new DialogLine { Text = text, Mood = mood, OnReach = onReach, AutoAdvance = true };

        public static DialogLine NameInput(string prompt, DialogMood mood = DialogMood.Happy)
            => new DialogLine { Text = prompt, Mood = mood, PauseType = DialogPauseType.NameInput };

        public static DialogLine ColorPick(string prompt, DialogMood mood = DialogMood.Calm)
            => new DialogLine { Text = prompt, Mood = mood, PauseType = DialogPauseType.ColorPicker };

        public static DialogLine WaitEvent(string text = "", DialogMood mood = DialogMood.Calm, Action onReach = null)
            => new DialogLine { Text = text, Mood = mood, PauseType = DialogPauseType.WaitForEvent, OnReach = onReach };

        public static DialogLine Choice(string prompt, ChoiceOption[] choices, DialogMood mood = DialogMood.Calm)
            => new DialogLine { Text = prompt, Mood = mood, PauseType = DialogPauseType.Choice, Choices = choices };
    }
}
