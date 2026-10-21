using System;
using System.Collections.Generic;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの解答のクラスです。
    /// </summary>
    /// <typeparam name="STATE_T">ゲームの盤面の状態を示すクラスです。</typeparam>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public class GameSolution<STATE_T, OPERATION_T>
        where STATE_T : GameBoardState<STATE_T, OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        private readonly STATE_T _initialState;
        private readonly STATE_T _lastState;

        internal GameSolution(STATE_T initialState, STATE_T lastState)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(initialState.Status == GameBoardStatus.Initial);
            System.Diagnostics.Debug.Assert(lastState.Status is GameBoardStatus.Success or GameBoardStatus.Failed);
#endif
            Status = lastState.Status;
            _initialState = initialState;
            _lastState = lastState;
        }

        /// <summary>
        /// ゲームの結果の概要を取得します。
        /// </summary>
        /// <value>
        /// ゲームの結果の概要を示す <see cref="GameBoardStatus"/> 値です。
        /// </value>
        public GameBoardStatus Status { get; }

        /// <summary>
        /// ゲームの盤面の履歴を初期の盤面から最後の盤面まで取得します。
        /// </summary>
        /// <value>
        /// ゲームの盤面の状態を示す <see cref="GameBoardState{STATE_T, OPERATION_T}"/> オブジェクトの列挙子です。
        /// </value>
        public IEnumerable<STATE_T> StateHistory
        {
            get
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(_initialState.Status == GameBoardStatus.Initial);
                System.Diagnostics.Debug.Assert(_lastState.Status is GameBoardStatus.Success or GameBoardStatus.Failed);
#endif
                yield return _initialState;
                var state = _initialState;
                var operations = _lastState.OperationLog;
                for (var operationIndex = 0; operationIndex < operations.Length; ++operationIndex)
                {
                    if (state.Status is not GameBoardStatus.Initial and not GameBoardStatus.Solving)
                        throw new ApplicationException();
                    var nextState = state.Trace(operations[operationIndex]);
                    yield return nextState;
                    state = nextState;
                }
#if DEBUG
                System.Diagnostics.Debug.Assert(_lastState.Status is GameBoardStatus.Success or GameBoardStatus.Failed);
#endif
            }
        }
    }
}
