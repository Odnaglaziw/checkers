using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System;

namespace checkers
{
    public partial class Form1 : Form
    {
        private ClientWebSocket client; // ���������� WebSocket
        private bool white = true;

        public Form1()
        {
            DoubleBuffered = true;
            InitializeComponent();
            InitializeWebSocket(); // ������������� WebSocket
            Upd();
            gameBoard1.MoveMade += GameBoard1_MoveMade;
            System.Windows.Forms.Timer gameBoard1_Timer = new System.Windows.Forms.Timer();
            gameBoard1_Timer.Interval = 16;
            gameBoard1_Timer.Tick += GameBoard1_Timer_Tick;
            gameBoard1_Timer.Start();
        }

        private async void GameBoard1_Timer_Tick(object? sender, EventArgs e)
        {
            // ��������� ������ �� �������
            byte[] buffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
            try
            {
                // �������������� JSON-������ � ������ ������ Action
                var action = JsonSerializer.Deserialize<Action>(response);

                if (action != null)
                {
                    //MessageBox.Show($"��������: {action.action}\n" +
                    //                $"������: {action.from}\n" +
                    //                $"����: {action.to}\n" +
                    //                $"��� ������: {action.iscapture}");
                    Point from = new Point(int.Parse(action.from.Split(':')[0]), int.Parse(action.from.Split(':')[1]));
                    Point to = new Point(int.Parse(action.to.Split(':')[0]), int.Parse(action.to.Split(':')[1]));
                    bool isBlack = action.action == "black";
                    Point capture = Point.Empty;
                    if (action.iscapture)
                    {
                        capture = new Point(int.Parse(action.capture.Split(':')[0]), int.Parse(action.capture.Split(':')[1]));
                    }
                    gameBoard1.MoveTo(from.X, from.Y, to.X, to.Y, isBlack, capture.X, capture.Y, action.iscapture);
                    byte[] buffer2 = Encoding.UTF8.GetBytes("������� ��������, ����������");
                    await client.SendAsync(new ArraySegment<byte>(buffer2), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    MessageBox.Show("�� ������� ���������� ����� �������.");
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"������ �������������� JSON: {ex.Message}");
            }
        }

        private async void InitializeWebSocket()
        {
            client = new ClientWebSocket();
            Uri serverUri = new Uri("ws://192.168.1.199:65000/");
            await client.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("���������� � �������� �����������!");
        }

        private async void GameBoard1_MoveMade(object? sender, MoveEventArgs e)
        {
            if (client.State != WebSocketState.Open)
            {
                MessageBox.Show("���������� � �������� ���������. ������������� ������.");
                return;
            }
            string comand = e.MoverIsBlack ? "black" : "white";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var message = JsonSerializer.Serialize(new
            {
                Action = comand,
                From = $"{e.From.X}:{e.From.Y}",
                To = $"{e.To.X}:{e.To.Y}",
                IsCapture = e.IsCapture,
                capture = $"{e.Capture.X}:{e.Capture.Y}"
            }, options);
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"��������� ����������: {message}");
        }

        private void Upd()
        {
            if (white)
            {
                pictureBox1.BackColor = Color.White;
            }
            else
            {
                pictureBox1.BackColor = Color.Black;
            }
            gameBoard1.White = white;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            white = !white;
            Upd();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // �������� WebSocket ��� ���������� ������ ����������
            if (client != null && client.State == WebSocketState.Open)
            {
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "���������� �������", CancellationToken.None).Wait();
                client.Dispose();
            }
        }
    }
}
