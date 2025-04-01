using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
using Chess.Objects;
using Chess.Tools;
using static Chess.Objects.ChessOnline;

namespace Chess.View
{
    public partial class BoardWindow : UserControl
    {
        private DispatcherTimer timer;

        public bool timerStarted = false;
        public int whiteTime = 300;
        public int blackTime = 300;

        public bool isBoardFlipped = false;

        // Board variables
        private Rectangle[,] PiecesDisplay = new Rectangle[8, 8];
        private Rectangle[,] Squares = new Rectangle[8, 8];
        private bool promotionIsWhite;
        public Chessboard board = new Chessboard();
        private int promotionSquare = -1;
        private Point originalMouseOffset;
        private bool isDragging = false;
        private UIElement? selectedPiece;
        int selectedSquare;
        public int playerColor;

        private bool enableAI;
        private bool pvpLAN;
        private bool pvpLocal;
        ChessAI bot;

        public List<string> PrevMoveCache = new List<string>();
        public List<(int WhiteTime, int BlackTime)> TimeCache = new List<(int WhiteTime, int BlackTime)>(); 
        public List<MoveData> movesMade = new List<MoveData>();

        List<int> possibleMoves = new List<int>();

        public bool started = false;
        public bool whiteTurn = true;
        private NotationPanelManager notationManager;
        public CancellationTokenSource cancellationTokenSource;
        public BoardWindow()
        {
            InitializeComponent();
            InitializeBoardView();
            CheckmateBackToMenu.Click += BackToMenu_Click;
            StalemateBackToMenu.Click += BackToMenu_Click;
        }
        public void InitializeBoardView()
        {
            board = new Chessboard();
            bot = new ChessAI(0, 0);
            whiteTime = 300;
            blackTime = 300;
            DrawChessboard();
            PlacePieces();
        }
        private ChessOnline _chessOnline; 

