using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの列のビット集合を示す列挙体です。
    /// </summary>
    [Flags]
    internal enum SudokuBoardCellColumnBitSet
        : ushort
    {
        /// <summary>
        /// 空集合です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 全ての列を含む集合です。
        /// </summary>
        AllColumns = (1 << 9) -1,
    }
}
