using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;
using Game.SudokuSolver.Operations.Basic;
using Game.SudokuSolver.Operations.BeginnerClass;
using Game.SudokuSolver.Operations.EasyClass;
using Game.SudokuSolver.Operations.ExtremeClass;
using Game.SudokuSolver.Operations.HardClass;
using Game.SudokuSolver.Operations.InsaneClass;
using Game.SudokuSolver.Operations.MediumClass;
using Game.SudokuSolver.Operations.NonstandardClass;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の状態のクラスです。
    /// </summary>
    public sealed class SudokuBoardState
        : GameBoardState<SudokuBoardState, SudokuBoardOperation>
    {
        private sealed class CellCollection
            : IReadOnlySudokuBoardCellCollection
        {
            private readonly SudokuBoardCell[] _cells;

            public CellCollection(SudokuBoardCell[] cells)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(cells.Length == SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1);
#endif
                _cells = cells;
            }

            public SudokuBoardCell this[SudokuBoardCellIndex index]
            {
                get
                {
#if DEBUG
                    if (index is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(index));
#endif

                    return _cells[index - SudokuBoardCellIndex.MinValue];
                }
            }

            public int Length => _cells.Length;
        }

        private readonly SudokuBoardCellsSet _cells;
        private readonly UInt128 _isInitialDigitsBitSet;

        /// <summary>
        /// 指定されたセルの数字のコレクションによって数独の盤面の状態を初期化します。
        /// </summary>
        /// <param name="initialCellDigits">
        /// <para>
        /// 数独の初期盤面のセルの数字を示す <see cref="string"/> オブジェクトの配列です。
        /// </para>
        /// <para>
        /// <paramref name="initialCellDigits"/> のサイズは 81 でなければなりません。
        /// </para>
        /// <para>
        /// 数独の盤面のあるセルの位置が R 行 C 列である場合、それに対応する <paramref name="initialCellDigits"/> のインデックス値は R * 9 + C です。
        /// </para>
        /// <para>
        /// 配列の要素は以下の何れかでなければなりません。
        /// <list type="bullet">
        /// <item>
        /// <term>空文字列 (<see cref="string.Empty"/>)</term>
        /// <description>その位置のセルの数字が不明であることを示します。</description>
        /// </item>
        /// <item>
        /// <term>"1" から "9" までの数字の長さ 1 の文字列</term>
        /// <description>その位置のセルはその数字に確定していることを示します。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="initialCellDigits"/> のサイズが 81 ではありません。
        /// </exception>
        internal SudokuBoardState(ReadOnlySpan<string> initialCellDigits)
        {
            const int TOTAL_NUMBER_OF_CELLS = SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1;
            if (initialCellDigits.Length != TOTAL_NUMBER_OF_CELLS)
                throw new ArgumentException($"The size of the Sudoku cell array must be {TOTAL_NUMBER_OF_CELLS}.", nameof(initialCellDigits));

            _cells = new SudokuBoardCellsSet(initialCellDigits);
            _isInitialDigitsBitSet = UInt128.Zero;
            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                if (_cells.IsDetermined(cellIndex))
                    _isInitialDigitsBitSet |= cellIndex.ToCellIndexBitSet();
            }
        }

        internal SudokuBoardState(SudokuBoardState state, UInt128 isInitialDigitsBitSet, SudokuBoardOperation operation)
            : base(state, operation)
        {
            _cells = operation.Execute(state._cells);
            _isInitialDigitsBitSet = isInitialDigitsBitSet;
        }

        /// <summary>
        /// この盤面のセルのコレクションを取得します。
        /// </summary>
        /// <returns>
        /// セルのコレクションにアクセスするための <see cref="IReadOnlySudokuBoardCellCollection"/> オブジェクトです。
        /// </returns>
        public IReadOnlySudokuBoardCellCollection GetCells()
        {
            var cellsBuffer = new SudokuBoardCell[81];
            var positivelyHighlightedBackgroundCellsBitSet = UInt128.Zero;
            var negativelyHighlightedBackgroundCellsBitSet = UInt128.Zero;
            if (LatestOperation is not null)
            {
                positivelyHighlightedBackgroundCellsBitSet = LatestOperation.PositivelyRelatedCellIndexBitSet;
                negativelyHighlightedBackgroundCellsBitSet = LatestOperation.NegativelyRelatedCellIndexBitSet;
            }

            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellIndexBitSet = cellIndex.ToCellIndexBitSet();
                var isPositivelyHighlightedBackground = (positivelyHighlightedBackgroundCellsBitSet & cellIndexBitSet) != 0;
                var isNegativelyHighlightedBackground = (negativelyHighlightedBackgroundCellsBitSet & cellIndexBitSet) != 0;
                if (_cells.IsDetermined(cellIndex))
                {
                    var latestDeterminedCell = LatestOperation?.DeterminedCell;
                    var latestDeterminedCellIndex =
                        latestDeterminedCell is not null
                        ? (SudokuBoardCellIndex?)latestDeterminedCell.Value.CellIndex
                        : null;
                    if (latestDeterminedCellIndex is not null && latestDeterminedCellIndex.Value == cellIndex)
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreatePositivelyHighlightedCell(isPositivelyHighlightedBackground, isNegativelyHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
                    }
                    else if ((_isInitialDigitsBitSet & cellIndexBitSet) != 0)
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreateInitialCell(isPositivelyHighlightedBackground, isNegativelyHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
                    }
                    else
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreateUpdatedCell(isPositivelyHighlightedBackground, isNegativelyHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
                    }
                }
                else
                {
                    var enabledCellNoteBitSet = _cells.GetNoteBitSet(cellIndex);
                    var positivelyHighlightedCellNoteBitSet = SudokuBoardCellNoteBitSet.None;
                    var negativelyHighlightedCellNoteBitSet = SudokuBoardCellNoteBitSet.None;
                    if (LatestOperation is not null)
                    {
                        for (var index = 0; index < LatestOperation.HighlightedCellNotes.Length; ++index)
                        {
                            var cellNote = LatestOperation.HighlightedCellNotes.Span[index];
                            if (cellNote.CellIndex == cellIndex)
                                positivelyHighlightedCellNoteBitSet |= cellNote.Digit.ToCellNoteBitSet();
                        }

                        for (var index = 0; index < LatestOperation.RemovedCellNotes.Length; ++index)
                        {
                            var cellNote = LatestOperation.RemovedCellNotes.Span[index];
                            if (cellNote.CellIndex == cellIndex)
                                negativelyHighlightedCellNoteBitSet |= cellNote.Digit.ToCellNoteBitSet();
                        }
                    }

                    cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                        SudokuBoardNotDeterminedCell.Create(isPositivelyHighlightedBackground, isNegativelyHighlightedBackground, enabledCellNoteBitSet, positivelyHighlightedCellNoteBitSet, negativelyHighlightedCellNoteBitSet);
                }
            }

            return
                new CellCollection(cellsBuffer);
        }

        /// <inheritdoc/>
        protected override ReadOnlyMemory<uint> GenerateUniqueKeySource()
        {
            var cellsKey = _cells.Pack().AsSpan();
            var buffer = new uint[cellsKey.Length + 1];
            buffer[0] = (uint)Status;
            cellsKey.CopyTo(buffer.AsSpan(1));
            return buffer;
        }

        /// <inheritdoc/>
        protected override IEnumerable<SudokuBoardOperation> EnumerateNextOperations()
        {
            var workspace = new SudokuBoardWorkspace(_cells);

            var operation =
                CompleteOperation.MatchPattern(workspace)
                ?? NoNoteOperation.MatchPattern(workspace);
            if (operation is not null)
                return [operation];

            #region BeginnerClass

            operation =
                FullHouseOperation.MatchPattern(workspace)
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            #region EasyClass

            operation =
                NakedSingleOperation.MatchPattern(workspace)
                .Concat(HiddenSingleOperation.MatchPattern(workspace))
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            #region MediumClass

            operation =
                PointingOperation.MatchPattern(workspace)
                .Concat(BoxLineReductionOperation.MatchPattern(workspace))
                .Concat(NakedPairOperation.MatchPattern(workspace))
                .Concat(NakedTripleOperation.MatchPattern(workspace))
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            #region HardClass

            operation =
                NakedQuadOperation.MatchPattern(workspace)
                .Concat(HiddenPairOperation.MatchPattern(workspace))
#if false // 未実装
                .Concat(XWingOperation.MatchPattern(workspace))
#endif
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            #region ExtremeClass

            operation =
                HiddenTripleOperation.MatchPattern(workspace)
                .Concat(HiddenQuadOperation.MatchPattern(workspace))
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            #region InsaneClass

            operation =
                ForcingChainOperation.MatchPattern(workspace)
                .MaxBy(operation => operation.RemovedCellNotes.Length);
            if (operation is not null)
                return [operation];

            #endregion

            return operation is not null
                ? Enumerable.Empty<SudokuBoardOperation>().Append(operation)
                : BruteForceOperation.MatchPattern(workspace);
        }

        /// <inheritdoc/>
        protected override SudokuBoardState GenerateNextState(SudokuBoardOperation operation) => new(this, _isInitialDigitsBitSet, operation);
    }
}
