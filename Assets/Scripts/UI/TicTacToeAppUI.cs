using System;
using System.Collections;
using aerisOS.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace aerisOS.UI
{
    public enum TicTacToeOutcome { PlayerWins, TerraWins, Draw }

    public class TicTacToeAppUI : MonoBehaviour
    {
        public static event Action<TicTacToeOutcome> OnGameFinished;
        private readonly int[] _board = new int[9]; // 0=пусто, 1=X(игрок), 2=O(Terra)
        private bool _gameOver;
        private int _terraMovesTotal;
        private Text _statusText;
        private readonly Text[] _cellTexts = new Text[9];

        public void Build(RectTransform body)
        {
            // Заголовок
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(body, false);
            var titleRT = (RectTransform)titleGO.transform;
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot     = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0, -14);
            titleRT.sizeDelta = new Vector2(460, 34);
            var titleT = titleGO.GetComponent<Text>();
            titleT.text      = "Let's Play Tic-Tac-Toe!";
            titleT.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleT.fontSize  = 26;
            titleT.fontStyle = FontStyle.Bold;
            titleT.alignment = TextAnchor.MiddleCenter;
            titleT.color     = new Color(0.08f, 0.28f, 0.62f);
            titleT.raycastTarget = false;
            var ts = titleGO.AddComponent<Shadow>();
            ts.effectColor    = new Color(0.5f, 0.82f, 1f, 0.55f);
            ts.effectDistance = new Vector2(1f, -1f);

            // Подсказка роли
            var subGO = new GameObject("Roles", typeof(RectTransform), typeof(Text));
            subGO.transform.SetParent(body, false);
            var subRT = (RectTransform)subGO.transform;
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot     = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0, -53);
            subRT.sizeDelta = new Vector2(460, 20);
            var subT = subGO.GetComponent<Text>();
            subT.text      = "You are  X   •   Terra is  O";
            subT.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subT.fontSize  = 14;
            subT.alignment = TextAnchor.MiddleCenter;
            subT.color     = new Color(0.22f, 0.46f, 0.76f, 0.85f);
            subT.raycastTarget = false;

            // Статус хода
            var statusGO = new GameObject("Status", typeof(RectTransform), typeof(Text));
            statusGO.transform.SetParent(body, false);
            var statusRT = (RectTransform)statusGO.transform;
            statusRT.anchorMin = new Vector2(0.5f, 1f);
            statusRT.anchorMax = new Vector2(0.5f, 1f);
            statusRT.pivot     = new Vector2(0.5f, 1f);
            statusRT.anchoredPosition = new Vector2(0, -78);
            statusRT.sizeDelta = new Vector2(460, 22);
            _statusText = statusGO.GetComponent<Text>();
            _statusText.text      = "Your turn!";
            _statusText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize  = 16;
            _statusText.fontStyle = FontStyle.Bold;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color     = new Color(0.1f, 0.5f, 0.2f);
            _statusText.raycastTarget = false;

            // Игровая сетка 3×3
            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            grid.transform.SetParent(body, false);
            var gridRT = (RectTransform)grid.transform;
            gridRT.anchorMin = new Vector2(0.5f, 1f);
            gridRT.anchorMax = new Vector2(0.5f, 1f);
            gridRT.pivot     = new Vector2(0.5f, 1f);
            gridRT.anchoredPosition = new Vector2(0, -108);
            gridRT.sizeDelta = new Vector2(312, 312);
            var gl = grid.GetComponent<GridLayoutGroup>();
            gl.cellSize      = new Vector2(96, 96);
            gl.spacing       = new Vector2(12, 12);
            gl.constraint    = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 3;
            gl.childAlignment  = TextAnchor.UpperCenter;

            for (int i = 0; i < 9; i++)
            {
                int idx = i;
                var btn = aerisButton.Create(
                    grid.transform, "", new Vector2(96, 96),
                    () => OnPlayerClick(idx),
                    Color.white.WithAlpha(0.85f),
                    new Color(0.78f, 0.92f, 1f, 0.75f),
                    fontSize: 44);

                var labelT = btn.transform.Find("Label").GetComponent<Text>();
                _cellTexts[i] = labelT;
            }

            // Кнопка рестарта снизу
            var restart = aerisButton.Create(body, "Restart", new Vector2(180, 44),
                RestartGame, ColorPalette.AeroCyan, new Color(0f, 0.42f, 0.85f));
            var rRT = (RectTransform)restart.transform;
            rRT.anchorMin = new Vector2(0.5f, 0f);
            rRT.anchorMax = new Vector2(0.5f, 0f);
            rRT.pivot     = new Vector2(0.5f, 0f);
            rRT.anchoredPosition = new Vector2(0, 10);
        }

        private void OnPlayerClick(int idx)
        {
            if (_gameOver || _board[idx] != 0) return;

            PlaceMove(idx, 1);

            int winner = CheckWinner();
            if (winner != 0 || IsBoardFull()) { EndGame(winner); return; }

            _statusText.text  = "Terra is thinking...";
            _statusText.color = new Color(0.3f, 0.35f, 0.55f);
            StartCoroutine(TerraThink());
        }

        private IEnumerator TerraThink()
        {
            yield return new WaitForSeconds(0.45f);
            if (this == null) yield break;

            _terraMovesTotal++;
            int move = (_terraMovesTotal % 3 == 0) ? GetRandomMove() : GetBestMove();
            PlaceMove(move, 2);

            int winner = CheckWinner();
            if (winner != 0 || IsBoardFull()) { EndGame(winner); yield break; }

            _statusText.text  = "Your turn!";
            _statusText.color = new Color(0.1f, 0.5f, 0.2f);
        }

        private void PlaceMove(int idx, int player)
        {
            _board[idx] = player;
            _cellTexts[idx].text  = player == 1 ? "X" : "O";
            _cellTexts[idx].color = player == 1
                ? new Color(0.12f, 0.38f, 0.9f)   // X — синий
                : new Color(0.9f,  0.28f, 0.48f);  // O — розовый
        }

        private void EndGame(int winner)
        {
            _gameOver = true;
            TicTacToeOutcome outcome;
            switch (winner)
            {
                case 1:
                    _statusText.text  = "You win!";
                    _statusText.color = new Color(0.1f, 0.5f, 0.2f);
                    outcome = TicTacToeOutcome.PlayerWins;
                    break;
                case 2:
                    _statusText.text  = "Terra wins! Try again?";
                    _statusText.color = new Color(0.7f, 0.18f, 0.1f);
                    outcome = TicTacToeOutcome.TerraWins;
                    break;
                default:
                    _statusText.text  = "It's a draw!";
                    _statusText.color = new Color(0.3f, 0.35f, 0.5f);
                    outcome = TicTacToeOutcome.Draw;
                    break;
            }
            OnGameFinished?.Invoke(outcome);
        }

        private void RestartGame()
        {
            StopAllCoroutines();
            Array.Clear(_board, 0, 9);
            _gameOver        = false;
            _terraMovesTotal = 0;
            for (int i = 0; i < 9; i++) _cellTexts[i].text = "";
            _statusText.text  = "Your turn!";
            _statusText.color = new Color(0.1f, 0.5f, 0.2f);
        }

        private int CheckWinner()
        {
            int[][] lines =
            {
                new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
                new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
                new[]{0,4,8}, new[]{2,4,6}
            };
            foreach (var line in lines)
            {
                int a = _board[line[0]], b = _board[line[1]], c = _board[line[2]];
                if (a != 0 && a == b && b == c) return a;
            }
            return 0;
        }

        private bool IsBoardFull()
        {
            foreach (int v in _board) if (v == 0) return false;
            return true;
        }

        private int GetRandomMove()
        {
            var free = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 9; i++) if (_board[i] == 0) free.Add(i);
            return free[UnityEngine.Random.Range(0, free.Count)];
        }

        private int GetBestMove()
        {
            int bestScore = int.MinValue, best = -1;
            for (int i = 0; i < 9; i++)
            {
                if (_board[i] != 0) continue;
                _board[i] = 2;
                int score = Minimax(false, 0);
                _board[i] = 0;
                if (score > bestScore) { bestScore = score; best = i; }
            }
            return best;
        }

        // Terra — максимизатор (2), игрок — минимизатор (1)
        private int Minimax(bool maximizing, int depth)
        {
            int w = CheckWinner();
            if (w == 2) return 10 - depth;
            if (w == 1) return depth - 10;
            if (IsBoardFull()) return 0;

            if (maximizing)
            {
                int best = int.MinValue;
                for (int i = 0; i < 9; i++)
                {
                    if (_board[i] != 0) continue;
                    _board[i] = 2;
                    best = Math.Max(best, Minimax(false, depth + 1));
                    _board[i] = 0;
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                for (int i = 0; i < 9; i++)
                {
                    if (_board[i] != 0) continue;
                    _board[i] = 1;
                    best = Math.Min(best, Minimax(true, depth + 1));
                    _board[i] = 0;
                }
                return best;
            }
        }
    }
}
