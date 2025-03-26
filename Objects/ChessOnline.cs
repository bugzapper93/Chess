using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Chess.Objects;

namespace Chess.Objects
{
    public class ChessOnline
    {
        public readonly P2PNetworkManager _networkManager;
        private readonly MainWindow _chessMainWindow;
        private string _nickname;
        public ChessOnline(MainWindow chessMainWindow)
        {
            _networkManager = new P2PNetworkManager(NetworkConfig.MulticastGroup, NetworkConfig.Port);
            _chessMainWindow = chessMainWindow;
            _networkManager.OnChatMessageReceived += ReceiveMoveMessage;
            _networkManager.OnConnectionStateChanged += UpdateUI;
            _networkManager.OnPlayerListUpdated += UpdatePlayerList;
            _networkManager.OnHostDiscovered += AddHostToList;
            _networkManager.OnError += ShowErrorMessage;
        }

        public void ShowServerPanel()
        {
            _chessMainWindow.ServerPanel.Visibility = Visibility.Visible;
            _chessMainWindow.MainMenu.Visibility = Visibility.Hidden;
            _chessMainWindow.HideBtn.Visibility = Visibility.Visible;
            UpdateUI(); // Initial UI update
        }

        public async Task SendMoveAsync(Position start, Position end, int playerColor)
        {
            if (_networkManager.IsConnected || _networkManager.IsHosting)
            {
                _nickname = _chessMainWindow.txtNick.Text.Trim();
                string moveMessage = $"MOVE|{start.row},{start.column}|{end.row},{end.column}|{playerColor}";
                await _networkManager.SendChatMessageAsync(_nickname, moveMessage);
            }
        }

        private void ReceiveMoveMessage(string senderNick, string message)
        {
            if (message.StartsWith("MOVE|"))
            {
                var parts = message.Split('|');
                if (parts.Length == 4)
                {
                    var startParts = parts[1].Split(',');
                    var endParts = parts[2].Split(',');
                    if (startParts.Length == 2 && endParts.Length == 2)
                    {
                        Position start = new Position(int.Parse(startParts[0]), int.Parse(startParts[1]));
                        Position end = new Position(int.Parse(endParts[0]), int.Parse(endParts[1]));
                        int opponentColor = int.Parse(parts[3]); // Opponent's color from the message
                        _chessMainWindow.Dispatcher.Invoke(() =>
                        {
                            _chessMainWindow.MovePiece(start, end, opponentColor);
                            // Remove FlipBoard() call since orientation is fixed
                        });
                    }
                }
            }
            else
            {
                _chessMainWindow.Dispatcher.Invoke(() =>
                {
                    _chessMainWindow.lstChatMessages.Items.Add($"{senderNick}: {message}");
                });
            }
        }
        private void UpdateUI()
        {
            _chessMainWindow.Dispatcher.Invoke(() =>
            {
                _chessMainWindow.btnHost.IsEnabled = !_networkManager.IsHosting && !_networkManager.IsConnected;
                _chessMainWindow.btnDolacz.IsEnabled = !_networkManager.IsHosting && !_networkManager.IsConnected;
                _chessMainWindow.btnWyjdz.IsEnabled = _networkManager.IsHosting || _networkManager.IsConnected;
                _chessMainWindow.btnRefresh.IsEnabled = !_networkManager.IsHosting;
                _chessMainWindow.btnSendMessage.IsEnabled = _networkManager.IsConnected;
                _chessMainWindow.toolStripStatusLabel1.Text = _networkManager.IsHosting ? "Status: Hosting" :
                                                              _networkManager.IsConnected ? "Status: Connected" :
                                                              "Status: Disconnected";
                _chessMainWindow.toolStripStatusLabelIP.Text = "Local IP: " + P2PNetworkManager.GetLocalIPAddress();
            });
        }

        private void UpdatePlayerList(string[] players)
        {
            _chessMainWindow.Dispatcher.Invoke(() =>
            {
                _chessMainWindow.lstGracze.Items.Clear();
                string hostNick = _networkManager.IsHosting ? _nickname : _networkManager._hostNickname;
                foreach (var player in players)
                {
                    string displayName = player == hostNick ? $"{player} (Host)" : player;
                    if (!_chessMainWindow.lstGracze.Items.Contains(displayName))
                    {
                        _chessMainWindow.lstGracze.Items.Add(displayName);
                    }
                }
                _chessMainWindow.playersGroupBox.Header = $"Players ({players.Length})";
            });
        }

