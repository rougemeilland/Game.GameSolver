using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// ボックスライン・リダクションのオペレーションのクラスです。
    /// </summary>
    internal sealed class BoxLineReductionOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private BoxLineReductionOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "ボックスライン・リダクション (Claiming)",
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
            // B 内の CorR 上の何れかのセルの数字の候補に D が存在し、かつ B の範囲外の CorR 上以外のセルの数字の候補に D が存在しない場合
            // => B 内の CorR 上のセルの数字の候補は何れかが D であるので、仮に B 内の CorR 上以外のセルの数字の候補に D が あってもそれらのセルは D にはなり得ない。

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
                                row.ToFriendlyString(),
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
                                column.ToFriendlyString(),
                                cellDigit,
                                allCellIndexBitSet);
                        if (operation is not null)
                            yield return operation;
                    }
                }
            }

            static BoxLineReductionOperation? MatchPatternByHouse(
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
                var highlightedCellNoteBitSet = allCellIndexBitSet & intersectionBitMask;
                var excludedCellNoteBitSet = allCellIndexBitSet & blockWingBitMask;
                if (highlightedCellNoteBitSet == 0 || (allCellIndexBitSet & lineWingBitMask) != 0 || excludedCellNoteBitSet == 0)
                    return null;

                return
                    new BoxLineReductionOperation(
                        excludedCellNoteBitSet.EnumerateAsCellNotes(cellDigit),
                        intersectionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(lineHouseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(blockWingBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        highlightedCellNoteBitSet.EnumerateAsCellNoteEffects(cellDigit, SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(block, lineHouseName, cellDigit));

                static Func<string> GetDescriptionBuilder(SudokuBoardCellBlock block, string lineHouseName, SudokuBoardCellDigit cellDigit)
                {
                    return
                        () =>
                        {
                            var blockHouseName = block.ToHouseName();
                            var cellDigitChar = cellDigit.ToCellDigitText();
                            return
                                $"{lineHouseName}には{blockHouseName}と重なるマスのみに数字の候補 {cellDigitChar} が含まれており、" +
                                $"{cellDigitChar} は必ずこれらのマスの何れかに入ります。\n" +
                                $"したがって、{blockHouseName}にある他のマスには {cellDigitChar} は入り得ないので、それらのマスの数字の候補から {cellDigitChar} を排除します。";
                        };
                }
            }
        }
    }
}
