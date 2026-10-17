using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面の状態を示す抽象クラスです。
    /// </summary>
    /// <typeparam name="STATE_T">ゲームの盤面の状態を示すクラスです。</typeparam>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public abstract class GameBoardState<STATE_T, OPERATION_T>
        where STATE_T : GameBoardState<STATE_T, OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        private sealed class GameBoardOperationCollection
            : IGameBoardOperationCollection<OPERATION_T>
        {
            private readonly List<OPERATION_T> _operations;

            public GameBoardOperationCollection()
            {
                _operations = [];
            }

            public void Add(OPERATION_T operation) => _operations.Add(operation);

            public GameBoardOperationCollection Clone() => [.. this];

            IEnumerator<OPERATION_T> IEnumerable<OPERATION_T>.GetEnumerator() => _operations.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _operations.GetEnumerator();
        }

        private sealed class CompositKey
            : IGameBoardStateUniqueKey
        {
            private readonly uint[] _data;
            private readonly Lazy<int> _hashCode;

            public CompositKey(uint[] data)
            {
                _data = new uint[data.Length];
                data.CopyTo(_data, 0);
                _hashCode = new Lazy<int>(GetHashCodeCore);
            }

            public bool Equals(IGameBoardStateUniqueKey? other)
                => other is not null
                    && GetType() == other.GetType()
                    && EqualsCore((CompositKey)other);

            public override bool Equals(object? other)
                => other is not null
                    && GetType() == other.GetType()
                    && EqualsCore((CompositKey)other);

            public override int GetHashCode() => _hashCode.Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool EqualsCore(CompositKey other) => _data.SequenceEqual(other._data);

            private int GetHashCodeCore()
            {
                if (_data is null)
                    return 0;
                var hash = new HashCode();
                foreach (var element in _data)
                    hash.Add(element);
                return hash.ToHashCode();
            }
        }

        private readonly GameBoardOperationCollection _operationLog;
        private readonly Lazy<GameBoardStatus> _status;
        private readonly Lazy<CompositKey> _uniqueKey;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        protected GameBoardState()
            : this(0, new GameBoardOperationCollection(), null)
        {
        }

        /// <summary>
        /// <typeparamref name="STATE_T"/> オブジェクトをコピーするコンストラクタです。ただし <see cref="StepCount"/> プロパティの値が 1 だけ加算され、オペレーションのログにオペレーションが新たに追加されます。ます。
        /// </summary>
        /// <param name="source">
        /// コピー元となる <typeparamref name="STATE_T"/> オブジェクトです。
        /// </param>
        /// <param name="newOperation">
        /// <see cref="OperationLog"/> に新たに追加される <typeparamref name="OPERATION_T"/> オブジェクトです。
        /// </param>
        protected GameBoardState(STATE_T source, OPERATION_T newOperation)
            : this(source.StepCount + 1, source._operationLog.Clone(), newOperation)
        {
        }

        private GameBoardState(int stepCount, GameBoardOperationCollection operationLog, OPERATION_T? newOperation)
        {
            _status = new(CheckStatus);
            _uniqueKey = new Lazy<CompositKey>(() => new CompositKey(GenerateUniqueKeySource()));
            StepCount = stepCount;
            _operationLog = operationLog;
            LatestOperation = newOperation;
            if (newOperation is not null)
                _operationLog.Add(newOperation);
        }

        /// <summary>
        /// 盤面の状態の概要を取得します。
        /// </summary>
        /// <value>
        /// 盤面の概要を示す <see cref="GameBoardStatus"/> 値です。
        /// </value>
        public GameBoardStatus Status => _status.Value;

        /// <summary>
        /// 盤面の探索深度です。初期状態は 0 です。
        /// </summary>
        public int StepCount { get; }

        /// <summary>
        /// 盤面に対してこれまでに行われたオペレーションを示す <typeparamref name="OPERATION_T"/> オブジェクトのコレクションです。
        /// </summary>
        public IGameBoardOperationCollection<OPERATION_T> OperationLog => _operationLog;

        /// <summary>
        /// 直近に行ったオペレーションを取得します。
        /// </summary>
        /// <value>
        /// 初期盤面である場合には <see langword="null"/>、そうではない場合には直近に行ったオペレーションを示す <typeparamref name="OPERATION_T"/> オブジェクトです。
        /// </value>
        public OPERATION_T? LatestOperation { get; }

        internal IGameBoardStateUniqueKey UniqueKey => _uniqueKey.Value;

        /// <summary>
        /// 現在の盤面から次の盤面を可能な限り列挙します。
        /// </summary>
        /// <returns>
        /// 次の盤面を表す <typeparamref name="STATE_T"/> オブジェクトの列挙子です。
        /// </returns>
        public IEnumerable<STATE_T> EnumerateNextState()
        {
            foreach (var operation in EnumerateNextOperations())
                yield return GenerateNextState(operation);
        }

        /// <summary>
        /// 指定された盤面に指定されたオペレーションを適用し、新たな盤面を生成します。
        /// </summary>
        /// <param name="operation">
        /// 適用するオペレーションを示す <typeparamref name="OPERATION_T"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// 生成された新たな盤面を示す <typeparamref name="STATE_T"/> オブジェクトです。
        /// </returns>
        public STATE_T Trace(OPERATION_T operation) => GenerateNextState(operation);

        /// <summary>
        /// 盤面の状態の概要を取得します。
        /// </summary>
        /// <returns>
        /// 盤面の状態の概要を示す <see cref="GameBoardStatus"/> 値を返します。
        /// </returns>
        protected abstract GameBoardStatus CheckStatus();

        /// <summary>
        /// 盤面の状態をコンパクトにまとめた <see cref="uint"/> の配列を生成します。
        /// </summary>
        /// <returns>
        /// 盤面の状態を示す <see cref="uint"/> の配列です。
        /// </returns>
        /// <remarks>
        /// <para>
        /// このメソッドが返す配列のサイズ及び内容は実装依存です。
        /// </para>
        /// <para>
        /// このメソッドが返す配列は、既に探索済みの盤面と同等の盤面の再探索を抑止するために使用されます。
        /// したがって、このメソッドが返す配列は、そのために必要十分な内容でなければなりません。
        /// また、配列のサイズが可能な限り小さくなるようにパッキング方法を工夫してください。
        /// </para>
        /// </remarks>
        internal protected abstract uint[] GenerateUniqueKeySource();

        /// <summary>
        /// 現在の盤面から採り得るオペレーションを全て列挙します。
        /// </summary>
        /// <returns>
        /// <typeparamref name="OPERATION_T"/> オブジェクトの列挙子です。
        /// </returns>
        protected abstract IEnumerable<OPERATION_T> EnumerateNextOperations();

        /// <summary>
        /// 指定されたオペレーションに従って現在の盤面から次の盤面を生成します。
        /// </summary>
        /// <param name="operation">
        /// 適用するオペレーションを示す <typeparamref name="OPERATION_T"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// 生成された新たな盤面を示す <typeparamref name="STATE_T"/> オブジェクトです。
        /// </returns>
        protected abstract STATE_T GenerateNextState(OPERATION_T operation);
    }
}
