using System.Collections.Generic;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の解決の結果を保持するクラスです。
    /// </summary>
    public class SudokuSolution
    {
        private readonly GameSolution<SudokuBoardState, SudokuBoardOperation> _solution;

        internal SudokuSolution(GameSolution<SudokuBoardState, SudokuBoardOperation> solution)
        {
            _solution = solution;
        }

        /// <summary>
        /// 数独の盤面の解決の結果の概要を取得します。
        /// </summary>
        /// <value>
        /// 数独の盤面の解決の結果の概要を示す <see cref="GameBoardStatus"/> 値です。
        /// </value>
        public GameBoardStatus Status => _solution.Status;

        /// <summary>
        /// 数独の盤面の履歴を初期の盤面から最後の盤面まで取得します。
        /// </summary>
        /// <value>
        /// 数独の盤面の状態を示す <see cref="GameBoardState{STATE_T, OPERATION_T}"/> オブジェクトの列挙子です。
        /// </value>
        public IEnumerable<SudokuBoardState> StateHistory => _solution.StateHistory;
    }
}
