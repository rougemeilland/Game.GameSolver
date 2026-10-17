using System;
using System.Collections.Generic;
using System.Data.Common;
using Game.GameSolver;
using Game.SudokuSolver.Operations.BeginnerClass;
using Game.SudokuSolver.Operations.EasyClass;
using Game.SudokuSolver.Operations.ExtremeClass;
using Game.SudokuSolver.Operations.HardClass;
using Game.SudokuSolver.Operations.InsaneClass;
using Game.SudokuSolver.Operations.MediumClass;

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
        /// 数独の初期盤面のセルの数字の配列です。
        /// </param>
        /// <exception cref="ArgumentException">
        /// 配列のサイズが 81 ではありません。
        /// </exception>
        public SudokuBoardState(ReadOnlySpan<string> initialCellDigits)
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
            var highlightedBackgroundCellsBitSet = UInt128.Zero;
            if (LatestOperation is not null)
            {
                var relatedPositions = LatestOperation.RelatedCellPositions.Span;
                for (var index = 0; index < LatestOperation.RelatedCellPositions.Length; ++index)
                {
                    var relatedPosition = relatedPositions[index];
                    highlightedBackgroundCellsBitSet |= SudokuBoardCellGeometry.GetCellIndex(relatedPosition.Column, relatedPosition.Row).ToCellIndexBitSet();
                }
            }

            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                var isHighlightedBackground = (highlightedBackgroundCellsBitSet & cellIndex.ToCellIndexBitSet()) != 0;
                if (_cells.IsDetermined(cellIndex))
                {
                    var latestDeterminedCell = LatestOperation?.DeterminedCell;
                    var latestDeterminedCellIndex =
                        latestDeterminedCell is not null
                        ? (SudokuBoardCellIndex?)latestDeterminedCell.Value.CellIndex
                        : null;
                    if (latestDeterminedCellIndex is not null)
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreatePositivelyHighlightedCell(isHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
                    }
                    else if ((_isInitialDigitsBitSet & cellIndex.ToCellIndexBitSet()) != 0)
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreateInitialCell(isHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
                    }
                    else
                    {
                        cellsBuffer[cellIndex - SudokuBoardCellIndex.MinValue] =
                            SudokuBoardDeterminedCell.CreateUpdatedCell(isHighlightedBackground, _cells.GetDeterminedCellDigit(cellIndex));
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
                        SudokuBoardNotDeterminedCell.Create(isHighlightedBackground, enabledCellNoteBitSet, positivelyHighlightedCellNoteBitSet, negativelyHighlightedCellNoteBitSet);

                }
            }

            return
                new CellCollection(cellsBuffer);
        }

        /// <inheritdoc/>
        protected override GameBoardStatus CheckStatus()
        {
            if (_cells.CheckIfSuccess())
                return GameBoardStatus.Success;
            else if (_cells.CheckIfFailed())
                return GameBoardStatus.Failed;
            else
                return GameBoardStatus.Solving;
        }

        /// <inheritdoc/>
        protected override uint[] GenerateUniqueKeySource() => _cells.Pack();

        /// <inheritdoc/>
        protected override IEnumerable<SudokuBoardOperation> EnumerateNextOperations()
        {
            var workspace = new SudokuBoardWorkspace(_cells);

            #region BeginnerClass

            var operation = (SudokuBoardOperation?)FullHouseOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            #endregion

            #region EasyClass

            operation = NakedSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = HiddenSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region MediumClass

            operation = PointingOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = BoxLineReductionOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = NakedPairOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = NakedTripleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region HardClass

            operation = NakedQuadOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = HiddenPairOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = XWingOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region ExtremeClass

            operation = HiddenTripleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = HiddenQuadOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region InsaneClass

            return ForcingChainOperation.MatchPattern(workspace);

            #endregion

        }

        /// <inheritdoc/>
        protected override SudokuBoardState GenerateNextState(SudokuBoardOperation operation) => new(this, _isInitialDigitsBitSet, operation);
    }
}
