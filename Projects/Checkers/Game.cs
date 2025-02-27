namespace Checkers;

public class Game
{
    // The total number of pieces each player starts with.
    private const int PiecesPerColor = 12;

    // Indicates whose turn it is (Black or White).
    public PieceColor Turn { get; private set; }

    // The game board instance.
    public Board Board { get; }

    // Stores the winner of the game, if there is one.
    public PieceColor? Winner { get; private set; }

    // A list of players participating in the game.
    public List<Player> Players { get; }
    // Turn counter, each turn is a move of black and white pieces
    public int TurnCount { get; private set; } = 1; // The initial round is 1
    // Initializes the game with a specified number of human players.
    public Game(int humanPlayerCount)
    {
        // Ensures the number of human players is either 0, 1, or 2.
        if (humanPlayerCount < 0 || 2 < humanPlayerCount) throw new ArgumentOutOfRangeException(nameof(humanPlayerCount));

        // Initializes the game board.
        Board = new Board();

        // Creates players, assigning human control based on the input count.
        Players = new()
        {
            new Player(humanPlayerCount >= 1, Black), // First player (Black)
            new Player(humanPlayerCount >= 2, White)  // Second player (White)
        };

        // Sets the starting turn to Black.
        Turn = Black;

        // No winner at the start of the game.
        Winner = null;
    }

    // Executes a move on the board.
    // Executes a move on the board.
    public void PerformMove(Move move)
    {
        // If the turn number is less than or equal to 5, and the target chess piece is the King, it is not allowed to capture it
        if (TurnCount <= 5 && move.PieceToCapture?.Type == PieceType.King)
        {
            // Not allowed to eat King
            return;
        }
        // Updates the piece's position to the destination.
        (move.PieceToMove.X, move.PieceToMove.Y) = move.To;
        // If the piece is a soldier, increase StepsMoved
        if (move.PieceToMove.Type == PieceType.Soldier)
        {
            move.PieceToMove.StepsMoved++;
        }
        // Removes the captured piece from the board if a capture occurred.
        if (move.PieceToCapture is not null)
        {
            Board.Pieces.Remove(move.PieceToCapture);
		}

		// Delete the consecutive capture rule: Regardless of whether to capture the piece, switch directly to the opponent's turn
		Board.Aggressor = null;

        // Switch turns
        Turn = Turn is Black ? White : Black;

        // If the game switches back to Black, it means a turn is over and the number of turns is increased
        if (Turn == Black)
        {
            TurnCount++;
        }
        // Checks if there is a winner after the move.
        CheckForWinner();
	}
	// Determines if there is a winner based on the game state.
	public void CheckForWinner()
	{
		// If there are no Black pieces left, White wins.
		if (!Board.Pieces.Any(piece => piece.Color is Black))
        {
            Winner = White;
        }

        // If there are no White pieces left, Black wins.
        if (!Board.Pieces.Any(piece => piece.Color is White))
        {
            Winner = Black;
        }

        // If the current player has no valid moves, they lose (stalemate).
        if (Winner is null && Board.GetPossibleMoves(Turn).Count is 0)
        {
            Winner = Turn is Black ? White : Black;
        }
    }

    // // Returns the number of pieces that have been captured for a given color.
    // public int TakenCount(PieceColor colour) =>
    //     PiecesPerColor - Board.Pieces.Count(piece => piece.Color == colour);
    
    // Count the remaining number of each chess piece
    public Dictionary<PieceType, int> GetRemainingPieces(PieceColor color)
    {
        var remainingPieces = new Dictionary<PieceType, int>
        {
            { PieceType.Soldier, 0 },
            { PieceType.Cannon, 0 },
            { PieceType.Horse, 0 },
            { PieceType.Dragon, 0 },
            { PieceType.King, 0 }
        };

        foreach (var piece in Board.Pieces.Where(p => p.Color == color))
        {
            remainingPieces[piece.Type]++;
        }

        return remainingPieces;
    }
}
