namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の数字が確定されたセルのクラスです。
    /// </summary>
    public class SudokuBoardDeterminedCell
        : SudokuBoardCell
    {
        internal SudokuBoardDeterminedCell(SudokuBoardCellBackgroundEffect backgroundEffect, SudokuBoardCellDigit determinedDigit, SudokuBoardCellForegroundEffect foregroundEffect)
            : base(backgroundEffect)
        {
            DeterminedDigit = determinedDigit;
            ForegroundEffect = foregroundEffect;
        }

        /// <summary>
        /// 確定された数字を取得します。
        /// </summary>
        /// <value>
        /// 確定された数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </value>
        public SudokuBoardCellDigit DeterminedDigit { get; }

        /// <summary>
        /// 最初の盤面 (設問) から確定していたかどうかを取得します。
        /// </summary>
        /// <value>
        /// 最初の盤面から確定していたかどうかを示す <see cref="bool"/> 値です。
        /// 最初の盤面から確定していたならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public SudokuBoardCellForegroundEffect ForegroundEffect { get; }
    }
}
