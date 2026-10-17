using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// ポインティングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class PointingOperation
        : SudokuBoardOperation
    {
        private PointingOperation(ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base("ポインティング", SudokuBoardDifficultyLevel.Medium, null, highlightedCellNotes, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static PointingOperation? MatchPattern(SudokuBoardWorkspace ws)
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
                            return operation;
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
                            return operation;
                    }
                }
            }

            return null;

            static PointingOperation? MatchPatternByHouse(SudokuBoardCellBlock block, UInt128 blockHouseBitMask, UInt128 rowBitMask, string columnOrRowHouseText, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var highlightedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & rowBitMask;
                var removedCellNoteBitSet = cellNoteBitSet & ~blockHouseBitMask & rowBitMask;
                if (highlightedCellNoteBitSet != 0 && (cellNoteBitSet & blockHouseBitMask & ~rowBitMask) == 0 && removedCellNoteBitSet != 0)
                {
                    var cellDigitChar = cellDigit.ToCellDigitChar();
                    var removedCellNotePositionTexts = $"({string.Join(", ", removedCellNoteBitSet.EnumerateAsCellIndexes().ToCellPositions())})";
                    var description
                        = $"{block.ToFriendlyString()}のブロックには{columnOrRowHouseText}のセルのみに数字の候補 '{cellDigitChar}' が含まれているので、" +
                        $"必ずこれらのセルの何れかが '{cellDigitChar}' となります。" +
                        $"そのため、{columnOrRowHouseText}にある他のセル {removedCellNotePositionTexts} の数字の候補から '{cellDigitChar}' を削除します。";
                    return
                        new PointingOperation(
                            highlightedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray(),
                            (blockHouseBitMask | rowBitMask).EnumerateAsCellIndexes().ToCellPositions().ToArray(),
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
                if (!newCellSet.RemoveNote(removedCellNote.Column, removedCellNote.Row, removedCellNote.Digit))
                    throw new ApplicationException();
            }

            return newCellSet;
        }
    }
}
