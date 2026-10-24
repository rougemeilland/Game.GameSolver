namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの背景効果を示す列挙体です。
    /// </summary>
    public enum SudokuBoardCellBackgroundEffect
    {
        /// <summary>
        /// 強調されていない背景効果です。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 強く肯定的に強調されている背景効果です。
        /// </summary>
        StrongPotisivelyHighlighted,

        /// <summary>
        /// 弱く肯定的に強調されている背景効果です。
        /// </summary>
        WeakPositivelyHighlighted,

        /// <summary>
        /// 否定的に強調されている背景効果です。
        /// </summary>
        NegativelyHighlighted,
    }
}
