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
            CellIndex = cellIndex;
            (Column, Row, _) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex);
        }

        /// <summary>
        /// セルの位置を示す番号を取得します。
        /// </summary>
        /// <value>
        /// セルの位置の番号を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </value>
        public SudokuBoardCellIndex CellIndex { get; }

        /// <summary>
        /// セルの位置の列の値を取得します。
        /// </summary>
        /// <value>
        /// セルの位置の列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </value>
        internal SudokuBoardCellColumn Column { get; }

        /// <summary>
        /// セルの位置の行の値を取得します。
        /// </summary>
        /// <value>
        /// セルの位置の行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </value>
        internal SudokuBoardCellRow Row { get; }

        /// <inheritdoc/>
        public bool Equals(SudokuBoardCellPosition other)
            => CellIndex == other.CellIndex;

        /// <inheritdoc/>
        public override bool Equals(object? other)
            => other != null && GetType() == other.GetType() && Equals((SudokuBoardCellPosition)other);

        /// <inheritdoc/>
        public override int GetHashCode()
            => CellIndex.GetHashCode();

        /// <summary>
        /// 二つの <see cref="SudokuBoardCellPosition"/> 値が等値かどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等値であれば <see langword="true"/>、そうでなければ <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SudokuBoardCellPosition left, SudokuBoardCellPosition right)
            => left.Equals(right);

        /// <summary>
        /// 二つの <see cref="SudokuBoardCellPosition"/> 値が等値でないかどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等値でなければ <see langword="true"/>、そうでなければ <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SudokuBoardCellPosition left, SudokuBoardCellPosition right)
            => !left.Equals(right);
    }
}
