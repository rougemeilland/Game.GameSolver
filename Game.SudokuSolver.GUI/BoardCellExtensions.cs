using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal static class BoardCellExtensions
    {
        #region for char

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

        #endregion

        #region for UInt128

        /// <summary>
        /// 与えられた整数をセルの位置のビット集合とみなし、そのうちのひとつのセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セル位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellIndex GetOneCellIndex(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(cellIndexBitSet, UInt128.One << 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cellIndexBitSet, UInt128.One << (BoardCellIndex.MaxValue - BoardCellIndex.MinValue + 1));
            if (UInt128.IsPow2(cellIndexBitSet) == false)
                throw new ArgumentException($"The \"{nameof(cellIndexBitSet)}\" argument must be a power of 2.");
#endif

            return BoardCellIndex.FirstValue + (int)UInt128.TrailingZeroCount(cellIndexBitSet);
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を列挙します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCellIndex> EnumerateAsCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            if ((cellIndexBitSet & ~BoardWorkspace.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return cellIndex;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

        #endregion

        #region for BoardCellColumn

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

        #endregion

        #region for BoardCellRow

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

        #endregion

        #region for BoardCellBlock

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

        #endregion

        #region for BoardCellIndex

        /// <summary>
        /// 与えられたセルの位置に対応するビット集合を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt128 ToCellIndexBitSet(this BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return UInt128.One << (cellIndex - BoardCellIndex.FirstValue);
        }

        /// <summary>
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFriendlyString(this BoardCellIndex cellIndex)
        {
            var (column, row, _) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
            return ToFriendlyString(column, row);
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="BoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCellPosition> ToCellPositions(this IEnumerable<BoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new BoardCellPosition(cellIndex);
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置を示す文字列のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="BoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトの列挙子を返します。
        /// </returns>
        public static IEnumerable<string> ToCellPositionTexts(this IEnumerable<BoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new BoardCellPosition(cellIndex).ToFriendlyString();
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置と数字のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="BoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置と数字を示す <see cref="BoardCell"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCell> ToCells(this IEnumerable<BoardCellIndex> cellIndexes, BoardCellDigit cellDigit)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new BoardCell(cellIndex, cellDigit);
        }

        /// <summary>
        /// セルの番号のコレクションを読みやすい文字列のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="BoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セル番号の読みやすい文字列を示す <see cref="string"/> オブジェクトの列挙子を返します。
        /// </returns>
        public static IEnumerable<string> ToFriendlyStrings(this IEnumerable<BoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return cellIndex.ToFriendlyString();
        }

        #endregion

        #region for BoardCellDigit

        /// <summary>
        /// 与えられたセルの数字に対応するからビット集合を取得します。
        /// </summary>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellNoteBitSet ToCellNoteBitSet(this BoardCellDigit cellDigit)
        {
#if DEBUG
            if (cellDigit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellDigit));
#endif

            return (BoardCellNoteBitSet)(1u << (cellDigit - BoardCellDigit.One));
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
        /// セルの数字のコレクションをセルの位置と数字のコレクションに変換します。
        /// </summary>
        /// <param name="cellDigits">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値の列挙子です。
        /// </param>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置と数字を示す <see cref="BoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCell> ToCells(this IEnumerable<BoardCellDigit> cellDigits, BoardCellIndex cellIndex)
        {
            foreach (var cellDigit in cellDigits)
                yield return new BoardCell(cellIndex, cellDigit);
        }

        #endregion

        #region for BoardCellNoteBitSet

        /// <summary>
        /// 与えられたセルの数字のビット集合に含まれる数字がひとつだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="BoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる数字が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSingleBit(this BoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is BoardCellNoteBitSet.None or > BoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合に含まれる数字の数を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="BoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる数字の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPopCount(this BoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is BoardCellNoteBitSet.None or > BoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合からひとつの数字を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="BoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardCellDigit GetOneCellNoteDigit(this BoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is BoardCellNoteBitSet.None or > BoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return BoardCellDigit.One + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を列挙します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="BoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellDigit"/> 値の列挙子を返します。
        /// </returns>
        public static IEnumerable<BoardCellDigit> EnumerateAsCellDigits(this BoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if ((value & ~BoardWorkspace.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellDigit = cellIndexBitSet.GetOneCellNoteDigit();
                yield return cellDigit;
                cellIndexBitSet &= ~cellDigit.ToCellNoteBitSet();
            }
        }

        #endregion

        #region for BoardCellPosition

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
            => ToFriendlyString(position.Column, position.Row);

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToFriendlyString(BoardCellColumn column, BoardCellRow row)
            => $"{row - BoardCellRow.One + 1}行{column - BoardCellColumn.One + 1}列";

        #endregion
    }
}
