namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの列を示す列挙体です。
    /// </summary>
    internal enum SudokuBoardCellColumn
    {
        /// <summary>
        /// 1 列目
        /// </summary>
        One = 0,

        /// <summary>
        /// 最小値
        /// </summary>
        MinValue = One,

        /// <summary>
        /// 最大値
        /// </summary>
        MaxValue = 8,
    }
}
