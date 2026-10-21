using System.Globalization;
using System.Linq;

namespace Game.SudokuSolver.GUI.Models
{
    internal partial class MainWindowModel
    {
        internal static readonly int[] sourceArray_Beginner =
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

        internal static readonly int[] sourceArray_Fails =
        [
            0, 0, 0, 0, 0, 0, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 3, 0, 0,
            1, 2, 3, 4, 5, 6, 0, 8, 9,
            0, 0, 0, 0, 0, 0, 5, 0, 0,
            0, 0, 0, 0, 0, 0, 6, 0, 0,
            0, 0, 0, 0, 0, 0, 7, 0, 0,
            0, 0, 0, 0, 0, 0, 8, 0, 0,
            0, 0, 0, 0, 0, 0, 9, 0, 0,
        ];

        private static string[] CreateInitialBoard()
            => [.. sourceArray_Beginner.Select(n => n is >= 1 and <= 9 ? n.ToString(CultureInfo.InvariantCulture) : "")];
    }
}
