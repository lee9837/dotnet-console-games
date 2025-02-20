Exception? exception = null;

Encoding encoding = Console.OutputEncoding;

try
{
    Console.OutputEncoding = Encoding.UTF8;
    Game game = ShowIntroScreenAndGetOption(); // Call the function to display the game introduction and get the game mode selected by the player
    Console.Clear();
    RunGameLoop(game); // Run the main game loop
    RenderGameState(game, promptPressKey: true); // Render the final game state
    Console.ReadKey(true);
}
catch (Exception e)
{
    exception = e;
    throw;
}
finally
{
    Console.OutputEncoding = encoding;
    Console.CursorVisible = true;
    Console.Clear();
    Console.WriteLine(exception?.ToString() ?? "Checkers was closed.");
}

Game ShowIntroScreenAndGetOption()
{
    Console.Clear();
    Console.WriteLine(); // Display the game title and game rules introduction
    Console.WriteLine("  Checkers");
    Console.WriteLine();
    Console.WriteLine("  Checkers is played on an 8x8 board between two sides commonly known as black");
    Console.WriteLine("  and white. The objective is simple - capture all your opponent's pieces. An");
    Console.WriteLine("  alternative way to win is to trap your opponent so that they have no valid");
    Console.WriteLine("  moves left.");
    Console.WriteLine();
    Console.WriteLine("  Black starts first and players take it in turns to move their pieces forward");
    Console.WriteLine("  across the board diagonally. Should a piece reach the other side of the board");
    Console.WriteLine("  the piece becomes a king and can then move diagonally backwards as well as");
    Console.WriteLine("  forwards.");
    Console.WriteLine();
    Console.WriteLine("  Pieces are captured by jumping over them diagonally. More than one enemy piece");
    Console.WriteLine("  can be captured in the same turn by the same piece. If you can capture a piece");
    Console.WriteLine("  you must capture a piece.");
    Console.WriteLine();
    Console.WriteLine("  Moves are selected with the arrow keys. Use the [enter] button to select the");
    Console.WriteLine("  from and to squares. Invalid moves are ignored.");
    Console.WriteLine();
    Console.WriteLine("  Press a number key to choose number of human players:");
    Console.WriteLine("    [0] Black (computer) vs White (computer)");
    Console.WriteLine("    [1] Black (human) vs White (computer)");
    Console.Write("    [2] Black (human) vs White (human)");

    int? humanPlayerCount = null; // Initialize the number of players to null
    while (humanPlayerCount is null) // Loop until the player selects a valid mode
    {
        Console.CursorVisible = false; // Hide the cursor
        switch (Console.ReadKey(true).Key) // Select mode
        {
            case ConsoleKey.D0 or ConsoleKey.NumPad0: humanPlayerCount = 0; break;
            case ConsoleKey.D1 or ConsoleKey.NumPad1: humanPlayerCount = 1; break;
            case ConsoleKey.D2 or ConsoleKey.NumPad2: humanPlayerCount = 2; break;
        }
    }
    return new Game(humanPlayerCount.Value); // Create and return a new game object based on the player's selection
}

