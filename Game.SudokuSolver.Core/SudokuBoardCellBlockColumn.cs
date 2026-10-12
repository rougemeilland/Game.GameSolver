namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のブロック内のセルの列のオフセットを示す列挙体です。
    /// </summary>
    internal enum SudokuBoardCellBlockColumn
    {
        /// <summary>
        /// オフセット 0
        /// </summary>
        Zero = 0,

        /// <summary>
        /// 最小値
        /// </summary>
        MinValue = 0,

        /// <summary>
        /// 最大値
        /// </summary>
        MaxValue = 2,
    }
}
