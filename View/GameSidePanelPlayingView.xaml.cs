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
        public GameSidePanelPlayingView()
        {
            InitializeComponent();
        }
        private void notationType_Checked(object sender, RoutedEventArgs e)
        {
            if (notationType.IsChecked == true)
            {
                // notationPanelManager.SetNotationType(true, board);
            }
        }

        private void notationType_Unchecked(object sender, RoutedEventArgs e)
        {
            if (notationType.IsChecked == false)
            {
                // notationPanelManager.SetNotationType(false, board);
            }
        }
    }
}
