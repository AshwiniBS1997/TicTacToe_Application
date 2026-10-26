using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Pure helper functions about the board. No state is stored here,
/// which makes these very easy to test and reuse (GameService and ComputerPlayer both use them).
/// </summary>
public static class GameRules
{
    public const int BoardSize = 3;

    /// <summary>All 8 ways to win: 3 rows, 3 columns, 2 diagonals (as board indexes).</summary>
    public static readonly int[][] WinningLines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, // rows
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, // columns
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                     // diagonals
    };

    public static bool IsInsideBoard(int row, int column)
    {
        return row >= 0 && row < BoardSize && column >= 0 && column < BoardSize;
    }

    /// <summary>Converts (row, column) to a flat index. Example: (1, 2) -> 5.</summary>
    public static int ToIndex(int row, int column) => row * BoardSize + column;

    public static int ToRow(int index) => index / BoardSize;

    public static int ToColumn(int index) => index % BoardSize;

    /// <summary>Returns the 3 winning cell indexes, or null if nobody has won.</summary>
    public static int[]? FindWinningLine(string[] board)
    {
        foreach (var line in WinningLines)
        {
            string first = board[line[0]];
            if (first != Players.Empty && first == board[line[1]] && first == board[line[2]])
            {
                return line;
            }
        }
        return null;
    }

    public static bool IsBoardFull(string[] board)
    {
        return board.All(cell => cell != Players.Empty);
    }

    public static string OtherPlayer(string player)
    {
        return player == Players.X ? Players.O : Players.X;
    }
}
