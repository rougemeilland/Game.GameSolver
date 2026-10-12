using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルまたはセルの数字の候補の位置と数字を示す構造体です。
    /// </summary>
    public readonly struct SudokuBoardCell
        : IEquatable<SudokuBoardCell>
    {
        internal SudokuBoardCell(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            var (column, row, _) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex);
            Column = column;
            Row = row;
            Digit = digit;
        }

        public SudokuBoardCell(SudokuBoardCellColumn column, SudokuBoardCellRow row, SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            Column = column;
            Row = row;
            Digit = digit;
        }

        public SudokuBoardCellColumn Column { get; }
        public SudokuBoardCellRow Row { get; }
        public SudokuBoardCellDigit Digit { get; }

        public bool Equals(SudokuBoardCell other)
            => Column == other.Column && Row == other.Row && Digit == other.Digit;

        public override bool Equals(object? other)
            => other is not null && GetType() == other.GetType() && Equals((SudokuBoardCell)other);

        public override int GetHashCode()
            => HashCode.Combine(Column, Row, Digit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SudokuBoardCell left, SudokuBoardCell right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SudokuBoardCell left, SudokuBoardCell right)
            => !left.Equals(right);
    }
}
