using System;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCell
    {
        public BoardCell(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            var (column, row) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
            Digit = digit;
        }

        public BoardCell(BoardCellColumn column, BoardCellRow row, BoardCellDigit digit)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            Column = column;
            Row = row;
            Digit = digit;
        }

        public BoardCellColumn Column { get; }
        public BoardCellRow Row { get; }
        public BoardCellDigit Digit { get; }
    }
}
