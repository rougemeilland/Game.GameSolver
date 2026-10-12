using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.BeginnerClass
{
    /// <summary>
    /// フルハウスパターンオペレーションのクラスです。
    /// </summary>
    internal class FullHouseOperation
        : SudokuBoardOperation
    {
        private FullHouseOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, ReadOnlyMemory<BoardCellPosition> relatedCellPositions, string description)
            : base("フルハウス", DifficultyLevel.Beginner, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static FullHouseOperation? MatchPattern(BoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H において、あるセル C を除きすべてのセルが確定している場合
            // => C の数字は、H の他のセルの数字のどれとも一致しない数字で確定する。

            for (var block = BoardCellBlock.MinValue; block <= BoardCellBlock.MaxValue; ++block)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        HouseType.Block,
                        BoardCellGeometry.GetBlockCellsBitSet(block));
                if (operation is not null)
                    return operation;
            }

            for (var column = BoardCellColumn.MinValue; column <= BoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        HouseType.Column,
                        BoardCellGeometry.GetColumnCellsBitSet(column));
                if (operation is not null)
                    return operation;
            }

            for (var row = BoardCellRow.MinValue; row <= BoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws, HouseType.Row,
                        BoardCellGeometry.GetRowCellsBitSet(row));
                if (operation is not null)
                    return operation;
            }

            return null;

            static FullHouseOperation? MatchPatternByHouse(BoardWorkspace ws, HouseType house, UInt128 houseMask)
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
                System.Diagnostics.Debug.Assert(determinedCellIndex is >= BoardCellIndex.MinValue and <= BoardCellIndex.MaxValue);
#endif

                // 未確定であり、確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellNoteBitSet =
                    ~ws.DeterminedCellBitSet
                    & ~foundCellSingleBitSet
                    & BoardCellGeometry.GetVisibleCellsBitSetByCellIndex(determinedCellIndex);

                for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
                {
                    if ((~ws.GetNoteBitSet(cellDigit) & foundCellSingleBitSet) != 0)
                    {
                        var removedCellNotes = removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                        var (determinedCellColumn, determinedCellRow, determinedCellBlock) = BoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                        var determinedCellPositionText = new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString();
                        var determinedCellDigitText = cellDigit.ToCellDigitChar();
                        var description =
                            house switch
                            {
                                HouseType.Row =>
                                    $"{determinedCellRow.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                HouseType.Column =>
                                    $"{determinedCellColumn.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                HouseType.Block =>
                                    $"{determinedCellBlock.ToFriendlyString()}のブロックのセルのうち{determinedCellPositionText}以外のセルが確定しているため、" +
                                    $"{determinedCellPositionText}のセルの数字を '{determinedCellDigitText}' に確定します。" +
                                    $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                                _ => throw new ApplicationException(),
                            };
                        return
                            new FullHouseOperation(
                                new BoardCell(determinedCellIndex, cellDigit),
                                removedCellNotes,
                                houseMask.EnumerateAsCellIndexes().ToCellPositions().ToArray(),
                                description);
                    }
                }

                throw new ApplicationException("Reached code that should be unreachable.");
            }
        }

        public override BoardCellsSet Execute(BoardCellsSet cells)
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
