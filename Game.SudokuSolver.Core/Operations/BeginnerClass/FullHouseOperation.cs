using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.BeginnerClass
{
    /// <summary>
    /// フルハウスパターンオペレーションのクラスです。
    /// </summary>
    internal sealed class FullHouseOperation
        : SudokuBoardOperation
    {
        private FullHouseOperation(SudokuBoardCell? determinedCell, ReadOnlyMemory<SudokuBoardCell> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base("フルハウス", SudokuBoardDifficultyLevel.Beginner, determinedCell, ReadOnlyMemory<SudokuBoardCell>.Empty, removedCellNotes, relatedCellPositions, description)
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
                var determinedCellIndex = foundCellSingleBitSet.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(determinedCellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
#endif

                // 未確定であり、確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellNoteBitSet =
                    ~ws.DeterminedCellBitSet
                    & ~foundCellSingleBitSet
                    & SudokuBoardCellGeometry.GetVisibleCellsBitSetByCellIndex(determinedCellIndex);

                for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; ++cellDigit)
                {
                    if ((~ws.GetNoteBitSet(cellDigit) & foundCellSingleBitSet) != 0)
                    {
                        var removedCellNotes = removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                        var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                        var determinedCellPositionText = new SudokuBoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString();
                        var determinedCellDigitText = cellDigit.ToCellDigitChar();
                        var description =
                            house switch
                            {
                                SudokuBoardHouseType.Row =>
                                    $"{determinedCellRow.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                SudokuBoardHouseType.Column =>
                                    $"{determinedCellColumn.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                SudokuBoardHouseType.Block =>
                                    $"{determinedCellBlock.ToFriendlyString()}のブロックのセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                _ => throw new ApplicationException(),
                            };
                        return
                            new FullHouseOperation(
                                new SudokuBoardCell(determinedCellIndex, cellDigit),
                                removedCellNotes,
                                houseMask.EnumerateAsCellIndexes().ToCellPositions().ToArray(),
                                description);
                    }
                }

                throw new ApplicationException("Reached code that should be unreachable.");
            }
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.Column, DeterminedCell.Value.Row, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
