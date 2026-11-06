using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// ポインティングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class PointingOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private PointingOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "ポインティング (Pointing)",
                  SudokuBoardDifficultyLevel.Medium,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるブロック B とある行または列 CorR とある数字 D に対して、
            // B 内の CorR 上の何れかのセルの数字の候補に D が存在し、かつ B 内の CorR 上以外のセルの数字の候補に D が存在しない場合
            // => B 内の CorR 上の何れかのセルの数字が D であるので、B の範囲外の CorR 上のセルの数字の候補では D を排除出来る。

            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; cellDigit++)
            {
                var allCellIndexBitSet = ws.GetCellIndexBitSet(cellDigit);
                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var blockHouseBitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block);

                    // 行ハウスの検索
                    for (var blockRowOffset = SudokuBoardCellBlockRow.MinValue; blockRowOffset <= SudokuBoardCellBlockRow.MaxValue; ++blockRowOffset)
                    {
                        var row = SudokuBoardCellGeometry.GetRow(block, blockRowOffset);
                        var operation =
                            MatchPatternByHouse(
                                block,
                                blockHouseBitMask,
                                SudokuBoardCellGeometry.GetRowCellIndexBitSet(row),
                                row.ToHouseName(),
                                cellDigit,
                                allCellIndexBitSet);
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
                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column),
                                column.ToHouseName(),
                                cellDigit,
                                allCellIndexBitSet);
                        if (operation is not null)
                            yield return operation;
                    }
                }
            }

            static PointingOperation? MatchPatternByHouse(
                SudokuBoardCellBlock block,
                UInt128 blockHouseBitMask,
                UInt128 lineHouseBitMask,
                string lineHouseName,
                SudokuBoardCellDigit cellDigit,
                UInt128 allCellIndexBitSet)
            {
                var intersectionBitMask = blockHouseBitMask & lineHouseBitMask;
                var blockWingBitMask = blockHouseBitMask & ~lineHouseBitMask;
                var lineWingBitMask = ~blockHouseBitMask & lineHouseBitMask;
                var KeyCellIndexBitSet = allCellIndexBitSet & intersectionBitMask;
                var excludedCellNoteBitSet = allCellIndexBitSet & lineWingBitMask;
                if (KeyCellIndexBitSet == 0 || (allCellIndexBitSet & blockWingBitMask) != 0 || excludedCellNoteBitSet == 0)
                    return null;

                return
                    new PointingOperation(
                        excludedCellNoteBitSet.EnumerateAsCellNotes(cellDigit),
                        intersectionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                        .Concat(blockWingBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(lineWingBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        KeyCellIndexBitSet.EnumerateAsCellNoteEffects(cellDigit, SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(block, lineHouseName, cellDigit, KeyCellIndexBitSet));

                static Func<string> GetDescriptionBuilder(SudokuBoardCellBlock block, string lineHouseName, SudokuBoardCellDigit cellDigit, UInt128 KeyCellIndexBitSet)
                {
                    return
                        () =>
                        {
                            var blockHouseName = block.ToHouseName();
                            var cellDigitChar = cellDigit.ToCellDigitText();
                            var keyCellPositionsText = string.Join(", ", KeyCellIndexBitSet.EnumerateAsCellIndexes().ToCellPositionTexts());
                            return
                                $"{blockHouseName}には、{lineHouseName}と重なるマス ({keyCellPositionsText}) のみに数字の候補 {cellDigitChar} が含まれているので、" +
                                $"必ずこれらのマスの何れかに {cellDigitChar} が入ります。\n" +
                                $"そのため、{lineHouseName}にある {keyCellPositionsText} 以外のマスには {cellDigitChar} は入り得ないので、それらのマスの数字の候補から {cellDigitChar} を排除します。";
                        };
                }
            }
        }
    }
}
