using System;
using System.Collections.Generic;

namespace Game.SudokuSolver.GUI.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal class NakedSingleOperation
        : SudokuBoardOperation
    {
        private NakedSingleOperation(int determinedCellColumn, int determinedCellRow, int determinedCellDigit, UInt128 removedCellNoteBits)
            : base(DifficultyLevel.Easy)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(determinedCellColumn, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(determinedCellColumn, 9);
            ArgumentOutOfRangeException.ThrowIfLessThan(determinedCellRow, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(determinedCellRow, 9);
            ArgumentOutOfRangeException.ThrowIfLessThan(determinedCellDigit, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(determinedCellDigit, 9);
            System.Diagnostics.Debug.Assert((removedCellNoteBits & ~BoardWorkspace.AllCellsBits) == 0);
#endif
            DeterminedCell = new BoardCell(determinedCellColumn, determinedCellRow, determinedCellDigit);

            var removedCellNotes = new List<BoardCell>();
            for (var cellIndex = 0; cellIndex < 81; ++cellIndex)
            {
                var cellBitMask = UInt128.One << cellIndex;
                if ((removedCellNoteBits & cellBitMask) != 0)
                    removedCellNotes.Add(new BoardCell(cellIndex % 9, cellIndex / 9, determinedCellDigit));
            }

            RemovedCellNotes = removedCellNotes.ToArray();

            Description = $"行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルがの候補はひとつしかないため、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルの数字は {determinedCellDigit} に確定します。{(RelatedCells.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}";
        }

        /// <inheritdoc/>
        public override BoardCell? DeterminedCell { get; }

        /// <inheritdoc/>
        public override ReadOnlyMemory<BoardCell> RemovedCellNotes { get; }

        /// <inheritdoc/>
        public override string Description { get; }

        public static NakedSingleOperation? MatchPattern(BoardWorkspace ws)
        {
            for (var row = 0; row < 9; ++row)
            {
                for (var column = 0; column < 9; ++column)
                {
                    var cellNoteBits = ws.GetNoteBits(row, column);
                    if (!ws.IsDetermined(column, row) && ushort.IsPow2(cellNoteBits))
                    {
                        var cellDigitMinusOne = ushort.TrailingZeroCount(cellNoteBits);

                        var removedCellNoteBits =
                            ws.GetBitsByDigit(cellDigitMinusOne + 1)
                            & ~ws.DeterminedCellBits
                            & ~(UInt128.One << row * 9 + column)
                            & ~BoardCellHouse.GetVisibleCellsBitMask(row, column);
                        return new NakedSingleOperation(column, row, cellDigitMinusOne + 1, removedCellNoteBits);
                    }
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
