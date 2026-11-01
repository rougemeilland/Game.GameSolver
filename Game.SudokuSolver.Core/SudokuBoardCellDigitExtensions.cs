using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// <see cref="SudokuBoardCellDigit"/> 型の拡張クラスです。
    /// </summary>
    public static class SudokuBoardCellDigitExtensions
    {
        /// <summary>
        /// 与えられたセルの数字を要素に持つビット集合を取得します。
        /// </summary>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字のビットマスクを示す <see cref="SudokuBoardCellDigitBitSet"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellDigitBitSet ToCellDigitBitSet(this SudokuBoardCellDigit cellDigit)
        {
#if DEBUG
            if (cellDigit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellDigit));
#endif

            return (SudokuBoardCellDigitBitSet)(1u << (cellDigit - SudokuBoardCellDigit.One));
        }

        /// <summary>
        /// セルの数字 <see cref="SudokuBoardCellDigit"/> 値を視覚的にわかりやすい文字に変換します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="char"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static char ToCellDigitChar(this SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return (char)('1' + (digit - SudokuBoardCellDigit.One));
        }

        /// <summary>
        /// セルの数字 <see cref="SudokuBoardCellDigit"/> 値を視覚的にわかりやすい文字列に変換します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <param name="quoted">
        /// 出力する文字列をシングルクォーテーションで囲む場合は <see langword="true"/>、そうではない場合は <see langword="false"/> です。
        /// 既定値は <see langword="true"/> です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="string"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToCellDigitText(this SudokuBoardCellDigit digit, bool quoted = true)
        {
#if DEBUG
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            return
                quoted
                ? $"'{(char)('1' + (digit - SudokuBoardCellDigit.One))}'"
                : new string((char)('1' + (digit - SudokuBoardCellDigit.One)), 1);
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
    }
}
