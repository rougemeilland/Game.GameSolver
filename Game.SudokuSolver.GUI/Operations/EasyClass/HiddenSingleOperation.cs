using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.EasyClass
{
    /// <summary>
    /// 隠れシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal class HiddenSingleOperation
        : SudokuBoardOperation
    {
        private HiddenSingleOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, ReadOnlyMemory<BoardCellPosition> relatedCellPositions, string description)
            : base("隠れシングル", DifficultyLevel.Easy, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static HiddenSingleOperation? MatchPattern(BoardWorkspace ws)
        {
            for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
            {
                var noteBits = ws.GetNoteBits(cellDigit);

                for (var block = BoardCellBlock.MinValue; block <= BoardCellBlock.MaxValue; ++block)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Block,
                            BoardCellGeometry.GetBlockCellsBitMask(block),
                            cellDigit,
                            noteBits);
                    if (operation is not null)
                        return operation;
                }

                for (var column = BoardCellColumn.MinValue; column <= BoardCellColumn.MaxValue; ++column)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Column,
                            BoardCellGeometry.GetColumnCellsBitMask(column),
                            cellDigit + 1,
                            noteBits);
                    if (operation is not null)
                        return operation;
                }

                for (var row = BoardCellRow.MinValue; row <= BoardCellRow.MaxValue; ++row)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Row,
                            BoardCellGeometry.GetRowCellsBitMask(row),
                            cellDigit + 1,
                            noteBits);
                    if (operation is not null)
                        return operation;
                }
            }

            return null;

            static HiddenSingleOperation? MatchPatternByHouse(BoardWorkspace ws, HouseType house, UInt128 houseBitMask, BoardCellDigit cellDigit, UInt128 noteBits)
            {
                var foundBits = noteBits & houseBitMask;
                if (UInt128.PopCount(foundBits) != 1)
                    return null;
                var determinedCellIndex = foundBits.GetOneCellIndex();

                // 未確定であり、指定された数字をメモに含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellsNoteBits =
                    ~ws.DeterminedCellBits
                    & noteBits
                    & BoardCellGeometry.GetVisibleCellsBitMaskByCellIndex(determinedCellIndex)
                    & ~foundBits;

                var removedCellNotes = removedCellsNoteBits.EnumerateForCells(cellDigit).ToArray();
                var relatedCellPositions = houseBitMask.EnumerateForCellPositions().ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = BoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                var determinedCellPosiionText = new BoardCellPosition(determinedCellIndex).ToFriendlyString();
                var determinedCellChar = cellDigit.ToCellDigitChar();

                var description =
                    house switch
                    {
                        HouseType.Row => $"{determinedCellRow.ToFriendlyString()}のセルのうち、数字 '{determinedCellChar}' であり得るセルが{determinedCellPosiionText}のみであるため、{determinedCellPosiionText}のセルの数字は '{determinedCellChar}' に確定します。{(removedCellNotes.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                        HouseType.Column => $"{determinedCellColumn.ToFriendlyString()} のセルのうち、数字 '{determinedCellChar}' であり得るセルが{determinedCellPosiionText}のみであるため、{determinedCellPosiionText}のセルの数字は '{determinedCellChar}' に確定します。{(removedCellNotes.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                        HouseType.Block => $"{determinedCellBlock.ToFriendlyString()}のセルが属しているブロックのうち、数字 '{determinedCellChar}' であり得るセルが{determinedCellPosiionText}のみであるため、{determinedCellPosiionText}のセルの数字は '{determinedCellChar}' に確定します。{(removedCellNotes.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                        _ => throw new ArgumentOutOfRangeException(nameof(house)),
                    };

                return
                    new HiddenSingleOperation(
                        new BoardCell(determinedCellIndex, cellDigit),
                        removedCellNotes,
                        relatedCellPositions,
                        description);
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
