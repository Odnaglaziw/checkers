using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    public class Client
    {
        public readonly ClientWebSocket _client;
        private readonly Uri _serverUri;
        private CancellationTokenSource _cancellationTokenSource;
        private Task? _receiveTask;

        public event Action<string>? OnMessageReceived;
        public event Action<string>? OnDisconnected;

        public Client(string serverUri)
        {
            _client = new ClientWebSocket();
            _serverUri = new Uri(serverUri);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            if (_client.State == WebSocketState.Open)
                return;

            try
            {
                await _client.ConnectAsync(_serverUri, _cancellationTokenSource.Token);
                LogConnection("Соединение установлено.");
                StartReceiving();
            }
            catch (Exception ex)
            {
                LogError($"Ошибка подключения: {ex.Message}");
                throw;
            }
        }

        private async void CloseWebSocketClient()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Клиент завершает соединение", CancellationToken.None);
                Console.WriteLine("Соединение закрыто корректно.");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_client.State != WebSocketState.Open)
            {
                string errorMessage = "Соединение не установлено.";
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(errorMessage);
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            Log($"Сообщение отправлено: {message}");
        }

        private void StartReceiving()
        {
            _receiveTask = Task.Run(async () =>
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    while (_client.State == WebSocketState.Open)
                    {
                        var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnDisconnected?.Invoke("Соединение закрыто сервером.");
                            LogConnection("Соединение закрыто сервером.");
                            break;
                        }

                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (!string.IsNullOrEmpty(message)) OnMessageReceived?.Invoke(message);
                        Log($"Получено сообщение: {message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogError($"Ошибка получения сообщения: {ex.Message}");
                    OnDisconnected?.Invoke($"Ошибка получения сообщения: {ex.Message}");
                }
            });
        }

        public void Dispose()
        {
            CloseWebSocketClient();
            _cancellationTokenSource.Cancel();
            _client.Dispose();
        }

        private static void Log(string message)
        {
            WriteLog("log.txt", message);
        }

        private static void LogError(string message)
        {
            WriteLog("logError.txt", message);
        }

        private static void LogConnection(string message)
        {
            WriteLog("logConnection.txt", message);
        }

        private static void WriteLog(string fileName, string message)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            string logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";
            File.AppendAllText(filePath, logMessage);
        }
    }
}
