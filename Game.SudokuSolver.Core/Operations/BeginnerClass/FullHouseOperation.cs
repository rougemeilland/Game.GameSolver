using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

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
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "フルハウス",
                  SudokuBoardDifficultyLevel.Beginner,
                  determinedCell,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  description)
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
                        SudokuBoardHouseType.Block,
                        SudokuBoardCellGeometry.GetBlockCellsBitSet(block));
                if (operation is not null)
                    yield return operation;
            }

            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardHouseType.Column,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column));
                if (operation is not null)
                    yield return operation;
            }

            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws, SudokuBoardHouseType.Row,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row));
                if (operation is not null)
                    yield return operation;
            }

            static FullHouseOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, SudokuBoardHouseType house, UInt128 houseMask)
            {
                var determinedCellBitSetByHouse = ws.DeterminedCellBitSet & houseMask;
                if (UInt128.PopCount(determinedCellBitSetByHouse) != 8)
                    return null;
                var foundCellSingleBitSet = ~determinedCellBitSetByHouse & houseMask;
#if DEBUG
                System.Diagnostics.Debug.Assert(UInt128.IsPow2(foundCellSingleBitSet));
#endif

                var undeterminedCellIndex = foundCellSingleBitSet.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(undeterminedCellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
#endif
                var foundCellDigit = FindMissingCellDigit(ws, determinedCellBitSetByHouse);

                var excludedCellNotes =
                    ws.GetCellIndexBitSetForExcludedNoteDigit(undeterminedCellIndex, foundCellDigit)
                    .EnumerateAsCellNotes(foundCellDigit)
                    .ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(undeterminedCellIndex);
                var determinedCellPositionText = undeterminedCellIndex.ToFriendlyString();
                var determinedCellDigitText = foundCellDigit.ToCellDigitText();
                var description =
                    house switch
                    {
                        SudokuBoardHouseType.Row =>
                            $"{determinedCellRow.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが {determinedCellDigitText} 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        SudokuBoardHouseType.Column =>
                            $"{determinedCellColumn.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが {determinedCellDigitText} 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        SudokuBoardHouseType.Block =>
                            $"{determinedCellBlock.ToFriendlyString()}のブロックのセルのうち{determinedCellPositionText}以外のセルが {determinedCellDigitText} 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        _ => throw new ApplicationException(),
                    };
#if DEBUG
                System.Diagnostics.Debug.Assert(ws.CanCetermineCell(undeterminedCellIndex, foundCellDigit));
#endif
                return
                    new FullHouseOperation(
                        new SudokuBoardCellPositionDigit(undeterminedCellIndex, foundCellDigit),
                        excludedCellNotes,
                        houseMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                            .ConcatIf(excludedCellNotes.Length > 0, SudokuBoardCellGeometry.GetVisibleCellsBitSet(undeterminedCellIndex).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        description);

                static SudokuBoardCellDigit FindMissingCellDigit(SudokuBoardWorkspace ws, UInt128 determinedCellBitSetByHouse)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(UInt128.PopCount(determinedCellBitSetByHouse) == 8);
#endif
                    var undeterminedCellDigitBitSet = SudokuBoardCellNoteBitSet.AllCells;
                    while (determinedCellBitSetByHouse != 0)
                    {
                        var cellIndex = determinedCellBitSetByHouse.GetOneCellIndex();
#if DEBUG
                        System.Diagnostics.Debug.Assert(ws.IsDetermined(cellIndex));
#endif
                        var cellDigit = ws.GetDeterminedCellDigit(cellIndex);
                        undeterminedCellDigitBitSet &= ~cellDigit.ToCellNoteBitSet();
                        determinedCellBitSetByHouse &= ~cellIndex.ToCellIndexBitSet();
                    }

#if DEBUG
                    System.Diagnostics.Debug.Assert(undeterminedCellDigitBitSet.IsSingleBit());
#endif
                    return undeterminedCellDigitBitSet.GetOneCellNoteDigit();
                }
            }
        }
    }
}
