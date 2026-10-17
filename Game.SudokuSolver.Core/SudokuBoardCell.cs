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
        /// <param name="isHighlightedBackground">
        /// セルの背景を強調するかどうかの <see cref="bool"/> 値です。セルの背景を強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </param>
        protected SudokuBoardCell(bool isHighlightedBackground)
        {
            IsHighlightedBackground = isHighlightedBackground;
        }

        /// <summary>
        /// セルの背景を強調するかどうかの <see cref="bool"/> 値を取得します。
        /// </summary>
        /// <value>
        /// セルの背景を強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public bool IsHighlightedBackground { get; }
    }
}
