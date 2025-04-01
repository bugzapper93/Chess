using Chess.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Chess.Objects.ChessOnline;

namespace Chess.View
{
    public partial class GameView : UserControl
    {
        //Timer - variables
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private bool isSlowGame;
        private bool isWhiteLast;
        private GameSidePanelBotChooseView botChooseView;

        public bool pvpLAN = false;
        public bool pvpLocal = false;
        public bool AIGame = false;
        public bool GMAI = false;
        public string selectedGM = "Magnus Carlsen";
        public bool canForfeit = false;
        private NotationPanelManager notationManager;
        public GameView()
        {
            InitializeComponent();

            // Board = new BoardWindow();
            Board.GameTimeout += EndGameByTimeout;
            Board.TimerUpdate += UpdateTimerDisplays;
            if (NotationPanel is GameSidePanelPlayingView notationPanel)
            {
                notationPanel.SetBoardWindow(Board);
                notationManager = new NotationPanelManager(notationPanel.NotationGrid, Board.movesMade);
            }
            NotationPanel.Visibility = Visibility.Hidden;

            string resource = "Chess.Resources.Images.reverse.png";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                reverseImg.Source = bitmap;
            }
            resource = "Chess.Resources.Images.idea.png";
            assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                ideaImg.Source = bitmap;
            }
        }
        public void Initialize()
        {
            Board.InitializeBoardView();
            if (NotationPanel is GameSidePanelPlayingView notationPanel)
            {
                notationPanel.SetBoardWindow(Board);
                notationManager = new NotationPanelManager(notationPanel.NotationGrid, Board.movesMade); 
            }
            if (AIGame)
                GameSidePanelBotChooseView();
            else if (pvpLocal)
                GameSidePanelPlayerChooseView();
            else if (pvpLAN)
            {
                GameSidePanelPlayerChooseView();
            }
        }

        public void TipClick(object sender, RoutedEventArgs e)
        {
            if (canForfeit)
            {
                Board.GetTip();
            }
        }
        public void RevClick(object sender, RoutedEventArgs e)
        {
            if (canForfeit)
            {
                Board.ReverseMove();
            }
        }

        public void GameSidePanelBotChooseView()
        {
            botChooseView = new GameSidePanelBotChooseView();
            CC.Content = botChooseView;
        }
        
        public void GameSidePanelPlayerChooseView()
        {
            CC.Content = new GameSidePanelPlayerChooseView();
        }

        public void GameSidePanelPlayingView()
        {
            var playingView = new GameSidePanelPlayingView();
            CC.Content = playingView;
        }
        private void NerdViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (AIGame)
            {
                NerdInfoLabel.Text = Board.GetInformationForNerdAI();
            }
            else if (pvpLAN)
            {
                NerdInfoLabel.Text = Board.GetInformationForNerdsLAN();
            }
            NerdViewPanel.Visibility = Visibility.Visible; 
        }

        private void NerdViewOKButton_Click(object sender, RoutedEventArgs e)
        {
            NerdViewPanel.Visibility = Visibility.Collapsed;
        }
        private void play_forfeit_Click(object sender, RoutedEventArgs e)
        {
            if (!canForfeit)
            {
                ResetGame();
            }
            else
            {
                ForfeitGame();
            }
        }
        public void ResetGame()
        {
            _cts = new CancellationTokenSource();
            int depth = 0;
            int chosenColor = Pieces.White; 
            bool playGM = false;
            string chosenGM = "";
            int maxTime = 300;
            Board.cancellationTokenSource = _cts;
            // Rzeczy dla AI
            if (botChooseView is not null)
            {
                chosenColor = botChooseView.chosenColor;
                if (botChooseView.FiveMinutesRadioButton.IsChecked.Value)
                {
                    maxTime = 300;
                }
                else
                {
                    maxTime = 600;
                }
                if (botChooseView.GMmodeEnabled != null && botChooseView.bots.SelectedItem != null)
                {
                    playGM = botChooseView.GMmodeEnabled.IsChecked.Value;
                    chosenGM = botChooseView.bots.SelectedItem.ToString();
                }

                if (AIGame)
                    depth = botChooseView.SelectedDepth;

                Board.cancellationTokenSource = _cts;
                // Koniec rzeczy dla AI
            }
            if (pvpLocal || AIGame)
            {
                if (chosenColor == Pieces.Black)
                    Board.FlipBoard();

                Board.InitializeGame(
                    chosenColor,
                    AIGame,
                    pvpLAN,
                    pvpLocal,
                    depth,
                    playGM,
                    chosenGM,
                    notationManager
                );
            }
            
            if (pvpLAN)
            {
                chosenColor = Board.playerColor;

                if (chosenColor == Pieces.Black)
                    Board.FlipBoard();

                Board.InitializeGame(
                    chosenColor,
                    false,
                    true,
                    false
                );
            }

            Board.whiteTime = maxTime;
            Board.blackTime = maxTime;

            canForfeit = true;
            GameSidePanelPlayingView();
            play_forfeit.Style = (Style)FindResource("GrayButtonStyle");
            play_forfeit.Content = "Forfeit";
            NotationPanel.Visibility = Visibility.Visible;
        }
        private void ForfeitGame()
        {
            Board.cancellationTokenSource.Cancel();
            ForfeitPanel.Visibility = Visibility.Visible;
            NotationPanel.Visibility = Visibility.Hidden;
            Board.board.InitializeBoard();
            Board.InitializeBoardView();
            play_forfeit.Style = (Style)FindResource("BlueButtonStyle");
            play_forfeit.Content = "Play";
            Board.timerStarted = false;
        }
        private void UpdateTimerDisplays(int whiteTime, int blackTime)
        {
            WhiteTimerText.Text = TimeSpan.FromSeconds(whiteTime).ToString(@"mm\:ss");
            BlackTimerText.Text = TimeSpan.FromSeconds(blackTime).ToString(@"mm\:ss");
        }
        private void ForfeitBackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowHomeClick(sender, e); 
                Board.InitializeBoardView();
            }
            ForfeitPanel.Visibility = Visibility.Collapsed; 
            canForfeit = false; 
            play_forfeit.Style = (Style)FindResource("BlueButtonStyle"); 
            play_forfeit.Content = "Play";
        }
        private void EndGameByTimeout(int losingColor)
        {
            Board.started = false;
            string winner = losingColor == Pieces.White ? "CZARNE" : "BIAŁE";
            TimeoutLabel.Text = $"Czas upłynął! Wygrywają {winner} przez przekroczenie czasu.";
            TimeoutPanel.Visibility = Visibility.Visible;
        }

        private void TimeoutBackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowHomeClick(sender, e);
              //  Board.ResetBoard();
            }
            TimeoutPanel.Visibility = Visibility.Collapsed;
            canForfeit = false;
            play_forfeit.Style = (Style)FindResource("BlueButtonStyle");
            play_forfeit.Content = "Play";
        }
    }
}
