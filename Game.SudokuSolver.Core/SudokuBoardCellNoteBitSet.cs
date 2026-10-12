using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のひとつのセルの数字の候補のビット集合の列挙体です。
    /// </summary>
    [Flags]
    internal enum SudokuBoardCellNoteBitSet
        : ushort
    {
        None = 0,
        AllCells = (1 << 9) - 1,
    }
}
