using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームを解決するジェネリック抽象クラスです。
    /// </summary>
    /// <typeparam name="STATE_T">ゲームの盤面を示すクラスです。</typeparam>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public abstract class GameSolverEngineGameSolverState<STATE_T, OPERATION_T>
        where STATE_T : GameBoardState<STATE_T, OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        private readonly Stack<STATE_T> _stateColelction;
        private readonly Dictionary<IGameBoardStateUniqueKey, int> _alreadyAppearedState;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        protected GameSolverEngineGameSolverState()
        {
            _stateColelction = new Stack<STATE_T>();
            _alreadyAppearedState = [];
        }

        /// <summary>
        /// ゲームを解きます。
        /// </summary>
        /// <param name="initialState">ゲームの初期盤面を示すオブジェクトです。</param>
        /// <returns>
        /// 解答が存在すればその解答を示す盤面のオブジェクトが返ります。解答が存在しない場合は <see langword="null"/> が返ります。
        /// </returns>
        public STATE_T? Solve(STATE_T initialState)
        {
            _stateColelction.Clear();
            _alreadyAppearedState.Clear();
            var resultOfSuccess = (STATE_T?)null;
            var resultOfFailed = (STATE_T?)null;
            _stateColelction.Push(initialState);
            while (_stateColelction.Count > 0)
            {
                var state = _stateColelction.Pop();
                switch (state.Status)
                {
                    case GameBoardStatus.Solving:
                    {
                        foreach (var newState in state.EnumerateNextState().Reverse())
                        {
                            if (!_alreadyAppearedState.TryGetValue(newState.UniqueKey, out var previousStepCount))
                            {
                                _stateColelction.Push(newState);
                                _alreadyAppearedState[state.UniqueKey] = state.StepCount;
                            }
                            else if (state.StepCount < previousStepCount)
                            {
                                _alreadyAppearedState[state.UniqueKey] = state.StepCount;
                            }
                            else
                            {
                            }
                        }

                        break;
                    }
                    case GameBoardStatus.Success:
                    {
                        if (resultOfSuccess is null || state.StepCount < resultOfSuccess.StepCount)
                            resultOfSuccess = state;
                        break;
                    }
                    case GameBoardStatus.Failed:
                    {
                        if (resultOfFailed is null || state.StepCount < resultOfFailed.StepCount)
                            resultOfFailed = state;
                        break;
                    }
                    default:
                        throw new ApplicationException();
                }
            }

            return resultOfSuccess ?? resultOfFailed;
        }
    }
}
