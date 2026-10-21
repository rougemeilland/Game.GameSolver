using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面の抽象クラスです。
    /// </summary>
    /// <typeparam name="STATE_T">ゲームの盤面を示すクラスです。</typeparam>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public abstract class GameBoard<STATE_T, OPERATION_T>
        where STATE_T : GameBoardState<STATE_T, OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        private sealed class MinimumStepSolutionCollection
            : IEnumerable<STATE_T>, IReadOnlyVectorIndexer<STATE_T, int>
        {
            private readonly List<STATE_T> _solutions = [];
            private int _stepCount = int.MaxValue;

            public MinimumStepSolutionCollection(int limit)
            {
                ActualLength = limit;
            }

            public STATE_T this[int index]
            {
                get
                {
#if DEBUG
                    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(index, 0);
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _solutions.Count);
#endif
                    return _solutions[index];
                }
            }

            public int Length { get; private set; }
            public int ActualLength { get; }

            public void Add(STATE_T state)
            {
                if (state.StepCount < _stepCount)
                {
                    _solutions.Clear();
                    Length = 0;
                    _stepCount = state.StepCount;
                }

                if (Length < ActualLength)
                    _solutions.Add(state);
                ++Length;
            }

            public IEnumerator<STATE_T> GetEnumerator() => _solutions.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private const int _MAXIMUM_SOLUTION_COLLECTION_COUNT = 100;
        private readonly Lazy<STATE_T> _initialState;
        private readonly Stack<STATE_T> _stateColelction;
        private readonly Dictionary<IGameBoardStateUniqueKey, int> _alreadyAppearedState;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        /// <param name="initialState">
        /// ゲームの初期盤面の状態を示す <typeparamref name="STATE_T"/> オブジェクトです。
        /// </param>
        protected GameBoard(Lazy<STATE_T> initialState)
        {
            _stateColelction = new Stack<STATE_T>();
            _alreadyAppearedState = [];
            _initialState = initialState;
        }

        /// <summary>
        /// ゲームの解答とその解法を列挙します。
        /// </summary>
        /// <returns>
        /// ゲームの解答とその解法を示す <see cref="GameSolution{STATE_T, OPERATION_T}"/> の列挙子です。
        /// </returns>
        protected IEnumerable<GameSolution<STATE_T, OPERATION_T>> SolveCore()
        {
            var initialState = _initialState.Value;
            foreach (var solution in Solve(initialState))
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(solution.Status is GameBoardStatus.Initial or GameBoardStatus.Success or GameBoardStatus.Failed);
#endif
                yield return new GameSolution<STATE_T, OPERATION_T>(initialState, solution);
            }
        }

        private MinimumStepSolutionCollection Solve(STATE_T initialState)
        {
            _stateColelction.Clear();
            _alreadyAppearedState.Clear();
            var resultOfSuccess = new MinimumStepSolutionCollection(_MAXIMUM_SOLUTION_COLLECTION_COUNT);
            var resultOfFailed = new MinimumStepSolutionCollection(_MAXIMUM_SOLUTION_COLLECTION_COUNT);
            _stateColelction.Push(initialState);
            _alreadyAppearedState[initialState.UniqueKey] = initialState.StepCount;
            while (_stateColelction.Count > 0)
            {
                var state = _stateColelction.Pop();
                switch (state.Status)
                {
                    case GameBoardStatus.Initial:
                    case GameBoardStatus.Solving:
                    {
                        foreach (var newState in state.EnumerateNextState().Reverse())
                        {
                            if (_alreadyAppearedState.TryGetValue(newState.UniqueKey, out var previousStepCount))
                            {
                                if (newState.StepCount < previousStepCount)
                                    _alreadyAppearedState[newState.UniqueKey] = newState.StepCount;
                            }
                            else
                            {
                                _stateColelction.Push(newState);
                                _alreadyAppearedState.Add(newState.UniqueKey, newState.StepCount);
                            }
                        }

                        break;
                    }
                    case GameBoardStatus.Success:
                        resultOfSuccess.Add(state);
                        break;
                    case GameBoardStatus.Failed:
                        resultOfFailed.Add(state);
                        break;
                    default:
                        throw new ApplicationException();
                }
            }

            return resultOfSuccess.Length > 0 ? resultOfSuccess : resultOfFailed;
        }
    }
}
