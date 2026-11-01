using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のひとつのセルの数字の候補のビット集合の列挙体です。
    /// </summary>
    [Flags]
    internal enum SudokuBoardCellDigitBitSet
        : ushort
    {
        /// <summary>
        /// 空集合です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 全ての数字の候補を含む集合です。
        /// </summary>
        AllCellDigits = (1 << 9) - 1,
    }
}
