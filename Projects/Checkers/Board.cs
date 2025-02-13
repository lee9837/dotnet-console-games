namespace Checkers;

public class Board
{
	public List<Piece> Pieces { get; }// 2D array representing the game board. Each cell can contain a Piece or be null (empty).

	public Piece? Aggressor { get; set; }// Property to track the aggressor (a piece that must continue capturing).
										 // Indexer to access pieces on the board using coordinates (x, y).
	public Piece? this[int x, int y] =>
		Pieces.FirstOrDefault(piece => piece.X == x && piece.Y == y);
	// Initialize the board and place the pieces according to the initial layout of international checkers
	public Board()
	{
		Aggressor = null;
		Pieces = new List<Piece>
			{
				new() { NotationPosition ="A3", Color = Black},
				new() { NotationPosition ="A1", Color = Black},
				new() { NotationPosition ="B2", Color = Black},
				new() { NotationPosition ="C3", Color = Black},
				new() { NotationPosition ="C1", Color = Black},
				new() { NotationPosition ="D2", Color = Black},
				new() { NotationPosition ="E3", Color = Black},
				new() { NotationPosition ="E1", Color = Black},
				new() { NotationPosition ="F2", Color = Black},
				new() { NotationPosition ="G3", Color = Black},
				new() { NotationPosition ="G1", Color = Black},
				new() { NotationPosition ="H2", Color = Black},

				new() { NotationPosition ="A7", Color = White},
				new() { NotationPosition ="B8", Color = White},
				new() { NotationPosition ="B6", Color = White},
				new() { NotationPosition ="C7", Color = White},
				new() { NotationPosition ="D8", Color = White},
				new() { NotationPosition ="D6", Color = White},
				new() { NotationPosition ="E7", Color = White},
				new() { NotationPosition ="F8", Color = White},
				new() { NotationPosition ="F6", Color = White},
				new() { NotationPosition ="G7", Color = White},
				new() { NotationPosition ="H8", Color = White},
				new() { NotationPosition ="H6", Color = White}
			};
	}
	// Convert chessboard coordinates (x, y) to chessboard position marks 
	public static string ToPositionNotationString(int x, int y)
	{
		if (!IsValidPosition(x, y)) throw new ArgumentException("Not a valid position!");
		return $"{(char)('A' + x)}{y + 1}";
	}
	// Parse the board position markers (such as A3, B5) and convert them to (x, y) coordinates
	public static (int X, int Y) ParsePositionNotation(string notation)
	{
		if (notation is null) throw new ArgumentNullException(nameof(notation));
		notation = notation.Trim().ToUpper();
		if (notation.Length is not 2 ||
			notation[0] < 'A' || 'H' < notation[0] ||
			notation[1] < '1' || '8' < notation[1])
			throw new FormatException($@"{nameof(notation)} ""{notation}"" is not valid");
		return (notation[0] - 'A', notation[1] - '1');
	}
// Determine whether the coordinates are within the chessboard range
	public static bool IsValidPosition(int x, int y) =>
		0 <= x && x < 8 &&
		0 <= y && y < 8;
// Get the closest pair of opponent chess pieces on the board
	public (Piece A, Piece B) GetClosestRivalPieces(PieceColor priorityColor)
	{
		double minDistanceSquared = double.MaxValue;
		(Piece A, Piece B) closestRivals = (null!, null!);
		// Iterate through all pieces of the given color
		foreach (Piece a in Pieces.Where(piece => piece.Color == priorityColor))
		{// Compare against all pieces of the opposing color
			foreach (Piece b in Pieces.Where(piece => piece.Color != priorityColor))
			{// Compute the squared distance between the two pieces
				(int X, int Y) vector = (a.X - b.X, a.Y - b.Y);
				double distanceSquared = vector.X * vector.X + vector.Y * vector.Y;
				if (distanceSquared < minDistanceSquared)// Update the closest pair if a shorter distance is found
				{
					minDistanceSquared = distanceSquared;
					closestRivals = (a, b);
				}
			}
		}
		return closestRivals;
	}

