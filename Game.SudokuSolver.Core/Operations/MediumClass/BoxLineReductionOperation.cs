using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

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
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "ボックスライン・リダクション (Locked Candidates (Claiming))",
                  SudokuBoardDifficultyLevel.Medium,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
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

            static BoxLineReductionOperation? MatchPatternByHouse(SudokuBoardCellBlock block, UInt128 blockHouseBitMask, UInt128 columnOrRowHouseBitMask, string lineName, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var highlightedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & columnOrRowHouseBitMask;
                var excludedCellNoteBitSet = cellNoteBitSet & blockHouseBitMask & ~columnOrRowHouseBitMask;
                if (highlightedCellNoteBitSet == 0 || (cellNoteBitSet & ~blockHouseBitMask & columnOrRowHouseBitMask) != 0 || excludedCellNoteBitSet == 0)
                    return null;

                var blockName = block.ToFriendlyString();
                var cellDigitChar = cellDigit.ToCellDigitText();
                var excludedCellNotePositionTexts = $"({string.Join(", ", excludedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings())})";
                var description =
                    $"{lineName}には{blockName}のブロックと重なるマスのみに数字の候補 {cellDigitChar} が含まれており、" +
                    $"{cellDigitChar} は必ずこれらのマスの何れかに入ります。\n" +
                    $"したがって、{blockName}のブロックにある他のマス {excludedCellNotePositionTexts} の数字の候補から {cellDigitChar} を排除します。";
                return
                    new BoxLineReductionOperation(
                        excludedCellNoteBitSet.EnumerateAsCellNotes(cellDigit),
                        columnOrRowHouseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                            .Concat((blockHouseBitMask & ~columnOrRowHouseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        highlightedCellNoteBitSet.EnumerateAsCellNoteEffects(cellDigit, SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        description);
            }
        }
    }
}
