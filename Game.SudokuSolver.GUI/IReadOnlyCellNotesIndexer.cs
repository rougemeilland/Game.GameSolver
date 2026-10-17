using Game.GameSolver;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI
{
    internal interface IReadOnlyCellNotesIndexer
            : IReadOnlyVectorIndexer<SudokuBoardCellNoteModel, int>, IReadOnlyVectorIndexer<SudokuBoardCellNoteModel, SudokuBoardCellDigit>
    {
    }
}
