using Chess.Objects;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chess.View
{
    public partial class MpPanelView : UserControl
    {
        public MpPanelView()
        {
            InitializeComponent();
        }
        private ChessOnline _chessOnline;
        private int playerColor = Pieces.White;
        private MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_chessOnline._networkManager.IsHosting) return;

            btnRefresh.IsEnabled = false;
            try
            {
                lvwHosts.Items.Clear();
                await _chessOnline._networkManager.DiscoverHostsAsync();
                if (lvwHosts.Items.Count == 0)
                {
                    MessageBox.Show("No hosts found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing host list: " + ex.Message);
            }
            finally
            {
                btnRefresh.IsEnabled = true;
            }
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            try
            {
                lstChatMessages.Items.Add($"{txtNick.Text.Trim()}: {message}");
                await _chessOnline._networkManager.SendChatMessageAsync(txtNick.Text.Trim(), message);
                txtChatInput.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }

        private async void btnHost_Click(object sender, RoutedEventArgs e)
        {
            if (_chessOnline._networkManager.IsHosting) return;

            string nickname = txtNick.Text.Trim();
            if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < 3 || nickname.Length > 20)
            {
                MessageBox.Show("Nickname must be between 3 and 20 characters.");
                return;
            }

            try
            {
                btnHost.IsEnabled = false;
                await _chessOnline._networkManager.StartHostingAsync(nickname);
                MessageBox.Show("Hosting started successfully!");
                playerColor = Pieces.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting host: " + ex.Message);
                await _chessOnline._networkManager.LeaveAsync(nickname);
                btnHost.IsEnabled = true;
            }
        }

        private async void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            if (_chessOnline._networkManager.IsConnected) return;

            string nickname = txtNick.Text.Trim();
            if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < 3 || nickname.Length > 20)
            {
                MessageBox.Show("Nickname must be between 3 and 20 characters.");
                return;
            }

            if (lvwHosts.SelectedItem == null)
            {
                MessageBox.Show("Select a lobby first!");
                return;
            }

            var selectedHost = (ChessOnline.HostInfo)lvwHosts.SelectedItem;
            string ip = selectedHost.IP;
            string hostNickname = selectedHost.Nickname;
            try
            {
                await _chessOnline._networkManager.JoinLobbyAsync(nickname, ip, hostNickname);
                MessageBox.Show($"Joined {ip}! Press 'Leave' to exit.");
                playerColor = Pieces.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error joining: " + ex.Message);
            }
        }

        private async void btnLeave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _chessOnline._networkManager.LeaveAsync(txtNick.Text.Trim());
                lstGracze.Items.Clear();
                playersGroupBox.Header = "Players (0)";
                MessageBox.Show("Left the lobby.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error leaving: " + ex.Message);
            }
        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSendMessage_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (_chessOnline._networkManager.IsConnected || _chessOnline._networkManager.IsHosting)
            {
                if (mainWindow != null)
                {
                //    mainWindow.ShowBoard();
                }
                else
                {
                    MessageBox.Show("Main window reference is not available.");
                }
            }
            else
            {
                MessageBox.Show("You must be connected to a lobby to start the game.");
            }
        }
    }
}