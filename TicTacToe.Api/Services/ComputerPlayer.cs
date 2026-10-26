using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Simple rule-based computer opponent. It always plays O.
/// Priority (from the problem statement):
///   1. Win if possible
///   2. Block X if X can win next
///   3. Take the center
///   4. Take a corner
///   5. Take any free cell
/// </summary>
public class ComputerPlayer
{
    private const int CenterCell = 4;
    private static readonly int[] CornerCells = { 0, 2, 6, 8 };

    /// <summary>Returns the board index (0..8) the computer wants to play.</summary>
    public int ChooseMove(string[] board)
    {
        int? winningMove = FindWinningMove(board, Players.O);
        if (winningMove.HasValue) return winningMove.Value;

        int? blockingMove = FindWinningMove(board, Players.X);
        if (blockingMove.HasValue) return blockingMove.Value;

        if (board[CenterCell] == Players.Empty) return CenterCell;

        int? corner = FindFreeCorner(board);
        if (corner.HasValue) return corner.Value;

        int? anyCell = FindAnyFreeCell(board);
        if (anyCell.HasValue) return anyCell.Value;

        throw new InvalidMoveException("The computer has no available cell to play.");
    }

    /// <summary>
    /// Tries each empty cell: "if this player put a mark here, would they win?"
    /// Used both for winning (player = O) and for blocking (player = X).
    /// </summary>
    public int? FindWinningMove(string[] board, string player)
    {
        for (int index = 0; index < board.Length; index++)
        {
            if (board[index] != Players.Empty) continue;

            string[] testBoard = board.ToArray();   // copy so we don't change the real board
            testBoard[index] = player;

            int[]? line = GameRules.FindWinningLine(testBoard);
            if (line != null && testBoard[line[0]] == player)
            {
                return index;
            }
        }
        return null;
    }

    public int? FindFreeCorner(string[] board)
    {
        foreach (int corner in CornerCells)
        {
            if (board[corner] == Players.Empty) return corner;
        }
        return null;
    }

    public int? FindAnyFreeCell(string[] board)
    {
        for (int index = 0; index < board.Length; index++)
        {
            if (board[index] == Players.Empty) return index;
        }
        return null;
    }
}
