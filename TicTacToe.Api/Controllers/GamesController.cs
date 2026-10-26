using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

/// <summary>
/// Game endpoints. Each action method handles exactly one feature.
/// The controller has no game rules: it calls GameService and turns the result
/// (or the error) into an HTTP response.
/// </summary>
[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ScoreboardService _scoreboardService;

    public GamesController(GameService gameService, ScoreboardService scoreboardService)
    {
        _gameService = gameService;
        _scoreboardService = scoreboardService;
    }

    /// <summary>POST /api/games — create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        Game game = _gameService.CreateGame(request.Mode);
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, BuildResponse(game));
    }

    /// <summary>GET /api/games/{id} — get the current game state.</summary>
    [HttpGet("{id:guid}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        try
        {
            Game game = _gameService.GetGame(id);
            return Ok(BuildResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/moves — submit a player move.</summary>
    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MoveRequest request)
    {
        try
        {
            Game game = _gameService.MakeMove(id, request.Player, request.Row, request.Column);
            return Ok(BuildResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/undo — undo the last move (or move pair in Computer mode).</summary>
    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateResponse> UndoLastMove(Guid id)
    {
        try
        {
            Game game = _gameService.UndoLastMove(id);
            return Ok(BuildResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/reset — reset the board, keep the scoreboard.</summary>
    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateResponse> ResetGame(Guid id)
    {
        try
        {
            Game game = _gameService.ResetGame(id);
            return Ok(BuildResponse(game));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>Converts the internal Game into the JSON shape the frontend uses.</summary>
    private GameStateResponse BuildResponse(Game game)
    {
        return GameStateResponse.FromGame(
            game,
            _scoreboardService.GetScoreboard(),
            _gameService.CanUndo(game));
    }
}
