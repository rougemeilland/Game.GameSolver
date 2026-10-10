namespace Game.SudokuSolver.GUI
{
    /// <summary>
    /// セルの位置を示す列挙体です。
    /// </summary>
    internal enum BoardCellIndex
    {
        /// <summary>
        /// 最初の値
        /// </summary>
        FirstValue = 0,

        /// <summary>
        /// 最小値
        /// </summary>
        MinValue = 0,

        /// <summary>
        /// 最大値
        /// </summary>
        MaxValue = 80,

        /// <summary>
        /// 未定義
        /// </summary>
        Undefined = int.MaxValue,
    }
}