        private void AddHostToList(string nickname, string ip)
        {
            _chessMainWindow.Dispatcher.Invoke(() =>
            {
                if (!_chessMainWindow.lvwHosts.Items.Cast<HostInfo>().Any(item => item.IP == ip))
                {
                    _chessMainWindow.lvwHosts.Items.Add(new HostInfo { Nickname = nickname, IP = ip });
                }
            });
        }

        private void ShowErrorMessage(string message)
        {
            _chessMainWindow.Dispatcher.Invoke(() => MessageBox.Show(message));
        }

        public void Dispose()
        {
            _networkManager?.DisposeAsync().GetAwaiter().GetResult();
        }
        public class HostInfo
        {
            public string Nickname { get; set; }
            public string IP { get; set; }

        }
        public class UIManager(MainWindow window)
        {
            public void UpdateButtonStates()
            {

            }

            public static string? ValidateNickname(string? input)
            {
                input = input?.Trim();
                if (string.IsNullOrWhiteSpace(input) || input.Length < 3 || input.Length > 20)
                {
                    MessageBox.Show("Nickname must be between 3 and 20 characters.");
                    return null;
                }

                input = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_]", "");
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("Nickname must contain at least one alphanumeric character.");
                    return null;
                }

                return input;
            }
        }
        public static class NetworkConfig
        {
            public const int DefaultPort = 5000;
            public const string DefaultMulticastGroup = "239.0.0.1";
            public static int Port { get; set; } = DefaultPort;
            public static string MulticastGroup { get; set; } = DefaultMulticastGroup;
        }

        public enum MessageType
        {
            Discover,
            PlayerListRequest,
            PlayerList,
            Join,
            Leave,
            Chat,
            Error
        }

        public class MessageHandler(P2PNetworkManager networkManager)
        {
            public async Task ProcessMessageAsync(UdpReceiveResult result, string localNickname, IPEndPoint groupEndPoint)
            {
                string message = Encoding.UTF8.GetString(result.Buffer);
                var parts = message.Split('|');
                if (parts.Length < 1) return;

                string? senderNick = parts.Length > 1 ? parts[1] : null;
                if (senderNick == localNickname) return;

                MessageType type = ParseMessageType(parts[0]);
                switch (type)
                {
                    case MessageType.Discover:
                        if (networkManager.IsHosting)
                        {
                            await HandleDiscoverAsync(result.RemoteEndPoint, localNickname);
                        }
                        break;
                    case MessageType.PlayerListRequest:
                        if (networkManager.IsHosting)
                        {
                            await networkManager.BroadcastPlayerListAsync(groupEndPoint);
                        }
                        break;
                    case MessageType.Join:
                        if (parts.Length > 1 && networkManager.IsHosting)
                        {
                            await HandleJoinAsync(parts[1], result.RemoteEndPoint, groupEndPoint);
                        }
                        break;
                    case MessageType.Leave:
                        if (parts.Length > 1)
                        {
                            await HandleLeaveAsync(parts[1], groupEndPoint);
                        }
                        break;
                    case MessageType.Chat:
                        if (parts.Length > 3)
                        {
                            await HandleChatAsync(parts[1], parts[2], string.Join("|", parts.Skip(3)), groupEndPoint);
                        }
                        break;
                    case MessageType.PlayerList:
                        if (networkManager.IsConnected && parts.Length > 1)
                        {
                            string listStr = string.Join("|", parts.Skip(1));
                            var players = listStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            networkManager.UpdatePlayerList(players);
                        }
                        break;
                }
            }

            private static MessageType ParseMessageType(string header)
            {
                return header switch
                {
                    "DISCOVER" => MessageType.Discover,
                    "PLAYERLIST_REQUEST" => MessageType.PlayerListRequest,
                    "PLAYERLIST" => MessageType.PlayerList,
                    "JOIN" => MessageType.Join,
                    "LEAVE" => MessageType.Leave,
                    "CHAT" => MessageType.Chat,
                    "ERROR" => MessageType.Error,
                    _ => throw new ArgumentException("Unknown message type")
                };
            }

            private async Task HandleDiscoverAsync(IPEndPoint remoteEndPoint, string nickname)
            {
                string reply = $"{nickname}|{P2PNetworkManager.GetLocalIPAddress()}";
                await networkManager.SendAsync(Encoding.UTF8.GetBytes(reply), remoteEndPoint);
            }

            private async Task HandleJoinAsync(string playerNick, IPEndPoint remoteEndPoint, IPEndPoint groupEndPoint)
            {
                if (!networkManager.ConnectedPlayers.Contains(playerNick))
                {
                    networkManager.ConnectedPlayers.Add(playerNick);
                    networkManager.UpdatePlayerList([.. networkManager.ConnectedPlayers]);
                    await networkManager.SendAsync(Encoding.UTF8.GetBytes("JOINED"), remoteEndPoint);
                    await networkManager.BroadcastPlayerListAsync(groupEndPoint);
                }
                else
                {
                    await networkManager.SendAsync(Encoding.UTF8.GetBytes("ERROR|Nickname taken!"), remoteEndPoint);
                }
            }

            private async Task HandleLeaveAsync(string leavingNick, IPEndPoint groupEndPoint)
            {
                var newList = networkManager.ConnectedPlayers.Where(p => p != leavingNick).ToList();
                networkManager.ConnectedPlayers.Clear();
                newList.ForEach(networkManager.ConnectedPlayers.Add);
                networkManager.UpdatePlayerList([.. networkManager.ConnectedPlayers]);

                // Check if the leaving player is the host and this is a client
                if (!networkManager.IsHosting && leavingNick == networkManager._hostNickname)
                {
                    await networkManager.LeaveAsync(networkManager._clientNickname);
                    MessageBox.Show("The host has left the lobby. You have been disconnected.");
                }
                else if (networkManager.IsHosting)
                {
                    await networkManager.BroadcastPlayerListAsync(groupEndPoint);
                }
            }

            private async Task HandleChatAsync(string senderNick, string messageId, string message, IPEndPoint groupEndPoint)
            {
                if (networkManager.ProcessedMessageIds.Add(messageId))
                {
                    networkManager.ReceiveChatMessage(senderNick, message);
                    if (networkManager.IsHosting)
                    {
                        await networkManager.BroadcastMessageAsync($"CHAT|{senderNick}|{messageId}|{message}", groupEndPoint);
                    }
                }
            }
        }

        public class P2PNetworkManager : IDisposable
        {
            private readonly string _multicastGroup;
            private readonly int _port;
            private UdpClient? _sendUdpClient;
            private UdpClient? _receiveUdpClient;
            private CancellationTokenSource? _cts;
            private readonly ConcurrentBag<string> _connectedPlayers = [];
            private bool _isHosting;
            private bool _isConnected;
            private readonly HashSet<string> _processedMessageIds = [];
            public string? _hostNickname;
            public string? _clientNickname;
            private Task? _listenerTask;
            private readonly MessageHandler _messageHandler;

            public event Action<string[]>? OnPlayerListUpdated;
            public event Action<string, string>? OnChatMessageReceived;
            public event Action<string>? OnError;
            public event Action? OnConnectionStateChanged;
            public event Action<string, string>? OnHostDiscovered;

            public bool IsHosting => _isHosting;
            public bool IsConnected => _isConnected;
            public ConcurrentBag<string> ConnectedPlayers => _connectedPlayers;
            public HashSet<string> ProcessedMessageIds => _processedMessageIds;

            public P2PNetworkManager(string multicastGroup, int port)
            {
                _multicastGroup = multicastGroup;
                _port = port;
                _messageHandler = new MessageHandler(this);
            }

            public async Task StartGameAsync()
            {

            }

            public async Task StartHostingAsync(string nickname)
            {
                if (string.IsNullOrEmpty(nickname)) throw new ArgumentException("Nickname cannot be null or empty.", nameof(nickname));
                if (_isHosting) return;

                try
                {
                    _cts = new CancellationTokenSource();
                    _connectedPlayers.Clear();
                    _connectedPlayers.Add(nickname);
                    _clientNickname = nickname;
                    _hostNickname = nickname; // Set host nickname explicitly

                    _receiveUdpClient = new UdpClient();
                    _receiveUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _receiveUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
                    _receiveUdpClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                    _sendUdpClient = new UdpClient();
                    _sendUdpClient.Client.MulticastLoopback = false; // Prevent host from receiving its own multicast
                    _sendUdpClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                    await BroadcastPlayerListAsync(new IPEndPoint(IPAddress.Parse(_multicastGroup), _port));
                    OnPlayerListUpdated?.Invoke([.. _connectedPlayers]);
                    _isHosting = true;
                    _isConnected = true;
                    OnConnectionStateChanged?.Invoke();

                    _listenerTask = Task.Run(() => HostLobbyAsync(nickname, _cts.Token));
                    _ = _listenerTask.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            OnError?.Invoke($"Host listener failed: {t.Exception?.InnerException?.Message}");
                            _ = LeaveAsync(nickname);
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in StartHostingAsync: " + ex.ToString());
                    try
                    {
                        await LeaveAsync(nickname);
                    }
                    catch (Exception leaveEx)
                    {
                        OnError?.Invoke($"Cleanup failed: {leaveEx.Message}");
                    }
                    throw;
                }
            }

            public async Task JoinLobbyAsync(string nickname, string hostIp, string hostNickname)
            {
                if (string.IsNullOrEmpty(nickname)) throw new ArgumentException("Nickname cannot be null or empty.", nameof(nickname));
                if (_isConnected) return;

                try
                {
                    using (var joinClient = new UdpClient())
                    {
                        joinClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        joinClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
                        joinClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                        var hostEndPoint = new IPEndPoint(IPAddress.Parse(hostIp), _port);
                        string joinMessage = $"JOIN|{nickname}";
                        await joinClient.SendAsync(Encoding.UTF8.GetBytes(joinMessage), hostEndPoint);

                        var receiveTask = joinClient.ReceiveAsync();
                        var timeoutTask = Task.Delay(3000);
                        var finishedTask = await Task.WhenAny(receiveTask, timeoutTask);

                        if (finishedTask == timeoutTask)
                            throw new TimeoutException("Host did not respond in time.");

                        string response = Encoding.UTF8.GetString(receiveTask.Result.Buffer);
                        if (response.StartsWith("ERROR|"))
                            throw new InvalidOperationException(response[6..]);
                    }

                    _sendUdpClient = new UdpClient();
                    _sendUdpClient.Client.MulticastLoopback = false; // Prevent client from receiving its own multicast
                    _sendUdpClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                    _receiveUdpClient = new UdpClient();
                    _receiveUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _receiveUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
                    _receiveUdpClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                    _isConnected = true;
                    _hostNickname = hostNickname;
                    _clientNickname = nickname;
                    await _sendUdpClient.SendAsync(Encoding.UTF8.GetBytes("PLAYERLIST_REQUEST"), new IPEndPoint(IPAddress.Parse(hostIp), _port));
                    _cts = new CancellationTokenSource();
                    OnConnectionStateChanged?.Invoke();

                    _listenerTask = Task.Run(() => ClientListenerAsync(_cts.Token));
                    _ = _listenerTask.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            OnError?.Invoke($"Client listener failed: {t.Exception?.InnerException?.Message}");
                            _ = LeaveAsync(nickname);
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                catch
                {
                    _sendUdpClient?.Dispose();
                    _receiveUdpClient?.Dispose();
                    _sendUdpClient = null;
                    _receiveUdpClient = null;
                    throw;
                }
            }

            private async Task HostLobbyAsync(string nickname, CancellationToken token)
            {
                try
                {
                    var groupEndPoint = new IPEndPoint(IPAddress.Parse(_multicastGroup), _port);
                    while (!token.IsCancellationRequested)
                    {
                        var result = await _receiveUdpClient!.ReceiveAsync(token);
                        await _messageHandler.ProcessMessageAsync(result, nickname, groupEndPoint);
                    }
                }
                catch (OperationCanceledException)
                {
                    OnError?.Invoke("Host listener canceled");
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("Broadcast/Listening error: " + ex.Message);
                    throw;
                }
            }

            private async Task ClientListenerAsync(CancellationToken token)
            {
                try
                {
                    var groupEndPoint = new IPEndPoint(IPAddress.Parse(_multicastGroup), _port);
                    while (!token.IsCancellationRequested)
                    {
                        var result = await _receiveUdpClient!.ReceiveAsync(token);
                        await _messageHandler.ProcessMessageAsync(result, _clientNickname!, groupEndPoint);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("Client listener error: " + ex.Message);
                }
            }

            public async Task SendChatMessageAsync(string nickname, string message)
            {
                if (string.IsNullOrEmpty(nickname)) throw new ArgumentException("Nickname cannot be null or empty.", nameof(nickname));
                if (_sendUdpClient == null) throw new InvalidOperationException("Not connected to a lobby.");

                string messageId = Guid.NewGuid().ToString("N");
                string chatMessage = $"CHAT|{nickname}|{messageId}|{message}";
                await BroadcastMessageAsync(chatMessage, new IPEndPoint(IPAddress.Parse(_multicastGroup), _port));
            }

            public async Task BroadcastMessageAsync(string message, IPEndPoint groupEndPoint)
            {
                if (_sendUdpClient == null) return;

                var data = Encoding.UTF8.GetBytes(message);
                await _sendUdpClient.SendAsync(data, groupEndPoint);
            }

            public async Task BroadcastPlayerListAsync(IPEndPoint groupEndPoint)
            {
                string listMessage = "PLAYERLIST|" + string.Join(",", _connectedPlayers);
                await BroadcastMessageAsync(listMessage, groupEndPoint);
            }

            public async Task SendAsync(byte[] data, IPEndPoint endPoint)
            {
                if (_receiveUdpClient != null)
                    await _receiveUdpClient.SendAsync(data, endPoint);
            }

            public async Task LeaveAsync(string? nickname)
            {
                _isHosting = false;
                _isConnected = false;
                _connectedPlayers.Clear();
                _processedMessageIds.Clear();
                _hostNickname = null;
                _clientNickname = null;
                OnPlayerListUpdated?.Invoke([]);

                if (_sendUdpClient != null && nickname != null)
                {
                    try
                    {
                        var groupEndPoint = new IPEndPoint(IPAddress.Parse(_multicastGroup), _port);
                        string leaveMessage = $"LEAVE|{nickname}";
                        await _sendUdpClient.SendAsync(Encoding.UTF8.GetBytes(leaveMessage), groupEndPoint);
                    }
                    catch { }
                }

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                _sendUdpClient?.Close();
                _receiveUdpClient?.Close();
                _sendUdpClient?.Dispose();
                _receiveUdpClient?.Dispose();
                _sendUdpClient = null;
                _receiveUdpClient = null;

                OnConnectionStateChanged?.Invoke();
            }

            public async Task DiscoverHostsAsync()
            {
                using var searchClient = new UdpClient();
                searchClient.JoinMulticastGroup(IPAddress.Parse(_multicastGroup));

                await searchClient.SendAsync(Encoding.UTF8.GetBytes("DISCOVER"), new IPEndPoint(IPAddress.Parse(_multicastGroup), _port));

                var startTime = DateTime.Now;
                const int timeoutMs = 3000;

                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    var receiveTask = searchClient.ReceiveAsync();
                    var delayTask = Task.Delay(timeoutMs - (int)(DateTime.Now - startTime).TotalMilliseconds);
                    var finishedTask = await Task.WhenAny(receiveTask, delayTask);

                    if (finishedTask == delayTask) break;

                    string receivedData = Encoding.UTF8.GetString(receiveTask.Result.Buffer);
                    if (receivedData.Contains('|'))
                    {
                        var parts = receivedData.Split('|');
                        if (parts.Length == 2 && !string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                        {
                            OnHostDiscovered?.Invoke(parts[0], parts[1]);
                        }
                    }
                }
            }

            public static string GetLocalIPAddress()
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                }
                return "127.0.0.1";
            }

            public void UpdatePlayerList(string[] players)
            {
                OnPlayerListUpdated?.Invoke(players);
            }

            public void ReceiveChatMessage(string nickname, string message)
            {
                OnChatMessageReceived?.Invoke(nickname, message);
            }

            public void Dispose()
            {
                try
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts?.Dispose();
                    _sendUdpClient?.Dispose();
                    _receiveUdpClient?.Dispose();
                }
                catch { }
            }

            public async ValueTask DisposeAsync()
            {
                try
                {
                    await LeaveAsync(_clientNickname);
                    if (_listenerTask != null && !_listenerTask.IsCompleted)
                    {
                        await _listenerTask;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Error during disposal: {ex.Message}");
                }
                Dispose();
            }
        }

    }

    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return (width - 5) / 2;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}