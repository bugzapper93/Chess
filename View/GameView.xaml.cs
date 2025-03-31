using Chess.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private bool canForfeit = false;
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
                notationManager = new NotationPanelManager(notationPanel.NotationGrid);
            }
            NotationPanel.Visibility = Visibility.Hidden;
        }
        public void Initialize()
        {
            Board.InitializeBoardView();
            if (AIGame)
                GameSidePanelBotChooseView();
            else if (pvpLocal)
                GameSidePanelPlayerChooseView();
            else if (pvpLAN)
            {
                GameSidePanelPlayerChooseView();
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
            var playingView = new GameSidePanelPlayingView(Board);
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
                _cts = new CancellationTokenSource();
                int depth = 0;
                if (AIGame)
                    depth = botChooseView.SelectedDepth;
                Board.InitializeGame(
                            Pieces.White,   
                            AIGame,         
                            pvpLAN,         
                            pvpLocal,      
                            depth,         
                            GMAI,           
                            selectedGM,    
                            notationManager 
                        );

                canForfeit = true;
                GameSidePanelPlayingView();
                play_forfeit.Style = (Style)FindResource("GrayButtonStyle");
                play_forfeit.Content = "Forfeit";
                NotationPanel.Visibility = Visibility.Visible;
                Board.cancellationTokenSource = _cts;
            }
            else
            {
                Board.cancellationTokenSource.Cancel();
                ForfeitPanel.Visibility = Visibility.Visible;
                NotationPanel.Visibility = Visibility.Hidden;
                Board.board.InitializeBoard();
                Board.InitializeBoardView();
                play_forfeit.Style = (Style)FindResource("BlueButtonStyle");
                play_forfeit.Content = "Play";
            }
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
