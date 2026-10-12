using System;

namespace Game.SudokuSolver.GUI
{
    [Flags]
    internal enum BoardCellNoteBitSet
        : ushort
    {
        None = 0,
        AllCells = (1 << 9) - 1,
    }
}
