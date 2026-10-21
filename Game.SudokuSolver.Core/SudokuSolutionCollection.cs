using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の解答のコレクションのクラスです。
    /// </summary>
    public sealed class SudokuSolutionCollection
    {
        internal SudokuSolutionCollection(GameSolutionCollection<SudokuBoardState, SudokuBoardOperation> solutions)
        {
            SolutionCount = solutions.SolutionCount;
            ActualSolutionCount = solutions.ActualSolutionCount;
            Solutions = solutions.Solutions.Select(solution => new SudokuSolution(solution)).ToList();
        }

        /// <summary>
        /// 解答の数を取得します。
        /// </summary>
        /// <value>
        /// 解答の数を示す <see cref="int"/> 値です。
        /// </value>
        public int SolutionCount { get; }

        /// <summary>
        /// 実際の解答の数です。
        /// </summary>
        /// <value>
        /// 実際の解答の数を示す <see cref="int"/> 値です。
        /// </value>
        /// <remarks>
        /// 数条は <see cref="SolutionCount"/> と <see cref="ActualSolutionCount"/> は共に <see cref="Solutions"/> の要素数、つまり解答の数と等しくなります。
        /// しかし、解答の数が多すぎる場合は解答が途中で打ち切られ、<see cref="SolutionCount"/> の値は全ての解答の数と等しくなり、<see cref="ActualSolutionCount"/> の値は実際に <see cref="Solutions"/> で列挙することが出来る要素の数と等しくなります。
        /// </remarks>
        public int ActualSolutionCount { get; }

        /// <summary>
        /// 解答のコレクションの列挙子を取得します。
        /// </summary>
        /// <value>
        /// 解答を示す <see cref="GameSolutionCollection{STATE_T, OPERATION_T}"/> オブジェクトの列挙子です。
        /// </value>
        public IEnumerable<SudokuSolution> Solutions { get; }
    }
}
