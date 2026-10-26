namespace TicTacToe.Api.Models;

/// <summary>
/// The full state of one game session. The backend is the source of truth for this.
/// The board is a flat array of 9 cells:
///   index 0 | 1 | 2
///   index 3 | 4 | 5
///   index 6 | 7 | 8
/// </summary>
public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
    public string[] Board { get; set; } = CreateEmptyBoard();
    public string CurrentPlayer { get; set; } = Players.X;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public string? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<Move> Moves { get; set; } = new();

    /// <summary>Guards against counting the same finished game twice on the scoreboard.</summary>
    public bool IsResultRecorded { get; set; }

    public static string[] CreateEmptyBoard() => Enumerable.Repeat(Players.Empty, 9).ToArray();
}
