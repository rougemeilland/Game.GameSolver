using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellColumnExtensions
    {
        private readonly static string[] _friendlyNameTable =
            [.. Enumerable.Range(1, 9).Select(n => $"{n}列目")];

        private readonly static string[] _houseNameTable =
            [.. Enumerable.Range(0, 9).Select(n => (SudokuBoardCellColumn.MinValue + n).ToHouseNameCore())];

        /// <summary>
        /// 与えられたセルの列を要素に持つビット集合を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellColumnBitSet ToCellColumnBitSet(this SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return (SudokuBoardCellColumnBitSet)(1u << (column - SudokuBoardCellColumn.One));
        }

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

            return _friendlyNameTable[column - SudokuBoardCellColumn.MinValue];
        }

        /// <summary>
        /// セルの列のハウス名を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列のハウス名を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToHouseName(this SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif
            return _houseNameTable[column - SudokuBoardCellColumn.MinValue];
        }

        private static string ToHouseNameCore(this SudokuBoardCellColumn column)
            => column.ToFriendlyString();
    }
}
