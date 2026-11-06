using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.BeginnerClass
{
    /// <summary>
    /// フルハウスパターンオペレーションのクラスです。
    /// </summary>
    internal sealed class FullHouseOperation
        : SudokuBoardDetermineCellOperation
    {
        private FullHouseOperation(
            SudokuBoardCellPositionDigit determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            Func<string> descriptionBuilder)
            : base(
                  "フルハウス (Full House)",
                  SudokuBoardDifficultyLevel.Beginner,
                  determinedCell,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H において、あるセル C を除きすべてのセルが確定している場合
            // => C の数字は、H の他のセルの数字のどれとも一致しない数字で確定する。

            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        block.ToHouseName(),
                        SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block));
                if (operation is not null)
                    yield return operation;
            }

            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        column.ToHouseName(),
                        SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column));
                if (operation is not null)
                    yield return operation;
            }

            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        row.ToHouseName(),
                        SudokuBoardCellGeometry.GetRowCellIndexBitSet(row));
                if (operation is not null)
                    yield return operation;
            }

            static FullHouseOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, string houseName, UInt128 houseMask)
            {
                var determinedCellBitSetByHouse = ws.DeterminedCellBitSet & houseMask;
                if (UInt128.PopCount(determinedCellBitSetByHouse) != 8)
                    return null;
                var undeterminedCellIndex = (~determinedCellBitSetByHouse & houseMask).AsSingleCellIndex();
                var foundCellDigit = FindMissingCellDigit(ws, determinedCellBitSetByHouse);

                var excludedCellNotes =
                    ws.GetCellIndexBitSetForExcludedNoteDigit(undeterminedCellIndex, foundCellDigit)
                    .EnumerateAsCellNotes(foundCellDigit)
                    .ToArray();
#if DEBUG
                System.Diagnostics.Debug.Assert(ws.CanCetermineCell(undeterminedCellIndex, foundCellDigit));
#endif
                return
                    new FullHouseOperation(
                        new SudokuBoardCellPositionDigit(undeterminedCellIndex, foundCellDigit),
                        excludedCellNotes,
                        houseMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                            .ConcatIf(excludedCellNotes.Length > 0, (SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(undeterminedCellIndex) & ~houseMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        GetDescriptionBuilder(houseName, undeterminedCellIndex, foundCellDigit, excludedCellNotes.Length > 0));

                static SudokuBoardCellDigit FindMissingCellDigit(SudokuBoardWorkspace ws, UInt128 determinedCellBitSetByHouse)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(UInt128.PopCount(determinedCellBitSetByHouse) == 8);
#endif
                    var undeterminedCellDigitBitSet = SudokuBoardCellDigitBitSet.AllCellDigits;
                    while (determinedCellBitSetByHouse != 0)
                    {
                        var cellIndex = determinedCellBitSetByHouse.GetOneCellIndex();
#if DEBUG
                        System.Diagnostics.Debug.Assert(ws.IsDetermined(cellIndex));
#endif
                        var cellDigit = ws.GetDeterminedCellDigit(cellIndex);
                        undeterminedCellDigitBitSet &= ~cellDigit.ToCellDigitBitSet();
                        determinedCellBitSetByHouse &= ~cellIndex.ToCellIndexBitSet();
                    }

#if DEBUG
                    System.Diagnostics.Debug.Assert(undeterminedCellDigitBitSet.IsSingleBit());
#endif
                    return undeterminedCellDigitBitSet.AsSingleCellDigit();
                }

                static Func<string> GetDescriptionBuilder(string houseName, SudokuBoardCellIndex undeterminedCellIndex, SudokuBoardCellDigit foundCellDigit, bool existExcludedCellNotes)
                {
                    return
                        () =>
                        {
                            var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(undeterminedCellIndex);
                            var determinedCellPositionText = undeterminedCellIndex.ToFriendlyString();
                            var determinedCellDigitText = foundCellDigit.ToCellDigitText();
                            return
                                $"{houseName}のマスのうち{determinedCellPositionText}以外のマスが {determinedCellDigitText} 以外の数字で確定しているため、" +
                                $"{determinedCellPositionText}のマスの数字は {determinedCellDigitText} に確定します。" +
                                $"{(existExcludedCellNotes ? $"\nそれとともに、確定したマスと同じハウスに属する別のマスの数字の候補から {determinedCellDigitText} を排除します。" : "")}";
                        };
                }
            }
        }
    }
}
