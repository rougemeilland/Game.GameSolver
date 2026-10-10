namespace Game.SudokuSolver.GUI
{
    /// <summary>
    /// セルのブロックを示す列挙体です。
    /// </summary>
    internal enum BoardCellBlock
    {
        /// <summary>
        /// 上段左
        /// </summary>
        TopLeft = 0,

        /// <summary>
        /// 上段中央
        /// </summary>
        TopCenter = 1,

        /// <summary>
        /// 上段右
        /// </summary>
        TopRight = 2,

        /// <summary>
        /// 中段左
        /// </summary>
        CenterLeft = 3,

        /// <summary>
        /// 中段中央
        /// </summary>
        CenterCenter = 4,

        /// <summary>
        /// 中段右
        /// </summary>
        CenterRight = 5,

        /// <summary>
        /// 下段左
        /// </summary>
        BottomLeft = 6,

        /// <summary>
        /// 下段中央
        /// </summary>
        BottomCenter = 7,

        /// <summary>
        /// 下段右
        /// </summary>
        BottomRight = 8,

        /// <summary>
        /// 最初の値
        /// </summary>
        FirstValue = TopLeft,

        /// <summary>
        /// 最小値
        /// </summary>
        MinValue = TopLeft,

        /// <summary>
        /// 最大値
        /// </summary>
        MaxValue = BottomRight,
    }
}
