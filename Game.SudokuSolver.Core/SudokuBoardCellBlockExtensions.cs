using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellBlockExtensions
    {
        private readonly static string[] _friendlyNameTable =
        [
            "上段左",
            "上段中央",
            "上段右",
            "中段左",
            "中段中央",
            "中段右",
            "下段左",
            "下段中央",
            "下段右",
        ];

        private readonly static string[] _houseNameTable =
            [.. Enumerable.Range(0, 9).Select(n => (SudokuBoardCellBlock.MinValue + n).ToHouseNameCore())];

        /// <summary>
        /// 与えられたセルのブロックを要素に持つビット集合を取得します。
        /// </summary>
        /// <param name="block">
        /// セルのブロックを示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値を返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellBlockBitSet ToCellBlockBitSet(this SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif

            return (SudokuBoardCellBlockBitSet)(1u << (block - SudokuBoardCellBlock.FirstValue));
        }

        /// <summary>
        /// セルのブロックを読みやすい文字列に変換します。
        /// </summary>
        /// <param name="block">
        /// セルのブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックを示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif
            return _friendlyNameTable[block - SudokuBoardCellBlock.MinValue];
        }

        /// <summary>
        /// セルのブロックのハウス名を取得します。
        /// </summary>
        /// <param name="block">
        /// セルのブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックのハウス名を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToHouseName(this SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif
            return _houseNameTable[block - SudokuBoardCellBlock.MinValue];
        }

        private static string ToHouseNameCore(this SudokuBoardCellBlock block)
            => $"{block.ToFriendlyString()}のブロック";

    }
}
