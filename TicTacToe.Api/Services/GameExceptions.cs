namespace TicTacToe.Api.Services;

/// <summary>Thrown when a move or action breaks a game rule. The controller turns this into HTTP 400.</summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}

/// <summary>Thrown when the game id does not exist. The controller turns this into HTTP 404.</summary>
public class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid id) : base($"Game '{id}' was not found.") { }
}
