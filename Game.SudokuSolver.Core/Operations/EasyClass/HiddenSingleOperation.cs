using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// 隠れシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenSingleOperation
        : SudokuBoardDetermineCellOperation
    {
        private HiddenSingleOperation(
            SudokuBoardCellPositionDigit determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            Func<string> descriptionBuilder)
            : base(
                  "隠れシングル (Hidden Single)",
                  SudokuBoardDifficultyLevel.Easy,
                  determinedCell,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と H 内のセル C とある数字 D に対して、
            // H 内の C の数字の候補に D が含まれており、かつ C 以外のすべてのセルの数字の候補に D が含まれていない場合
            // => C の数字は D で確定する。

            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; ++cellDigit)
            {
                var cellIndexBitSet = ws.GetCellIndexBitSet(cellDigit);

                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            block.ToHouseName(),
                            SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block),
                            cellDigit,
                            cellIndexBitSet);
                    if (operation is not null)
                        yield return operation;
                }

                for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            column.ToHouseName(),
                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column),
                            cellDigit,
                            cellIndexBitSet);
                    if (operation is not null)
                        yield return operation;
                }

                for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            row.ToHouseName(),
                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(row),
                            cellDigit,
                            cellIndexBitSet);
                    if (operation is not null)
                        yield return operation;
                }
            }

            static HiddenSingleOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, string houseName, UInt128 houseBitMask, SudokuBoardCellDigit cellDigit, UInt128 cellIndexBitSet)
            {
                var foundBitSet = cellIndexBitSet & houseBitMask;
                if (UInt128.PopCount(foundBitSet) != 1)
                    return null;
                var determinedCellIndex = foundBitSet.AsSingleCellIndex();
                var excludedCellNotes =
                    ws.GetCellIndexBitSetForExcludedNoteDigit(determinedCellIndex, cellDigit)
                    .EnumerateAsCellNotes(cellDigit)
                    .ToArray();

#if DEBUG
                if (!ws.CanCetermineCell(determinedCellIndex, cellDigit))
                    throw new ApplicationException();
                System.Diagnostics.Debug.Assert(ws.CanCetermineCell(determinedCellIndex, cellDigit));
#endif
                return
                    new HiddenSingleOperation(
                        new SudokuBoardCellPositionDigit(determinedCellIndex, cellDigit),
                        excludedCellNotes,
                        houseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                            .ConcatIf(excludedCellNotes.Length > 0, (SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(determinedCellIndex) & ~houseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        GetDescriptionBuilder(houseName, cellDigit, determinedCellIndex, excludedCellNotes));

                static Func<string> GetDescriptionBuilder(string houseName, SudokuBoardCellDigit cellDigit, SudokuBoardCellIndex determinedCellIndex, ReadOnlyMemory<SudokuBoardCellPositionDigit> excludedCellNotes)
                {
                    return
                        () =>
                        {
                            var determinedCellPosiionText = determinedCellIndex.ToFriendlyString();
                            var determinedCellDigitText = cellDigit.ToCellDigitText();
                            var excludedCellPositionsText = string.Join(
                                ", ",
                                excludedCellNotes
                                .AsEnumerable()
                                .Select(cellNote => new SudokuBoardCellPosition(cellNote.CellIndex).ToFriendlyString()));
                            return
                                $"{houseName}のマスのうち {determinedCellDigitText} であり得るマスが{determinedCellPosiionText}のみであるため、" +
                                $"{determinedCellPosiionText}のマスの数字は {determinedCellDigitText} に確定します。" +
                                $"{(excludedCellNotes.Length > 0 ? $"\nそれとともに、確定したマスと同じハウスに属する別のマス ({excludedCellPositionsText}) の数字の候補から {determinedCellDigitText} を排除します。" : "")}";
                        };
                }
            }
        }
    }
}
