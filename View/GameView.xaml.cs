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

namespace Chess.View
{
    public partial class GameView : UserControl
    {       
        //Timer - variables
        private bool isSlowGame;
        private TimeSpan timePlayerWhite;
        private TimeSpan timePlayerBlack;
        private TimeSpan elapsedWhite = TimeSpan.Zero;
        private TimeSpan elapsedBlack = TimeSpan.Zero;
        private Stopwatch turnClock = new Stopwatch();
        private DispatcherTimer timer;
        private bool isWhiteLast;
        private BoardWindow boardWindow;
        private Chessboard board;
        private GameSidePanelBotChooseView botChooseView;

        private bool canForfeit = false;
        public GameView()
        {
            InitializeComponent();
            SetupTimer();

            board = new Chessboard();
            UpdateTimerDisplays();
        }
        public void GameSidePanelBotChooseView()
        {
            CC.Content = new GameSidePanelBotChooseView();
            botChooseView = new GameSidePanelBotChooseView();
        }

        public void GameSidePanelPlayerChooseView()
        {
            CC.Content = new GameSidePanelPlayerChooseView();
        }

        public void GameSidePanelPlayingView()
        {
            CC.Content = new GameSidePanelPlayingView();
        }
        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }

        private void play_forfeit_Click(object sender, RoutedEventArgs e)
        {
            if (!canForfeit)
            {
                Board.InitializeGame(Pieces.White, true, 5, true, "Alireza Firouzja");

                play_forfeit.Style = (Style)FindResource("GrayButtonStyle");
                play_forfeit.Content = "Forfeit";
                GameSidePanelPlayingView();
                canForfeit = true;
                InitializeTimers(botChooseView.GetSelectedTime());
                isWhiteLast = board.isWhiteTurn;
                turnClock.Restart();
                timer.Start();
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Gra zakończona przez forfeit");
            }
        }

        private void Timer_Tick(object? sender, EventArgs? e)
        {
            if (!Board.started) return;

            if (board.isWhiteTurn != isWhiteLast)
            {
                var elapsed = turnClock.Elapsed;
                if (isWhiteLast)
                    elapsedWhite += elapsed;
                else
                    elapsedBlack += elapsed;

                isWhiteLast = board.isWhiteTurn;
                turnClock.Restart();
            }

            UpdateTimerDisplays();

            if (board.isWhiteTurn)
            {
                var remaining = timePlayerWhite - elapsedWhite - turnClock.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    EndGameByTimeout(Pieces.White);
                    return;
                }
            }
            else
            {
                var remaining = timePlayerBlack - elapsedBlack - turnClock.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    EndGameByTimeout(Pieces.Black);
                    return;
                }
            }
        }

        private void UpdateTimerDisplays()
        {
            if (board.isWhiteTurn)
            {
                var remaining = timePlayerWhite - elapsedWhite - turnClock.Elapsed;
                WhiteTimerText.Text = remaining.ToString(@"mm\:ss");
                BlackTimerText.Text = (timePlayerBlack - elapsedBlack).ToString(@"mm\:ss");
            }
            else
            {
                var remaining = timePlayerBlack - elapsedBlack - turnClock.Elapsed;
                BlackTimerText.Text = remaining.ToString(@"mm\:ss");
                WhiteTimerText.Text = (timePlayerWhite - elapsedWhite).ToString(@"mm\:ss");
            }
        }

        private void EndGameByTimeout(int losingColor)
        {
            timer.Stop();
            Board.started = false;
            string winner = losingColor == Pieces.White ? "CZARNE" : "BIAŁE";
            MessageBox.Show($"Czas upłynął! Wygrywają {winner} przez przekroczenie czasu.");
        }

        public void InitializeTimers(TimeSpan timeForBothPlayers)
        {
            timePlayerWhite = timeForBothPlayers;
            timePlayerBlack = timeForBothPlayers;
            elapsedWhite = TimeSpan.Zero;
            elapsedBlack = TimeSpan.Zero;
            UpdateTimerDisplays();
        }

    }
}
