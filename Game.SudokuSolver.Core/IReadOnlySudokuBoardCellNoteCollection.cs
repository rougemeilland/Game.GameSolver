using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの数字の候補のコレクションのインターフェースです。
    /// </summary>
    public interface IReadOnlySudokuBoardCellNoteCollection
            : IReadOnlyVectorIndexer<SudokuBoardCellNote, SudokuBoardCellDigit>
    {
    }
}
