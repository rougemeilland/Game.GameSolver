using System.Collections.Generic;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの数字の候補のコレクションを示すインターフェースです。
    /// </summary>
    public interface ISudokuBoardCellNoteCollection
        : IEnumerable<SudokuBoardCellPositionDigit>, IReadOnlyVectorIndexer<SudokuBoardCellPositionDigit, int>
    {
    }
}
