using System.Collections.Generic;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// <see cref="SudokuBoardState"/> の読み込み専用のコレクションにインターフェースです。
    /// </summary>
    public interface ISudokuBoardStateCollection
        : IEnumerable<SudokuBoardState>, IReadOnlyVectorIndexer<SudokuBoardState, int>
    {
    }
}
