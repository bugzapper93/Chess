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
using System.Windows.Threading;

namespace Chess;

public partial class MainWindow : Window
{
    private NotationPanelManager notationPanelManager;
    public bool isBoardFlipped = false;

    // Board variables
    private int SquareSize = Constants.Square_Size;
    private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
    private Rectangle[,] Squares = new Rectangle[8, 8];
    public Chessboard Board = new Chessboard();

    // Mouse piece movement
    private Position selectedPosition;
    private Point originalMouseOffset;
    private bool isDragging = false;
    private UIElement? selectedPiece;

    // AI variables
    public bool aiModeON = false;
    public bool onlineModeON = false;
    public int depth = 2;
    private ChessAI AI = new ChessAI(3);
    
    // LAN and player turn variables
    private ChessOnline _chessOnline;
    private int playerColor = Pieces.White;

    //Promotion variables
    private bool isPromotionPending = false;
    private Position? promotionPosition;
    private Move? lastMove;

    //Timer variables
    private bool isSlowGame;
    private TimeSpan timePlayerWhite;
    private TimeSpan timePlayerBlack;
    private TimeSpan elapsedWhite = TimeSpan.Zero;
    private TimeSpan elapsedBlack = TimeSpan.Zero;
    private Stopwatch turnClock = new Stopwatch();
    private DispatcherTimer timer;
    private bool isWhiteLast;

    public MainWindow()
    {
        isWhiteLast = true;
        InitializeComponent();
        notationPanelManager = new NotationPanelManager(NotationGrid);
        _chessOnline = new ChessOnline(this);
        DrawChessboard();
        PlacePieces();
        CheckVisibility();
        SetupTimer();
    }

    #region Initialization

