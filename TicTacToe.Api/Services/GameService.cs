using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// All game rules live here: creating games, validating and applying moves,
/// win/draw detection, the computer's turn, undo and reset.
/// Controllers only translate HTTP <-> these methods.
/// </summary>
public class GameService
{
    private readonly GameStore _gameStore;
    private readonly ScoreboardService _scoreboardService;
    private readonly ComputerPlayer _computerPlayer;

    // One lock so two requests can't change game state at the same time.
    private readonly object _lock = new();

    public GameService(GameStore gameStore, ScoreboardService scoreboardService, ComputerPlayer computerPlayer)
    {
        _gameStore = gameStore;
        _scoreboardService = scoreboardService;
        _computerPlayer = computerPlayer;
    }

    // ------------------------------------------------------------------
    // Public methods (one per feature)
    // ------------------------------------------------------------------

    /// <summary>Creates a new game. X always starts.</summary>
    public Game CreateGame(GameMode mode)
    {
        var game = new Game { Mode = mode };
        _gameStore.Save(game);
        return game;
    }

    /// <summary>Gets a game by id, or throws GameNotFoundException.</summary>
    public Game GetGame(Guid gameId)
    {
        return _gameStore.Find(gameId) ?? throw new GameNotFoundException(gameId);
    }

    /// <summary>
    /// Plays a human move. In Computer mode, the computer (O) replies automatically
    /// in the same request if the game is still in progress.
    /// </summary>
    public Game MakeMove(Guid gameId, string player, int row, int column)
    {
        lock (_lock)
        {
            Game game = GetGame(gameId);

            ValidateMove(game, player, row, column);
            ApplyMove(game, player, row, column);
            UpdateGameStatus(game);

            if (ShouldComputerPlay(game))
            {
                PlayComputerMove(game);
            }

            return game;
        }
    }

    /// <summary>
    /// Undo rules:
    ///  - Two Player mode: remove only the last move.
    ///  - Computer mode: remove the computer's move AND the human move before it.
    ///  - Not allowed when there are no moves, or the game is finished (Option A).
    /// </summary>
    public Game UndoLastMove(Guid gameId)
    {
        lock (_lock)
        {
            Game game = GetGame(gameId);

            if (game.Moves.Count == 0)
                throw new InvalidMoveException("There are no moves to undo.");

            if (game.Status != GameStatus.InProgress)
                throw new InvalidMoveException("Undo is disabled after the game is completed.");

            Move removedMove = RemoveLastMove(game);

            bool computerMoveWasRemoved = game.Mode == GameMode.Computer && removedMove.Player == Players.O;
            if (computerMoveWasRemoved && game.Moves.Count > 0)
            {
                RemoveLastMove(game);   // also remove the human (X) move before it
            }

            RecalculateStatus(game);
            return game;
        }
    }

    /// <summary>Clears the board and history and starts again with X. Scoreboard is NOT touched.</summary>
    public Game ResetGame(Guid gameId)
    {
        lock (_lock)
        {
            Game game = GetGame(gameId);

            game.Board = Game.CreateEmptyBoard();
            game.Moves.Clear();
            game.CurrentPlayer = Players.X;
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells.Clear();
            game.IsResultRecorded = false;

            return game;
        }
    }

    /// <summary>Undo is allowed only when there is a move to undo and the game is still running.</summary>
    public bool CanUndo(Game game)
    {
        return game.Moves.Count > 0 && game.Status == GameStatus.InProgress;
    }

    // ------------------------------------------------------------------
    // Private helpers (each does one small job)
    // ------------------------------------------------------------------

    /// <summary>Throws InvalidMoveException if the move breaks any rule. Does not change the game.</summary>
    private static void ValidateMove(Game game, string player, int row, int column)
    {
        if (game.Status != GameStatus.InProgress)
            throw new InvalidMoveException("The game is already completed. Reset to play again.");

        if (player != Players.X && player != Players.O)
            throw new InvalidMoveException("Player must be 'X' or 'O'.");

        if (player != game.CurrentPlayer)
            throw new InvalidMoveException($"It is {game.CurrentPlayer}'s turn, not {player}'s.");

        if (!GameRules.IsInsideBoard(row, column))
            throw new InvalidMoveException("The move is outside the board. Row and column must be 0, 1 or 2.");

        int index = GameRules.ToIndex(row, column);
        if (game.Board[index] != Players.Empty)
            throw new InvalidMoveException("That cell is already taken.");
    }

    /// <summary>Puts the mark on the board, records history and switches turn.</summary>
    private static void ApplyMove(Game game, string player, int row, int column)
    {
        int index = GameRules.ToIndex(row, column);
        game.Board[index] = player;

        game.Moves.Add(new Move
        {
            MoveNumber = game.Moves.Count + 1,
            Player = player,
            Row = row,
            Column = column
        });

        game.CurrentPlayer = GameRules.OtherPlayer(player);
    }

    /// <summary>Works out Won / Draw / InProgress, then updates the scoreboard once if the game just ended.</summary>
    private void UpdateGameStatus(Game game)
    {
        RecalculateStatus(game);
        RecordResultOnScoreboard(game);
    }

    /// <summary>Reads the board and sets Status, Winner and WinningCells. Does NOT touch the scoreboard.</summary>
    private static void RecalculateStatus(Game game)
    {
        int[]? winningLine = GameRules.FindWinningLine(game.Board);

        if (winningLine != null)
        {
            game.Status = GameStatus.Won;
            game.Winner = game.Board[winningLine[0]];
            game.WinningCells = winningLine.ToList();
        }
        else if (GameRules.IsBoardFull(game.Board))
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells = new List<int>();
        }
        else
        {
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells = new List<int>();
        }
    }

    /// <summary>Adds the result to the scoreboard exactly once per completed game.</summary>
    private void RecordResultOnScoreboard(Game game)
    {
        if (game.Status == GameStatus.InProgress || game.IsResultRecorded)
            return;

        if (game.Status == GameStatus.Won && game.Winner != null)
            _scoreboardService.RecordWin(game.Winner);
        else if (game.Status == GameStatus.Draw)
            _scoreboardService.RecordDraw();

        game.IsResultRecorded = true;
    }

    private static bool ShouldComputerPlay(Game game)
    {
        return game.Mode == GameMode.Computer
            && game.Status == GameStatus.InProgress
            && game.CurrentPlayer == Players.O;
    }

    private void PlayComputerMove(Game game)
    {
        int index = _computerPlayer.ChooseMove(game.Board);
        ApplyMove(game, Players.O, GameRules.ToRow(index), GameRules.ToColumn(index));
        UpdateGameStatus(game);
    }

    /// <summary>Removes the newest move from board + history and gives the turn back to that player.</summary>
    private static Move RemoveLastMove(Game game)
    {
        Move lastMove = game.Moves[^1];

        game.Board[GameRules.ToIndex(lastMove.Row, lastMove.Column)] = Players.Empty;
        game.Moves.RemoveAt(game.Moves.Count - 1);
        game.CurrentPlayer = lastMove.Player;

        return lastMove;
    }
}
