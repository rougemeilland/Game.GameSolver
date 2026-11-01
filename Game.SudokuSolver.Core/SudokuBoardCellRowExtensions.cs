using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellRowExtensions
    {
        /// <summary>
        /// 与えられたセルの行を要素に持つビット集合を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの行を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行のビットマスクを示す <see cref="SudokuBoardCellRowBitSet"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellRowBitSet ToCellRowBitSet(this SudokuBoardCellRow column)
        {
#if DEBUG
            if (column is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return (SudokuBoardCellRowBitSet)(1u << (column - SudokuBoardCellRow.One));
        }

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
    }
}
