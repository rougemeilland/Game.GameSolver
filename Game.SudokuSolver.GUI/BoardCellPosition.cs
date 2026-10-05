using System;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCellPosition
    {
        public BoardCellPosition(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
#endif
            Column = (byte)column;
            Row = (byte)row;
        }

        public byte Column { get; }
        public byte Row { get; }
    }
}