	public List<Move> GetPossibleMoves(PieceColor color)// Retrieves all possible moves for a given piece color.
	{
		List<Move> moves = new();
		if (Aggressor is not null)// If there is an aggressor (a piece that must continue capturing), only return its valid capture moves
		{
			if (Aggressor.Color != color)
			{
				throw new Exception($"{nameof(Aggressor)} is not null && {nameof(Aggressor)}.{nameof(Aggressor.Color)} != {nameof(color)}");
			}
			moves.AddRange(GetPossibleMoves(Aggressor).Where(move => move.PieceToCapture is not null));
		}
		else// Otherwise, compute all valid moves for the specified color
		{
			foreach (Piece piece in Pieces.Where(piece => piece.Color == color))
			{
				moves.AddRange(GetPossibleMoves(piece));
			}
		}
		// If capture moves exist, only return those
		return moves.Any(move => move.PieceToCapture is not null)
			? moves.Where(move => move.PieceToCapture is not null).ToList()
			: moves;
	}
	// Retrieves all possible moves for a specific piece.
	public List<Move> GetPossibleMoves(Piece piece)
	{// Check all four diagonal directions for valid moves.
		List<Move> moves = new();
		ValidateDiagonalMove(-1, -1);
		ValidateDiagonalMove(-1, 1);
		ValidateDiagonalMove(1, -1);
		ValidateDiagonalMove(1, 1);
		return moves.Any(move => move.PieceToCapture is not null)
			? moves.Where(move => move.PieceToCapture is not null).ToList()
			: moves;

		void ValidateDiagonalMove(int dx, int dy)// Determines if a diagonal move is valid.
		{   // Non-promoted black pieces cannot move backward.
			if (!piece.Promoted && piece.Color is Black && dy is -1) return;
			// Non-promoted white pieces cannot move backward.
			if (!piece.Promoted && piece.Color is White && dy is 1) return;
			(int X, int Y) target = (piece.X + dx, piece.Y + dy);
			if (!IsValidPosition(target.X, target.Y)) return;
			PieceColor? targetColor = this[target.X, target.Y]?.Color;
			// If the target square is empty, the move is valid.
			if (targetColor is null)
			{
				if (!IsValidPosition(target.X, target.Y)) return;
				Move newMove = new(piece, target);
				moves.Add(newMove);
			}
			// If the target square is occupied by an opponent's piece, check for a jump move.
			else if (targetColor != piece.Color)
			{
				(int X, int Y) jump = (piece.X + 2 * dx, piece.Y + 2 * dy);
				if (!IsValidPosition(jump.X, jump.Y)) return;
				PieceColor? jumpColor = this[jump.X, jump.Y]?.Color;
				if (jumpColor is not null) return;
				Move attack = new(piece, jump, this[target.X, target.Y]);
				moves.Add(attack);
			}
		}
	}

	/// <summary>Returns a <see cref="Move"/> if <paramref name="from"/>-&gt;<paramref name="to"/> is valid or null if not.</summary>
	public Move? ValidateMove(PieceColor color, (int X, int Y) from, (int X, int Y) to)
	{
		Piece? piece = this[from.X, from.Y];
		if (piece is null)
		{
			return null;
		}
		foreach (Move move in GetPossibleMoves(color))
		{
			if ((move.PieceToMove.X, move.PieceToMove.Y) == from && move.To == to)
			{
				return move;
			}
		}
		return null;
	}

	public static bool IsTowards(Move move, Piece piece)
	{
		(int Dx, int Dy) a = (move.PieceToMove.X - piece.X, move.PieceToMove.Y - piece.Y);
		int a_distanceSquared = a.Dx * a.Dx + a.Dy * a.Dy;
		(int Dx, int Dy) b = (move.To.X - piece.X, move.To.Y - piece.Y);
		int b_distanceSquared = b.Dx * b.Dx + b.Dy * b.Dy;
		return b_distanceSquared < a_distanceSquared;
	}
}
