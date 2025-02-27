namespace Checkers;

public class Piece
{
    public int X { get; set; }
    public int Y { get; set; }

    public string NotationPosition
    {
        get => Board.ToPositionNotationString(X, Y);
        set => (X, Y) = Board.ParsePositionNotation(value);
    }

    public PieceColor Color { get; init; }

    public PieceType Type { get; set; } // Add a new attribute to indicate the chess piece type
    public int StepsMoved { get; set; } = 0; // Record the number of steps the soldier has moved
    public bool Promoted { get; set; }
}

public enum PieceType
{
    Soldier, 
    Cannon,  
    Horse,   
    Dragon,  
    King     
}