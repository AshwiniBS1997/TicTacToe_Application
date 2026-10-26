namespace TicTacToe.Api.Models;

/// <summary>One entry in the move history. Row and Column are 0-based (0..2).</summary>
public class Move
{
    public int MoveNumber { get; set; }
    public string Player { get; set; } = Players.Empty;
    public int Row { get; set; }
    public int Column { get; set; }

    /// <summary>Human-friendly, 1-based text, e.g. "Row 1, Column 1".</summary>
    public string Position => $"Row {Row + 1}, Column {Column + 1}";
}
