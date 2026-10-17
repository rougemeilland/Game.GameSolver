namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの行を示す列挙体です。
    /// </summary>
    internal enum SudokuBoardCellRow
    {
        /// <summary>
        /// 一行目
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
