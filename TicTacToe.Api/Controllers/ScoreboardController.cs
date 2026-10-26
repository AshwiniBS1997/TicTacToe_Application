using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly ScoreboardService _scoreboardService;

    public ScoreboardController(ScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>GET /api/scoreboard — get X wins, O wins and draws.</summary>
    [HttpGet]
    public ActionResult<Scoreboard> GetScoreboard()
    {
        return Ok(_scoreboardService.GetScoreboard());
    }

    /// <summary>POST /api/scoreboard/reset — set all counts back to 0.</summary>
    [HttpPost("reset")]
    public ActionResult<Scoreboard> ResetScoreboard()
    {
        return Ok(_scoreboardService.ResetScoreboard());
    }
}
