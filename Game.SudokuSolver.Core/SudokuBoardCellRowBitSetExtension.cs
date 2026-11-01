using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellRowBitSetExtension
    {
        private static readonly UInt128 _row0BitMask = new(0x0000000000000000, 0x00000000000001ff);
        private static readonly UInt128 _row1BitMask = new(0x0000000000000000, 0x000000000003fe00);
        private static readonly UInt128 _row2BitMask = new(0x0000000000000000, 0x0000000007fc0000);
        private static readonly UInt128 _row3BitMask = new(0x0000000000000000, 0x0000000ff8000000);
        private static readonly UInt128 _row4BitMask = new(0x0000000000000000, 0x00001ff000000000);
        private static readonly UInt128 _row5BitMask = new(0x0000000000000000, 0x003fe00000000000);
        private static readonly UInt128 _row6BitMask = new(0x0000000000000000, 0x7fc0000000000000);
        private static readonly UInt128 _row7BitMask = new(0x00000000000000ff, 0x8000000000000000);
        private static readonly UInt128 _row8BitMask = new(0x000000000001ff00, 0x0000000000000000);

        /// <summary>
        /// 与えられたセルの行のビット集合の要素が一つだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合の要素が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSingleBit(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
            var value = (ushort)cellRowBitSet;
#if DEBUG
            if (cellRowBitSet is SudokuBoardCellRowBitSet.None or > SudokuBoardCellRowBitSet.AllRows)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルの行のビット集合の要素の数を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる要素の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPopCount(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
            var value = (ushort)cellRowBitSet;
#if DEBUG
            if (cellRowBitSet > SudokuBoardCellRowBitSet.AllRows)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルの行のビット集合からひとつの行を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellRow GetOneCellRow(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
            var value = (ushort)cellRowBitSet;
#if DEBUG
            if (cellRowBitSet is SudokuBoardCellRowBitSet.None or > SudokuBoardCellRowBitSet.AllRows)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
#endif

            return SudokuBoardCellRow.One + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定されたセルの行のビット集合に含まれる行の全てのセルの位置を含むビット集合を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static UInt128 ToCellIndexBitSet(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var uint64Mask = (((ushort)cellRowBitSet & (1 << 0)) != 0 ? 0ul : 1ul) - 1ul;
                var result = new UInt128(uint64Mask, uint64Mask) & _row0BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 1)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row1BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 2)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row2BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 3)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row3BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 4)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row4BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 5)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row5BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 6)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row6BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 7)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row7BitMask;
                uint64Mask = (((ushort)cellRowBitSet & (1 << 8)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _row8BitMask;
                return result;
            }
        }

        /// <summary>
        /// 指定されたセルの行のビット集合からセルの行のシーケンスを取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellRow> EnumerateAsCellRows(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
#if DEBUG
            if ((cellRowBitSet & ~SudokuBoardCellRowBitSet.AllRows) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
#endif

            while (cellRowBitSet != 0)
            {
                var row = cellRowBitSet.GetOneCellRow();
                yield return row;
                cellRowBitSet &= ~row.ToCellRowBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの行のビット集合からセルの行の視覚的にわかりやすい文字行のシーケンスを取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行の視覚的にわかりやすい文字行を示す <see cref="string"/> オブジェクトのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<string> EnumerateAsCellRowTexts(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
#if DEBUG
            if ((cellRowBitSet & ~SudokuBoardCellRowBitSet.AllRows) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
#endif

            while (cellRowBitSet != 0)
            {
                var row = cellRowBitSet.GetOneCellRow();
                yield return row.ToFriendlyString();
                cellRowBitSet &= ~row.ToCellRowBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの行の集合の要素を指定されたメモリに格納します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <param name="cellRowBuffer">
        /// セルの行 <see cref="SudokuBoardCellRow"/> 値の格納先です。
        /// </param>
        /// <returns>
        /// 実際にセルの行 <see cref="SudokuBoardCellRow"/> が格納されたメモリを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        internal static Span<SudokuBoardCellRow> EnumerateAsCellRows(this SudokuBoardCellRowBitSet cellRowBitSet, Span<SudokuBoardCellRow> cellRowBuffer)
        {
#if DEBUG
            if ((cellRowBitSet & ~SudokuBoardCellRowBitSet.AllRows) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellRowBitSet));
            if (cellRowBuffer.Length < cellRowBitSet.GetPopCount())
                throw new ArgumentException("The destination buffer is not long enough.", nameof(cellRowBuffer));
#endif
            var index = 0;
            while (cellRowBitSet != 0)
            {
                var row = cellRowBitSet.GetOneCellRow();
                cellRowBuffer[index++] = row;
                cellRowBitSet &= ~row.ToCellRowBitSet();
            }

            return cellRowBuffer[..index];
        }
    }
}
