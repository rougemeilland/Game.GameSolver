using System.Linq;

namespace Game.SudokuSolver.GUI.Models
{
    internal sealed partial class MainWindowModel
        : Model
    {
        public MainWindowModel()
        {
            var game = new SudokuGameBoard(CreateInitialBoard());
            var solution = game.Solve().First();

            SlideShow = new SudokuBoardSolutionModel(solution);
        }

        public SudokuBoardSolutionModel SlideShow { get; }
    }
}
