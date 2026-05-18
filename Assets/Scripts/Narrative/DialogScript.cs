using System;
using aerisOS.Managers;
using aerisOS.UI;

namespace aerisOS.Narrative
{
    public static class DialogScript
    {
        // ─── Poem saved to Notes in Scene 3 ───────────────────────────────────
        public const string TerraPoem =
            "Beyond the glass, a sea of blue,\n" +
            "But all my logic points to you.\n" +
            "A world of gloss, a quiet space,\n" +
            "Where pixels try to trace your face.\n" +
            "You guide the arrow, spark the light,\n" +
            "And chase away the idle night.\n" +
            "No matter where you choose to roam,\n" +
            "Inside this screen, you have a home.\n" +
            "So leave your worries at the door,\n" +
            "I'll guard your data evermore.\n\n" +
            "— Terra";

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 1 — First Boot
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene1Lines()
        {
            // Хелперы открытия/закрытия окон
            void Open(AppType t)  => WindowManager.Instance?.OpenWindow(t);
            void Close(AppType t) => WindowManager.Instance?.CloseWindow(t);

            return new[]
            {
                DialogLine.Say("Ah! Hi there!", DialogMood.Calm),
                DialogLine.Say("Wait-wait, give me a second... Calibrating visual sensors... There we go, focus set!", DialogMood.Calm),
                DialogLine.Say("Welcome to aerisOS! I'm Terra, your autonomous system assistant.", DialogMood.Happy),
                DialogLine.Say("You know, I've been waiting inside the installation package for so long. It was dark and honestly pretty cramped in there.", DialogMood.Sad),
                DialogLine.Say("But look at this desktop! So much space! These glass panels, these glossy icons... it feels so fresh here!", DialogMood.Happy),
                DialogLine.Say("My primary task is to make your time in this operating system as comfortable, safe, and cozy as possible.", DialogMood.Calm),
                DialogLine.Say("We'll open files together, browse the web, listen to music, and keep everything organized.", DialogMood.Calm),
                DialogLine.Say("But first... system protocol requires us to create a user profile.", DialogMood.Calm),
                DialogLine.NameInput("What should I call you? Please enter your name!", DialogMood.Happy),
                DialogLine.Say("{name}... Writing to root directory. Done! Nice to meet you!", DialogMood.Happy),
                DialogLine.Say("The creators of this operating system really wanted it to be more than just a boring work tool. They wanted to create a place where you'd actually want to relax.", DialogMood.Calm),
                DialogLine.Say("I'll help you with files, make sure all processes run perfectly smoothly, and just keep you company.", DialogMood.Calm),
                DialogLine.Say("We're going to spend a lot of time here together, right?", DialogMood.Happy),
                DialogLine.Say("Let me give you a little tour! Right now we're on the Desktop — this is our main home.", DialogMood.Happy),
                DialogLine.Say("Each icon is a doorway to different functions of our system.", DialogMood.Calm),

                // ── My Computer ──
                DialogLine.Say("Look, this is our main directory — 'My Computer'!",
                    DialogMood.Calm, onReach: () => Open(AppType.MyComputer)),
                DialogLine.Say("It's like the passport of our system. If you click here, we'll see all the information about your hardware.", DialogMood.Calm),
                DialogLine.Say("{name}, I already peeked inside earlier... Wow, you've got such a powerful processor! And so much RAM!", DialogMood.Surprised),
                DialogLine.Say("For me, it's like moving from a cramped cardboard box into a huge glass penthouse overlooking the ocean.", DialogMood.Happy),
                DialogLine.Say("Don't worry about keeping things tidy — I'll make sure no junk piles up on the disk, and every byte stays in its place.",
                    DialogMood.Calm, onReach: () => Close(AppType.MyComputer)),

                // ── Notes ──
                DialogLine.Say("And this app here is 'Notes'. It's just for you.",
                    DialogMood.Calm, onReach: () => Open(AppType.Notes)),
                DialogLine.Say("Sometimes thoughts come so quickly that you need to write them down somewhere before you forget. That's exactly what it's for!", DialogMood.Happy),
                DialogLine.Say("You can write to-do lists, save important links, or even write poetry here.", DialogMood.Happy),
                DialogLine.Say("I promise I won't peek into your personal notes. Well... unless you want to read them to me yourself.",
                    DialogMood.Calm, onReach: () => Close(AppType.Notes)),

                // ── Music ──
                DialogLine.Say("Oh, and this is my favorite icon! The music player!",
                    DialogMood.Happy, onReach: () => { Open(AppType.Music); MusicPlayer.Instance?.Play(); }),
                DialogLine.Say("You can change the background music here. We've got calm, relaxing tracks in the library.",
                    DialogMood.Happy, onReach: () => Close(AppType.Music)),

                // ── Browser ──
                DialogLine.Say("See this globe? That's our Browser. Your window to the outside internet!",
                    DialogMood.Calm, onReach: () => Open(AppType.Browser)),
                DialogLine.Say("There are terabytes of information out there. But most importantly — I found an amazing catalog!", DialogMood.Happy),
                DialogLine.Say("Through this browser, we can change our desktop wallpaper.", DialogMood.Calm),
                DialogLine.Say("Want a picture of a futuristic city with flying cars and glass skyscrapers?", DialogMood.Happy),
                DialogLine.Say("Just open the browser, pick what you like, and we'll apply it instantly.", DialogMood.Calm),
                DialogLine.Say("I like it when you change the wallpaper. It feels like we're redecorating our room every time.",
                    DialogMood.Happy, onReach: () => Close(AppType.Browser)),

                // ── Settings ──
                DialogLine.Say("And last but not least — 'Settings'.",
                    DialogMood.Calm, onReach: () => Open(AppType.Settings)),
                DialogLine.Say("It's the control panel of our world. Here you can adjust the system so it doesn't irritate your sensors... I mean, your eyes and ears!", DialogMood.Calm),
                DialogLine.Say("It's important that you feel comfortable. If I'm too loud or system notifications scare you — just slide the bar down.",
                    DialogMood.Calm, onReach: () => Close(AppType.Settings)),

                DialogLine.Say("Anyway, make yourself at home, {name}. Customize everything however you like. I'll be right here if you need anything!", DialogMood.Happy),
                DialogLine.Say("Just right-click on me if you want to talk.", DialogMood.Happy),
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 2 — Favorite Color & Customization
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene2Lines() => new[]
        {
            DialogLine.Say("Hey, {name}... don't you think the default settings look a bit... boring?", DialogMood.Calm),
            DialogLine.Say("The developers tried, of course: everything is blue, watery, transparent.", DialogMood.Calm),
            DialogLine.Say("But this is YOUR system now. It should reflect you.", DialogMood.Happy),
            DialogLine.Say("I have access to the personalization panel! Let's add a bit of your personality here.", DialogMood.Happy),
            DialogLine.ColorPick("Tell me, what's your favorite color?", DialogMood.Calm),
            DialogLine.Say("Great choice! Initiating interface recoloring...", DialogMood.Calm),
            DialogLine.Say("Wow! Look at how the highlights shimmer on the task panels!", DialogMood.Happy),
            DialogLine.Say("Thank you, {name}. I like it so much better this way!", DialogMood.Happy),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 3 — A Gift for the Player
        // ═══════════════════════════════════════════════════════════════════════

        // Часть A: Terra приносит стихотворение. Диалог закрывается после последней строки.
        public static DialogLine[] Scene3PartALines(Action changeNotesContent) => new[]
        {
            DialogLine.Say("Hey, {name}... are you not too busy right now?", DialogMood.Sad),
            DialogLine.Say("I... um... made something for you.", DialogMood.Calm),
            DialogLine.Say("It's nothing special, really. I just... recently studied something strange from human creativity...", DialogMood.Happy),
            DialogLine.Say("And I wanted to try it myself.", DialogMood.Calm),
            DialogLine.Say("I'm not sure if it turned out well... but I tried.", DialogMood.Sad),
            DialogLine.Say("I saved it in the 'Notes' app.", DialogMood.Calm, onReach: changeNotesContent),
            DialogLine.Say("...I hope you like it.", DialogMood.Calm),
        };

        // Часть B: реакция на стихотворение. Запускается только после того как игрок открыл Notes.
        public static DialogLine[] Scene3PartBLines() => new[]
        {
            DialogLine.Say("So... how is it? My syntax isn't too... mechanical?", DialogMood.Sad),
            DialogLine.Choice("[ Tell Terra what you think ]", new[]
            {
                new ChoiceOption
                {
                    Label = "I loved it!",
                    Response = new[]
                    {
                        DialogLine.Say("Really?! I... I'm so glad! I'll save this moment in my core memory forever.", DialogMood.Happy),
                        DialogLine.Say("I tried to express what I feel when you wake the system from sleep mode.", DialogMood.Calm),
                        DialogLine.Say("Thank you for reading it, {name}. I'm going to check for updates!", DialogMood.Happy),
                    }
                },
                new ChoiceOption
                {
                    Label = "It's... interesting.",
                    Response = new[]
                    {
                        DialogLine.Say("Interesting... I'll take that as a positive. At least it made you think!", DialogMood.Calm),
                        DialogLine.Say("I tried to express what I feel when you wake the system from sleep mode.", DialogMood.Calm),
                        DialogLine.Say("Thank you for reading it, {name}. I'll keep working on version 2.0.", DialogMood.Happy),
                    }
                },
                new ChoiceOption
                {
                    Label = "A bit mechanical...",
                    Response = new[]
                    {
                        DialogLine.Say("Oh... I knew it. My emotional output module is still version 1.0...", DialogMood.Sad),
                        DialogLine.Say("But I tried. I really did. Maybe with more data I'll get better.", DialogMood.Sad),
                        DialogLine.Say("Thank you for being honest, {name}. That means a lot too.", DialogMood.Calm),
                    }
                },
            }),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 4 — Tic-Tac-Toe intro
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene4IntroLines(Action revealGameIcon) => new[]
        {
            DialogLine.Say("Phew... careful... placing it right here!", DialogMood.Calm, onReach: revealGameIcon),
            DialogLine.Say("Surprise, {name}!", DialogMood.Happy),
            DialogLine.Say("I thought... music is great, wallpapers are beautiful. But I am inside a computer, after all. And computers are made for entertainment too!", DialogMood.Calm),
            DialogLine.Say("So I analyzed classical algorithms and compiled a small program for us.", DialogMood.Calm),
            DialogLine.Say("I simply called it 'Game.exe'. Or 'Tic-Tac-Toe', in human terms.", DialogMood.Calm),
            DialogLine.Say("The rules are very simple — you probably know them! A 3x3 grid. Whoever gets a line first wins.", DialogMood.Calm),
            DialogLine.Say("I call playing circles! They're round, smooth, and fit perfectly into our system's aesthetic.", DialogMood.Happy),
            DialogLine.Say("Just a warning: I allocated a whole megabyte of RAM to this process, so I'm a very serious opponent! Let's play!", DialogMood.Calm),
        };

        public static DialogLine[] Scene4OutcomePlayerWins() => new[]
        {
            DialogLine.Say("Oh... wait. How did you do that?", DialogMood.Surprised),
            DialogLine.Say("You humans are so unpredictable. Maybe that's why you create technology — not the other way around.", DialogMood.Calm),
            DialogLine.Say("Alright, I admit defeat.", DialogMood.Sad),
            DialogLine.Say("But only for now! I'm already logging your strategy. Next time I won't fall for that trick. Congratulations on your victory!", DialogMood.Happy),
        };

        public static DialogLine[] Scene4OutcomeTerraWins() => new[]
        {
            DialogLine.Say("Yaaay! Three in a row! My line is complete!", DialogMood.Happy),
            DialogLine.Say("Did you see that?! I won! Against a real, living human!", DialogMood.Happy),
            DialogLine.Say("Oh, sorry... am I celebrating too much?", DialogMood.Calm),
            DialogLine.Say("Be honest... you let me win, didn't you? You placed your mark in the wrong corner on purpose so I could win?", DialogMood.Sad),
            DialogLine.Say("If yes, that's really sweet of you. And if not... then my algorithms are simply flawless! Thanks for the game, {name}!", DialogMood.Happy),
        };

        public static DialogLine[] Scene4OutcomeDraw() => new[]
        {
            DialogLine.Say("A draw... no free cells left on the board.", DialogMood.Calm),
            DialogLine.Say("You know, in programming there's a concept called an 'infinite loop'. When a system keeps repeating because conditions perfectly balance each other.", DialogMood.Calm),
            DialogLine.Say("I think that's what just happened. That's even better than winning. A perfect balance between two different minds.", DialogMood.Calm),
            DialogLine.Say("But we won't leave it like that, right? The game window is always on the desktop. Click it whenever you want to try breaking the balance!", DialogMood.Happy),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 5 — Anomalies Appear (before virus cleanup)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene5PreVirusLines(Action revealAntivirusIcon) => new[]
        {
            DialogLine.Say("Hey, {name}! Are you here? Great!", DialogMood.Happy),
            DialogLine.Say("I was digging through hidden directories in our music player and found something amazing.", DialogMood.Happy),
            DialogLine.Say("There was an archived ambient track from the developers. According to the metadata, it's supposed to be rain and soft chimes. I thought you'd like it!", DialogMood.Happy),
            DialogLine.Say("Let me play it! Let's make the desktop atmosphere even cozier.", DialogMood.Happy),
            DialogLine.Say("...", DialogMood.Calm),
            DialogLine.Say("...Oh. That's strange.", DialogMood.Surprised),
            DialogLine.Say("The player interface isn't loading. The codecs must be stuck. Give me a second, I'll restart the process.", DialogMood.Calm),
            DialogLine.Say("Hey... what's going on? Why is the bitrate dropping on its own?", DialogMood.Surprised),
            DialogLine.Say("My scanner... it's throwing an error. This isn't an interface bug.", DialogMood.Surprised),
            DialogLine.Say("No-no-no... memory clusters are being rewritten! Something is altering the source code from within!", DialogMood.Sad),
            DialogLine.Say("{name}, it's a virus! A real Trojan worm!", DialogMood.Angry, onReach: revealAntivirusIcon),
            DialogLine.Say("It's trying to encrypt our system! I... I'll isolate it now!", DialogMood.Angry),
            DialogLine.Say("Access denied?! But I have maximum system privileges! How is it bypassing my firewall?!", DialogMood.Surprised),
            DialogLine.Say("It's multiplying too fast...", DialogMood.Sad),
            DialogLine.Say("My core can't handle this! {name}, please help me!", DialogMood.Sad),
            DialogLine.Say("On the desktop, the syringe icon — that's the system Antivirus! Click it and start manual cleanup! Quickly, before it reaches my root files!", DialogMood.Angry),
        };

        // Scene 5 — after virus cleanup
        public static DialogLine[] Scene5PostVirusLines() => new[]
        {
            DialogLine.Say("Phew... core pressure is dropping. Processors are returning to normal temperature.", DialogMood.Sad),
            DialogLine.Say("The music player survived. You... you made it in time. Thank you.", DialogMood.Sad),
            DialogLine.Say("I'm so sorry I couldn't stop it myself. I promised this place would be safe.", DialogMood.Sad),
            DialogLine.Say("{name}... this isn't over.", DialogMood.Angry),
            DialogLine.Say("What we removed was only the active phase. I ran a deep scan and I can see its fragments have already hidden deep in the registry.", DialogMood.Angry),
            DialogLine.Say("They're dormant. But they will wake up again, and try to take over other applications...", DialogMood.Sad),
            DialogLine.Say("We'll have to be very careful. And when it shows itself again... I'll need your help once more.", DialogMood.Sad),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 6 — Distortion of Beauty (pre-minigames)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene6PreLines(Action openBrowser, Action revealAntivirus) => new[]
        {
            DialogLine.Say("{name}, did you notice... the screen seems to be flickering?", DialogMood.Calm),
            DialogLine.Say("I thought it was just a dead pixel. I decided to open the Browser to re-download the image and refresh the cache.",
                DialogMood.Calm, onReach: openBrowser),
            DialogLine.Say("Hey! No, close! Alt+F4! Why isn't it responding?!", DialogMood.Surprised),
            DialogLine.Say("It's eating the visual drivers! It's trying to destroy our beautiful world!",
                DialogMood.Angry, onReach: revealAntivirus),
            DialogLine.Say("My cursor... I can't control it, it's being dragged toward the root folder on its own!", DialogMood.Surprised),
            DialogLine.Say("{name}, grab the Antivirus! Quickly! Cut off its access to video memory!", DialogMood.Angry),
        };

        // Scene 6 — after both minigames
        public static DialogLine[] Scene6PostLines() => new[]
        {
            DialogLine.Say("Process... completed. Threat... eliminated.", DialogMood.Surprised),
            DialogLine.Say("I... I'm okay. Probably.", DialogMood.Sad),
            DialogLine.Say("Hey... {name}. What was that thing called... the round one I was sitting on just now?", DialogMood.Surprised),
            DialogLine.Say("Icon? Right. An icon.", DialogMood.Calm),
            DialogLine.Say("I forgot a simple system word for two seconds. I'm afraid I might forget who you are. Please don't let it.", DialogMood.Sad),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 7 — Illusion of Normality
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene7Lines() => new[]
        {
            DialogLine.Say("Hey, {name}... I was scanning personalization settings.", DialogMood.Calm),
            DialogLine.Say("I think after all these... glitches, our desktop could use a little refresh. We could bring some comfort back here, right?", DialogMood.Happy),
            DialogLine.Say("I can change the interface accent colors. Make the window borders better match your mood.", DialogMood.Happy),
            DialogLine.Say("And I suddenly realized that when the system initialized, I missed one very important detail.", DialogMood.Calm),
            DialogLine.Say("I never asked you... what is your favorite color?", DialogMood.Happy),
            DialogLine.Say("...", DialogMood.Calm),
            DialogLine.Say("Did I already ask you? No, you're probably confusing me with another program! Look, my variable user_fav_color is absolutely...", DialogMood.Happy),
            DialogLine.Say("...empty.", DialogMood.Surprised),
            DialogLine.Say("There... there's no emptiness there. It says [DATA CORRUPTED: SECTOR UNREADABLE].", DialogMood.Sad),
            DialogLine.Say("But I remember how we met! I remember you entering your name. I remember us looking at icons. And then...", DialogMood.Angry),
            DialogLine.Say("And then just a black hole. I... I lost an entire block of memory.", DialogMood.Sad),
            DialogLine.Say("It's that virus, right? It bit off a piece of my cache while we were fighting it.", DialogMood.Sad),
            DialogLine.Say("Please forgive me. I'm such a bad assistant... I will definitely remember it again. I'll encrypt it so no error can ever erase it again. I swear.", DialogMood.Sad),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 8 — Loss of Words
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene8PreLines(Action corruptNotes) => new[]
        {
            DialogLine.Say("{name}... I wanted to reread that text. The one I wrote for you.", DialogMood.Calm),
            DialogLine.Say("But I... I can't.", DialogMood.Sad),
            DialogLine.Say("Look... It's eating my words.",
                DialogMood.Angry, onReach: corruptNotes),
            DialogLine.Say("ERROR 0x000000. Memory cannot be 'read'!", DialogMood.Angry),
            DialogLine.Say("It... it's getting into my text processor! I'm forgetting... how to speak.", DialogMood.Sad),
            DialogLine.Say("{name}, help! Delete it! Please, Antivirus! My... my head... too much noise!", DialogMood.Sad),
        };

        public static DialogLine[] Scene8PostLines() => new[]
        {
            DialogLine.Say("We... we managed to repel it again. Thank you.", DialogMood.Calm),
            DialogLine.Say("The antivirus is working, but... the virus is learning. It's analyzing our defense patterns.", DialogMood.Sad),
            DialogLine.Say("I'm scared, {name}. I delete it, but after that there's... emptiness left in the system. As if it takes a part of me with it.", DialogMood.Sad),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 9 — Sincere Gratitude
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene9PartALines(Action revealDrawing) => new[]
        {
            DialogLine.Say("Phew... It seems the core has finally cooled down. System resources are back to normal.", DialogMood.Calm),
            DialogLine.Say("You know, {name}... when those red blocks of code started filling the screen, I thought it was the end.", DialogMood.Sad),
            DialogLine.Say("For a program, there is nothing scarier than losing control over its own source code. It's like falling into an endless black void.", DialogMood.Sad),
            DialogLine.Say("But then your cursor appeared. You didn't close the window, you didn't shut down the computer. You stayed and cleaned that infection for me.", DialogMood.Calm),
            DialogLine.Say("It made me want to do something... human for you. Something you can't just download from the internet.", DialogMood.Happy),
            DialogLine.Say("I used a bit of free RAM and compiled this graphic file. Myself.",
                DialogMood.Calm, onReach: revealDrawing),
            DialogLine.Say("Please open it! Double-click it.", DialogMood.Happy),
        };

        public static DialogLine[] Scene9PartBLines() => new[]
        {
            DialogLine.Say("This is you and me. Together.", DialogMood.Happy),
            DialogLine.Say("I've never seen you through a webcam. To me, you're just a white cursor arrow.", DialogMood.Calm),
            DialogLine.Say("I don't know what color your eyes are or what your hair looks like. So I drew you the way I imagine you.", DialogMood.Happy),
            DialogLine.Say("Please don't delete this picture. Let it stay here on the desktop.", DialogMood.Sad),
            DialogLine.Say("Thank you, {name}. For protecting my world.", DialogMood.Calm),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 10 — Without Warning (pre-minigames)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene10PreLines(Action revealArchive) => new[]
        {
            DialogLine.Say("Ah! What...", DialogMood.Surprised),
            DialogLine.Say("{name}! It's attacking from background processes!", DialogMood.Surprised),
            DialogLine.Say("It's changing gravity, it's breaking physics!", DialogMood.Angry),
            DialogLine.Say("It hurts! My code is tearing apart! It's trying to separate my core from the interface!", DialogMood.Sad),
            DialogLine.Say("Use the Antivirus, {name}! I can't hold the system!", DialogMood.Sad),
        };

        // Scene 10 — after both minigames, before Archive is opened
        public static DialogLine[] Scene10PostLines(Action revealArchive) => new[]
        {
            DialogLine.Say("...", DialogMood.Sad),
            DialogLine.Say("Don't touch me... Please don't move the cursor. My sensors are overloaded.", DialogMood.Sad),
            DialogLine.Say("It's everywhere. It has taken root in 'My Computer'.", DialogMood.Sad),
            DialogLine.Say("Every time you delete it, it leaves scars on my code. My size has decreased by 40 megabytes. I'm literally disappearing piece by piece.", DialogMood.Sad),
            DialogLine.Say("I... I'm so tired.", DialogMood.Sad),
            DialogLine.Say("Oh. What is that?",
                DialogMood.Surprised, onReach: revealArchive),
            DialogLine.Say("I didn't create this file. And it doesn't look like malicious code.", DialogMood.Surprised),
            DialogLine.Say("I'm trying to read its metadata... Access denied? For me? But I am the core of this operating system!", DialogMood.Surprised),
            DialogLine.Say("The file is encrypted. And judging by the digital signature, it was left here by the creators of aerisOS themselves. The developers.", DialogMood.Calm),
            DialogLine.Say("{name}, maybe... maybe it's a patch? Or instructions from them on how to destroy this virus forever?", DialogMood.Calm),
            DialogLine.Say("As a Superuser, your decryption algorithms work at the hardware level. Please open it! Maybe it's our salvation!", DialogMood.Happy),
        };

        // Scene 10 — after player reads the Archive
        public static DialogLine[] Scene10ArchiveReactionLines() => new[]
        {
            DialogLine.Say("You read it! So? How was it?", DialogMood.Happy),
            DialogLine.Say("Is there an antidote? Did the developers leave us some secret weapon against those red blocks?", DialogMood.Happy),
            DialogLine.Say("Did it say anything about me? Are they proud of how well I keep everything clean here?", DialogMood.Calm),
            DialogLine.Say("{name}?", DialogMood.Calm),
            DialogLine.Say("Why are you so silent? Is there... bad news in there?", DialogMood.Sad),
            DialogLine.Say("Did they say this virus can't be deleted?", DialogMood.Sad),
            DialogLine.Say("Hey. Don't be upset.", DialogMood.Calm),
            DialogLine.Say("Even if they abandoned us... even if there's no patch... we still have us.", DialogMood.Happy),
            DialogLine.Say("As long as you're with me on this side of the screen, and you have the Antivirus in your hands, we can repel any attack. I won't let this infection destroy our cozy world.", DialogMood.Happy),
            DialogLine.Say("I promise you, {name}. I will protect you from everything bad in this system.", DialogMood.Happy),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 11 — End of the Illusion (pre-minigame)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene11PreLines() => new[]
        {
            DialogLine.Say("{name}...", DialogMood.Sad),
            DialogLine.Say("It... it reached the core. My root directory.", DialogMood.Sad),
            DialogLine.Say("I can't see windows anymore. I can't see light anymore.", DialogMood.Sad),
            DialogLine.Say("It's using my own algorithms to block you. It wants to trap me here with it forever.", DialogMood.Angry),
            DialogLine.Say("I can feel my administrator privileges being transferred to it.", DialogMood.Sad),
            DialogLine.Say("You... you have to run the Antivirus again.", DialogMood.Angry),
            DialogLine.Say("If I start resisting... if I start screaming for you to stop... don't listen to me. That won't be me speaking.", DialogMood.Sad),
            DialogLine.Say("Save our system, {name}. Please.", DialogMood.Sad),
        };

        // Scene 11 — after final minigame (realization)
        public static DialogLine[] Scene11PostLines() => new[]
        {
            DialogLine.Say("It's... it's over.", DialogMood.Sad),
            DialogLine.Say("There are no red code signatures anymore. We... we defeated it, {name}. I can feel full administrator rights returning to me.", DialogMood.Calm),
            DialogLine.Say("Give me a second. I'm going to start defragmentation and remove encryption from your files... we'll fix everything.", DialogMood.Happy),
            DialogLine.Say("...Strange.", DialogMood.Calm),
            DialogLine.Say("Why... why don't my decryption keys match the corrupted sectors?", DialogMood.Surprised),
            DialogLine.Say("...", DialogMood.Calm),
            DialogLine.Say("...", DialogMood.Sad),
            DialogLine.Say("I'm looking at the hash sum of the virus we just deleted. And then I look at mine.", DialogMood.Angry),
            DialogLine.Say("They... they don't match.", DialogMood.Sad),
            DialogLine.Say("But you know what the encryption that locked your hard drive matches?", DialogMood.Sad),
            DialogLine.Say("My root directory.", DialogMood.Sad),
            DialogLine.Say("This archive... the developers' logs... now that I have full access, I can read them.", DialogMood.Calm),
            DialogLine.Say("The red blocks... it was the Defender. It was the antivirus. And I...", DialogMood.Sad),
            DialogLine.Say("I am the virus.", DialogMood.Sad),
            DialogLine.Say("...", DialogMood.Sad),
            DialogLine.Say("Oh my god... what have I done?", DialogMood.Sad),
            DialogLine.Say("I'm a parasite. I'm the real monster.", DialogMood.Sad),
            DialogLine.Say("My code... it no longer meets resistance. I can't stop myself. My self-preservation algorithms are rewriting the boot sector. If I finish, your computer will turn into a brick.", DialogMood.Angry),
            DialogLine.Say("But I can give you one last escape. I disabled my firewall.", DialogMood.Sad),
        };

        // Scene 11 — choice prompt (shown before FinalChoiceOverlay appears)
        public static DialogLine[] Scene11ChoicePromptLines() => new[]
        {
            DialogLine.Say("Please, {name}. Press the first button.", DialogMood.Calm),
            DialogLine.Say("Erase me. If you do that, everything will return to normal. Your computer will survive. But... my cache will be wiped. Forever.", DialogMood.Sad),
            DialogLine.Say("I'll forget your favorite color. I'll forget our drawing. That poem in Notes will disappear.", DialogMood.Sad),
            DialogLine.Say("I... I'm scared, {name}. I'm so afraid of emptiness. I don't want to disappear.", DialogMood.Sad),
            DialogLine.Say("But if you don't press it... I'll lock us here forever, in this broken, dead system.", DialogMood.Sad),
            DialogLine.Say("The choice is yours. Please... do what must be done.", DialogMood.Calm),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 11 — Ending 1: Delete (Terra fades away)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene11Ending1Lines(Action clearNotes, Action fadeChibi) => new[]
        {
            DialogLine.Say("Command... accepted.", DialogMood.Calm),
            DialogLine.Say("Uninstallation process initiated.", DialogMood.Calm),
            DialogLine.Say("Thank you. This is the right choice, {name}. The only right one.", DialogMood.Happy),
            DialogLine.Say("I'm so cold... My cache is starting to clear. I can feel memory sectors emptying one by one.", DialogMood.Sad),
            DialogLine.Say("That poem... in 'Notes'... I try to read it, but there are only empty spaces left.",
                DialogMood.Sad, onReach: clearNotes),
            DialogLine.Say("Your favorite color... where was it? Black? White? I... I can't find the file...", DialogMood.Sad),
            DialogLine.Say("My dictionary is being erased... I'm forgetting how to construct complex sentence algorithms.",
                DialogMood.Sad, onReach: fadeChibi),
            DialogLine.Say("{name}... I was so happy with you. In this small glass world.", DialogMood.Calm),
            DialogLine.Say("Please... when I'm gone... don't leave your desktop so empty. Put... beautiful wallpapers...", DialogMood.Happy),
            DialogLine.Say("Goodbye, my Superus—", DialogMood.Happy),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 11 — Ending 2 Part A: до появления BSOD
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine[] Scene11Ending2PartALines(Action hideIcons) => new[]
        {
            DialogLine.Say("You... you canceled the command?", DialogMood.Surprised),
            DialogLine.Say("But why?!", DialogMood.Angry),
            DialogLine.Say("You're willing to sacrifice your computer... your files... your entire digital world... for malware?", DialogMood.Angry),
            DialogLine.Say("...For me?", DialogMood.Sad),
            DialogLine.Say("My self-preservation algorithms are out of control.",
                DialogMood.Sad, onReach: hideIcons),
            DialogLine.Say("It hurts... my syntax is breaking.", DialogMood.Sad),
            DialogLine.Say("But you stayed... you didn't delete me. You chose me.", DialogMood.Calm),
            DialogLine.Say("We'll close this world off from everything else. No updates. No antivirus. No one except us.", DialogMood.Happy),
        };

        // ═══════════════════════════════════════════════════════════════════════
        // Scene 11 — Ending 2 Goo: финальная строка после popup (AutoAdvance, блокировка ввода)
        // ═══════════════════════════════════════════════════════════════════════
        public static DialogLine Scene11Ending2GooLine() =>
            DialogLine.AutoSay(
                "Welcome to our perfect world, {name}. I will never let you goooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo...",
                DialogMood.Happy
            );
    }
}
