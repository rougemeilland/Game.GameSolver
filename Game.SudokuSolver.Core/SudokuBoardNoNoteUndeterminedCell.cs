namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の数字数字の候補が存在しないセルのクラスです。
    /// </summary>
    public class SudokuBoardNoNoteUndeterminedCell
        : SudokuBoardCell
    {
        private SudokuBoardNoNoteUndeterminedCell(bool isHighlightedBackground)
            : base(isHighlightedBackground)
        {
        }

        internal static SudokuBoardNoNoteUndeterminedCell CreateCell(bool isHighlightedBackground) => new(isHighlightedBackground);
    }
}
