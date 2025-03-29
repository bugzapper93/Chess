using Chess.Objects;
using System;
using System.Collections.Generic;
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

namespace Chess.View
{
    /// <summary>
    /// Logika interakcji dla klasy GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        bool can_forfeit = false;

        public void GameSidePanelBotChooseView()
        {
            CC.Content = new GameSidePanelBotChooseView();
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
            if (can_forfeit == false)
            {
                Board.started = true;
                Board.InitializeGame(Pieces.Black, false, 5, false);

                play_forfeit.Style = (Style)FindResource("GrayButtonStyle");
                play_forfeit.Content = "Forfeit";
                GameSidePanelPlayingView();
                can_forfeit = true;

                return;
            }
            else
            {
                
            }
        }
    }
}
