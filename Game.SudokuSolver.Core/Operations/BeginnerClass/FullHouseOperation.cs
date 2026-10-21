using System;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.BeginnerClass
{
    /// <summary>
    /// フルハウスパターンオペレーションのクラスです。
    /// </summary>
    internal sealed class FullHouseOperation
        : SudokuBoardOperation
    {
        private FullHouseOperation(
            SudokuBoardCellPositionDigit? determinedCell,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 positivelyRelatedCellIndexBitSet,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "フルハウス",
                  SudokuBoardDifficultyLevel.Beginner,
                  determinedCell,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  removedCellNotes,
                  positivelyRelatedCellIndexBitSet,
                  negativelyRelatedCellIndexBitSet,
                  description)
        {
        }

        public static FullHouseOperation? MatchPattern(SudokuBoardWorkspace ws)
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
                    return operation;
            }

            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardHouseType.Column,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column));
                if (operation is not null)
                    return operation;
            }

            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws, SudokuBoardHouseType.Row,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row));
                if (operation is not null)
                    return operation;
            }

            return null;

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
                var removedCellNotes =
                    ws.GetCellIndexBitSetForRemovedNoteDigit(undeterminedCellIndex, foundCellDigit)
                    .EnumerateAsCellIndexes()
                    .ToCells(foundCellDigit)
                    .ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(undeterminedCellIndex);
                var determinedCellPositionText = new SudokuBoardCellPosition(undeterminedCellIndex).ToFriendlyString();
                var determinedCellDigitText = foundCellDigit.ToCellDigitChar();
                var description =
                    house switch
                    {
                        SudokuBoardHouseType.Row =>
                            $"{determinedCellRow.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが '{determinedCellDigitText}' 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は '{determinedCellDigitText}' に確定します。\n" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        SudokuBoardHouseType.Column =>
                            $"{determinedCellColumn.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが '{determinedCellDigitText}' 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は '{determinedCellDigitText}' に確定します。\n" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        SudokuBoardHouseType.Block =>
                            $"{determinedCellBlock.ToFriendlyString()}のブロックのセルのうち{determinedCellPositionText}以外のセルが '{determinedCellDigitText}' 以外の数字で確定しているため、" +
                            $"{determinedCellPositionText}のセルの数字は '{determinedCellDigitText}' に確定します。\n" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        _ => throw new ApplicationException(),
                    };
                return
                    new FullHouseOperation(
                        new SudokuBoardCellPositionDigit(undeterminedCellIndex, foundCellDigit),
                        removedCellNotes,
                        houseMask,
                        removedCellNotes.Length > 0 ? SudokuBoardCellGeometry.GetVisibleCellsBitSet(undeterminedCellIndex) : UInt128.Zero,
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

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.CellIndex, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
