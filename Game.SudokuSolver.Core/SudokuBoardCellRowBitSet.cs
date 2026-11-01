using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの行のビット集合を示す列挙体です。
    /// </summary>
    [Flags]
    internal enum SudokuBoardCellRowBitSet
    {
        /// <summary>
        /// 空集合です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 全ての行を含む集合です。
        /// </summary>
        AllRows = (1 << 9) - 1,
    }
}
