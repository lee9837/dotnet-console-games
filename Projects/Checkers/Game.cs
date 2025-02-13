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
	public void PerformMove(Move move)
	{
		// Updates the piece's position to the destination.
		(move.PieceToMove.X, move.PieceToMove.Y) = move.To;

		// Promotes a piece if it reaches the opposite end of the board.
		if ((move.PieceToMove.Color is Black && move.To.Y is 7) ||
			(move.PieceToMove.Color is White && move.To.Y is 0))
		{
			move.PieceToMove.Promoted = true;
		}

		// Removes the captured piece from the board if a capture occurred.
		if (move.PieceToCapture is not null)
		{
			Board.Pieces.Remove(move.PieceToCapture);
		}

		// If a piece can continue capturing, it must do so (forced capture rule).
		if (move.PieceToCapture is not null &&
			Board.GetPossibleMoves(move.PieceToMove).Any(m => m.PieceToCapture is not null))
		{
			// Sets the piece as the aggressor (must continue capturing).
			Board.Aggressor = move.PieceToMove;
		}
		else
		{
			// Resets the aggressor and switches turns.
			Board.Aggressor = null;
			Turn = Turn is Black ? White : Black;
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

	// Returns the number of pieces that have been captured for a given color.
	public int TakenCount(PieceColor colour) =>
		PiecesPerColor - Board.Pieces.Count(piece => piece.Color == colour);
}
