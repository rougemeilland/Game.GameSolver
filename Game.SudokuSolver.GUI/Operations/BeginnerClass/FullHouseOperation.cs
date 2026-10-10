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
            for (var block = BoardCellBlock.MinValue; block <= BoardCellBlock.MaxValue; ++block)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        HouseType.Block,
                        BoardCellGeometry.GetBlockCellsBitMask(block));
                if (operation is not null)
                    return operation;
            }

            for (var column = BoardCellColumn.MinValue; column <= BoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        HouseType.Column,
                        BoardCellGeometry.GetColumnCellsBitMask(column));
                if (operation is not null)
                    return operation;
            }

            for (var row = BoardCellRow.MinValue; row <= BoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws, HouseType.Row,
                        BoardCellGeometry.GetRowCellsBitMask(row));
                if (operation is not null)
                    return operation;
            }

            return null;

            static FullHouseOperation? MatchPatternByHouse(BoardWorkspace ws, HouseType house, UInt128 houseMask)
            {
                var determinedCellBitsByBlock = ws.DeterminedCellBits & houseMask;
                if (UInt128.PopCount(determinedCellBitsByBlock) != 8)
                    return null;
                var foundBitMask = ~determinedCellBitsByBlock & houseMask;
#if DEBUG
                System.Diagnostics.Debug.Assert(UInt128.IsPow2(foundBitMask));
#endif
                var determinedCellIndex = foundBitMask.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(determinedCellIndex is >= BoardCellIndex.MinValue and <= BoardCellIndex.MaxValue);
#endif

                // 未確定であり、確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellNoteBits =
                    ~ws.DeterminedCellBits
                    & ~foundBitMask
                    & BoardCellGeometry.GetVisibleCellsBitMaskByCellIndex(determinedCellIndex);

                for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
                {
                    var cellNoteBits = ws.GetNotNoteBits(cellDigit);
                    if ((cellNoteBits & foundBitMask) != 0)
                    {
                        var removedCellNotePositions = removedCellNoteBits.EnumerateForCellPositions().ToArray();
                        var (determinedCellColumn, determinedCellRow, determinedCellBlock) = BoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                        var determinedCellPositionText = new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString();
                        var determinedCellDigitText = cellDigit.ToCellDigitChar();
                        var description =
                            house switch
                            {
                                HouseType.Row => $"{determinedCellRow.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、{determinedCellPositionText}セルの数字を '{determinedCellDigitText}' に確定します。{(removedCellNotePositions.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                                HouseType.Column => $"{determinedCellColumn.ToFriendlyString()}の行のセルのうち{determinedCellPositionText}以外のセルが確定しているため、{determinedCellPositionText}セルの数字を '{determinedCellDigitText}' に確定します。{(removedCellNotePositions.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                                HouseType.Block => $"{determinedCellBlock.ToFriendlyString()}のブロックのセルのうち{determinedCellPositionText}以外のセルが確定しているため、{determinedCellPositionText}セルの数字を '{determinedCellDigitText}' に確定します。{(removedCellNotePositions.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                                _ => throw new ApplicationException(),
                            };
                        return
                            new FullHouseOperation(
                                new BoardCell(determinedCellIndex, cellDigit),
                                cellNoteBits.EnumerateForCells(cellDigit).ToArray(),
                                removedCellNotePositions,
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
