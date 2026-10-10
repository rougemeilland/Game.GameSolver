using System;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCellPosition
    {
        public BoardCellPosition(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            var (column, row) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
        }

        public BoardCellPosition(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif
            Column = column;
            Row = row;
        }

        public BoardCellColumn Column { get; }
        public BoardCellRow Row { get; }
    }
}
