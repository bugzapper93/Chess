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
    public partial class GameSidePanelPlayingView : UserControl
    {
        private NotationPanelManager notationManager;
        private BoardWindow boardWindow;

        public GameSidePanelPlayingView()
        {
            InitializeComponent();
        }

        public GameSidePanelPlayingView(BoardWindow window) : this()
        {
            SetBoardWindow(window);
        }

        public void SetBoardWindow(BoardWindow window)
        {
            boardWindow = window;
            notationManager = new NotationPanelManager(NotationGrid, window.movesMade); // Move initialization here
        }

        private void notationType_Checked(object sender, RoutedEventArgs e)
        {
            if (notationManager != null)
            {
                notationManager.SetNotationType(true);
            }
        }

        private void notationType_Unchecked(object sender, RoutedEventArgs e)
        {
            if (notationManager != null)
            {
                notationManager.SetNotationType(false);
            }
        }
    }
}
