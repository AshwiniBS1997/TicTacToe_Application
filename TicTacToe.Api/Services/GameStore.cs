using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// In-memory storage for game sessions. Data is lost when the API stops,
/// which the problem statement allows.
/// </summary>
public class GameStore
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public void Save(Game game)
    {
        _games[game.Id] = game;
    }

    public Game? Find(Guid id)
    {
        _games.TryGetValue(id, out var game);
        return game;
    }
}
