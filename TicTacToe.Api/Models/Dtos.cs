namespace TicTacToe.Api.Models;

// DTOs = "Data Transfer Objects": the exact shapes the API receives and sends.

/// <summary>Body for POST /api/games</summary>
public class CreateGameRequest
{
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

/// <summary>Body for POST /api/games/{id}/moves. Row and Column are 0-based (0..2).</summary>
public class MoveRequest
{
    public string Player { get; set; } = Players.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
}

/// <summary>Everything the frontend needs to draw the screen.</summary>
public class GameStateResponse
{
    public Guid Id { get; set; }
    public string[] Board { get; set; } = Array.Empty<string>();
    public string CurrentPlayer { get; set; } = Players.X;
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; }
    public string? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<Move> Moves { get; set; } = new();
    public bool CanUndo { get; set; }
    public Scoreboard Scoreboard { get; set; } = new();

    public static GameStateResponse FromGame(Game game, Scoreboard scoreboard, bool canUndo)
    {
        return new GameStateResponse
        {
            Id = game.Id,
            Board = game.Board.ToArray(),          // copy, so callers can't change our state
            CurrentPlayer = game.CurrentPlayer,
            Mode = game.Mode,
            Status = game.Status,
            Winner = game.Winner,
            WinningCells = game.WinningCells.ToList(),
            Moves = game.Moves.ToList(),
            CanUndo = canUndo,
            Scoreboard = scoreboard
        };
    }
}

/// <summary>Error body returned with 400 / 404 responses.</summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
