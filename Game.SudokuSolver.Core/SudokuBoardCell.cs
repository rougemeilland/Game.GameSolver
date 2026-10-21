namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの抽象クラスです。
    /// </summary>
    public abstract class SudokuBoardCell
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="isPositivelyHighlightedBackground">
        /// セルの背景を肯定的に強調するかどうかの <see cref="bool"/> 値です。
        /// セルの背景を肯定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </param>
        /// <param name="isNegativelyHighlightedBackground">
        /// セルの背景を否定的に強調するかどうかの <see cref="bool"/> 値です。
        /// セルの背景を否定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </param>
        protected SudokuBoardCell(bool isPositivelyHighlightedBackground, bool isNegativelyHighlightedBackground)
        {
            IsPositivelyHighlightedBackground = isPositivelyHighlightedBackground;
            IsNegativelyHighlightedBackground = isNegativelyHighlightedBackground && !isPositivelyHighlightedBackground;
        }

        /// <summary>
        /// セルの背景を肯定的に強調するかどうかの <see cref="bool"/> 値を取得します。
        /// </summary>
        /// <value>
        /// セルの背景を肯定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public bool IsPositivelyHighlightedBackground { get; }

        /// <summary>
        /// セルの背景を否定的に強調するかどうかの <see cref="bool"/> 値を取得します。
        /// </summary>
        /// <value>
        /// セルの背景を否定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public bool IsNegativelyHighlightedBackground { get; }
    }
}
