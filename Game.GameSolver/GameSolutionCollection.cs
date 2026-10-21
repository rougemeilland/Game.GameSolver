using System.Collections.Generic;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの解答のコレクションのクラスです。
    /// </summary>
    /// <typeparam name="STATE_T">ゲームの盤面の状態を示すクラスです。</typeparam>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public sealed class GameSolutionCollection<STATE_T, OPERATION_T>
        where STATE_T : GameBoardState<STATE_T, OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        internal GameSolutionCollection(int solutionCount, int actualSolutionCount, IEnumerable<GameSolution<STATE_T, OPERATION_T>> solutions)
        {
            SolutionCount = solutionCount;
            ActualSolutionCount = actualSolutionCount;
            Solutions = solutions;
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
        public IEnumerable<GameSolution<STATE_T, OPERATION_T>> Solutions { get; }
    }
}
