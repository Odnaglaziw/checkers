using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace checkers
{
    public class LobbyItem : Panel
    {
        private ClientWebSocket client;
        public LobbyItem(Lobby lobby, ClientWebSocket socket)
        {
            client = socket;
            // Настройки панели
            Size = new Size(300, 50);
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(5);
            BackColor = Color.LightGray;

            // Добавляем текстовое поле для отображения имени лобби
            Label lobbyNameLabel = new Label
            {
                Text = lobby.LobbyName,
                AutoSize = true,
                Location = new Point(10, 15),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            Controls.Add(lobbyNameLabel);
            Label ConnectedCount = new Label
            {
                Text = lobby.Connected.ToString(),
                AutoSize = true,
                Location = new Point(100, 15),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            Controls.Add(ConnectedCount);

            // Добавляем кнопку "Присоединиться"
            Button joinButton = new Button
            {
                Text = "Присоединиться",
                Size = new Size(100, 30),
                Location = new Point(180, 10)
            };
            joinButton.Click += (sender, e) => {
                string message = $"join:{lobby.LobbyId}";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                Program.game = new Form1(lobby);
                Program.game.ShowDialog();
            };
            Controls.Add(joinButton);
        }
    }

}
