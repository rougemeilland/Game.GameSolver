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
        /// 肯定的に強調されている背景効果です。(タイプ4)
        /// </summary>
        /// <remarks>
        /// <see cref="PositivelyHighlightedType2"/> の効果と <see cref="PositivelyHighlightedType3"/> の効果をアニメーションで繰り返すような効果を推奨します。
        /// </remarks>
        PositivelyHighlightedType4,

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
