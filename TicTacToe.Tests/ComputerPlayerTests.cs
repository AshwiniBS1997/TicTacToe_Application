using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

/// <summary>Checks each step of the computer's priority list.</summary>
public class ComputerPlayerTests
{
    private readonly ComputerPlayer _computer = new();

    [Fact]
    public void Priority1_TakesWinningMove()
    {
        // O can win at index 5; X could also win at index 2 -> winning beats blocking
        string[] board =
        {
            "X", "X", "",
            "O", "O", "",
            "X", "",  ""
        };

        Assert.Equal(5, _computer.ChooseMove(board));
    }

    [Fact]
    public void Priority2_BlocksX()
    {
        string[] board =
        {
            "X", "X", "",
            "",  "O", "",
            "",  "",  ""
        };

        Assert.Equal(2, _computer.ChooseMove(board));
    }

    [Fact]
    public void Priority3_TakesCenter()
    {
        string[] board =
        {
            "X", "", "",
            "",  "", "",
            "",  "", ""
        };

        Assert.Equal(4, _computer.ChooseMove(board));
    }

    [Fact]
    public void Priority4_TakesCorner()
    {
        string[] board =
        {
            "",  "", "",
            "",  "X", "",
            "",  "", ""
        };

        Assert.Equal(0, _computer.ChooseMove(board));
    }

    [Fact]
    public void Priority5_TakesAnyFreeCell()
    {
        // Center and all corners are taken, and nobody can win next move.
        // Only cells 1 and 7 are free.
        string[] board =
        {
            "O", "",  "X",
            "X", "X", "O",
            "O", "",  "X"
        };

        int move = _computer.ChooseMove(board);

        Assert.True(board[move] == "", "Computer must pick an empty cell");
        Assert.Contains(move, new[] { 1, 7 });
    }
}
