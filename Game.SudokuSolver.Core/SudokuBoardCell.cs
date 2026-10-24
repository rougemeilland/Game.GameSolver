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
        /// <param name="backgroundEffect">
        /// セルの背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値です。
        /// </param>
        protected SudokuBoardCell(SudokuBoardCellBackgroundEffect backgroundEffect)
        {
            BackgroundEffect = backgroundEffect;
        }

        /// <summary>
        /// セルの背景効果を示す値を取得します。
        /// </summary>
        /// <value>
        /// セリの背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値です。
        /// </value>
        public SudokuBoardCellBackgroundEffect BackgroundEffect { get; }
    }
}
