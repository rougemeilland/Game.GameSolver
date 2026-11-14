using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations
{
    /// <summary>
    /// ベースラインが行である退化可能なフィッシュのクラスです。
    /// </summary>
    internal sealed class SashimiFishDegeneratableByRow
        : SashimiFishDegeneratable
    {
        private readonly List<string> _messages;
        private UInt128 _keyCellIndexBitSet;
        private SudokuBoardCellRowBitSet _baseLineBitSet;
        private SudokuBoardCellColumnBitSet _coverLineBitSet;
        private UInt128 _determinedCellIndexBitSet;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="keyCellIndexBitSet">
        /// フィッシュの対象の数字が存在するすべての交差点のセルの位置の集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        public SashimiFishDegeneratableByRow(UInt128 keyCellIndexBitSet)
        {
            _messages = [];
            _keyCellIndexBitSet = keyCellIndexBitSet;
            _baseLineBitSet = keyCellIndexBitSet.GetUnionByRow();
            _coverLineBitSet = keyCellIndexBitSet.GetUnionByColumn();
            _determinedCellIndexBitSet = UInt128.Zero;
#if DEBUG
            System.Diagnostics.Debug.Assert(_keyCellIndexBitSet != 0);
            System.Diagnostics.Debug.Assert(_baseLineBitSet.GetPopCount() > 0);
            System.Diagnostics.Debug.Assert(_coverLineBitSet.GetPopCount() > 0);
            System.Diagnostics.Debug.Assert(_baseLineBitSet.GetPopCount() == _coverLineBitSet.GetPopCount());
#endif
        }

        /// <inheritdoc/>
        protected override bool DegenerateOneStep(
            CellDeterminedMessageFormatter baseLineCellDeterminedMessageFormatter,
            CellDeterminedMessageFormatter coverLineLineCellDeterminedMessageFormatter)
        {
            if (_keyCellIndexBitSet == 0)
                return false;

            {
                var result =
                    DegenerateOneStepByRow(
                        _keyCellIndexBitSet,
                        _baseLineBitSet,
                        _coverLineBitSet,
                        _determinedCellIndexBitSet,
                        (row, effectedColumn, cellIndex) => baseLineCellDeterminedMessageFormatter(row.ToHouseName(), effectedColumn.ToHouseName(), cellIndex.ToFriendlyString()));
                if (result is not null)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedRowBitSet != _baseLineBitSet && (result.Value.undeterminedRowBitSet & ~_baseLineBitSet) == SudokuBoardCellRowBitSet.None);
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedColumnBitSet != _coverLineBitSet && (result.Value.undeterminedColumnBitSet & ~_coverLineBitSet) == SudokuBoardCellColumnBitSet.None);
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedColumnBitSet.GetPopCount() == result.Value.undeterminedRowBitSet.GetPopCount());
#endif
                    _keyCellIndexBitSet = result.Value.keyCellIndexBitSet;
                    _baseLineBitSet = result.Value.undeterminedRowBitSet;
                    _coverLineBitSet = result.Value.undeterminedColumnBitSet;
                    _determinedCellIndexBitSet = result.Value.determinedCellIndexBitSet;
                    _messages.Add(result.Value.message);
                    return true;
                }
            }

            {
                var result =
                    DegenerateOneStepByColumn(
                        _keyCellIndexBitSet,
                        _coverLineBitSet,
                        _baseLineBitSet,
                        _determinedCellIndexBitSet,
                        (column, effectedRow, cellIndex) => coverLineLineCellDeterminedMessageFormatter(column.ToHouseName(), effectedRow.ToHouseName(), cellIndex.ToFriendlyString()));
                if (result is not null)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedRowBitSet != _baseLineBitSet && (result.Value.undeterminedRowBitSet & ~_baseLineBitSet) == SudokuBoardCellRowBitSet.None);
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedColumnBitSet != _coverLineBitSet && (result.Value.undeterminedColumnBitSet & ~_coverLineBitSet) == SudokuBoardCellColumnBitSet.None);
                    System.Diagnostics.Debug.Assert(result.Value.undeterminedColumnBitSet.GetPopCount() == result.Value.undeterminedRowBitSet.GetPopCount());
#endif
                    _keyCellIndexBitSet = result.Value.keyCellIndexBitSet;
                    _coverLineBitSet = result.Value.undeterminedColumnBitSet;
                    _baseLineBitSet = result.Value.undeterminedRowBitSet;
                    _determinedCellIndexBitSet = result.Value.determinedCellIndexBitSet;
                    _messages.Add(result.Value.message);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        protected override (ReadOnlyMemory<string> degenerationMessages, ReadOnlyMemory<string> undegeneratedBaseLineHouseNames, ReadOnlyMemory<string> undegeneratedCoverLineHouseNames, UInt128 determinedCellIndexBitSet) Finalize()
            => (_messages.ToArray(), _baseLineBitSet.EnumerateAsCellRowTexts().ToArray(), _coverLineBitSet.EnumerateAsCellColumnTexts().ToArray(), _determinedCellIndexBitSet);
    }
}
