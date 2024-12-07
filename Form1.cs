using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Http.Headers;

namespace checkers
{
    public partial class Form1 : Form
    {
        private bool white = true;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer()
        {
            Interval = 1
        };
        DateTime lastpart;

        public Form1(Lobby lobby)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer()
            {
                Interval = 16
            };
            timer.Tick += (s, e) =>
            {
                label4.Text = (lastpart - DateTime.Now).TotalSeconds.ToString("3F");
            };
            timer.Start();
            if (lobby.Connected != 0) 
            {
                white = false;
                gameBoard1.CanPlay = false;
            }
            else
            {
                white = true;
                gameBoard1.CanPlay = true;
            }
            Text = lobby.LobbyName;
            DoubleBuffered = true;
            InitializeComponent();
            Upd();
            lastpart = DateTime.Now+new TimeSpan(0,0,60);
            gameBoard1.MoveMade += GameBoard1_MoveMade;
            
            System.Windows.Forms.Timer gameBoard1_Timer = new System.Windows.Forms.Timer();
            gameBoard1_Timer.Interval = 16;
            gameBoard1_Timer.Tick += GameBoard1_Timer_Tick;
            gameBoard1_Timer.Start();
        }

        public void Client_OnMessageReceived(string message)
        {
            //MessageBox.Show($"GAME\n{message}");
            try
            {
                if (message == "swap")
                {
                    gameBoard1.CanPlay = !gameBoard1.CanPlay;
                    lastpart = DateTime.Now + new TimeSpan(0, 0, 60);
                    if (gameBoard1.CanPlay)
                    {
                        label5.Text = "��� ���";
                    }
                    else
                    {
                        label5.Text = "����� ���";
                    }
                    return;
                }
                var text = message.Substring(7);
                var action = JsonSerializer.Deserialize<Action>(text);
                if (action != null)
                {
                    Point from = new Point(int.Parse(action.from.Split(':')[0]), int.Parse(action.from.Split(':')[1]));
                    Point to = new Point(int.Parse(action.to.Split(':')[0]), int.Parse(action.to.Split(':')[1]));
                    bool isBlack = action.action == "black";
                    Point capture = Point.Empty;
                    if (action.iscapture)
                    {
                        capture = new Point(int.Parse(action.capture.Split(':')[0]), int.Parse(action.capture.Split(':')[1]));
                    }
                    gameBoard1.MoveTo(from.X, from.Y, to.X, to.Y, isBlack, capture.X, capture.Y, action.iscapture);
                }
                else
                {
                    MessageBox.Show("������ �������� ���������.");
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"������ JSON: {ex.Message}");
            }
        }

        private async void GameBoard1_Timer_Tick(object? sender, EventArgs e)
        {
            label2.Text = $"�����: {gameBoard1.WhiteCount}";
            label3.Text = $"׸����: {gameBoard1.BlackCount}";
        }
        
        private async void GameBoard1_MoveMade(object? sender, MoveEventArgs e)
        {
            string comand = e.MoverIsBlack ? "black" : "white";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var action = JsonSerializer.Serialize(new
            {
                Action = comand,
                From = $"{e.From.X}:{e.From.Y}",
                To = $"{e.To.X}:{e.To.Y}",
                e.IsCapture,
                capture = $"{e.Capture.X}:{e.Capture.Y}"
            }, options);
            string message = $"action:{action}";
            await Program.client.SendMessageAsync(message);
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
            Program.game = null;
        }
    }
}
