namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの前景効果を示す列挙体です。
    /// </summary>
    public enum SudokuBoardCellForegroundEffect
    {
        /// <summary>
        /// 確定していないセルの前景効果です。
        /// </summary>
        NotDetermined = 0,

        /// <summary>
        /// 通常の前景効果です。
        /// </summary>
        Normal,

        /// <summary>
        /// 初期配置では確定していないセルの前景効果です。
        /// </summary>
        Updated,

        /// <summary>
        /// 肯定的に強調される前景効果です。
        /// </summary>
        PositivelyHighlighted,

        /// <summary>
        /// 否定的に強調される前景効果です。
        /// </summary>
        NegativelyHighlighted,
    }
}
