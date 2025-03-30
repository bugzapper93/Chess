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
        private bool isWhiteLast;
        private GameSidePanelBotChooseView botChooseView;

        public bool pvpLAN = false;
        public bool pvpLocal = false;
        public bool AIGame = false;

        private bool canForfeit = false;
        public GameView()
        {
            InitializeComponent();

            // Board = new BoardWindow();

            Board.TimerUpdate += UpdateTimerDisplays;
        }
        public void Initialize()
        {
            if (AIGame)
                GameSidePanelBotChooseView();
            else if (pvpLocal)
                GameSidePanelPlayerChooseView();
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
            CC.Content = new GameSidePanelPlayingView();
        }
        

        private void play_forfeit_Click(object sender, RoutedEventArgs e)
        {
            if (!canForfeit)
            {
                int depth = 0;
                if (AIGame)
                    depth = botChooseView.SelectedDepth;

                Board.InitializeGame(Pieces.White, AIGame, pvpLAN, pvpLocal);
                canForfeit = true;

                play_forfeit.Style = (Style)FindResource("GrayButtonStyle");
                play_forfeit.Content = "Forfeit";

                GameSidePanelPlayingView();

            }
            else
            {
                MessageBox.Show("Gra zakończona przez forfeit");
            }
        }
        private void UpdateTimerDisplays(int whiteTime, int blackTime)
        {
            WhiteTimerText.Text = TimeSpan.FromSeconds(whiteTime).ToString(@"mm\:ss");
            BlackTimerText.Text = TimeSpan.FromSeconds(blackTime).ToString(@"mm\:ss");
        }

        private void EndGameByTimeout(int losingColor)
        {
            Board.started = false;
            string winner = losingColor == Pieces.White ? "CZARNE" : "BIAŁE";
            MessageBox.Show($"Czas upłynął! Wygrywają {winner} przez przekroczenie czasu.");
        }
    }
}
