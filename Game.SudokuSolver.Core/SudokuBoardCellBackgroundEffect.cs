namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの背景効果を示す列挙体です。
    /// </summary>
    /// <remarks>
    /// 同じセルに異なる <see cref="SudokuBoardCellBackgroundEffect"/> 値が指定されている場合、値が大きい方が優先して適用されます。
    /// </remarks>
    public enum SudokuBoardCellBackgroundEffect
    {
        /// <summary>
        /// 強調されていない背景効果です。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 肯定的に強調されている背景効果です。(タイプ2とタイプ3の点滅)
        /// </summary>
        PositivelyHighlightedBlinkingType2AndType3,

        /// <summary>
        /// 肯定的に強調されている背景効果です。(タイプ1とタイプ2の点滅)
        /// </summary>
        PositivelyHighlightedBlinkingType1AndType2,

        /// <summary>
        /// 肯定的に強調されている背景効果です。(タイプ3)
        /// </summary>
        PositivelyHighlightedType3,

        /// <summary>
        /// 肯定的に強調されている背景効果です。(タイプ2)
        /// </summary>
        PositivelyHighlightedType2,

        /// <summary>
        /// 肯定的に強調されている背景効果です。(タイプ1)
        /// </summary>
        PositivelyHighlightedType1,

        /// <summary>
        /// 肯定的に強調されている背景効果です。
        /// </summary>
        PrimaryPositivelyHighlighted = PositivelyHighlightedType1,

        /// <summary>
        /// 否定的に強調されている背景効果です。
        /// </summary>
        NegativelyHighlighted,
    }
}
