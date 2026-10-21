using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の解答とその解法を保持するクラスです。
    /// </summary>
    public class SudokuSolution
    {
        private sealed class StateCollection
            : ISudokuBoardStateCollection
        {
            private readonly SudokuBoardState[] _source;

            public StateCollection(SudokuBoardState[] source)
            {
                _source = source;
            }

            public SudokuBoardState this[int index]
            {
                get
                {
                    ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _source.Length);

                    return _source[index];
                }
            }

            public int Length => _source.Length;

            public IEnumerator<SudokuBoardState> GetEnumerator() => _source.AsEnumerable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _source.GetEnumerator();
        }

        private readonly GameSolution<SudokuBoardState, SudokuBoardOperation> _solution;
        private readonly SudokuBoardState[] _stateHistory;

        internal SudokuSolution(GameSolution<SudokuBoardState, SudokuBoardOperation> solution)
        {
            _solution = solution;
            _stateHistory = [.. solution.StateHistory];
            Result = _stateHistory[^1];
            Difficulty = GetMaxDifficulty(Result.OperationLog);
#if DEBUG
            System.Diagnostics.Debug.Assert(_solution.Status == Result.Status);
#endif

            static SudokuBoardDifficultyLevel GetMaxDifficulty(IGameBoardOperationCollection<SudokuBoardOperation> operations)
            {
                var maxDifficulty = SudokuBoardDifficultyLevel.Beginner;
                for (var index = 0; index < operations.Length; ++index)
                {
                    var operation = operations[index];
                    if (maxDifficulty < operation.Difficulty)
                        maxDifficulty = operation.Difficulty;
                }

                return maxDifficulty;
            }
        }

        /// <summary>
        /// 数独の盤面の解答の概要を取得します。
        /// </summary>
        /// <value>
        /// 数独の盤面の解答の概要を示す <see cref="GameBoardStatus"/> 値です。
        /// </value>
        public GameBoardStatus Status => _solution.Status;

        /// <summary>
        /// 数独の盤面の解法の難易度を取得します。
        /// </summary>
        /// 解法の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// <remarks>
        /// このプロパティの値は、解答を求めるために使用された定石の難易度の最大値に等しくなります。
        /// </remarks>
        public SudokuBoardDifficultyLevel Difficulty { get; }

        /// <summary>
        /// 数独の解答を取得します。
        /// </summary>
        /// 数独の解答を示す <see cref="SudokuBoardState"/> オブジェクトです。
        public SudokuBoardState Result { get; }

        /// <summary>
        /// 数独の盤面の履歴を初期の盤面から最後の盤面まで取得します。
        /// </summary>
        /// <value>
        /// 数独の盤面の状態の履歴を示す <see cref="ISudokuBoardStateCollection"/> オブジェクトです。
        /// </value>
        public ISudokuBoardStateCollection History => new StateCollection(_stateHistory);
    }
}
