namespace TicTacToe.Api.Models;

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

/// <summary>
/// Player symbols kept as constants so we never mistype "X" / "O".
/// An empty cell on the board is represented by an empty string.
/// </summary>
public static class Players
{
    public const string X = "X";
    public const string O = "O";
    public const string Empty = "";
}
