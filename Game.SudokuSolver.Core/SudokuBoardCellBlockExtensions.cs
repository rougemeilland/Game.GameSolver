using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellBlockExtensions
    {
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
    }
}
