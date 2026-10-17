using System.Globalization;
using System.Linq;

namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class MainWindowModel
        : Model
    {
        internal static readonly int[] sourceArray =
        [
            0, 1, 9, 0, 6, 2, 0, 5, 3,
            5, 4, 7, 0, 3, 8, 6, 2, 9,
            0, 3, 0, 9, 7, 5, 4, 1, 8,
            0, 8, 5, 3, 9, 0, 2, 0, 1,
            0, 2, 1, 0, 8, 4, 0, 7, 0,
            7, 0, 4, 2, 0, 6, 3, 8, 0,
            4, 0, 2, 8, 5, 9, 0, 3, 7,
            1, 7, 8, 0, 4, 3, 5, 9, 2,
            9, 5, 0, 0, 2, 1, 8, 6, 4,
        ];

        public MainWindowModel()
        {
            var state =
                new SudokuBoardState(
                    sourceArray
                    .Select(n => n is >= 1 and <= 9 ? n.ToString(CultureInfo.InvariantCulture) : "")
                .ToArray());
            Board = new SudokuBoardModel(state.GetCells());    
        }

        public SudokuBoardModel Board { get; }
    }
}
