using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellIndexExtensions
    {
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
            return (column, row).ToFriendlyString();
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
                yield return cellIndex.ToFriendlyString();
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
    }
}
