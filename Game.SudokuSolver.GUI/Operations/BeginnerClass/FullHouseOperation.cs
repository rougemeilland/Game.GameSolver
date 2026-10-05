using System;
using System.Collections.Generic;

namespace Game.SudokuSolver.GUI.Operations.BeginnerClass
{
    /// <summary>
    /// フルハウスパターンオペレーションのクラスです。
    /// </summary>
    internal class FullHouseOperation
        : SudokuBoardOperation
    {
        private FullHouseOperation(HouseType house, UInt128 cellHouseBits, int determinedCellColumn, int determinedCellRow, int determinedCellDigit, UInt128 removedCellNoteBits)
            : base(DifficultyLevel.Beginner)
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

            var relatedCells = new List<BoardCellPosition>();
            for (var cellIndex = 0; cellIndex < 81; ++cellIndex)
            {
                if ((cellHouseBits & UInt128.One << cellIndex) != 0)
                    relatedCells.Add(new BoardCellPosition(cellIndex % 9, cellIndex / 9));
            }

            RelatedCells = relatedCells.ToArray();

            Description =
                house switch
                {
                    HouseType.Row => $"列 {determinedCellRow + 1} のセルのうち、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} 以外のセルが既に確定しているため、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルの数字は {determinedCellDigit} に確定します。{(RelatedCells.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                    HouseType.Column => $"行 {determinedCellColumn + 1} のセルのうち、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} 以外のセルが既に確定しているため、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルの数字は {determinedCellDigit} に確定します。{(RelatedCells.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                    HouseType.Block => $"行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルが属しているブロックのうち、{determinedCellColumn + 1} 列 {determinedCellRow + 1} 以外のセルが既に確定しているため、行 {determinedCellColumn + 1} 列 {determinedCellRow + 1} のセルの数字は {determinedCellDigit} に確定します。{(RelatedCells.Length > 0 ? "これにより他のいくつかのセルのメモが削除されます。" : "")}",
                    _ => throw new ArgumentOutOfRangeException(nameof(house)),
                };
        }

        /// <inheritdoc/>
        public override BoardCell? DeterminedCell { get; }

        /// <inheritdoc/>
        public override ReadOnlyMemory<BoardCell> RemovedCellNotes { get; }

        /// <inheritdoc/>
        public override ReadOnlyMemory<BoardCellPosition> RelatedCells { get; }

        /// <inheritdoc/>
        public override string Description { get; }

        public static FullHouseOperation? MatchPattern(BoardWorkspace ws)
        {
            for (var block = 0; block < 9; ++block)
            {
                var houseMask = BoardCellHouse.GetBlockCellsBitMask(block);
                var operation = MatchPatternByHouse(ws, HouseType.Block, houseMask);
                if (operation is not null)
                    return operation;
            }

            for (var column = 0; column < 9; ++column)
            {
                var houseMask = BoardCellHouse.GetColumnCellsBitMask(column);
                var operation = MatchPatternByHouse(ws, HouseType.Column, houseMask);
                if (operation is not null)
                    return operation;
            }

            for (var row = 0; row < 9; ++row)
            {
                var houseMask = BoardCellHouse.GetRowCellsBitMask(row);
                var operation = MatchPatternByHouse(ws, HouseType.Row, houseMask);
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
                System.Diagnostics.Debug.Assert(UInt128.IsPow2(foundBitMask) == true && foundBitMask < UInt128.One << 81);
#endif
                var pos = (uint)UInt128.TrailingZeroCount(foundBitMask);
                var row = (int)(pos / 9);
                var column = (int)(pos % 9);
                var visibleBitMask =
                    ~ws.DeterminedCellBits
                    & ~foundBitMask
                    & BoardCellHouse.GetVisibleCellsBitMask(row, column);
                for (var digitMinusOne = 0; digitMinusOne < 9; ++digitMinusOne)
                {
                    var b = ws.GetBitsByDigit(digitMinusOne + 1);
                    if ((b & foundBitMask) != 0)
                        return new FullHouseOperation(house, houseMask, column, row, 1, b & visibleBitMask);
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
