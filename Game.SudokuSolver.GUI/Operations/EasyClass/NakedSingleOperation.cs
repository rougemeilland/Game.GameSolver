using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal class NakedSingleOperation
        : SudokuBoardOperation
    {
        private NakedSingleOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, string description)
            : base("一択セル", DifficultyLevel.Easy, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, ReadOnlyMemory<BoardCellPosition>.Empty, description)
        {
        }

        public static NakedSingleOperation? MatchPattern(BoardWorkspace ws)
        {
            for (var cellIndex = BoardCellIndex.MinValue; cellIndex <= BoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellNoteBits = ws.GetNoteBits(cellIndex);
                if (!ws.IsDetermined(cellIndex) && ushort.IsPow2(cellNoteBits))
                {
                    var cellDigit = cellNoteBits.ToCellDigit();

                    // 未確定であり、指定された数字をメモに含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellNoteBits =
                        ~ws.DeterminedCellBits
                        & ws.GetNoteBits(cellDigit)
                        & BoardCellGeometry.GetVisibleCellsBitMaskByCellIndex(cellIndex)
                        & ~cellIndex.ToBitMask();

                    var removedCellNotes = removedCellNoteBits.EnumerateForCells(cellDigit).ToArray();
                    var (determinedCellColumn, determinedCellRow, _) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
                    var determinedCellPositionText = new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString();
                    var description = $"{determinedCellPositionText}のセルがの候補はひとつしかないため{determinedCellPositionText}のセルの数字は '{cellDigit.ToCellDigitChar()}' に確定します。{(removedCellNotes.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}";
                    return
                        new NakedSingleOperation(
                            new BoardCell(determinedCellColumn, determinedCellRow, cellDigit),
                            removedCellNotes,
                            description);
                }
            }

            return null;
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