// Game main loop
void RunGameLoop(Game game)
{
    while (game.Winner is null) // Loop until there is a winner for the game
    {
        Player currentPlayer = game.Players.First(player => player.Color == game.Turn); // Get the player of the current round
        if (currentPlayer.IsHuman) // If the current player is a human
        {
            while (game.Turn == currentPlayer.Color) // Loop until the current player has finished moving
            {
                (int X, int Y)? selectionStart = null; // Initialize the moving starting position
                (int X, int Y)? from = game.Board.Aggressor is not null ? (game.Board.Aggressor.X, game.Board.Aggressor.Y) : null;
                List<Move> moves = game.Board.GetPossibleMoves(game.Turn); // Get all possible moves for the current player
                if (moves.Select(move => move.PieceToMove).Distinct().Count() is 1) // If only one piece can be moved
                {
                    Move must = moves.First(); // Get the required movement
                    from = (must.PieceToMove.X, must.PieceToMove.Y); // Set the starting position
                    selectionStart = must.To; // Set the selection starting position
                }
                while (from is null) // Loop until the player selects a valid starting position
                {
                    from = HumanMoveSelection(game); // Let the player choose a starting position
                    selectionStart = from; // Set the selection starting position
                }
                (int X, int Y)? to = HumanMoveSelection(game, selectionStart: selectionStart, from: from);
                Piece? piece = null; // Let the player choose the target location
                piece = game.Board[from.Value.X, from.Value.Y]; // Get the starting position of the chess piece
                if (piece is null || piece.Color != game.Turn) // If the starting position is invalid or the piece does not belong to the current player
                {
                    from = null;
                    to = null;
                }
                if (from is not null && to is not null) // If both the starting position and the target position are valid
                {
                    Move? move = game.Board.ValidateMove(game.Turn, from.Value, to.Value); // Verify that the move is possible
                    if (move is not null &&
                        (game.Board.Aggressor is null || move.PieceToMove == game.Board.Aggressor)) // If the move is valid and follows the rules
                    {
                        game.PerformMove(move); // Executes the move
                    }
                }
            }
        }
        else // If the current player is the computer
        {
            List<Move> moves = game.Board.GetPossibleMoves(game.Turn); // Gets all possible moves for the current player
            List<Move> captures = moves.Where(move => move.PieceToCapture is not null).ToList(); // Filters moves that capture an opponent's piece
            if (captures.Count > 0) // If there are capturing moves
            {
                game.PerformMove(captures[Random.Shared.Next(captures.Count)]); // Randomly selects a capturing move
            }
            else if (!game.Board.Pieces.Any(piece => piece.Color == game.Turn && !piece.Promoted)) // If no capturing moves, check if all pieces are kings
            {
                var (a, b) = game.Board.GetClosestRivalPieces(game.Turn); // Get the closest pair of rival pieces
                Move? priorityMove = moves.FirstOrDefault(move => move.PieceToMove == a && Board.IsTowards(move, b)); // Find a move that moves towards the closest rival piece
                game.PerformMove(priorityMove ?? moves[Random.Shared.Next(moves.Count)]); // Perform the priority move if found, otherwise perform a random move
            }
            else
            {
                game.PerformMove(moves[Random.Shared.Next(moves.Count)]); // Randomly select a move from all possible moves
            }
        }

        RenderGameState(game, playerMoved: currentPlayer, promptPressKey: true); // Renders the game state
        Console.ReadKey(true);
    }
}

