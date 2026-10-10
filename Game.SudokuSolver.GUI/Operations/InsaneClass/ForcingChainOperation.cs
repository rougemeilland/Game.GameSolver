using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.InsaneClass
{
    /// <summary>
    /// 強制チェーンパターンのオペレーションのクラスです。
    /// </summary>
    internal class ForcingChainOperation
        : SudokuBoardOperation
    {
        private ForcingChainOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, string description)
            : base("強制チェーン", DifficultyLevel.Insane, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, ReadOnlyMemory<BoardCellPosition>.Empty, description)
        {
        }

        public static IEnumerable<ForcingChainOperation> MatchPattern(BoardWorkspace ws)
        {
            var undterminedBits = ~ws.DeterminedCellBits & BoardWorkspace.AllCellBits;
#if DEBUG
            System.Diagnostics.Debug.Assert(undterminedBits != 0);
#endif
            var foundCellIndex = undterminedBits.GetOneCellIndex();
            var foundCellMask = foundCellIndex.ToBitMask();
            for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
            {
                var noteBits = ws.GetNoteBits(cellDigit);
                if ((noteBits & foundCellMask) != 0)
                {
                    // 未確定であり、指定された数字をメモに含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellsNoteBits =
                        ~ws.DeterminedCellBits
                        & noteBits
                        & BoardCellGeometry.GetVisibleCellsBitMaskByCellIndex(foundCellIndex)
                        & ~foundCellMask;

                    var (determinedCellColumn, determinedCellRow, _) = BoardCellGeometry.GetColumnRowBlock(foundCellIndex);
                    var removedCellNotes = removedCellsNoteBits.EnumerateForCells(cellDigit).ToArray();
                    var description = $"{new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString()}のセルの数字を '{cellDigit.ToCellDigitChar()}' と仮定します。{(removedCellNotes.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}";

                    yield return
                        new ForcingChainOperation(
                            new BoardCell(determinedCellColumn, determinedCellRow, cellDigit),
                            removedCellNotes,
                            description);
                }
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
