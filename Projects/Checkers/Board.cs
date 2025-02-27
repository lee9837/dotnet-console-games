namespace Checkers;

public class Board
{
    public List<Piece> Pieces { get; } // 2D array representing the game board. Each cell can contain a Piece or be null (empty).

    public Piece? Aggressor { get; set; } // Property to track the aggressor (a piece that must continue capturing).
                                         // Indexer to access pieces on the board using coordinates (x, y).
    public Piece? this[int x, int y] =>
        Pieces.FirstOrDefault(piece => piece.X == x && piece.Y == y);

    // Initialize the board and place the pieces according to the initial layout of international checkers
    public Board()
    {
        Aggressor = null;
        Pieces = new List<Piece>
        {
            // Soldier
            new() { NotationPosition = "A3", Color = Black, Type = PieceType.Soldier },
            new() { NotationPosition = "C3", Color = Black, Type = PieceType.Soldier },
            new() { NotationPosition = "E3", Color = Black, Type = PieceType.Soldier },
            new() { NotationPosition = "G3", Color = Black, Type = PieceType.Soldier },

            new() { NotationPosition = "H6", Color = White, Type = PieceType.Soldier },
            new() { NotationPosition = "F6", Color = White, Type = PieceType.Soldier },
            new() { NotationPosition = "D6", Color = White, Type = PieceType.Soldier },
            new() { NotationPosition = "B6", Color = White, Type = PieceType.Soldier },

            // Cannon
            new() { NotationPosition = "D2", Color = Black, Type = PieceType.Cannon },
            new() { NotationPosition = "F2", Color = Black, Type = PieceType.Cannon },

            new() { NotationPosition = "C7", Color = White, Type = PieceType.Cannon },
            new() { NotationPosition = "E7", Color = White, Type = PieceType.Cannon },

            // Horse
            new() { NotationPosition = "B2", Color = Black, Type = PieceType.Horse },
            new() { NotationPosition = "H2", Color = Black, Type = PieceType.Horse },

            new() { NotationPosition = "A7", Color = White, Type = PieceType.Horse },
            new() { NotationPosition = "G7", Color = White, Type = PieceType.Horse },

            // Dragon
            new() { NotationPosition = "A1", Color = Black, Type = PieceType.Dragon },
			new() { NotationPosition = "G1", Color = Black, Type = PieceType.Dragon },

			new() { NotationPosition = "A8", Color = White, Type = PieceType.Dragon },
			new() { NotationPosition = "G8", Color = White, Type = PieceType.Dragon },

            // 王
            new() { NotationPosition = "D1", Color = Black, Type = PieceType.King },
			new() { NotationPosition = "E8", Color = White, Type = PieceType.King }
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
        {
            // Compare against all pieces of the opposing color
            foreach (Piece b in Pieces.Where(piece => piece.Color != priorityColor))
            {
                // Compute the squared distance between the two pieces
                (int X, int Y) vector = (a.X - b.X, a.Y - b.Y);
                double distanceSquared = vector.X * vector.X + vector.Y * vector.Y;
                if (distanceSquared < minDistanceSquared) // Update the closest pair if a shorter distance is found
                {
                    minDistanceSquared = distanceSquared;
                    closestRivals = (a, b);
                }
            }
        }
        return closestRivals;
    }

	// Retrieves all possible moves for a given piece color.
	public List<Move> GetPossibleMoves(PieceColor color)
	{
		List<Move> moves = new();
		foreach (Piece piece in Pieces.Where(piece => piece.Color == color))
		{
			moves.AddRange(GetPossibleMoves(piece));
		}
		return moves; // No longer forced to return capture moves
	}

	// Retrieves all possible moves for a specific piece.
	public List<Move> GetPossibleMoves(Piece piece)
	{
		List<Move> moves = new();

		switch (piece.Type)
		{
			case PieceType.Soldier:
                moves.AddRange(GetSoldierMoves(piece));
                break;
            case PieceType.Cannon:
                moves.AddRange(GetCannonMoves(piece));
                break;
            case PieceType.Horse:
                moves.AddRange(GetHorseMoves(piece));
                break;
            case PieceType.Dragon:
                moves.AddRange(GetDragonMoves(piece));
                break;
            case PieceType.King:
                moves.AddRange(GetKingMoves(piece));
                break;
        }

        return moves;
    }
private List<Move> GetSoldierMoves(Piece piece)
{
    List<Move> moves = new();

    // The direction in which the soldier can move forward
    int forwardDirection = piece.Color == Black ? 1 : -1;

    // If StepsMoved < 2, the soldier can only move forward
    if (piece.StepsMoved < 2)
    {
        (int X, int Y) target = (piece.X, piece.Y + forwardDirection);

        if (IsValidPosition(target.X, target.Y))
        {
            Piece? targetPiece = this[target.X, target.Y];
            if (targetPiece == null)
            {
                moves.Add(new Move(piece, target));
            }
            else if (targetPiece.Color != piece.Color)
            {
                moves.Add(new Move(piece, target, targetPiece));
            }
        }
    }
    else
    {
        // If StepsMoved >= 2, the soldier can move forward, backward, left, and right
        int[] dx = { -1, 1, 0, 0 }; // Left, Right, Up, no Down
        int[] dy = { 0, 0, forwardDirection, 0};

        for (int i = 0; i < 4; i++)
        {
            (int X, int Y) target = (piece.X + dx[i], piece.Y + dy[i]);

            if (IsValidPosition(target.X, target.Y))
            {
                Piece? targetPiece = this[target.X, target.Y];
                if (targetPiece == null)
                {
                    moves.Add(new Move(piece, target));
                }
                else if (targetPiece.Color != piece.Color)
                {
                    moves.Add(new Move(piece, target, targetPiece));
                }
            }
        }
    }

    return moves;
}

	private List<Move> GetCannonMoves(Piece piece)
	{
		List<Move> moves = new();

		// Movement logic of the cannon: move any number of steps horizontally and vertically, but not diagonally
		int[] dx = { -1, 1, 0, 0 }; // Horizontal and vertical movement directions
		int[] dy = { 0, 0, -1, 1 };

		for (int i = 0; i < 4; i++) //Traverse four directions
		{
			(int X, int Y) current = (piece.X + dx[i], piece.Y + dy[i]);

			// Cannon movement: can move to any empty space until it encounters a chess piece
			while (IsValidPosition(current.X, current.Y))
			{
				Piece? targetPiece = this[current.X, current.Y];

				if (targetPiece == null) // If the target position is empty, you can move
				{
					moves.Add(new Move(piece, current));
				}
				else // If you encounter a chess piece, stop moving
				{
					break;
				}

				current = (current.X + dx[i], current.Y + dy[i]); // Continue moving in the same direction
			}

			// Cannon's capture logic: must be separated by a piece (cannon mount), and can only capture the first piece after the piece separated by a piece
			current = (piece.X + dx[i], piece.Y + dy[i]);
			bool hasJumped = false; // Has the chess piece been jumped?

			while (IsValidPosition(current.X, current.Y))
			{
				Piece? targetPiece = this[current.X, current.Y];

				if (targetPiece != null) // If you encounter a chess piece
				{
					if (hasJumped) // If the piece has been jumped, you can capture it
					{
						if (targetPiece.Color != piece.Color) // If the target piece is an enemy piece
						{
							moves.Add(new Move(piece, current, targetPiece));
						}
						break; // Stop after capturing
					}
					else // Otherwise, mark as jumping the piece 
					{
						hasJumped = true;
					}
				}

				current = (current.X + dx[i], current.Y + dy[i]); // Continue moving in the same direction
			}
		}

		return moves;
	}

	private List<Move> GetHorseMoves(Piece piece)
	{
		List<Move> moves = new();

		// Horse movement logic: move diagonally one square, cannot capture pieces continuously
		int[] dx = { -1, -1, 1, 1 }; // Diagonal movement direction
		int[] dy = { -1, 1, -1, 1 };

		for (int i = 0; i < 4; i++) // Traverse the four diagonal directions
		{
			(int X, int Y) target = (piece.X + dx[i], piece.Y + dy[i]);

			if (IsValidPosition(target.X, target.Y))
			{
				Piece? targetPiece = this[target.X, target.Y];

				if (targetPiece == null) // If the target position is empty, you can move
				{
					moves.Add(new Move(piece, target));
				}
				else if (targetPiece.Color != piece.Color) // If the target position is an enemy piece, you can capture it
				{
					moves.Add(new Move(piece, target, targetPiece));
				}
			}
		}

		return moves;
	}

	private List<Move> GetDragonMoves(Piece piece)
	{
		List<Move> moves = new();

		// Dragon's movement logic: move any number of steps horizontally or vertically, but not diagonally
		int[] dx = { -1, 1, 0, 0 }; 
		int[] dy = { 0, 0, -1, 1 };

		for (int i = 0; i < 4; i++) 
		{
			(int X, int Y) current = (piece.X + dx[i], piece.Y + dy[i]);

			while (IsValidPosition(current.X, current.Y))
			{
				Piece? targetPiece = this[current.X, current.Y];

				if (targetPiece == null) 
				{
					moves.Add(new Move(piece, current));
				}
				else if (targetPiece.Color != piece.Color) // If the target position is an enemy piece, you can capture it
				{
					moves.Add(new Move(piece, current, targetPiece));
					break; 
				}
				else 
				{
					break;
				}

				current = (current.X + dx[i], current.Y + dy[i]); 
			}
		}

		return moves;
	}

	private List<Move> GetKingMoves(Piece piece)
	{
		List<Move> moves = new();

		// King's movement logic: move one square in eight directions
		int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 }; 
		int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

		for (int i = 0; i < 8; i++) 
		{
			(int X, int Y) target = (piece.X + dx[i], piece.Y + dy[i]);

			if (IsValidPosition(target.X, target.Y))
			{
				Piece? targetPiece = this[target.X, target.Y];

				if (targetPiece == null) 
				{
					moves.Add(new Move(piece, target));
				}
				else if (targetPiece.Color != piece.Color) 
				{
					moves.Add(new Move(piece, target, targetPiece));
				}
			}
		}

		return moves;
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