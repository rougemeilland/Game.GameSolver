using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// ポインティングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class PointingOperation
        : SudokuBoardOperation
    {
        private PointingOperation(
            ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 positivelyRelatedCellIndexBitSet,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "ポインティング",
                  SudokuBoardDifficultyLevel.Medium,
                  null,
                  highlightedCellNotes,
                  removedCellNotes,
                  positivelyRelatedCellIndexBitSet,
                  negativelyRelatedCellIndexBitSet,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるブロック B とある行または列 CorR とある数字 D に対して、
            // B 内の CorR 上の何れかのセルの数字の候補に D が存在し、かつ B 内の CorR 上以外のセルの数字の候補に D が存在しない場合
            // => B 内の CorR 上の何れかのセルの数字が D であるので、B の範囲外の CorR 上のセルの数字の候補では D は否定される。

            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; cellDigit++)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);
                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var blockHouseBitMask = SudokuBoardCellGeometry.GetBlockCellsBitSet(block);

                    // 行ハウスの検索
                    for (var blockRowOffset = SudokuBoardCellBlockRow.MinValue; blockRowOffset <= SudokuBoardCellBlockRow.MaxValue; ++blockRowOffset)
                    {
                        var row = SudokuBoardCellGeometry.GetRow(block, blockRowOffset);
                        var operation =
                            MatchPatternByHouse(
                                block,
                                blockHouseBitMask,
                                SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                                row.ToFriendlyString(),
                                cellDigit,
                                cellNoteBitSet);
                        if (operation is not null)
                            yield return operation;
                    }

                    // 列ハウスの検索
                    for (var blockColumnOffset = SudokuBoardCellBlockColumn.MinValue; blockColumnOffset <= SudokuBoardCellBlockColumn.MaxValue; ++blockColumnOffset)
                    {
                        var column = SudokuBoardCellGeometry.GetColumn(block, blockColumnOffset);
                        var operation =
                            MatchPatternByHouse(
                                block,
                                blockHouseBitMask,
                                SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                                column.ToFriendlyString(),
                                cellDigit,
                                cellNoteBitSet);
                        if (operation is not null)
                            yield return operation;
                    }
                }
            }

            static PointingOperation? MatchPatternByHouse(SudokuBoardCellBlock block, UInt128 blockHouseBitMask, UInt128 columnOrRowHouseBitMask, string columnOrRowHouseText, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var highlightedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & columnOrRowHouseBitMask;
                var removedCellNoteBitSet = cellNoteBitSet & ~blockHouseBitMask & columnOrRowHouseBitMask;
                if (highlightedCellNoteBitSet != 0 && (cellNoteBitSet & blockHouseBitMask & ~columnOrRowHouseBitMask) == 0 && removedCellNoteBitSet != 0)
                {
                    var cellDigitChar = cellDigit.ToCellDigitChar();
                    var removedCellNotePositionTexts = $"({string.Join(", ", removedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings())})";
                    var description
                        = $"{block.ToFriendlyString()}のブロックには{columnOrRowHouseText}のセルのみに数字の候補 '{cellDigitChar}' が含まれているので、" +
                        $"必ずこれらのセルの何れかが '{cellDigitChar}' となります。\n" +
                        $"そのため、{columnOrRowHouseText}にある他のセル {removedCellNotePositionTexts} の数字の候補から '{cellDigitChar}' を削除します。";
                    return
                        new PointingOperation(
                            highlightedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            blockHouseBitMask,
                            columnOrRowHouseBitMask,
                            description);
                }

                return null;
            }
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is not null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            var removedCellNotes = RemovedCellNotes.Span;
            for (var index = 0; index < RemovedCellNotes.Length; ++index)
            {
                var removedCellNote = removedCellNotes[index];
                if (!newCellSet.RemoveNote(removedCellNote.CellIndex, removedCellNote.Digit))
                    throw new ApplicationException();
            }

            return newCellSet;
        }
    }
}
