using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// ボックスラインリダクションのオペレーションのクラスです。
    /// </summary>
    internal sealed class BoxLineReductionOperation
        : SudokuBoardOperation
    {
        private BoxLineReductionOperation(
            ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 positivelyRelatedCellIndexBitSet,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "ボックスラインリダクション",
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
            // B 内の CorR 上の何れかのセルの数字の候補に D が存在し、かつ B の範囲外の CorR 上以外のセルの数字の候補に D が存在しない場合
            // => B 内の CorR 上のセルの数字の候補は何れかが D であるので、仮に B 内の CorR 上以外のセルの数字の候補に D が あってもそれらのセルは D にはなり得ない。

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

            static BoxLineReductionOperation? MatchPatternByHouse(SudokuBoardCellBlock block, UInt128 blockHouseBitMask, UInt128 columnOrRowHouseBitMask, string columnOrRowHouseText, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var highlightedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & columnOrRowHouseBitMask;
                var removedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & ~columnOrRowHouseBitMask;
                if (highlightedCellNoteBitSet != 0 && (cellNoteBitSet & ~blockHouseBitMask & columnOrRowHouseBitMask) == 0 && removedCellNoteBitSet != 0)
                {
                    var cellDigitChar = cellDigit.ToCellDigitChar();
                    var removedCellNotePositionTexts = $"({string.Join(", ", removedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings())})";
                    var description =
                        $"{columnOrRowHouseText}には{block.ToFriendlyString()}のブロックと重なるセルのみに数字の候補 '{cellDigitChar}' が含まれており、" +
                        $"必ずこれらのセルの何れかが '{cellDigitChar}' になります。\n" +
                        $"したがって、{block.ToFriendlyString()}のブロックにある他のセル {removedCellNotePositionTexts} の数字の候補から '{cellDigitChar}' を削除します。";
                    return
                        new BoxLineReductionOperation(
                            highlightedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            columnOrRowHouseBitMask,
                            blockHouseBitMask,
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
