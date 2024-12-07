using System.Data;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace checkers
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        public void Client_OnMessageReceived(string message)
        {
            if (message != string.Empty)
            {
                if (message.StartsWith("action")) return;
                //MessageBox.Show($"MAIN\n{message}");
                if (message.StartsWith("c")) return;
                List<Lobby> lobbies = JsonSerializer.Deserialize<List<Lobby>>(message);
                flowLayoutPanel1.Controls.Clear();
                foreach (Lobby lobby in lobbies)
                {
                    if (flowLayoutPanel1.InvokeRequired)
                    {

                        flowLayoutPanel1.Invoke(new System.Action(() =>
                        {
                            flowLayoutPanel1.Controls.Add(new LobbyItem(lobby, Program.client._client));
                        }));
                    }
                }
            }
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Program.client.SendMessageAsync($"create_lobby:{textBox1.Text}");
            await Task.Delay(250);
            await Program.client.SendMessageAsync("get_lobbies");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await Program.client.SendMessageAsync("get_lobbies");
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Program.client.Dispose();
        }
    }
}
