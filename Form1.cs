using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System;
using System.Drawing;
using System.Windows.Forms;

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
            InitializeComponent();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer()
            {
                Interval = 16
            };
            timer.Tick += (s, e) =>
            {
                UpdateLabelSafe(label4, (lastpart - DateTime.Now).TotalSeconds.ToString());
            };

            if (lobby.Connected != 0)
            {
                white = false;
                gameBoard1.CanPlay = false;
            }
            else
            {
                white = true;
                gameBoard1.CanPlay = false;
                ChatMessage Chatmessage = new ChatMessage
                {
                    who = white ? "white" : "black",
                    text = "Ожидаем соперника :)"
                };
                AddMessageToPanelSafe(flowLayoutPanel1, Chatmessage);
            }
            Text = lobby.LobbyName;
            DoubleBuffered = true;
            SetLabelVisibilitySafe(label4,false);
            Upd();
            lastpart = DateTime.Now + new TimeSpan(0, 0, 60);
            gameBoard1.MoveMade += GameBoard1_MoveMade;
            timer.Start();
            System.Windows.Forms.Timer gameBoard1_Timer = new System.Windows.Forms.Timer();
            gameBoard1_Timer.Interval = 16;
            gameBoard1_Timer.Tick += GameBoard1_Timer_Tick;
            gameBoard1_Timer.Start();

        }

        public void Client_OnMessageReceived(string message)
        {
            try
            {
                if (message == "swap")
                {
                    gameBoard1.CanPlay = !gameBoard1.CanPlay;
                    lastpart = DateTime.Now + new TimeSpan(0, 0, 60);

                    UpdateLabelSafe(label5, gameBoard1.CanPlay ? "Ваш ход" : "Чужой ход");
                    return;
                }
                if (message == "start")
                {
                    SetLabelVisibilitySafe(label4, true);
                    if (white) gameBoard1.CanPlay = true;
                    lastpart = DateTime.Now + new TimeSpan(0, 0, 60);
                    UpdateLabelSafe(label5, gameBoard1.CanPlay ? "Ваш ход" : "Чужой ход");
                    ChatMessage Chatmessage = new ChatMessage
                    {
                        who = white ? "white" : "black",
                        text = "Игра началась!"
                    };
                    AddMessageToPanelSafe(flowLayoutPanel1, Chatmessage);
                    return;
                }
                if (message == "Противник отключился.")
                {
                    MessageBox.Show("Противник отключился.");
                    Program.game = null;
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new System.Action(() => this.Close()));
                    }
                    else
                    {
                        this.Close();
                    }
                    return;
                }
                if (message.StartsWith("chat"))
                {
                    var text = message.Substring(5);
                    var action = JsonSerializer.Deserialize<ChatMessage>(text);
                    if (action != null)
                    {
                        AddMessageToPanelSafe(flowLayoutPanel1, action);
                    }
                }
                if (message.StartsWith("action"))
                {
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
                        MessageBox.Show("Ошибка отправки сообщения.");
                    }
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"ошибка JSON: {ex.Message}");
            }
        }

        private async void GameBoard1_Timer_Tick(object? sender, EventArgs e)
        {
            UpdateLabelSafe(label2, $"Белых: {gameBoard1.WhiteCount}");
            UpdateLabelSafe(label3, $"Чёрных: {gameBoard1.BlackCount}");
        }

        private async void GameBoard1_MoveMade(object? sender, MoveEventArgs e)
        {
            string comand = e.MoverIsBlack ? "black" : "white";
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            // Создаем объект Action
            var action = new Action
            {
                action = comand,
                from = $"{e.From.X}:{e.From.Y}",
                to = $"{e.To.X}:{e.To.Y}",
                iscapture = e.IsCapture,
                capture = $"{e.Capture.X}:{e.Capture.Y}",
                HasCaptureMoves = e.HasCaptureMoves
            };

            // Сериализуем объект Action в строку JSON
            string actionJson = JsonSerializer.Serialize(action, options);

            string message = $"action:{actionJson}";
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

        private void UpdateLabelSafe(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new System.Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }
        public void AddMessageToPanelSafe(FlowLayoutPanel panel, ChatMessage message)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new System.Action(() => AddMessageToPanel(panel, message)));
            }
            else
            {
                AddMessageToPanel(panel, message);
            }
        }

        private void AddMessageToPanel(FlowLayoutPanel panel, ChatMessage message)
        {
            var messageItem = new MessageItem(message);
            panel.Controls.Add(messageItem);
            panel.ScrollControlIntoView(messageItem);

            panel.AutoScroll = true;
            panel.VerticalScroll.Value = panel.VerticalScroll.Maximum;
        }
        public void SetLabelVisibilitySafe(Label label, bool isVisible)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new System.Action(() => SetLabelVisibilitySafe(label, isVisible)));
            }
            else
            {
                label.Visible = isVisible;
            }
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                ChatMessage Chatmessage = new ChatMessage
                {
                    who = white ? "white" : "black",
                    text = textBox1.Text
                };
                string message = "chat:" + JsonSerializer.Serialize<ChatMessage>(Chatmessage);
                await Program.client.SendMessageAsync(message);
                AddMessageToPanelSafe(flowLayoutPanel1,Chatmessage);
            }
        }
    }
}