using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCellPosition
        : IEquatable<BoardCellPosition>
    {
        public BoardCellPosition(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            var (column, row, _) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
        }

        public BoardCellPosition(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif
            Column = column;
            Row = row;
        }

        public BoardCellColumn Column { get; }
        public BoardCellRow Row { get; }

        public bool Equals(BoardCellPosition other)
            => Column == other.Column && Row == other.Row;

        public override bool Equals(object? other)
            => other != null && GetType() == other.GetType() && Equals((BoardCellPosition)other);

        public override int GetHashCode()
            => HashCode.Combine(Column, Row);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoardCellPosition left, BoardCellPosition right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoardCellPosition left, BoardCellPosition right)
            => !left.Equals(right);
    }
}
