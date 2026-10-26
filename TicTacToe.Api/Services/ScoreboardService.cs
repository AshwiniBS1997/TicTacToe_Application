using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>Session-level scoreboard shared by all games while the API is running.</summary>
public class ScoreboardService
{
    private readonly object _lock = new();
    private int _xWins;
    private int _oWins;
    private int _draws;

    public Scoreboard GetScoreboard()
    {
        lock (_lock)
        {
            return new Scoreboard { XWins = _xWins, OWins = _oWins, Draws = _draws };
        }
    }

    public void RecordWin(string player)
    {
        lock (_lock)
        {
            if (player == Players.X) _xWins++;
            else if (player == Players.O) _oWins++;
        }
    }

    public void RecordDraw()
    {
        lock (_lock)
        {
            _draws++;
        }
    }

    public Scoreboard ResetScoreboard()
    {
        lock (_lock)
        {
            _xWins = 0;
            _oWins = 0;
            _draws = 0;
        }
        return GetScoreboard();
    }
}
