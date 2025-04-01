using Chess.Objects;
using Chess.Tools;
using Chess.View;
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
    private HomeView homeView;
    public GameView gameView;
    private CreditsView creditsView;
    private SettingsView settingsView;
    private ExitView exitView;
    private MultiPlayerView multiplayerView;
    private MpPanelView mpPanelView;
    private MediaPlayer backgroundMusicPlayer = new MediaPlayer();
    public MainWindow()
    {
        InitializeComponent();
        InitializeBackgroundMusic();
        homeView = (HomeView)FindName("HomeView");
        gameView = (GameView)FindName("GameView");
        creditsView = (CreditsView)FindName("CreditsView");
        settingsView = (SettingsView)FindName("SettingsView");
        exitView = (ExitView)FindName("ExitView");
        multiplayerView = (MultiPlayerView)FindName("MultiPlayerView");
        mpPanelView = (MpPanelView)FindName("MpPanelView");

        var gameButton = (Button)homeView.FindName("BotGame");
        gameButton.Click += ShowGameClick;

        var creditsButton = (Button)homeView.FindName("Credits");
        creditsButton.Click += ShowCreditsClick;

        var settingsButton = (Button)homeView.FindName("Settings");
        settingsButton.Click += ShowSettingsClick;

        var multiplayerButton = (Button)homeView.FindName("Multiplayer");
        multiplayerButton.Click += ShowMultiPlayerClick;

        var creditsToHomeButton = (Button)creditsView.FindName("Home");
        creditsToHomeButton.Click += ShowHomeClick;

        var settingsToHomeButton = (Button)settingsView.FindName("Home");
        settingsToHomeButton.Click += ShowHomeClick;

        var botGameToHomeButton = (Button)gameView.FindName("Home");
        botGameToHomeButton.Click += ShowHomeClick;

       // var botGameNerdButton = (Button)gameView.FindName();

        var exitButton = (Button)homeView.FindName("Exit");
        exitButton.Click += ShowExitClick;

        var exitToHome = (Button)exitView.FindName("Home");
        exitToHome.Click += ShowHomeClick;

        var exitApplication = (Button)exitView.FindName("Exit");
        exitApplication.Click += Exit;

        var multiPlayerToHomeButton = (Button)multiplayerView.FindName("Home");
        multiPlayerToHomeButton.Click += ShowHomeClick;

        var multiPlayerPvPButton = (Button)multiplayerView.FindName("PvPBtn");
        multiPlayerPvPButton.Click += ShowPvPClick;
        
        var multiPlayerPvPLanButton = (Button)multiplayerView.FindName("PvPLANBtn");
        multiPlayerPvPLanButton.Click += ShowPvPLanClick;

        var mpPanelViewHomeBtn = (Button)mpPanelView.FindName("Home");
        mpPanelViewHomeBtn.Click += ShowHomeClick;

        var mpPanelStartBtn = (Button)mpPanelView.FindName("btnStartGame");
        mpPanelStartBtn.Click += ShowOnlineModeClick;
    }
    #region Handling the UI
    private void ShowGameClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        gameView.Visibility = Visibility.Visible;
        gameView.NotationPanel.Visibility = Visibility.Collapsed;
        gameView.AIGame = true;
        gameView.pvpLocal = false;
        gameView.pvpLAN = false;
        gameView.GMAI = true;
        gameView.selectedGM = "Magnus Carlsen";
        gameView.Initialize();
    }
    public void ShowOnlineModeClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        gameView.Visibility = Visibility.Visible;
        gameView.AIGame = false;
        gameView.pvpLocal = false;
        gameView.pvpLAN = true;
        gameView.GMAI = false;
        gameView.selectedGM = "";
        gameView.NotationPanel.Visibility = Visibility.Collapsed;
        gameView.Initialize();
    }
    private void ShowSettingsClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        settingsView.Visibility = Visibility.Visible;
    }

    private void ShowCreditsClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        creditsView.Visibility = Visibility.Visible;
    }
    private void ShowMultiPlayerClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        multiplayerView.Visibility = Visibility.Visible;
        gameView.NotationPanel.Visibility = Visibility.Collapsed;
    }
    public void ShowHomeClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Visible;
        creditsView.Visibility = Visibility.Collapsed;
        gameView.Visibility = Visibility.Collapsed;
        settingsView.Visibility = Visibility.Collapsed;
        exitView.Visibility = Visibility.Collapsed;
        mpPanelView.Visibility = Visibility.Collapsed;
        multiplayerView.Visibility = Visibility.Collapsed;
        gameView.ResetGame();
        gameView.ForfeitGame(true);
        if (GetBoardView().GetNotationManager() != null)
        {
            GetBoardView().GetNotationManager().ClearNotations();
        }
        gameView.NotationPanel.Visibility = Visibility.Collapsed;
        gameView.Board.cancellationTokenSource.Cancel();
        var board = GetBoardView();
        board.whiteTime = 300; 
        board.blackTime = 300;
        board.timerStarted = false; 
        if (board.timer != null)
        {
            board.timer.Stop(); 
        }

        if (gameView.play_forfeit != null)
        {
            gameView.play_forfeit.Content = "Play";
            gameView.play_forfeit.Style = (Style)gameView.Resources["BlueButtonStyle"];
            gameView.canForfeit = false;
        }
    }

    private void ShowExitClick(object sender, RoutedEventArgs e)
    {
        homeView.Visibility = Visibility.Collapsed;
        exitView.Visibility = Visibility.Visible;
    }
    private void ShowPvPClick(object sender, RoutedEventArgs e)
    {
        multiplayerView.Visibility = Visibility.Collapsed;
        gameView.Visibility = Visibility.Visible;
        gameView.NerdViewButton.Visibility = Visibility.Collapsed;
        gameView.AIGame = false;
        gameView.pvpLocal = true;
        gameView.pvpLAN = false;
        gameView.GMAI = false;
        gameView.selectedGM = "";
        gameView.NotationPanel.Visibility = Visibility.Collapsed;
        gameView.Initialize();
    }
    private void ShowPvPLanClick(object sender, RoutedEventArgs e)
    {
        multiplayerView.Visibility = Visibility.Collapsed;
        mpPanelView.Visibility = Visibility.Visible;
        gameView.NotationPanel.Visibility = Visibility.Collapsed;

        gameView.Initialize();
    }
    public BoardWindow GetBoardView()
    {
        return gameView?.FindName("Board") as BoardWindow;
    }
    private void Exit(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    private void InitializeBackgroundMusic()
    {
        // Ścieżka do pliku MP3 w katalogu wyjściowym
        string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BGmusic.mp3");
        
        // Otwórz plik MP3
        backgroundMusicPlayer.Open(new Uri(musicPath, UriKind.Absolute));

        // Ustaw głośność (opcjonalne, wartość od 0.0 do 1.0)
        backgroundMusicPlayer.Volume = 1;

        // Włącz zapętlanie przez obsługę zdarzenia MediaEnded
        backgroundMusicPlayer.MediaEnded += (sender, e) =>
        {
            backgroundMusicPlayer.Position = TimeSpan.Zero; // Wróć na początek
            backgroundMusicPlayer.Play(); // Odtwarzaj od nowa
        };

        // Rozpocznij odtwarzanie
        backgroundMusicPlayer.Play();
    }
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        backgroundMusicPlayer.Stop();
        base.OnClosing(e);
    }
    #endregion
}