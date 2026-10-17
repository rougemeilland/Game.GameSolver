using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルのコレクションのインターフェースです。
    /// </summary>
    public interface IReadOnlySudokuBoardCellCollection
            : IReadOnlyVectorIndexer<SudokuBoardCell, SudokuBoardCellIndex>
    {
    }
}