void RenderGameState(Game game, Player? playerMoved = null, (int X, int Y)? selection = null, (int X, int Y)? from = null, bool promptPressKey = false)
{
    const char BlackPiece = '○';

    const char WhitePiece = '◙';
const char WhiteSoldier = '♙';
const char WhiteCannon = '♖';
const char WhiteHorse = '♘';
const char WhiteDragon = '♕';
const char WhiteKing = '♔';
    const char Vacant = '·';

    Console.CursorVisible = false;
    Console.SetCursorPosition(0, 0);
    StringBuilder sb = new();
    sb.AppendLine(); // Draws the game board
    sb.AppendLine("  Checkers");
	sb.AppendLine();
	sb.AppendLine("    Soldier : Moves forward one step and captures forward.");
	sb.AppendLine("    Horse   : Moves diagonally one step and can capture.");
	sb.AppendLine("    Dragon  : Moves horizontally or vertically any steps and captures the first piece on its path.");
	sb.AppendLine("    Cannon  : Moves horizontally or vertically and captures by jumping over one piece.");
	sb.AppendLine("    King    : Moves one step in any direction and captures the first piece on its path.");
	sb.AppendLine($"    ╔═══════════════════╗");
	sb.AppendLine($"  8 ║  {B(0, 7)} {B(1, 7)} {B(2, 7)} {B(3, 7)} {B(4, 7)} {B(5, 7)} {B(6, 7)} {B(7, 7)}  ║ {WhiteSoldier} = Soldier");
    sb.AppendLine($"  7 ║  {B(0, 6)} {B(1, 6)} {B(2, 6)} {B(3, 6)} {B(4, 6)} {B(5, 6)} {B(6, 6)} {B(7, 6)}  ║ {WhiteCannon} = Cannon");
    sb.AppendLine($"  6 ║  {B(0, 5)} {B(1, 5)} {B(2, 5)} {B(3, 5)} {B(4, 5)} {B(5, 5)} {B(6, 5)} {B(7, 5)}  ║ {WhiteHorse} = Horse");
    sb.AppendLine($"  5 ║  {B(0, 4)} {B(1, 4)} {B(2, 4)} {B(3, 4)} {B(4, 4)} {B(5, 4)} {B(6, 4)} {B(7, 4)}  ║ {WhiteDragon} = Dragon");
    sb.AppendLine($"  4 ║  {B(0, 3)} {B(1, 3)} {B(2, 3)} {B(3, 3)} {B(4, 3)} {B(5, 3)} {B(6, 3)} {B(7, 3)}  ║ {WhiteKing} = King");
    sb.AppendLine($"  3 ║  {B(0, 2)} {B(1, 2)} {B(2, 2)} {B(3, 2)} {B(4, 2)} {B(5, 2)} {B(6, 2)} {B(7, 2)}  ║ Taken:");
    sb.AppendLine($"  2 ║  {B(0, 1)} {B(1, 1)} {B(2, 1)} {B(3, 1)} {B(4, 1)} {B(5, 1)} {B(6, 1)} {B(7, 1)}  ║ {game.TakenCount(White),2} x {WhitePiece}");
    sb.AppendLine($"  1 ║  {B(0, 0)} {B(1, 0)} {B(2, 0)} {B(3, 0)} {B(4, 0)} {B(5, 0)} {B(6, 0)} {B(7, 0)}  ║ {game.TakenCount(Black),2} x {BlackPiece}");
    sb.AppendLine($"    ╚═══════════════════╝");
    sb.AppendLine($"       A B C D E F G H");
    sb.AppendLine();
    if (selection is not null)
    {
        sb.Replace(" $ ", $"[{ToChar(game.Board[selection.Value.X, selection.Value.Y])}]");
    }
    if (from is not null)
    {
        char fromChar = ToChar(game.Board[from.Value.X, from.Value.Y]);
        sb.Replace(" @ ", $"<{fromChar}>");
        sb.Replace("@ ", $"{fromChar}>");
        sb.Replace(" @", $"<{fromChar}");
    }
    PieceColor? wc = game.Winner;
    PieceColor? mc = playerMoved?.Color;
    PieceColor? tc = game.Turn;
    // Note: these strings need to match in length
    // so they overwrite each other.
    // Displays game state information
    string w = $"  *** {wc} wins ***";
    string m = $"  {mc} moved       ";
    string t = $"  {tc}'s turn      ";
    sb.AppendLine(
        game.Winner is not null ? w :
        playerMoved is not null ? m :
        t);
    string p = "  Press any key to continue...";
    string s = "                              ";
    sb.AppendLine(promptPressKey ? p : s);
    Console.Write(sb);

    char B(int x, int y) => // Gets the symbol for a specific position
        (x, y) == selection ? '$' : // If the position is selected, displays '$'
        (x, y) == from ? '@' : // If the position is the starting point, displays '@'
        ToChar(game.Board[x, y]); // Otherwise, displays the piece symbol

    static char ToChar(Piece? piece) =>
        piece is null ? Vacant :
        (piece.Color, piece.Type) switch
        {
            (Black, PieceType.Soldier) => '♟', 
            (Black, PieceType.Cannon) => '♜', 
            (Black, PieceType.Horse) => '♞', 
            (Black, PieceType.Dragon) => '♛', 
            (Black, PieceType.King) => '♚', 
            (White, PieceType.Soldier) => '♙', 
            (White, PieceType.Cannon) => '♖', 
            (White, PieceType.Horse) => '♘', 
            (White, PieceType.Dragon) => '♕', 
            (White, PieceType.King) => '♔', 
            _ => throw new NotImplementedException(),
        };
}

(int X, int Y)? HumanMoveSelection(Game game, (int X, int y)? selectionStart = null, (int X, int Y)? from = null)
{
    (int X, int Y) selection = selectionStart ?? (3, 3); // Initializes the selection position, defaulting to (3, 3)
    while (true) // Loops until the player selects a valid position
    {
        RenderGameState(game, selection: selection, from: from); // Renders the game state
        switch (Console.ReadKey(true).Key) // Listens for player input
        {
            case ConsoleKey.DownArrow: selection.Y = Math.Max(0, selection.Y - 1); break; // Moves down
            case ConsoleKey.UpArrow: selection.Y = Math.Min(7, selection.Y + 1); break; // Moves up
            case ConsoleKey.LeftArrow: selection.X = Math.Max(0, selection.X - 1); break; // Moves left
            case ConsoleKey.RightArrow: selection.X = Math.Min(7, selection.X + 1); break; // Moves right
            case ConsoleKey.Enter: return selection; // Confirms the selection
            case ConsoleKey.Escape: return null; // Cancels the selection
        }
    }
}