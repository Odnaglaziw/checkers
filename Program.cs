using System.Net.WebSockets;

namespace checkers
{
    internal static class Program
    {
        public static Client? client;
        public static Main main = null;
        public static Form1 game = null;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Настройка приложения
            ApplicationConfiguration.Initialize();

            // Инициализация WebSocket-клиента
            InitializeWebSocketClient();

            // Запуск главной формы 
            Application.Run(main = new Main());
        }
        private static async void InitializeWebSocketClient()
        {
            client = new Client("ws://odnaglaziw.online:65000/");
            client.OnMessageReceived += WebSocketClient_OnMessageReceived;
            client.OnDisconnected += WebSocketClient_OnDisconnected;

            try
            {
                var connectionTask = client.ConnectAsync();

                if (await Task.WhenAny(connectionTask, Task.Delay(200)) == connectionTask)
                {
                    await connectionTask;
                }
                else
                {
                    WebSocketClient_OnDisconnected("xexe");
                    //MessageBox.Show("Не удалось подключиться к серверу. Попробуйте позже.");
                    //Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении WebSocket: {ex.Message}");
                Application.Exit();
            }
        }
        private static void WebSocketClient_OnMessageReceived(string message)
        {
            // Логика обработки сообщений
            //MessageBox.Show($"Сообщение от сервера: {message}");
            if (message == "lobby_refresh") main.Client_OnMessageReceived("lobby_refresh");
            else if (game != null)
            { game.Client_OnMessageReceived(message); }
            else
            { main.Client_OnMessageReceived(message); }
        }
        private static void WebSocketClient_OnDisconnected(string text)
        {
            MessageBox.Show("Попытка переподключения...");
            Task.Run(async () =>
            {
                int attemptCount = 0;
                int[] waitTimes = { 2000, 5000, 10000, 15000 }; // Время ожидания в миллисекундах

                while (attemptCount < waitTimes.Length)
                {
                    try
                    {
                        client.Dispose();
                        client = new Client("ws://odnaglaziw.online:65000/");
                        client.OnMessageReceived += WebSocketClient_OnMessageReceived;
                        client.OnDisconnected += WebSocketClient_OnDisconnected;
                        await client.ConnectAsync();
                        MessageBox.Show("Подключение восстановленно.", "Подключение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                    catch
                    {
                        attemptCount++;
                        MessageBox.Show($"Попытка #{attemptCount} не удалась. Повторная попытка через {waitTimes[attemptCount - 1] / 1000} секунд...");
                        await Task.Delay(waitTimes[attemptCount - 1]); // Задержка перед повторной попыткой
                    }
                }

                if (attemptCount == waitTimes.Length)
                {
                    MessageBox.Show("Подключение невозможно после нескольких попыток.");
                }
            });
        }
    }
}