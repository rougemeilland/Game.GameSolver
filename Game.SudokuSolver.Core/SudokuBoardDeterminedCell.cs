using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の数字が確定されたセルのクラスです。
    /// </summary>
    public class SudokuBoardDeterminedCell
        : SudokuBoardCell
    {
        [Flags]
        private enum CellStatus
            : byte
        {
            None = 0,
            InitialSet = 1 << 0,
            PositiveHighlight = 1 << 1,
        }

        private readonly CellStatus _status;

        private SudokuBoardDeterminedCell(bool isHighlightedBackground, SudokuBoardCellDigit determinedDigit, CellStatus status)
            : base(isHighlightedBackground)
        {
            DeterminedDigit = determinedDigit;
            _status = status;
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
        public bool IsInitialSet => (_status & CellStatus.InitialSet) != CellStatus.None;

        /// <summary>
        /// 強調表示されるべきかどうかを取得します。
        /// </summary>
        /// <value>
        /// 強調表示されるべきかどうかを示す  <see cref="bool"/> 値です。
        /// 強調表示されるべきであれば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public bool IsPositivelyHighlighted => (_status & CellStatus.PositiveHighlight) != CellStatus.None;

        internal static SudokuBoardDeterminedCell CreateInitialCell(bool isHighlightedBackground, SudokuBoardCellDigit cellDigit) => new(isHighlightedBackground, cellDigit, CellStatus.InitialSet);
        internal static SudokuBoardDeterminedCell CreateUpdatedCell(bool isHighlightedBackground, SudokuBoardCellDigit cellDigit) => new(isHighlightedBackground, cellDigit, CellStatus.None);
        internal static SudokuBoardDeterminedCell CreatePositivelyHighlightedCell(bool isHighlightedBackground, SudokuBoardCellDigit cellDigit) => new(isHighlightedBackground, cellDigit, CellStatus.PositiveHighlight);
    }
}