    private void SetupTimer()
    {
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100);
        timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs? e)
    {
        if (Board.isWhiteTurn != isWhiteLast)
        {
            var elapsed = turnClock.Elapsed;

            if (isWhiteLast)
                elapsedWhite += elapsed;
            else             
                elapsedBlack += elapsed;

            isWhiteLast = Board.isWhiteTurn;
            turnClock.Restart();
        }

        var currentElapsed = turnClock.Elapsed;

        if (Board.isWhiteTurn)
        {
            var remaining = timePlayerWhite - elapsedWhite - currentElapsed;

            if (remaining <= TimeSpan.Zero)
            {
                WhiteTimerText.Text = "00:00";
                timer.Stop();
                MessageBox.Show("Białe przekroczyły czas!");
                return;
            }

            WhiteTimerText.Text = remaining.ToString(@"mm\:ss");
        }
        else
        {
            var remaining = timePlayerBlack - elapsedBlack - currentElapsed;

            if (remaining <= TimeSpan.Zero)
            {
                BlackTimerText.Text = "00:00";
                timer.Stop();
                MessageBox.Show("Czarne przekroczyły czas!");
                return;
            }

            BlackTimerText.Text = remaining.ToString(@"mm\:ss");
        }
    }


    private void DrawChessboard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Rectangle square = new Rectangle
                {
                    Width = SquareSize,
                    Height = SquareSize
                };
                int displayRow = isBoardFlipped ? 7 - row : row;
                int displayCol = isBoardFlipped ? 7 - col : col;
                Canvas.SetLeft(square, displayCol * SquareSize);
                Canvas.SetTop(square, displayRow * SquareSize + 5);
                Squares[row, col] = square;
                display.Children.Add(square);
            }
        }
        ResetBoard();
    }

    private void PlacePieces()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (Board.pieces[row, col].value != 0)
                {
                    Rectangle piece = Helpers.CreatePiece(Board.pieces[row, col].value);

                    int displayRow = isBoardFlipped ? 7 - row : row;
                    int displayCol = isBoardFlipped ? 7 - col : col;

                    double pieceLeft = displayCol * SquareSize + (SquareSize - piece.Width) / 2;
                    double pieceTop = (displayRow * SquareSize + (SquareSize - piece.Height) / 2) + 5;

                    Canvas.SetLeft(piece, pieceLeft);
                    Canvas.SetTop(piece, pieceTop);

                    piece.MouseDown += PieceMouseDown;
                    piece.MouseMove += PieceMouseMove;
                    piece.MouseUp += PieceMouseUp;

                    PiecesDisplay[row, col] = piece;
                    display.Children.Add(piece);
                }
            }
        }
    }
    #endregion
    #region Mouse handlers
    private void PieceMouseDown(object sender, MouseEventArgs e)
    {
        if (sender is Rectangle rect)
        {
            Rectangle piece = (Rectangle)sender;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (PiecesDisplay[i, j] == piece)
                    {
                        int pieceValue = Board.pieces[i, j].value;
                        int pieceColor = pieceValue >= 16 ? Pieces.Black : Pieces.White;
                        int currentColorTurn = Board.isWhiteTurn ? Pieces.White : Pieces.Black;
                        if (onlineModeON && (pieceColor != playerColor || playerColor != currentColorTurn))
                        {
                            return; // Only allow selecting own pieces on player's turn in online mode
                        }
                        selectedPiece = piece;
                        selectedPosition = new Position(i, j);
                        isDragging = true;

                        Point mousePosition = e.GetPosition(piece);
                        originalMouseOffset = new Point(mousePosition.X, mousePosition.Y);
                        HighlightBoard();
                        piece.CaptureMouse();
                    }
        }
    }
    private void PieceMouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
            if (parentCanvas == null)
                return;

            Point mousePosition = e.GetPosition(parentCanvas);
            Canvas.SetLeft(selectedPiece, mousePosition.X - originalMouseOffset.X);
            Canvas.SetTop(selectedPiece, mousePosition.Y - originalMouseOffset.Y);
        }
    }
    private async void PieceMouseUp(object sender, MouseEventArgs e)
    {
        if (isDragging && selectedPiece != null)
        {
            isDragging = false;
            var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;
            selectedPiece.ReleaseMouseCapture();
            Point mousePosition = e.GetPosition(display);
            int row = (int)(mousePosition.Y / SquareSize);
            int col = (int)(mousePosition.X / SquareSize);
            if (isBoardFlipped)
            {
                row = 7 - row;
                col = 7 - col;
            }
            Position targetPosition = new Position(row, col); // Define targetPosition here
            if (MovePiece(selectedPosition, targetPosition, playerColor))
            {
                if (!isPromotionPending)
                {
                    if (!aiModeON && !onlineModeON)
                    {
                        if (Board.isWhiteTurn == isBoardFlipped)
                        {
                            FlipBoard();
                            playerColor = playerColor == Pieces.Black ? Pieces.White : Pieces.Black;
                        }
                    }
                    else if (aiModeON)
                    {
                        int color = Board.isWhiteTurn ? Pieces.White : Pieces.Black;
                        Move bestMove = await AI.GetBestMove(Board, color);
                        MovePiece(bestMove.startPosition, bestMove.targetPosition, Pieces.GetOppositeColor(playerColor));
                    }
                    else if (onlineModeON)
                    {
                        await _chessOnline.SendMoveAsync(selectedPosition, targetPosition, playerColor); // Use targetPosition here
                        Trace.WriteLine($"[PieceMouseUp] Sending move from {selectedPosition.row},{selectedPosition.column} " +
                    $"to {targetPosition.row},{targetPosition.column}");
                    }
                }
            }
        }
    }
    #endregion
    #region BoardInteraction
    public bool MovePiece(Position positionStart, Position positionEnd, int playerColor)
    {
        int pieceValue = Board.pieces[positionStart.row, positionStart.column].value;
        int startRow = positionStart.row;
        int startCol = positionStart.column;
        int endRow = positionEnd.row;
        int endCol = positionEnd.column;

        int currentColorTurn = Board.isWhiteTurn ? Pieces.White : Pieces.Black;

        UIElement selectedPiece = PiecesDisplay[positionStart.row, positionStart.column];
        if (selectedPiece == null)
            return false;

        var parentCanvas = VisualTreeHelper.GetParent(selectedPiece) as Canvas;

        if (parentCanvas == null)
            return false;

        if (playerColor != currentColorTurn)
        {
            ResetPiecePosition(positionStart, selectedPiece);
            return false;
        }

        int index = Helpers.GetMoveIndex(Board.moveset.moves, positionStart, positionEnd);
        if (index == -1)
        {
            ResetPiecePosition(positionStart, selectedPiece);

            //int displayStartRow = isBoardFlipped ? 7 - positionStart.row : positionStart.row;
            //int displayStartCol = isBoardFlipped ? 7 - positionStart.column : positionStart.column;

            //double pieceLeft = displayStartCol * SquareSize + (SquareSize - selectedPiece.RenderSize.Width) / 2;
            //double pieceTop = (displayStartRow * SquareSize + (SquareSize - selectedPiece.RenderSize.Height) / 2) + 5;

            //Canvas.SetLeft(selectedPiece, pieceLeft);
            //Canvas.SetTop(selectedPiece, pieceTop);
            return false;
        }

        if (PiecesDisplay[positionEnd.row, positionEnd.column] != null && PiecesDisplay[positionEnd.row, positionEnd.column] != selectedPiece)
        {
            parentCanvas.Children.Remove(PiecesDisplay[positionEnd.row, positionEnd.column]);
            PiecesDisplay[positionEnd.row, positionEnd.column] = null;
        }

        int displayEndRow = isBoardFlipped ? 7 - positionEnd.row : positionEnd.row;
        int displayEndCol = isBoardFlipped ? 7 - positionEnd.column : positionEnd.column;

        Canvas.SetLeft(selectedPiece, displayEndCol * SquareSize);
        Canvas.SetTop(selectedPiece, displayEndRow * SquareSize + 5);


        PiecesDisplay[positionEnd.row, positionEnd.column] = (Rectangle)selectedPiece;
        PiecesDisplay[positionStart.row, positionStart.column] = null;

        Move move = Board.moveset.moves[index];
        lastMove = move;
        bool enPassant = false;
        if ((pieceValue & 7) == Pieces.Pawn && move.capture)
        {
            // En passant
            if (Board.pieces[endRow, endCol].value == 0)
            {
                parentCanvas.Children.Remove(PiecesDisplay[positionStart.row, positionEnd.column]);
                PiecesDisplay[positionStart.row, positionEnd.column] = null;
                enPassant = true;
            }
        }
        if ((pieceValue & 7) == Pieces.King && Math.Abs(move.startPosition.row - move.targetPosition.row) == 2)
        {
            int rookCol = move.targetPosition.column == 2 ? 0 : 7;
            int rookTargetCol = move.targetPosition.column == 2 ? 3 : 5;
            Rectangle rook = PiecesDisplay[startRow, rookCol];
            Canvas.SetLeft(rook, rookTargetCol * SquareSize);
            Canvas.SetTop(rook, startRow * SquareSize + 5);
            PiecesDisplay[startRow, rookTargetCol] = rook;
            PiecesDisplay[startRow, rookCol] = null;
        }
        PromotionChess(positionEnd, pieceValue);
        Board.MakeMove(move);

        string moveNotation = notationPanelManager.GetAlgebraicNotation(move, Board, pieceValue, enPassant);
       notationPanelManager.AddRowToTable(move, moveNotation, Board.isWhiteTurn, Board);
        ResetBoard();

        return true;
    }
    private void ResetPiecePosition(Position position, UIElement piece)
    {
        int displayStartRow = isBoardFlipped ? 7 - position.row : position.row;
        int displayStartCol = isBoardFlipped ? 7 - position.column : position.column;

        double pieceLeft = displayStartCol * SquareSize + (SquareSize - piece.RenderSize.Width) / 2;
        double pieceTop = (displayStartRow * SquareSize + (SquareSize - piece.RenderSize.Height) / 2) + 5;

        Canvas.SetLeft(piece, pieceLeft);
        Canvas.SetTop(piece, pieceTop);
    }
    private void HighlightBoard()
    {
        ResetBoard();
        int row = selectedPosition.row;
        int col = selectedPosition.column;
        List<Move> moves = Board.moveset.moves;
        foreach (Move move in moves)
        {
            if (move.startPosition == selectedPosition)
            {
                Rectangle square = Squares[move.targetPosition.row, move.targetPosition.column];
                square.Fill = Brushes.Red;
            }
        }
    }
    public void FlipBoard()
    {
        isBoardFlipped = !isBoardFlipped;
        display.Children.Clear();
        DrawChessboard();
        PlacePieces();
    }
    private void ResetBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Squares[row, col].Fill = ((row + col) % 2 == 0) ? Constants.Primary : Constants.Secondary;
            }
        }
    }
    #endregion
    #region UIManagement
    private void Hide(object sender, RoutedEventArgs e)
    {
        CheckVisibility();
    }
    private void startBTN_Click(object sender, RoutedEventArgs e)
    {
        ChooseModeMenu();
    }
    private void CheckVisibility()
    {
        promotionMenu.Visibility = Visibility.Hidden;
        if (display.Visibility == Visibility.Visible)
        {
            display.Visibility = Visibility.Hidden;
            notationType.Visibility = Visibility.Hidden;
            timers.Visibility = Visibility.Hidden;
            NotationGridScrollView.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;
            HideBtn.Visibility = Visibility.Hidden;
            ModeMenu.Visibility = Visibility.Hidden;
            AuthorsMenu.Visibility = Visibility.Hidden;
            SettingsMenu.Visibility = Visibility.Hidden;
        }
        else
        {
            if (ModeMenu.Visibility == Visibility.Visible
                || AuthorsMenu.Visibility == Visibility.Visible
                || SettingsMenu.Visibility == Visibility.Visible)
            {
                MainMenu.Visibility = Visibility.Visible;
                HideBtn.Visibility = Visibility.Hidden;
                ModeMenu.Visibility = Visibility.Hidden;
                AuthorsMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                display.Visibility = Visibility.Visible;
                notationType.Visibility = Visibility.Visible;
                timers.Visibility = Visibility.Visible;
                NotationGridScrollView.Visibility = Visibility.Visible;
                MainMenu.Visibility = Visibility.Hidden;
                HideBtn.Visibility = Visibility.Visible;
                ModeMenu.Visibility = Visibility.Hidden;
                AuthorsMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Visibility = Visibility.Hidden;
            }
        }
    }
    public void ShowBoard()
    {
        display.Visibility = Visibility.Visible;
        notationType.Visibility = Visibility.Visible;
        timers.Visibility = Visibility.Visible;
        HideBtn.Visibility = Visibility.Visible;
        NotationGridScrollView.Visibility = Visibility.Visible;
        ModeMenu.Visibility = Visibility.Hidden;
        startTimer();
        if (onlineModeON)
        {
            isBoardFlipped = (playerColor == Pieces.Black); // Black sees board flipped (black at bottom)
        }
        else
        {
            isBoardFlipped = false;
        }
        ResetGame();
    }
    private void ShowAuthorsPanel()
    {
        AuthorsMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
    }
    private void ShowSettingsPanel()
    {
        SettingsMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
    }

    private void ShowServerPanel()
    {
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Hidden;
        _chessOnline.ShowServerPanel();
    }
    private void PromotionChess(Position targetPos, int pieceValue)
    {
        int pieceType = Pieces.GetPieceValue(pieceValue) & 7;
        if ((targetPos.row == 0 || targetPos.row == 7) && pieceType == Pieces.Pawn)
        {
            promotionPosition = targetPos;
            promotionMenu.Visibility = Visibility.Visible;
            isPromotionPending = true;
        }
    }
    private void ChangeImageForPiece(Position pos, int newPieceValue)
    {
        if (PiecesDisplay[pos.row, pos.column] != null)
        {
            display.Children.Remove(PiecesDisplay[pos.row, pos.column]);
            PiecesDisplay[pos.row, pos.column] = null;
        }

        Rectangle newPiece = Helpers.CreatePiece(newPieceValue);

        int displayRow = isBoardFlipped ? 7 - pos.row : pos.row;
        int displayCol = isBoardFlipped ? 7 - pos.column : pos.column;

        double pieceLeft = displayCol * SquareSize + (SquareSize - newPiece.Width) / 2;
        double pieceTop = (displayRow * SquareSize + (SquareSize - newPiece.Height) / 2) + 5;

        Canvas.SetLeft(newPiece, pieceLeft);
        Canvas.SetTop(newPiece, pieceTop);

        newPiece.MouseDown += PieceMouseDown;
        newPiece.MouseMove += PieceMouseMove;
        newPiece.MouseUp += PieceMouseUp;

        PiecesDisplay[pos.row, pos.column] = newPiece;
        display.Children.Add(newPiece);
    }
    private void ResetGame()
    {
       // isBoardFlipped = false;
        Board = new Chessboard();
        display.Children.Clear();
        DrawChessboard();
        PlacePieces();
        notationPanelManager.ClearNotations();
        Board.isWhiteTurn = true;
    }
    private void ChooseModeMenu()
    {
        ModeMenu.Visibility = Visibility.Visible;
        MainMenu.Visibility = Visibility.Hidden;
        HideBtn.Visibility = Visibility.Visible;
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

    private async void btnDolacz_Click(object sender, RoutedEventArgs e)
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

    private async void btnWyjdz_Click(object sender, RoutedEventArgs e)
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

    private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            btnSendMessage_Click(sender, e);
            e.Handled = true;
        }
    }

    private async void btnClearChat_Click(object sender, RoutedEventArgs e)
    {
        await AnimateAndRemoveItems(lstChatMessages);
    }

    private void btnStartGame_Click(object sender, RoutedEventArgs e)
    {
        if (_chessOnline._networkManager.IsConnected || _chessOnline._networkManager.IsHosting)
        {
            ShowBoard();

            ServerPanel.Visibility = Visibility.Hidden;
        }
        else
        {
            MessageBox.Show("You must be connected to a lobby to start the game.");
        }
    }

    private void startTimer()
    {
        if (isSlowGame)
        {
            timePlayerWhite = TimeSpan.FromMinutes(15);
            timePlayerBlack = TimeSpan.FromMinutes(15);
        }
        else
        {
            timePlayerWhite = TimeSpan.FromMinutes(5);
            timePlayerBlack = TimeSpan.FromMinutes(5);
        }

        elapsedWhite = TimeSpan.Zero;
        elapsedBlack = TimeSpan.Zero;

        turnClock.Restart();

        WhiteTimerText.Text = timePlayerWhite.ToString(@"mm\:ss");
        BlackTimerText.Text = timePlayerBlack.ToString(@"mm\:ss");

        timer.Start();
    }

    private static async Task AnimateAndRemoveItems(ListBox listBox)
    {
        if (listBox.Items.Count == 0) return;

        var itemsToRemove = listBox.Items.Cast<object>().ToList();
        var tcs = new TaskCompletionSource<bool>();
        int animationsPending = itemsToRemove.Count;

        if (animationsPending == 0)
        {
            tcs.SetResult(true);
        }
        else
        {
            foreach (var item in itemsToRemove)
            {
                var listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem != null)
                {
                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.1)
                    };
                    fadeOut.Completed += (s, e) =>
                    {
                        listBox.Items.Remove(item);
                        if (System.Threading.Interlocked.Decrement(ref animationsPending) == 0)
                        {
                            tcs.SetResult(true);
                        }
                    };
                    listBoxItem.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    listBox.Items.Remove(item);
                    if (System.Threading.Interlocked.Decrement(ref animationsPending) == 0)
                    {
                        tcs.SetResult(true);
                    }
                }
            }
        }

        await tcs.Task;
    }

    private void SlowGameCheck_Checked(object sender, RoutedEventArgs e)
    {
        isSlowGame = true;
    }

    private void SpeedGameCheck_Checked(object sender, RoutedEventArgs e)
    {

        isSlowGame = false;
    }

    private void PvPBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = false;
        onlineModeON = false;
        ShowBoard();
    }

    private void PvCBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = true;
        onlineModeON = false;
        ShowBoard();
    }

    private void PvPLANBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = false;
        onlineModeON = true;
        ShowServerPanel();
        //ShowBoard();
    }

    private void PvCGMBtn_Click(object sender, RoutedEventArgs e)
    {
        aiModeON = true;
        onlineModeON = false;
        ShowBoard();
    }

    private void settingsBtn_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsPanel();
    }

    private void authorsBtn_Click(object sender, RoutedEventArgs e)
    {
        ShowAuthorsPanel();
    }

    private void exitBtn_Click(object sender, RoutedEventArgs e)
    {
        App.Current.Shutdown();
    }

    private void easyAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 1;
        AI = new ChessAI(depth);
    }

    private void mediumAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 3;
        AI = new ChessAI(depth);
    }

    private void hardAiDiffRB_Checked(object sender, RoutedEventArgs e)
    {
        depth = 5;
        AI = new ChessAI(depth);
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _chessOnline.Dispose(); // Clean up P2P resources
    }
    private async void RadioButtons_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && isPromotionPending && promotionPosition.HasValue && lastMove.HasValue)
        {
            string pieceName = rb.Name.Replace("Choose", "");
            int newPieceType = 0;
            switch (pieceName)
            {
                case "Knight":
                    newPieceType = Pieces.Knight;
                    break;
                case "Rook":
                    newPieceType = Pieces.Rook;
                    break;
                case "Bishop":
                    newPieceType = Pieces.Bishop;
                    break;
                case "Queen":
                    newPieceType = Pieces.Queen;
                    break;
                default:
                    return;
            }

            int pieceColor = Board.isWhiteTurn ? Pieces.Black : Pieces.White;
            int newPieceValue = newPieceType | pieceColor;

            Board.pieces[promotionPosition.Value.row, promotionPosition.Value.column].value = newPieceValue;
            ChangeImageForPiece(promotionPosition.Value, newPieceValue);

            int originalPieceValue = Pieces.Pawn | pieceColor;
            string moveNotation = notationPanelManager.GetAlgebraicNotation(lastMove.Value, Board, originalPieceValue, false);
            notationPanelManager.AddRowToTable(lastMove.Value ,moveNotation, !Board.isWhiteTurn, Board);

            promotionMenu.Visibility = Visibility.Hidden;
            isPromotionPending = false;


            if (!aiModeON && !onlineModeON)
            {
                if (Board.isWhiteTurn == isBoardFlipped)
                {
                    FlipBoard();
                    playerColor = playerColor == Pieces.Black ? Pieces.White : Pieces.Black;
                }
            }
            else if (aiModeON)
            {
                int color = Board.isWhiteTurn ? Pieces.White : Pieces.Black;
                Move bestMove = await AI.GetBestMove(Board, color);
                MovePiece(bestMove.startPosition, bestMove.targetPosition, Pieces.GetOppositeColor(playerColor));
            }
        }
    }
    private void notationType_Checked(object sender, RoutedEventArgs e)
    {
        if (notationType.IsChecked == true)
        {
            notationPanelManager.SetNotationType(true, Board); 
        }
    }
    private void notationType_Unchecked(object sender, RoutedEventArgs e)
    {
        if (notationType.IsChecked == false)
        {
            notationPanelManager.SetNotationType(false, Board);
        }
    }
    #endregion

}