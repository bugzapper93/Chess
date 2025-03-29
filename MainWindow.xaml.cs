using Chess.Objects;
using Chess.Tools;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var GameButton = (Button)HomeView.FindName("BotGame");

        GameButton.Click += ShowGameClick;
    }
    #region Handling the UI
    private void ShowGameClick(object sender, RoutedEventArgs e)
    {
        HomeView.Visibility = Visibility.Collapsed;
        GameView.Visibility = Visibility.Visible;
        GameView.GameSidePanelBotChooseView();
    }
    #endregion
}