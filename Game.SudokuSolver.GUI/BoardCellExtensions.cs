using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal static class BoardCellExtensions
    {
        /// <summary>
        /// 与えられたセルの位置に対応するからビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt128 ToBitMask(this BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return UInt128.One << (cellIndex - BoardCellIndex.FirstValue);
        }

        /// <summary>
        /// 与えられた整数をセルの位置のビット集合とみなし、そのうちのひとつのセルの位置を取得します。
        /// </summary>
        /// <param name="bits">
        /// セル位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellIndex GetOneCellIndex(this UInt128 bits)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(bits, UInt128.One << 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, UInt128.One << (BoardCellIndex.MaxValue - BoardCellIndex.MinValue + 1));
            if (UInt128.IsPow2(bits) == false)
                throw new ArgumentException($"The \"{nameof(bits)}\" argument must be a power of 2.");
#endif

            return BoardCellIndex.FirstValue + (int)UInt128.TrailingZeroCount(bits);
        }

        /// <summary>
        /// 与えられたセルの数字に対応するからビットマスクを取得します。
        /// </summary>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt128 ToBitMask(this BoardCellDigit cellDigit)
        {
#if DEBUG
            if (cellDigit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellDigit));
#endif

            return UInt128.One << (cellDigit - BoardCellDigit.One);
        }

        /// <summary>
        /// 与えられた整数をセルの数字のビット集合とみなし、そのセルの数字を取得します。
        /// </summary>
        /// <param name="bits">
        /// セルの数字のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値を返します。
        /// </returns>
        /// <remarks>
        /// <para>
        /// 与えられた <paramref name="bits"/> にビット 1 が複数存在した場合の動作は保証されません。
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellDigit ToCellDigit(this ushort bits)
        {
#if DEBUG
            if (bits is < (1 << 0) or >= (1 << (BoardCellDigit.MaxValue - BoardCellDigit.MinValue + 1)))
                throw new ArgumentOutOfRangeException(nameof(bits));
            if (!ushort.IsPow2(bits))
                throw new ArgumentException($"The \"{nameof(bits)}\" argument must be a power of 2.");
#endif

            return BoardCellDigit.One + ushort.TrailingZeroCount(bits);
        }

        /// <summary>
        /// セルの数字 <see cref="BoardCellDigit"/> 値を視覚的な文字に変換します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="char"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToCellDigitChar(this BoardCellDigit digit)
        {
#if DEBUG
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return (char)('1' + (digit - BoardCellDigit.One));
        }

        /// <summary>
        /// セルの数字を示す <see cref="char"/> 値を <see cref="BoardCellDigit"/> 値に変換します。
        /// </summary>
        /// <param name="digitChar">
        /// セルの数字を示す <see cref="char"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="BoardCellDigit"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellDigit ToCellDigit(this char digitChar)
        {
#if DEBUG
            if (digitChar is < '1' or > '9')
                throw new ArgumentOutOfRangeException(nameof(digitChar));
#endif

            return BoardCellDigit.One + (digitChar - '1');
        }

        /// <summary>
        /// セルの列を示す <see cref="BoardCellColumn"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFriendlyString(this BoardCellColumn column)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return $"{column - BoardCellColumn.MinValue + 1}列目";
        }

        /// <summary>
        /// セルの行を示す <see cref="BoardCellRow"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="row">
        /// セルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFriendlyString(this BoardCellRow row)
        {
#if DEBUG
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return $"{row - BoardCellRow.MinValue + 1}行目";
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を列挙します。
        /// </summary>
        /// <param name="cellBits">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCellPosition> EnumerateForCellPositions(this UInt128 cellBits)
        {
#if DEBUG
            if ((cellBits & ~BoardWorkspace.AllCellBits) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellBits));
#endif

            while (cellBits != 0)
            {
                var cellIndex = cellBits.GetOneCellIndex();
                yield return new BoardCellPosition(cellIndex);
                cellBits &= cellIndex.ToBitMask();
            }
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、指定されたセルの数字とともにセルの内容を列挙します。
        /// </summary>
        /// <param name="cellBits">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの内容を示す <see cref="BoardCell"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCell> EnumerateForCells(this UInt128 cellBits, BoardCellDigit digit)
        {
#if DEBUG
            if ((cellBits & ~BoardWorkspace.AllCellBits) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellBits));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            while (cellBits != 0)
            {
                var cellIndex = cellBits.GetOneCellIndex();
                yield return new BoardCell(cellIndex, digit);
                cellBits &= cellIndex.ToBitMask();
            }
        }

        /// <summary>
        /// セルのブロックを示す <see cref="BoardCellBlock"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="block">
        /// セルの行を示す <see cref="BoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFriendlyString(this BoardCellBlock block)
        {
#if DEBUG
            if (block is < BoardCellBlock.MinValue or > BoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif
            return
                block switch
                {
                    BoardCellBlock.TopLeft => "上段左",
                    BoardCellBlock.TopCenter => "上段中央",
                    BoardCellBlock.TopRight => "上段右",
                    BoardCellBlock.CenterLeft => "中段左",
                    BoardCellBlock.CenterCenter => "中段中央",
                    BoardCellBlock.CenterRight => "中段右",
                    BoardCellBlock.BottomLeft => "下段左",
                    BoardCellBlock.BottomCenter => "下段中央",
                    BoardCellBlock.BottomRight => "下段右",
                    _ => throw new ApplicationException(),
                };
        }

        /// <summary>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="position">
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFriendlyString(this BoardCellPosition position)
            => $"{position.Row - BoardCellRow.One + 1}行{position.Column - BoardCellColumn.One + 1}列";
    }
}
