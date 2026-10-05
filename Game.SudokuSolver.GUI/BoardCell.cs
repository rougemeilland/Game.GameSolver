using System;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCell
    {
        public BoardCell(int column, int row, int digit)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
            ArgumentOutOfRangeException.ThrowIfLessThan(digit, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9);
#endif
            Column = (byte)column;
            Row = (byte)row;
            Digit = (byte)digit;
        }

        public byte Column { get; }
        public byte Row { get; }
        public byte Digit { get; }
    }
}
