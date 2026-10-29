using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面に適用する定石の抽象クラスです。
    /// </summary>
    public abstract class SudokuBoardOperation
        : GameBoardOperation<SudokuBoardOperation>
    {
        private sealed class CellNoteCollection
            : ISudokuBoardCellNoteCollection
        {
            private readonly SudokuBoardCellPositionDigit[] _cellNotes;

            internal CellNoteCollection(IEnumerable<SudokuBoardCellPositionDigit> source)
            {
                _cellNotes = new HashSet<SudokuBoardCellPositionDigit>(source).ToArray();
            }

            /// <inheritdoc/>
            SudokuBoardCellPositionDigit IReadOnlyVectorIndexer<SudokuBoardCellPositionDigit, int>.this[int index]
            {
                get
                {
#if DEBUG
                    ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _cellNotes.Length);
#endif

                    return _cellNotes[index];
                }
            }

            /// <inheritdoc/>
            int IReadOnlyVectorIndexer<SudokuBoardCellPositionDigit, int>.Length => _cellNotes.Length;

            /// <inheritdoc/>
            IEnumerator<SudokuBoardCellPositionDigit> IEnumerable<SudokuBoardCellPositionDigit>.GetEnumerator() => _cellNotes.AsEnumerable().GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => _cellNotes.GetEnumerator();
        }

        private sealed class CellBackgroundEffectCollection
            : ISudokuBoardCellBackgroundEffectCollection
        {
            private readonly Dictionary<SudokuBoardCellIndex, SudokuBoardCellBackgroundEffect> _effects;

            internal CellBackgroundEffectCollection(IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> source)
            {
                _effects = [];
                foreach (var (cellIndex, effect) in source)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(cellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
#endif
                    if (!_effects.TryAdd(cellIndex, effect))
                    {
                        if (_effects[cellIndex] < effect)
                            _effects[cellIndex] = effect;
                    }
                }
            }

            /// <inheritdoc/>
            SudokuBoardCellBackgroundEffect IReadOnlyVectorIndexer<SudokuBoardCellBackgroundEffect, SudokuBoardCellIndex>.this[SudokuBoardCellIndex index]
            {
                get
                {
#if DEBUG
                    if (index is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(index));
#endif

                    return _effects[index];
                }
            }

            /// <inheritdoc/>
            bool ISudokuBoardCellBackgroundEffectCollection.TryGetValue(SudokuBoardCellIndex cellIndex, out SudokuBoardCellBackgroundEffect effect)
                => _effects.TryGetValue(cellIndex, out effect);

            /// <inheritdoc/>
            int IReadOnlyVectorIndexer<SudokuBoardCellBackgroundEffect, SudokuBoardCellIndex>.Length
                => _effects.Count;

            /// <inheritdoc/>
            IEnumerator<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)>.GetEnumerator()
                => GetEnumeratorCore();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumeratorCore();

            private IEnumerator<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> GetEnumeratorCore()
            {
                foreach (var keyValue in _effects)
                    yield return (keyValue.Key, keyValue.Value);
            }
        }

        private sealed class CellNoteEffectCollection
            : ISudokuBoardCellNoteEffectCollection
        {
            private readonly Dictionary<SudokuBoardCellPositionDigit, SudokuBoardCellNoteEffect> _effects;

            internal CellNoteEffectCollection(IEnumerable<(SudokuBoardCellPositionDigit cellIndex, SudokuBoardCellNoteEffect effect)> source)
            {
                _effects = [];
                foreach (var (cellPositionDigit, effect) in source)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(cellPositionDigit.CellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
                    System.Diagnostics.Debug.Assert(cellPositionDigit.Digit is >= SudokuBoardCellDigit.MinValue and <= SudokuBoardCellDigit.MaxValue);
#endif
                    if (!_effects.TryAdd(cellPositionDigit, effect))
                    {
                        if (_effects[cellPositionDigit] < effect)
                            _effects[cellPositionDigit] = effect;
                    }
                }
            }

            /// <inheritdoc/>
            SudokuBoardCellNoteEffect IReadOnlyVectorIndexer<SudokuBoardCellNoteEffect, SudokuBoardCellPositionDigit>.this[SudokuBoardCellPositionDigit index]
                => _effects[index];

            /// <inheritdoc/>
            bool ISudokuBoardCellNoteEffectCollection.TryGetValue(SudokuBoardCellPositionDigit positionDigit, out SudokuBoardCellNoteEffect effect)
                => _effects.TryGetValue(positionDigit, out effect);

            /// <inheritdoc/>
            SudokuBoardCellNoteEffect ISudokuBoardCellNoteEffectCollection.this[SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellNoteDigit]
            {
                get
                {
#if DEBUG
                    if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(cellIndex));
                    if (cellNoteDigit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(cellNoteDigit));
#endif

                    return _effects[new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit)];
                }
            }

            /// <inheritdoc/>
            bool ISudokuBoardCellNoteEffectCollection.TryGetValue(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellNoteDigit, out SudokuBoardCellNoteEffect effect)
                => _effects.TryGetValue(new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit), out effect);

            /// <inheritdoc/>
            int IReadOnlyVectorIndexer<SudokuBoardCellNoteEffect, SudokuBoardCellPositionDigit>.Length
                => _effects.Count;

            /// <inheritdoc/>
            IEnumerator<(SudokuBoardCellPositionDigit positionDigit, SudokuBoardCellNoteEffect effect)> IEnumerable<(SudokuBoardCellPositionDigit positionDigit, SudokuBoardCellNoteEffect effect)>.GetEnumerator()
                => GetEnumeratorCore();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumeratorCore();

            private IEnumerator<(SudokuBoardCellPositionDigit positionDigit, SudokuBoardCellNoteEffect effect)> GetEnumeratorCore()
            {
                foreach (var keyValue in _effects)
                    yield return (keyValue.Key, keyValue.Value);
            }
        }

        static SudokuBoardOperation()
        {
            EmptyCellBackgroundEffects = new CellBackgroundEffectCollection([]);
            EmptyCellNoteEffects = new CellNoteEffectCollection([]);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="status">
        /// 定石を適用した結果の盤面の状態を示す <see cref="GameBoardStatus"/> 値です。
        /// </param>
        /// <param name="title">
        /// 定石の名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="difficulty">
        /// 定石の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </param>
        /// <param name="determinedCell">
        /// 定石を適用した結果数字が確定したセルが存在する場合はそのセルの位置と数字を示す <see cref="SudokuBoardCellPositionDigit"/> 値、そうではない場合は <see langword="null"/> です。
        /// </param>
        /// <param name="excludedCellNotes">
        /// 定石を適用した結果排除されるセルの数字の候補とそのセルの位置を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスです。
        /// </param>
        /// <param name="cellBackgroundEffects">
        /// 定石の説明において背景効果を必要とするセルの位置を <see cref="SudokuBoardCellIndex"/> 値とその背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値のペアのシーケンスです。
        /// </param>
        /// <param name="cellNoteEffects">
        /// 定石の説明において強調効果を必要とするセルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値とその強調効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値のペアのシーケンスです。
        /// </param>
        /// <param name="description">
        /// 定石の説明のテキストを示す <see cref="string"/>オブジェクトです。
        /// </param>
        protected SudokuBoardOperation(
            GameBoardStatus status,
            string title,
            SudokuBoardDifficultyLevel difficulty,
            SudokuBoardCellPositionDigit? determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(status)
        {
            Title = title;
            Difficulty = difficulty;
            DeterminedCell = determinedCell;
            ExcludedCellNotes = new CellNoteCollection(excludedCellNotes);
            CellBackgroundEffects = new CellBackgroundEffectCollection(cellBackgroundEffects);
            CellNoteEffects = new CellNoteEffectCollection(cellNoteEffects);
            Description = description;
        }

        /// <summary>
        /// 定石の名前を取得します。
        /// </summary>
        /// 定石の名前を示す <see cref="string"/> オブジェクトです。
        public string Title { get; }

        /// <summary>
        /// 定石の難易度を取得します。
        /// </summary>
        /// <value>
        /// 定石の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </value>
        public SudokuBoardDifficultyLevel Difficulty { get; }

        /// <summary>
        /// オペレーションにより確定されるセルを取得します。
        /// </summary>
        /// <value>
        /// 確定されたセルが存在する場合はそのセルを示す <see cref="SudokuBoardCellPositionDigit"/> 値です。そうではない場合は <see langword="null"/> です。
        /// </value>
        /// <remarks>
        /// </remarks>
        public SudokuBoardCellPositionDigit? DeterminedCell { get; }

        /// <summary>
        /// オペレーションにより排除されるセルの数字の候補とそのセルの位置のペアのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルの数字の候補とそのセルの位置のペアのコレクションを示す <see cref="ISudokuBoardCellNoteCollection"/> オブジェクトです。
        /// </value>
        public ISudokuBoardCellNoteCollection ExcludedCellNotes { get; }

        /// <summary>
        /// オペレーションの説明に役立つセルの背景表示効果のコレクションを示す <see cref="ISudokuBoardCellBackgroundEffectCollection"/> オブジェクトを取得します。
        /// </summary>
        public ISudokuBoardCellBackgroundEffectCollection CellBackgroundEffects { get; }

        /// <summary>
        /// オペレーションの説明に役立つセルの数字の候補の表示効果のコレクションを示す <see cref="ISudokuBoardCellNoteEffectCollection"/> オブジェクトを取得します。
        /// </summary>
        public ISudokuBoardCellNoteEffectCollection CellNoteEffects { get; }

        /// <summary>
        /// オペレーションの動作内容を示す文字列を取得します。
        /// </summary>
        /// <value>
        /// オペレーションの動作内容を示す <see cref="string"/> です。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティが返す文字列は以下の内容を含まなければなりません。
        /// <list type="bullet">
        /// <item>このオペレーションが採用された根拠</item>
        /// <item>このオペレーションが盤面に及ぼす影響</item>
        /// </list>
        /// </para>
        /// </remarks>
        public string Description { get; }

        internal static ISudokuBoardCellBackgroundEffectCollection EmptyCellBackgroundEffects { get; }
        internal static ISudokuBoardCellNoteEffectCollection EmptyCellNoteEffects { get; }

        /// <summary>
        /// 定石を盤面に適用します。
        /// </summary>
        /// <param name="cells">
        /// 盤面の状態を示す <see cref="SudokuBoardCellsSet"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// オペレーションを実行した結果の盤面の状態を示す <see cref="SudokuBoardCellsSet"/> オブジェクトを返します。
        /// </returns>
        internal abstract SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells);
    }
}
