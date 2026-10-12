using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルまたはセルの数字の候補の位置を示す構造体です。
    /// </summary>
    public readonly struct SudokuBoardCellPosition
        : IEquatable<SudokuBoardCellPosition>
    {
        internal SudokuBoardCellPosition(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            var (column, row, _) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
        }

        public SudokuBoardCellPosition(SudokuBoardCellColumn column, SudokuBoardCellRow row)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif
            Column = column;
            Row = row;
        }

        public SudokuBoardCellColumn Column { get; }
        public SudokuBoardCellRow Row { get; }

        public bool Equals(SudokuBoardCellPosition other)
            => Column == other.Column && Row == other.Row;

        public override bool Equals(object? other)
            => other != null && GetType() == other.GetType() && Equals((SudokuBoardCellPosition)other);

        public override int GetHashCode()
            => HashCode.Combine(Column, Row);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SudokuBoardCellPosition left, SudokuBoardCellPosition right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SudokuBoardCellPosition left, SudokuBoardCellPosition right)
            => !left.Equals(right);
    }
}
