using Game.GameSolver;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI
{
    internal interface IReadOnlyCellsIndexer
            : IReadOnlyVectorIndexer<SudokuBoardCellModel, int>, IReadOnlyVectorIndexer<SudokuBoardCellModel, SudokuBoardCellIndex>
    {
    }
}
