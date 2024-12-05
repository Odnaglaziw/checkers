using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    public class Checker
    {
        private Point pos;
        public static readonly (int, int)[] moves = { (1, 1), (-1, 1), (-1, -1), (1, -1) }; // Возможные ходы
        private bool isKing; // Дамка
        private readonly bool isBlack; // Цвет шашки

        public Checker(int x, int y, bool isBlack)
        {
            pos = new Point(x, y);
            this.isBlack = isBlack;
            isKing = false; // По умолчанию не дамка
        }

        public int X => pos.X;
        public int Y => pos.Y;

        public bool IsBlack => isBlack;
        public bool IsKing
        {
            get => isKing;
            set => isKing = value;
        }

        public void Draw(Graphics g, int cellSize)
        {
            // Вычисляем центр клетки для рисования
            int centerX = X * cellSize + cellSize / 2;
            int centerY = Y * cellSize + cellSize / 2;
            int diameter = cellSize - 10; // Размер шашки (чуть меньше клетки)
            int innerDiameter = diameter - 10; // Внутренний круг

            Brush checkerBrush = isBlack ? Brushes.Black : Brushes.White;
            Brush innerBrush = isBlack ? Brushes.Gray : Brushes.LightGray;

            // Рисуем основной круг (шашку)
            g.FillEllipse(checkerBrush, centerX - diameter / 2, centerY - diameter / 2, diameter, diameter);
            g.DrawEllipse(Pens.Gray, centerX - diameter / 2, centerY - diameter / 2, diameter, diameter);

            // Рисуем внутренний круг для эффекта объёма
            g.FillEllipse(innerBrush, centerX - innerDiameter / 2, centerY - innerDiameter / 2, innerDiameter, innerDiameter);

            // Если дамка, рисуем дополнительный символ (например, корону)
            if (isKing)
            {
                Font font = new Font("Arial", 14, FontStyle.Bold);
                StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("K", font, Brushes.Gold, centerX, centerY, format);
            }
        }
        public void MoveTo(int x, int y)
        {
            pos = new Point(x, y);
        }
    }

}
