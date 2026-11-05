using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルのブロックのビット集合を示す列挙体です。
    /// </summary>
    [Flags]
    internal enum SudokuBoardCellBlockBitSet
        : ushort
    {
        /// <summary>
        /// 空集合です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 全てのブロックを含む集合です。
        /// </summary>
        AllBlocks = (1 << 9) - 1,
    }
}
