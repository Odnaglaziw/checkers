﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class GameBoard : Panel
    {
        private int gridSize = 8; 
        private Checker[,] checkers;
        private Checker selectedChecker = null!;
        private List<Point> validMoves = new List<Point>();
        private List<Point> capturedCheckers = new List<Point>();
        public event EventHandler<MoveEventArgs> MoveMade;
        public GameBoard()
        {
            DoubleBuffered = true;
            checkers = new Checker[gridSize, gridSize];
            InitializeCheckers();
            this.Paint += GameBoard_Paint!;
            this.Resize += GameBoard_Resize;
        }
        private void GameBoard_Resize(object? sender, EventArgs e)
        {
            Invalidate();
        }
        public int GridSize
        {
            get => gridSize;
            set
            {
                if (value < 2) throw new ArgumentException("Размер сетки должен быть больше 1.");
                gridSize = value;
                Invalidate(); // Перерисовать поле при изменении размера
            }
        }
        public bool White;
        public Color LightCellColor { get; set; } = Color.White; 
        public Color DarkCellColor { get; set; } = Color.Black;
        private void InitializeCheckers()
        {
            // Расставляем черные шашки (сверху)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if ((row + col) % 2 != 0)
                    {
                        checkers[row, col] = new Checker(col, row, isBlack: true);
                    }
                }
            }

            // Расставляем белые шашки (снизу)
            for (int row = gridSize - 3; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if ((row + col) % 2 != 0)
                    {
                        checkers[row, col] = new Checker(col, row, isBlack: false);
                    }
                }
            }
        }
        private void GameBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int cellSize = Math.Min(Width, Height) / gridSize;

            // Рисуем клетки
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Brush brush = (row + col) % 2 == 0 ? new SolidBrush(LightCellColor) : new SolidBrush(DarkCellColor);
                    g.FillRectangle(brush, col * cellSize, row * cellSize, cellSize, cellSize);
                }
            }

            // Рисуем шашки
            foreach (var checker in checkers)
            {
                checker?.Draw(g, cellSize);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            int cellSize = Math.Min(Width, Height) / gridSize;
            int clickedX = e.X / cellSize;
            int clickedY = e.Y / cellSize;

            // Поиск шашки в нажатой клетке
            selectedChecker = null!;
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (checkers[row, col] != null &&
                        checkers[row, col].X == clickedX &&
                        checkers[row, col].Y == clickedY &&
                        checkers[row, col].IsBlack != White) // Проверяем команду
                    {
                        selectedChecker = checkers[row, col];
                        break;
                    }
                }
                if (selectedChecker != null) break;
            }

            if (selectedChecker != null)
            {
                validMoves = GetValidMoves(selectedChecker);
                Invalidate(); // Перерисовываем поле для отображения подсветки
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (selectedChecker == null) return;

            int cellSize = Math.Min(Width, Height) / gridSize;
            int clickedX = e.X / cellSize;
            int clickedY = e.Y / cellSize;
            int capturedX=0;
            int capturedY=0;
            if (validMoves.Contains(new Point(clickedX, clickedY)))
            {
                int oldX = selectedChecker.X;
                int oldY = selectedChecker.Y;

                // Обновляем позицию шашки
                selectedChecker.MoveTo(clickedX, clickedY);
                checkers[oldY, oldX] = null!;
                checkers[clickedY, clickedX] = selectedChecker;

                // Проверяем, была ли это рубка
                bool isCapture = false;
                if (Math.Abs(clickedX - oldX) == 2 && Math.Abs(clickedY - oldY) == 2)
                {
                    capturedX = (clickedX + oldX) / 2;
                    capturedY = (clickedY + oldY) / 2;

                    // Удаляем шашку противника
                    checkers[capturedY, capturedX] = null!;
                    isCapture = true;
                }

                // Генерация события
                OnMoveMade(new MoveEventArgs
                {
                    MoverIsBlack = selectedChecker.IsBlack,
                    From = new Point(oldX, oldY),
                    To = new Point(clickedX, clickedY),
                    IsCapture = isCapture,
                    Capture = new Point(capturedX,capturedY)
                });

                // Сбрасываем выбор
                selectedChecker = null!;
            }
            validMoves.Clear();
            capturedCheckers.Clear();
            Invalidate(); // Перерисовываем поле
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            int cellSize = Math.Min(Width, Height) / gridSize;

            // Подсветка доступных клеток
            foreach (var move in validMoves)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Yellow)), move.X * cellSize, move.Y * cellSize, cellSize, cellSize);
            }
            foreach (var captured in capturedCheckers)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Purple)), captured.X * cellSize, captured.Y * cellSize, cellSize, cellSize);
            }

            // Рисуем шашки
            foreach (var checker in checkers)
            {
                checker?.Draw(g, cellSize);
            }
        }
        private List<Point> GetValidMoves(Checker checker, bool onlyCapture = false)
        {
            var moves = new List<Point>();
            var visited = new HashSet<Point>(); // Храним посещенные клетки

            // Вложенный метод для проверки ходов с учетом посещенных клеток
            void CheckMoves(Checker currentChecker, bool onlyCapture)
            {
                foreach (var move in Checker.moves)
                {
                    int newX = currentChecker.X + move.Item1;
                    int newY = currentChecker.Y + move.Item2;

                    if (newX >= 0 && newY >= 0 && newX < gridSize && newY < gridSize)
                    {
                        var currentPos = new Point(newX, newY);

                        if (checkers[newY, newX] == null && !visited.Contains(currentPos) && !onlyCapture)
                        {
                            moves.Add(currentPos); // Добавляем обычный ход
                            visited.Add(currentPos);
                        }
                        else if (checkers[newY, newX] != null &&
                                 checkers[newY, newX].IsBlack != currentChecker.IsBlack)
                        {
                            int jumpX = newX + move.Item1;
                            int jumpY = newY + move.Item2;

                            if (jumpX >= 0 && jumpY >= 0 && jumpX < gridSize && jumpY < gridSize &&
                                checkers[jumpY, jumpX] == null)
                            {
                                var jumpPos = new Point(jumpX, jumpY);

                                if (!visited.Contains(jumpPos))
                                {
                                    capturedCheckers.Add(currentPos); // Добавляем съеденную шашку
                                    moves.Add(jumpPos);
                                    visited.Add(jumpPos);

                                    // Рекурсивно проверяем дальнейшие ходы, если это рубка
                                    var tempChecker = new Checker(jumpX, jumpY, currentChecker.IsBlack);
                                    CheckMoves(tempChecker, onlyCapture: true); // Передаем параметр onlyCapture
                                }
                            }
                        }
                    }
                }
            }

            // Начинаем проверку с текущей шашки
            CheckMoves(checker, onlyCapture);

            return moves;
        }
        protected virtual void OnMoveMade(MoveEventArgs e)
        {
            MoveMade?.Invoke(this, e);
        }
        public void MoveTo(int oldX, int oldY, int newX, int newY, bool isBlack, int capturedX, int capturedY, bool isCapture)
        {
            if (checkers[oldX, oldY].IsBlack != isBlack) return;
            if (isCapture) checkers[capturedX, capturedY] = null!;
            checkers[oldX, oldY].MoveTo(newX, newY);
            checkers[oldY, oldX] = null!;
            Invalidate();
        }
    }
}
