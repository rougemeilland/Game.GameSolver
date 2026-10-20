using Game.GameSolver;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI
{
    internal interface IReadOnlySudokuBoardModelsIndexer
        : IReadOnlyVectorIndexer<SudokuBoardModel, int>
    {
    }
}
