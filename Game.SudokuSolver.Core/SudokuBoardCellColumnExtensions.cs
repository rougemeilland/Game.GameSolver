using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellColumnExtensions
    {
        /// <summary>
        /// 与えられたセルの列を要素に持つビット集合を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列のビットマスクを示す <see cref="UInt128"/> 値を返します。
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

            return $"{column - SudokuBoardCellColumn.MinValue + 1}列目";
        }
    }
}