        public void SetChessOnline(ChessOnline chessOnline)
        {
            _chessOnline = chessOnline;
        }
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
           
        }
        public event Action<int> GameTimeout;
        private void TimerTick(object sender, EventArgs e)
        {
            if (!timerStarted)
                return;

            if (board.isWhiteTurn)
            {
                whiteTime--;
                if (whiteTime <= 0)
                {
                    timer.Stop();
                    GameTimeout?.Invoke(Pieces.White);
                    return;
                }
            }
            else
            {
                blackTime--;
                if (blackTime <= 0)
                {
                    timer.Stop();
                    GameTimeout?.Invoke(Pieces.Black);
                    return;
                }
            }
            TimerUpdate?.Invoke(whiteTime, blackTime);
        }
        public event Action<int, int>? TimerUpdate;
        public void InitializeGame(int playerColor, bool AI, bool LAN, bool pvp, int depth = 0, bool grandmaster = false, string grandmasterName = "", NotationPanelManager notationManager = null)
        {
            InitializeBoardView();
            if (timer == null)
                InitializeTimer();

            timer.Start();
            this.notationManager = notationManager; 
            timerStarted = true;
            board.UpdateMoves();

            enableAI = AI;
            pvpLAN = LAN;
            pvpLocal = pvp;

            if (AI)
            {
                bot = new ChessAI(depth, playerColor, grandmaster, grandmasterName);
            }
            if (playerColor == Pieces.White)
            {
                started = true;
                this.playerColor = Pieces.White;
            }
            else
            {
                started = true;
                this.playerColor = Pieces.Black;
                if (AI)
                {
                    MakeAIMove();
                }
            }

            notationManager?.ClearNotations(); 
        }
        public async void GetTip()
        {
            Move bestMove = new Move();
            if (!enableAI)
            {
                ChessAI ai = new ChessAI(3, playerColor);
                
                bestMove = await ai.GetBestMove(board, playerColor, cancellationTokenSource.Token);
            }
            else
            {
                bestMove = await bot.GetBestMove(board, playerColor, cancellationTokenSource.Token);
            }
            HighlightBoard(bestMove.To);
        }
        public void ReverseMove()
        {
            if (board.CurrentMoves.Length > 0 && movesMade.Count > 0)
            {
                int lastMoveIndex = movesMade.Count - 1;

                // Canceling the AI move
                cancellationTokenSource.Cancel();
                
                
                whiteTime = TimeCache[lastMoveIndex].WhiteTime;
                blackTime = TimeCache[lastMoveIndex].BlackTime;
                board.CurrentMoves = PrevMoveCache[lastMoveIndex];
                board.UnmakeMove(movesMade[lastMoveIndex], true);
                board.UpdateMoves();
                whiteTurn = board.isWhiteTurn;

                movesMade.RemoveAt(lastMoveIndex);
                TimeCache.RemoveAt(lastMoveIndex);
                PrevMoveCache.RemoveAt(lastMoveIndex);

                // Redrawing the board
                display.Children.Clear();
                DrawChessboard();
                PlacePieces();
                cancellationTokenSource = new CancellationTokenSource();
                if (enableAI)
                {

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
        private async void MakeAIMove()
        {
            try
            {
                int botColor = playerColor == Pieces.White ? Pieces.Black : Pieces.White;
                Move move = await bot.GetBestMove(board.Clone(), botColor, cancellationTokenSource.Token);
                if (cancellationTokenSource.IsCancellationRequested)
                    return;
                MovePiece(move, false);
            }
            catch (OperationCanceledException)
            {

            }
        }
        private void DrawChessboard()
        {
            for (int square = 0; square < 64; square++)
            {
                Rectangle backgroundPanel = new Rectangle
                {
                    Width = Constants.SquareSize,
                    Height = Constants.SquareSize
                };

                int row = square / 8;
                int col = square % 8;

                int displayRow = isBoardFlipped ? 7 - row : row;
                int displayCol = isBoardFlipped ? 7 - col : col;

                Canvas.SetLeft(backgroundPanel, displayCol * Constants.SquareSize);
                Canvas.SetBottom(backgroundPanel, displayRow * Constants.SquareSize);

                Squares[row, col] = backgroundPanel;
                display.Children.Add(backgroundPanel);
            }
            ResetBoardView();
        }
        private void PlacePieces()
        {
            char[] pieces = Helpers.GetPieceArray(board);

            for (int square = 0; square < 64; square++)
            {
                if (pieces[square] == default(char))
                    continue;

                Rectangle piece = Helpers.GeneratePiece(pieces[square]);

                int row = square / 8;
                int col = square % 8;

                int displayRow = isBoardFlipped ? 7 - row : row;
                int displayCol = isBoardFlipped ? 7 - col : col;

                double pieceLeft = displayCol * Constants.SquareSize + (Constants.SquareSize - piece.Width) / 2;
                double pieceBottom = displayRow * Constants.SquareSize + (Constants.SquareSize - piece.Height) / 2;

                Canvas.SetLeft(piece, pieceLeft);
                Canvas.SetBottom(piece, pieceBottom);

                piece.MouseDown += PieceMouseDown;
                piece.MouseMove += PieceMouseMove;
                piece.MouseUp += PieceMouseUp;

                PiecesDisplay[row, col] = piece;
                display.Children.Add(piece);
            }
        }
        private void RepositionPiece(Move move, bool validMove = true)
        {
            int selectedRow = move.From / 8;
            int selectedColumn = move.From % 8;

            int targetRow = move.To / 8;
            int targetColumn = move.To % 8;

            UIElement selectedPiece = PiecesDisplay[selectedRow, selectedColumn];

            if (selectedPiece == null)
                return;

            if (!validMove)
            {
                double tempPosLeft = (isBoardFlipped ? (7 - selectedColumn) : selectedColumn) * Constants.SquareSize +
                 (Constants.SquareSize - ((Rectangle)selectedPiece).Width) / 2;

                double tempPosBottom = (isBoardFlipped ? (7 - selectedRow) : selectedRow) * Constants.SquareSize +
                                   (Constants.SquareSize - ((Rectangle)selectedPiece).Height) / 2;

                Canvas.SetLeft(selectedPiece, tempPosLeft);
                Canvas.SetBottom(selectedPiece, tempPosBottom);
                return;
            }

            double newLeft = (isBoardFlipped ? (7 - targetColumn) : targetColumn) * Constants.SquareSize +
                 (Constants.SquareSize - ((Rectangle)selectedPiece).Width) / 2;

            double newBottom = (isBoardFlipped ? (7 - targetRow) : targetRow) * Constants.SquareSize +
                               (Constants.SquareSize - ((Rectangle)selectedPiece).Height) / 2;

            Canvas.SetLeft(selectedPiece, newLeft);
            Canvas.SetBottom(selectedPiece, newBottom);

            if (PiecesDisplay[targetRow, targetColumn] != null && PiecesDisplay[targetRow, targetColumn] != selectedPiece)
            {
                RemovePiece(move.To);
            }

            PiecesDisplay[targetRow, targetColumn] = (Rectangle)selectedPiece;
            PiecesDisplay[selectedRow, selectedColumn] = null;
        }
        public NotationPanelManager GetNotationManager()
        {
            return notationManager;
        }
        private void RemovePiece(int square)
        {
            int targetRow = square / 8;
            int targetColumn = square % 8;

            if (PiecesDisplay[targetRow, targetColumn] == null)
                return;
            UIElement targetPiece = PiecesDisplay[targetRow, targetColumn];
            var parentCanvas = VisualTreeHelper.GetParent(targetPiece) as Canvas;
            if (parentCanvas == null)
                return;

            parentCanvas.Children.Remove(targetPiece);
            PiecesDisplay[targetRow, targetColumn] = null;
        }
        public bool MovePiece(Move move, bool checkIfValid = true)
        {
            int selectedRow = move.From / 8;
            int selectedColumn = move.From % 8;

            int targetRow = move.To / 8;
            int targetColumn = move.To % 8;

            int startSquare = move.From;
            int endSquare = move.To;

            ulong toMask = 1UL << endSquare;
            ulong fromMask = 1UL << startSquare;

            if (!possibleMoves.Contains(endSquare) && checkIfValid)
            {
                RepositionPiece(new Move(startSquare, startSquare), false);
                return false;
            }

            if (board.isWhiteTurn)
            {
                if (board.EnPassantSquare != null && (board.WhitePawns & fromMask) != 0 && ((1UL << board.EnPassantSquare) & toMask) != 0)
                {
                    RemovePiece(endSquare - 8);
                }
            }
            else
            {
                if (board.EnPassantSquare != null && (board.BlackPawns & fromMask) != 0 && ((1UL << board.EnPassantSquare) & toMask) != 0)
                {
                    RemovePiece(endSquare + 8);
                }
            }

            ulong kingMask = board.isWhiteTurn ? board.WhiteKing : board.BlackKing;
            if ((kingMask & fromMask) != 0 && Math.Abs(move.From - move.To) == 2)
            {
                int rightRookSquare = board.isWhiteTurn ? 7 : 63;
                int leftRookSquare = board.isWhiteTurn ? 0 : 56;

                if (move.To - move.From == 2)
                {
                    RepositionPiece(new Move(rightRookSquare, move.To - 1));
                }
                else if (move.To - move.From == -2)
                {
                    RepositionPiece(new Move(leftRookSquare, move.To + 1));
                }
            }

            RepositionPiece(move);
            bool wasWhiteTurn = board.isWhiteTurn; // Zapisz turę przed wykonaniem ruchu
            MoveData moveData = board.MakeMove(move);

            movesMade.Add(moveData.Clone());
            TimeCache.Add((whiteTime, blackTime));

            whiteTurn = board.isWhiteTurn; // Zaktualizuj turę po ruchu
            string moveNotation = NotationPanelManager.GetAlgebraicNotation(moveData);
            board.CurrentMoves += board.CurrentMoves == "" ? $"{moveNotation}" : $",{moveNotation}";
            PrevMoveCache.Add((string)board.CurrentMoves.Clone());
            if (notationManager != null)
            {
                notationManager.AddRowToTable(moveData, whiteTurn);
            }

            if (Helpers.GetPiece(board, endSquare) == Pieces.Pawn && (targetRow == 0 || targetRow == 7))
            {
                promotionSquare = endSquare;
                promotionIsWhite = wasWhiteTurn;
                ShowPromotionPanel(wasWhiteTurn);
                return true;
            }

            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                    ShowCheckmatePanel();
                else
                    ShowStalematePanel();
            }
            return true;
        }
        private void HighlightBoard(int square = -1)
        {
            if (square != -1)
            {
                int row = square / 8;
                int column = square % 8;
                Squares[row, column].Fill = Brushes.Gold;
                return;
            }

            possibleMoves = Helpers.GetPieceMoves(board, selectedSquare, board.isWhiteTurn);
            foreach (int possibleMove in possibleMoves)
            {
                int row = possibleMove / 8;
                int column = possibleMove % 8;
                Squares[row, column].Fill = Brushes.Red;
            }
        }
        private void PieceMouseDown(object sender, MouseEventArgs e)
        {
            int currentColorTurn = board.isWhiteTurn ? Pieces.White : Pieces.Black;
            if ((currentColorTurn != playerColor && !pvpLocal) || !started)
                return;
            if (sender is Rectangle rect)
            {
                Rectangle piece = (Rectangle)sender;
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        if (PiecesDisplay[i, j] == piece)
                        {
                            selectedPiece = piece;
                            isDragging = true;
                            selectedSquare = i * 8 + j;
                            Point mousePosition = e.GetPosition(piece);
                            originalMouseOffset = new Point(mousePosition.X, mousePosition.Y);
                            piece.CaptureMouse();
                            HighlightBoard();
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
                double newLeft = mousePosition.X - originalMouseOffset.X;

                double canvasHeight = parentCanvas.ActualHeight;
                double newBottom = canvasHeight - mousePosition.Y - (selectedPiece.RenderSize.Height - originalMouseOffset.Y);

                Canvas.SetLeft(selectedPiece, newLeft);
                Canvas.SetBottom(selectedPiece, newBottom);
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

                int row = (int)((display.ActualHeight - mousePosition.Y) / Constants.SquareSize);
                int col = (int)(mousePosition.X / Constants.SquareSize);
                if (isBoardFlipped)
                {
                    row = 7 - row;
                    col = 7 - col;
                }
                int targetSquare = row * 8 + col;
                selectedPiece = null;
                isDragging = false;
                ResetBoardView();

                if (MovePiece(new Move(selectedSquare, targetSquare)) && enableAI)
                {
                    MakeAIMove();
                }
                else if (pvpLAN)
                {
                    await _chessOnline.SendMoveAsync(new Move(selectedSquare, targetSquare));
                }
                else
                {
                    if (board.isWhiteTurn == isBoardFlipped)
                    {
                        FlipBoard();
                    }
                }
            }
        }
        private void ResetBoardView()
        {
            for (int square = 0; square < 64; square++)
            {
                int row = square / 8;
                int column = square % 8;

                Squares[row, column].Fill = (((row + column) % 2) == 0) ? Constants.Primary : Constants.Secondary;
            }
        }
        private void ShowCheckmatePanel()
        {
            CheckmatePanel.Visibility = Visibility.Visible;
            StalematePanel.Visibility = Visibility.Collapsed;
        }

        private void ShowStalematePanel()
        {
            CheckmatePanel.Visibility = Visibility.Collapsed;
            StalematePanel.Visibility = Visibility.Visible;
        }
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowHomeClick(sender, e); 
            }
            CheckmatePanel.Visibility = Visibility.Collapsed;
            StalematePanel.Visibility = Visibility.Collapsed;
        }
        public string GetInformationForNerdAI()
        {
            if (!enableAI || bot == null)
            {
                return "AI not initialized yet.";
            }

            int aiColor = playerColor == Pieces.White ? Pieces.Black : Pieces.White;
            int evaluationScore = bot.EvaluateBoard(board, aiColor); // Zakładam, że ChessAI ma EvaluateBoard
            string aiColorStr = aiColor == Pieces.Black ? "Black" : "White";
            return $"Evaluation Score: {evaluationScore}\n" +
                   $"AI Color: {aiColorStr}\n" +
                   $"Check?: {Helpers.isKingInCheck(board, board.isWhiteTurn)}\n" +
                   $"Checkmate?: {board.isCheckMate(board)}\n" +
                   $"Stalemate?: {board.isStaleMate(board)}";
        }

        public string GetInformationForNerdsLAN()
        {
            if (!pvpLAN || _chessOnline == null)
            {
                return "LAN mode active, but no network info available.";
            }
            return $"Local IP Address: {P2PNetworkManager.GetLocalIPAddress()}\n" +
                   $"Host Nickname: {_chessOnline._networkManager._hostNickname}\n" +
                   $"Client Nickname: {_chessOnline._networkManager._clientNickname}";
        }
        private void PromoteToQueenButton_Click(object sender, RoutedEventArgs e)
        {
            PromotePieceUI(Pieces.Queen);
        }

        private void PromoteToRookButton_Click(object sender, RoutedEventArgs e)
        {
            PromotePieceUI(Pieces.Rook);
        }

        private void PromoteToBishopButton_Click(object sender, RoutedEventArgs e)
        {
            PromotePieceUI(Pieces.Bishop);
        }

        private void PromoteToKnightButton_Click(object sender, RoutedEventArgs e)
        {
            PromotePieceUI(Pieces.Knight);
        }
        private void PromotePieceUI(int pieceType)
        {
            if (promotionSquare == -1) return;

            bool isWhite = promotionIsWhite; 
            board.PromotePawn(promotionSquare, pieceType, isWhite);

            RemovePiece(promotionSquare);

            char pieceChar = GetPieceChar(pieceType, isWhite);
            Rectangle piece = Helpers.GeneratePiece(pieceChar);

            int row = promotionSquare / 8;
            int col = promotionSquare % 8;
            int displayRow = isBoardFlipped ? 7 - row : row;
            int displayCol = isBoardFlipped ? 7 - col : col;

            double pieceLeft = displayCol * Constants.SquareSize + (Constants.SquareSize - piece.Width) / 2;
            double pieceBottom = displayRow * Constants.SquareSize + (Constants.SquareSize - piece.Height) / 2;

            Canvas.SetLeft(piece, pieceLeft);
            Canvas.SetBottom(piece, pieceBottom);

            piece.MouseDown += PieceMouseDown;
            piece.MouseMove += PieceMouseMove;
            piece.MouseUp += PieceMouseUp;

            PiecesDisplay[row, col] = piece;
            display.Children.Add(piece);

            PromotionPanel.Visibility = Visibility.Collapsed;
            promotionSquare = -1;
            promotionIsWhite = false;

            whiteTurn = board.isWhiteTurn;

            if (Helpers.GetMoveCount(board) == 0)
            {
                if (Helpers.isKingInCheck(board, board.isWhiteTurn))
                    ShowCheckmatePanel();
                else
                    ShowStalematePanel();
            }

            if (enableAI)
            {
                MakeAIMove();
            }
        }
        private char GetPieceChar(int pieceType, bool isWhite)
        {
            switch (pieceType)
            {
                case Pieces.Queen: return isWhite ? 'Q' : 'q';
                case Pieces.Rook: return isWhite ? 'R' : 'r';
                case Pieces.Bishop: return isWhite ? 'B' : 'b';
                case Pieces.Knight: return isWhite ? 'N' : 'n';
                default: return isWhite ? 'Q' : 'q'; 
            }
        }
        public void ShowPromotionPanel(bool isWhiteTurn)
        {
            bool isWhite = promotionIsWhite;

            Assembly assembly = Assembly.GetExecutingAssembly();

            Image LoadImage(string resourceName)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine($"Resource not found: {resourceName}");
                        return new Image { Source = null };
                    }

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    return new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.Fill
                    };
                }
            }

            PromoteToQueenButton.Content = LoadImage($"Chess.Resources.Images.queen_{(isWhite ? "white" : "black")}.png");
            PromoteToRookButton.Content = LoadImage($"Chess.Resources.Images.rook_{(isWhite ? "white" : "black")}.png");
            PromoteToBishopButton.Content = LoadImage($"Chess.Resources.Images.bishop_{(isWhite ? "white" : "black")}.png");
            PromoteToKnightButton.Content = new Image
            {
                Source = ((Image)LoadImage($"Chess.Resources.Images.knight_{(isWhite ? "white" : "black")}.png")).Source,
                Stretch = Stretch.Fill,
                Width = 30,
                Height = 30
            };

            PromotionPanel.Visibility = Visibility.Visible;
        }
    }
}


