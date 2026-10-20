using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の拡張メソッドのクラスです。
    /// </summary>
    public static class SudokuExtensions
    {
        #region for char

        /// <summary>
        /// セルの数字を示す <see cref="char"/> 値を <see cref="SudokuBoardCellDigit"/> 値に変換します。
        /// </summary>
        /// <param name="digitChar">
        /// セルの数字を示す <see cref="char"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellDigit ToCellDigit(this char digitChar)
        {
#if DEBUG
            if (digitChar is < '1' or > '9')
                throw new ArgumentOutOfRangeException(nameof(digitChar));
#endif

            return SudokuBoardCellDigit.One + (digitChar - '1');
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
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellIndex GetOneCellIndex(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(cellIndexBitSet, UInt128.One << 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cellIndexBitSet, UInt128.One << (SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1));
#endif

            return SudokuBoardCellIndex.FirstValue + (int)UInt128.TrailingZeroCount(cellIndexBitSet);
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を列挙します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellIndex> EnumerateAsCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return cellIndex;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

#if DEBUG

        internal static void PrintToDebugConsole(this UInt128 cellIndexBitSet, string title)
        {
            var mask = UInt128.One;
            System.Diagnostics.Debug.WriteLine($"[CellIndexBitSet-{title}]");
            for (var count = 0; count < 81; ++count)
            {
                System.Diagnostics.Debug.Write((cellIndexBitSet & mask) != 0 ? "*" : "_");
                if ((count + 1) % 9 == 0)
                    System.Diagnostics.Debug.WriteLine("");
                mask <<= 1;
            }
        }
#endif

        #endregion

        #region for SudokuBoardCellColumn

        /// <summary>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return $"{column - SudokuBoardCellColumn.MinValue + 1}列目";
        }

        #endregion

        #region for SudokuBoardCellRow

        /// <summary>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="row">
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellRow row)
        {
#if DEBUG
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return $"{row - SudokuBoardCellRow.MinValue + 1}行目";
        }

        #endregion

        #region for SudokuBoardCellBlock

        /// <summary>
        /// セルのブロックを示す <see cref="SudokuBoardCellBlock"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="block">
        /// セルの行を示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif
            return
                block switch
                {
                    SudokuBoardCellBlock.TopLeft => "上段左",
                    SudokuBoardCellBlock.TopCenter => "上段中央",
                    SudokuBoardCellBlock.TopRight => "上段右",
                    SudokuBoardCellBlock.CenterLeft => "中段左",
                    SudokuBoardCellBlock.CenterCenter => "中段中央",
                    SudokuBoardCellBlock.CenterRight => "中段右",
                    SudokuBoardCellBlock.BottomLeft => "下段左",
                    SudokuBoardCellBlock.BottomCenter => "下段中央",
                    SudokuBoardCellBlock.BottomRight => "下段右",
                    _ => throw new ApplicationException(),
                };
        }

        #endregion

        #region for SudokuBoardCellIndex

        /// <summary>
        /// 与えられたセルの位置に対応するビット集合を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UInt128 ToCellIndexBitSet(this SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return UInt128.One << (cellIndex - SudokuBoardCellIndex.FirstValue);
        }

        /// <summary>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellIndex cellIndex)
        {
            var (column, row, _) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex);
            return ToFriendlyString(column, row);
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="SudokuBoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellPosition> ToCellPositions(this IEnumerable<SudokuBoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new SudokuBoardCellPosition(cellIndex);
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置を示す文字列のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="SudokuBoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトの列挙子を返します。
        /// </returns>
        internal static IEnumerable<string> ToCellPositionTexts(this IEnumerable<SudokuBoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new SudokuBoardCellPosition(cellIndex).ToFriendlyString();
        }

        /// <summary>
        /// セルの番号のコレクションをセルの位置と数字のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="SudokuBoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置と数字を示す <see cref="SudokuBoardCellPositionDigit"/> 値の列挙子を返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellPositionDigit> ToCells(this IEnumerable<SudokuBoardCellIndex> cellIndexes, SudokuBoardCellDigit cellDigit)
        {
            foreach (var cellIndex in cellIndexes)
                yield return new SudokuBoardCellPositionDigit(cellIndex, cellDigit);
        }

        /// <summary>
        /// セルの番号のコレクションを読みやすい文字列のコレクションに変換します。
        /// </summary>
        /// <param name="cellIndexes">
        /// セルの番号を示す <see cref="SudokuBoardCellIndex"/> 値の列挙子です。
        /// </param>
        /// <returns>
        /// セル番号の読みやすい文字列を示す <see cref="string"/> オブジェクトの列挙子を返します。
        /// </returns>
        internal static IEnumerable<string> ToFriendlyStrings(this IEnumerable<SudokuBoardCellIndex> cellIndexes)
        {
            foreach (var cellIndex in cellIndexes)
                yield return cellIndex.ToFriendlyString();
        }

        #endregion

        #region for SudokuBoardCellDigit

        /// <summary>
        /// 与えられたセルの数字に対応するからビット集合を取得します。
        /// </summary>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字のビットマスクを示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellNoteBitSet ToCellNoteBitSet(this SudokuBoardCellDigit cellDigit)
        {
#if DEBUG
            if (cellDigit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellDigit));
