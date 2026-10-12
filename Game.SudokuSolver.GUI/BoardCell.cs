using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal readonly struct BoardCell
        : IEquatable<BoardCell>
    {
        public BoardCell(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            var (column, row, _) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
            Digit = digit;
        }

        public BoardCell(BoardCellColumn column, BoardCellRow row, BoardCellDigit digit)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            Column = column;
            Row = row;
            Digit = digit;
        }

        public BoardCellColumn Column { get; }
        public BoardCellRow Row { get; }
        public BoardCellDigit Digit { get; }

        public bool Equals(BoardCell other)
            => Column == other.Column && Row == other.Row && Digit == other.Digit;

        public override bool Equals(object? other)
            => other is not null && GetType() == other.GetType() && Equals((BoardCell)other);

        public override int GetHashCode()
            => HashCode.Combine(Column, Row, Digit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoardCell left, BoardCell right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoardCell left, BoardCell right)
            => !left.Equals(right);
    }
}
