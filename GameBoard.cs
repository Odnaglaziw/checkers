using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    using System;
    using System.Drawing;
    using System.Net.Http.Headers;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

    public class GameBoard : Panel
    {
        private readonly List<(int x,int y)> whites = new List<(int x, int y)>()
        {
            (0,5),(2,5),(4,5),(6,5),
            (1,6),(3,6),(5,6),(7,6),
            (0,7),(2,7),(4,7),(6,7)
        };
        private readonly List<(int x, int y)> blacks = new List<(int x, int y)>()
        {
            (1,0),(3,0),(5,0),(7,0),
            (0,1),(2,1),(4,1),(6,1),
            (1,2),(3,2),(5,2),(7,2)
        };
        private int gridSize = 8; 
        private Checker[,] checkers;
        private Checker selectedChecker = null!;
        private List<Point> validMoves = new List<Point>();
        private List<Point> capturedCheckers = new List<Point>();
        private List<Point> secondMoves = new List<Point>();
        public event EventHandler<MoveEventArgs> MoveMade;
        public int WhiteCount
        {
            get
            {
                List<Checker> xexe = new List<Checker>();
                foreach (Checker checker in checkers) { 
                    if (checker != null)
                    xexe.Add(checker);
                }
                return xexe.Count(c => !c.IsBlack);
            }
        }
        public int BlackCount
        {
            get
            {
                List<Checker> xexe = new List<Checker>();
                foreach (Checker checker in checkers)
                {
                    if (checker != null)
                    xexe.Add(checker);
                }
                return xexe.Count(c => c.IsBlack);
            }
        }
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
        public bool CanPlay;
        public Color LightCellColor { get; set; } = Color.White; 
        public Color DarkCellColor { get; set; } = Color.Black;
        private void InitializeCheckers()
        {
            // Расставляем черные шашки (сверху)
            foreach (var ints in blacks)
            {
                checkers[ints.x, ints.y] = new Checker(ints.x, ints.y, true)
                {
                    IsKing = false,
                };
            }
            //checkers[1, 2] = new Checker(1, 2, isBlack: true);
            //checkers[2, 5] = new Checker(2, 5, isBlack: false);
            // Расставляем белые шашки (снизу)
            foreach (var ints in whites)
            {
                checkers[ints.x, ints.y] = new Checker(ints.x, ints.y, false)
                {
                    IsKing = false,
                };
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
            if (!CanPlay) return;
            int cellSize = Math.Min(Width, Height) / gridSize;
            int clickedX = e.X / cellSize;
            int clickedY = e.Y / cellSize;

            // Поиск шашки в нажатой клетке
            //selectedChecker = null!;

            if (selectedChecker != null)
            {
                validMoves = GetValidMoves(selectedChecker);
                Invalidate(); // Перерисовываем поле для отображения подсветки
            }
            else
            {
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
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!CanPlay) return;
            bool isCapture = false;
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
                checkers[oldX, oldY] = null!;
                checkers[clickedX, clickedY] = selectedChecker;

                // Проверяем, была ли это рубка

                if (Math.Abs(clickedX - oldX) > 1 && Math.Abs(clickedY - oldY) > 1)
                {
                    // Вычисляем направление движения
                    int directionX = clickedX > oldX ? 1 : -1;
                    int directionY = clickedY > oldY ? 1 : -1;

                    // Пройдем по клеткам между старой и новой позицией
                    int x = clickedX - directionX;
                    int y = clickedY - directionY;
                    // Проверяем, есть ли шашка противника на текущей клетке
                    if (checkers[x, y] != null && checkers[x, y].IsBlack != selectedChecker.IsBlack)
                    {
                        // Удаляем шашку противника
                        checkers[x, y] = null!;
                        isCapture = true;
                    }

                    // Переходим к следующей клетке
                    capturedX = x;
                    capturedY = y;
                }

                // Генерация события
                OnMoveMade(new MoveEventArgs
                {
                    MoverIsBlack = selectedChecker.IsBlack,
                    From = new Point(oldX, oldY),
                    To = new Point(clickedX, clickedY),
                    IsCapture = isCapture,
                    Capture = new Point(capturedX,capturedY),
                    HasCaptureMoves = HasCaptureMoves(selectedChecker)
                });

                if ((selectedChecker.Y == 0 && !selectedChecker.IsBlack) ||
                    (selectedChecker.Y == gridSize - 1 && selectedChecker.IsBlack))
                {
                    selectedChecker.IsKing = true;
                }

            }
            // Сбрасываем выбор
            if (!isCapture) selectedChecker = null!;
            validMoves.Clear();
            capturedCheckers.Clear();
            secondMoves.Clear();
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
            foreach (var secondmove in secondMoves)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Gold)), secondmove.X * cellSize, secondmove.Y * cellSize, cellSize, cellSize);
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
            var visited = new HashSet<Point>();
            bool captureFind = false;  // Флаг для отслеживания захвата

            // Метод для проверки ходов дамки
            void CheckKingMoves(Checker currentChecker, (int, int) direction)
            {
                int stepX = currentChecker.X + direction.Item1;
                int stepY = currentChecker.Y + direction.Item2;
                Point? capturedCheckerPos = null;

                while (stepX >= 0 && stepY >= 0 && stepX < gridSize && stepY < gridSize)
                {
                    var currentPos = new Point(stepX, stepY);

                    if (checkers[stepX, stepY] == null)
                    {
                        if (capturedCheckerPos != null && !visited.Contains(currentPos))
                        {
                            // Добавляем возможный ход после рубки
                            moves.Add(currentPos);
                            visited.Add(currentPos);

                            // Сохраняем съеденную шашку
                            capturedCheckers.Add(capturedCheckerPos.Value);
                            captureFind = true;  // Устанавливаем флаг захвата

                            // Прекращаем дальнейшую проверку для дамки, как только захватили
                            break;
                        }
                        else if (capturedCheckerPos == null && !onlyCapture && !captureFind)
                        {
                            // Обычный ход, если рубки нет
                            moves.Add(currentPos);
                        }
                    }
                    else if (checkers[stepX, stepY].IsBlack != currentChecker.IsBlack && capturedCheckerPos == null)
                    {
                        // Шашка противника найдена, проверяем клетку за ней
                        int jumpX = stepX + direction.Item1;
                        int jumpY = stepY + direction.Item2;

                        if (jumpX >= 0 && jumpY >= 0 && jumpX < gridSize && jumpY < gridSize &&
                            checkers[jumpX, jumpY] == null)
                        {
                            capturedCheckerPos = new Point(stepX, stepY); // Фиксируем позицию шашки для рубки
                        }
                        else
                        {
                            break; // Невозможно перепрыгнуть
                        }
                    }
                    else
                    {
                        break; // Препятствие
                    }

                    stepX += direction.Item1;
                    stepY += direction.Item2;
                }
            }

            // Общий метод для проверки ходов
            void CheckMoves(Checker currentChecker, bool onlyCapture)
            {
                foreach (var move in currentChecker.moves)
                {
                    int newX = currentChecker.X + move.Item1;
                    int newY = currentChecker.Y + move.Item2;

                    if (newX >= 0 && newY >= 0 && newX < gridSize && newY < gridSize)
                    {
                        var currentPos = new Point(newX, newY);

                        if (checkers[newX, newY] == null && !visited.Contains(currentPos) && !onlyCapture)
                        {
                            moves.Add(currentPos);
                            visited.Add(currentPos);
                        }
                        else if (checkers[newX, newY] != null &&
                                 checkers[newX, newY].IsBlack != currentChecker.IsBlack)
                        {
                            int jumpX = newX + move.Item1;
                            int jumpY = newY + move.Item2;

                            if (jumpX >= 0 && jumpY >= 0 && jumpX < gridSize && jumpY < gridSize &&
                                checkers[jumpX, jumpY] == null)
                            {
                                var jumpPos = new Point(jumpX, jumpY);

                                if (!visited.Contains(jumpPos))
                                {
                                    moves.Add(jumpPos);
                                    visited.Add(jumpPos);

                                    // Сохраняем съеденную шашку
                                    capturedCheckers.Add(currentPos);

                                    // Рекурсивно проверяем дальнейшие ходы
                                    var tempChecker = new Checker(jumpX, jumpY, currentChecker.IsBlack);
                                    CheckMoves(tempChecker, true);
                                }
                            }
                        }
                    }
                }

                // Проверяем дополнительные ходы дамки
                if (currentChecker.IsKing && !captureFind)  // Если захват еще не произошел
                {
                    var directions = new (int, int)[] { (1, 1), (-1, 1), (-1, -1), (1, -1) };
                    foreach (var direction in directions)
                    {
                        CheckKingMoves(currentChecker, direction);
                    }
                }
            }

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
            selectedChecker = checkers[oldX, oldY];
            selectedChecker.MoveTo(newX, newY);
            checkers[oldX, oldY] = null!;
            checkers[newX, newY] = selectedChecker;
            selectedChecker = null!;
            Invalidate();
        }
        private bool HasCaptureMoves(Checker checker)
        {
            // Перебираем все возможные направления для текущей шашки
            foreach (var move in checker.moves)
            {
                int newX = checker.X + move.Item1;
                int newY = checker.Y + move.Item2;

                // Проверяем, что клетка находится в пределах игрового поля
                if (newX >= 0 && newY >= 0 && newX < gridSize && newY < gridSize)
                {
                    // Если на соседней клетке есть шашка противника
                    if (checkers[newX, newY] != null && checkers[newX, newY].IsBlack != checker.IsBlack)
                    {
                        int jumpX = newX + move.Item1;
                        int jumpY = newY + move.Item2;

                        // Проверяем, что клетка за шашкой противника свободна и находится в пределах игрового поля
                        if (jumpX >= 0 && jumpY >= 0 && jumpX < gridSize && jumpY < gridSize &&
                            checkers[jumpX, jumpY] == null)
                        {
                            return true; // Найдена возможность для рубки
                        }
                    }
                }
            }

            return false; // Возможностей для рубки нет
        }
    }
}