#endif

            return (SudokuBoardCellNoteBitSet)(1u << (cellDigit - SudokuBoardCellDigit.One));
        }

        /// <summary>
        /// セルの数字 <see cref="SudokuBoardCellDigit"/> 値を視覚的な文字に変換します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="char"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToCellDigitChar(this SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return (char)('1' + (digit - SudokuBoardCellDigit.One));
        }

        /// <summary>
        /// セルの数字のコレクションをセルの位置と数字のコレクションに変換します。
        /// </summary>
        /// <param name="cellDigits">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値の列挙子です。
        /// </param>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置と数字を示す <see cref="SudokuBoardCellPosition"/> 値の列挙子を返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellPositionDigit> ToCells(this IEnumerable<SudokuBoardCellDigit> cellDigits, SudokuBoardCellIndex cellIndex)
        {
            foreach (var cellDigit in cellDigits)
                yield return new SudokuBoardCellPositionDigit(cellIndex, cellDigit);
        }

        #endregion

        #region for SudokuBoardCellNoteBitSet

        /// <summary>
        /// 与えられたセルの数字のビット集合に含まれる数字がひとつだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる数字が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSingleBit(this SudokuBoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is SudokuBoardCellNoteBitSet.None or > SudokuBoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合に含まれる数字の数を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる数字の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPopCount(this SudokuBoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is SudokuBoardCellNoteBitSet.None or > SudokuBoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合からひとつの数字を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellDigit GetOneCellNoteDigit(this SudokuBoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if (cellIndexBitSet is SudokuBoardCellNoteBitSet.None or > SudokuBoardCellNoteBitSet.AllCells)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            return SudokuBoardCellDigit.One + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を列挙します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellDigit"/> 値の列挙子を返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellDigit> EnumerateAsCellDigits(this SudokuBoardCellNoteBitSet cellIndexBitSet)
        {
            var value = (ushort)cellIndexBitSet;
#if DEBUG
            if ((value & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellDigit = cellIndexBitSet.GetOneCellNoteDigit();
                yield return cellDigit;
                cellIndexBitSet &= ~cellDigit.ToCellNoteBitSet();
            }
        }

#if DEBUG

        internal static void PrintToDebugConsole(this SudokuBoardCellNoteBitSet cellNoteDigitBitSet, string title)
        {
            var mask = (SudokuBoardCellNoteBitSet)1;
            System.Diagnostics.Debug.WriteLine($"[CellNoteDigitBitSet-{title}]");
            System.Diagnostics.Debug.Write("  => [");
            var first = true;
            for (var count = 0; count < 9; ++count)
            {
                if ((cellNoteDigitBitSet & mask) != 0)
                {
                    if (!first)
                        System.Diagnostics.Debug.Write(", ");
                    System.Diagnostics.Debug.Write($"'{count + 1}'");
                    first = false;
                }

                mask = (SudokuBoardCellNoteBitSet)((int)mask << 1);
            }

            System.Diagnostics.Debug.WriteLine("]");
        }
#endif

        #endregion

        #region for SudokuBoardCellPosition

        /// <summary>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="position">
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellPosition position)
            => ToFriendlyString(position.Column, position.Row);

        #endregion

        #region SudokuBoardDifficultyLevel

        /// <summary>
        /// 数独のオペレーションの難易度を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="difficulty">
        /// 数独のオペレーションの難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </param>
        /// <returns>
        /// 数独のオペレーションの難易度を示す <see cref="string"/> オブジェクトです。
        /// </returns>
        public static string ToFriendlyString(this SudokuBoardDifficultyLevel difficulty)
            => difficulty switch
                {
                    SudokuBoardDifficultyLevel.Beginner => "入門",
                    SudokuBoardDifficultyLevel.Easy => "初級",
                    SudokuBoardDifficultyLevel.Medium => "中級",
                    SudokuBoardDifficultyLevel.Hard => "上級",
                    SudokuBoardDifficultyLevel.Extreme => "超上級",
                    SudokuBoardDifficultyLevel.Insane => "究極",
                    _ => throw new ArgumentOutOfRangeException(nameof(difficulty)),
                };

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToFriendlyString(SudokuBoardCellColumn column, SudokuBoardCellRow row)
            => $"{row - SudokuBoardCellRow.One + 1}行{column - SudokuBoardCellColumn.One + 1}列";

        #endregion
    }
}
