using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

/// <summary>
/// Tests for the game rules. Each test builds a fresh GameService,
/// so tests never share state with each other.
/// Row and column are 0-based: (0,0) is top-left, (2,2) is bottom-right.
/// </summary>
public class GameServiceTests
{
    private readonly ScoreboardService _scoreboard;
    private readonly GameService _service;

    public GameServiceTests()
    {
        _scoreboard = new ScoreboardService();
        _service = new GameService(new GameStore(), _scoreboard, new ComputerPlayer());
    }

    // Small helper so tests read like a game: Play(game, "X", 0, 0)
    private Game Play(Game game, string player, int row, int column)
    {
        return _service.MakeMove(game.Id, player, row, column);
    }

    [Fact]
    public void ValidMove_PlacesMarkAndRecordsHistory()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Play(game, "X", 0, 0);

        Assert.Equal("X", game.Board[0]);
        Assert.Single(game.Moves);
        Assert.Equal("Row 1, Column 1", game.Moves[0].Position);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void TurnSwitching_AlternatesBetweenXAndO()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Assert.Equal("X", game.CurrentPlayer);

        Play(game, "X", 0, 0);
        Assert.Equal("O", game.CurrentPlayer);

        Play(game, "O", 1, 1);
        Assert.Equal("X", game.CurrentPlayer);
    }

    [Fact]
    public void InvalidMove_OccupiedCell_IsRejectedAndTurnUnchanged()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);

        Assert.Throws<InvalidMoveException>(() => Play(game, "O", 0, 0));
        Assert.Equal("O", game.CurrentPlayer);
        Assert.Single(game.Moves);
    }

    [Fact]
    public void InvalidMove_OutsideBoard_IsRejected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidMoveException>(() => Play(game, "X", 3, 0));
        Assert.Throws<InvalidMoveException>(() => Play(game, "X", 0, -1));
        Assert.Empty(game.Moves);
    }

    [Fact]
    public void InvalidMove_WrongPlayer_IsRejected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidMoveException>(() => Play(game, "O", 0, 0));
        Assert.Equal("X", game.CurrentPlayer);
    }

    [Fact]
    public void RowWin_IsDetectedWithWinningCells()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 0);
        Play(game, "X", 0, 1);
        Play(game, "O", 1, 1);
        Play(game, "X", 0, 2);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal("X", game.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, game.WinningCells);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 0, 1);
        Play(game, "X", 1, 0);
        Play(game, "O", 1, 1);
        Play(game, "X", 2, 2);
        Play(game, "O", 2, 1);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal("O", game.Winner);
        Assert.Equal(new List<int> { 1, 4, 7 }, game.WinningCells);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 0, 1);
        Play(game, "X", 1, 1);
        Play(game, "O", 0, 2);
        Play(game, "X", 2, 2);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(new List<int> { 0, 4, 8 }, game.WinningCells);
    }

    [Fact]
    public void Draw_IsDetectedWhenBoardFullWithNoWinner()
    {
        // Final board:
        // X | O | X
        // X | O | O
        // O | X | X
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 0, 1);
        Play(game, "X", 0, 2);
        Play(game, "O", 1, 1);
        Play(game, "X", 1, 0);
        Play(game, "O", 1, 2);
        Play(game, "X", 2, 1);
        Play(game, "O", 2, 0);
        Play(game, "X", 2, 2);

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
        Assert.Equal(1, _scoreboard.GetScoreboard().Draws);
    }

    [Fact]
    public void MoveAfterGameCompletion_IsRejected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 0);
        Play(game, "X", 0, 1);
        Play(game, "O", 1, 1);
        Play(game, "X", 0, 2); // X wins

        Assert.Throws<InvalidMoveException>(() => Play(game, "O", 2, 2));
    }

    [Fact]
    public void ResetGame_ClearsBoardButKeepsScoreboard()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 0);
        Play(game, "X", 0, 1);
        Play(game, "O", 1, 1);
        Play(game, "X", 0, 2); // X wins

        _service.ResetGame(game.Id);

        Assert.All(game.Board, cell => Assert.Equal("", cell));
        Assert.Empty(game.Moves);
        Assert.Equal("X", game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
        Assert.Equal(1, _scoreboard.GetScoreboard().XWins); // unchanged
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 1);

        _service.UndoLastMove(game.Id);

        Assert.Single(game.Moves);
        Assert.Equal("", game.Board[4]);
        Assert.Equal("O", game.CurrentPlayer); // O's turn again
    }

    [Fact]
    public void Undo_ComputerMode_RemovesComputerAndHumanMove()
    {
        var game = _service.CreateGame(GameMode.Computer);
        Play(game, "X", 0, 0);          // computer replies automatically
        Assert.Equal(2, game.Moves.Count);

        _service.UndoLastMove(game.Id);

        Assert.Empty(game.Moves);
        Assert.All(game.Board, cell => Assert.Equal("", cell));
        Assert.Equal("X", game.CurrentPlayer);
    }

    [Fact]
    public void Undo_WithNoMoves_IsRejected()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.False(_service.CanUndo(game));
        Assert.Throws<InvalidMoveException>(() => _service.UndoLastMove(game.Id));
    }

    [Fact]
    public void Undo_AfterGameCompleted_IsDisabled()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 0);
        Play(game, "X", 0, 1);
        Play(game, "O", 1, 1);
        Play(game, "X", 0, 2);

        Assert.False(_service.CanUndo(game));
        Assert.Throws<InvalidMoveException>(() => _service.UndoLastMove(game.Id));
    }

    [Fact]
    public void Scoreboard_UpdatesOnlyOncePerCompletedGame()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        Play(game, "X", 0, 0);
        Play(game, "O", 1, 0);
        Play(game, "X", 0, 1);
        Play(game, "O", 1, 1);
        Play(game, "X", 0, 2); // X wins

        // Trying to keep playing must not change the score again
        Assert.Throws<InvalidMoveException>(() => Play(game, "O", 2, 2));

        var score = _scoreboard.GetScoreboard();
        Assert.Equal(1, score.XWins);
        Assert.Equal(0, score.OWins);
        Assert.Equal(0, score.Draws);
    }

    [Fact]
    public void ResetScoreboard_SetsAllCountsToZero()
    {
        _scoreboard.RecordWin("X");
        _scoreboard.RecordDraw();

        var score = _scoreboard.ResetScoreboard();

        Assert.Equal(0, score.XWins);
        Assert.Equal(0, score.OWins);
        Assert.Equal(0, score.Draws);
    }

    [Fact]
    public void ComputerMode_ComputerRepliesAutomaticallyWithCenter()
    {
        var game = _service.CreateGame(GameMode.Computer);

        Play(game, "X", 0, 0);

        Assert.Equal("O", game.Board[4]);           // computer took the center
        Assert.Equal("X", game.CurrentPlayer);      // back to the human
    }

    [Fact]
    public void ComputerMode_ComputerDoesNotMoveAfterHumanWins()
    {
        // Human X wins with the move that fills the row; computer must not reply.
        var game = _service.CreateGame(GameMode.Computer);
        game.Board = new[] { "X", "X", "", "O", "O", "", "", "", "" };
        game.Moves = new List<Move>
        {
            new() { MoveNumber = 1, Player = "X", Row = 0, Column = 0 },
            new() { MoveNumber = 2, Player = "O", Row = 1, Column = 0 },
            new() { MoveNumber = 3, Player = "X", Row = 0, Column = 1 },
            new() { MoveNumber = 4, Player = "O", Row = 1, Column = 1 },
        };

        Play(game, "X", 0, 2);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal("X", game.Winner);
        Assert.Equal(5, game.Moves.Count);          // no extra computer move
    }
}
