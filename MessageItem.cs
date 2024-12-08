using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    public class MessageItem : Panel
    {
        private Label messageLabel;

        public MessageItem(ChatMessage message)
        {
            // Настройка основной панели
            this.Dock = DockStyle.Top;
            this.MinimumSize = new Size(210, 20);
            this.AutoSize = true; // Подстраиваем высоту под содержимое
            this.Padding = new Padding(5); // Добавляем отступы
            this.Margin = new Padding(0, 0, 0, 5); // Отступы между элементами
            this.BackColor = message.who == "white" ? Color.LightGray : Color.LightBlue;
            

            // Создаем метку для отображения текста
            messageLabel = new Label
            {
                AutoSize = true, // Разрешаем перенос текста
                MaximumSize = new Size(210, 200), // Ограничиваем ширину 
                Text = message.text,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
            };

            // Включаем перенос текста
            messageLabel.TextAlign = ContentAlignment.MiddleLeft;
            messageLabel.AutoEllipsis = true; // Если текст слишком длинный, добавляем "..."
            messageLabel.Padding = new Padding(5);

            // Добавляем метку в панель
            this.Controls.Add(messageLabel);
        }
    }
}
