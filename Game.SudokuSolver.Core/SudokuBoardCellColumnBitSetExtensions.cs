using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellColumnBitSetExtensions
    {
        private static readonly UInt128 _column0BitMask = new(0x0000000000000100, 0x8040201008040201);
        private static readonly UInt128 _column1BitMask = new(0x0000000000000201, 0x0080402010080402);
        private static readonly UInt128 _column2BitMask = new(0x0000000000000402, 0x0100804020100804);
        private static readonly UInt128 _column3BitMask = new(0x0000000000000804, 0x0201008040201008);
        private static readonly UInt128 _column4BitMask = new(0x0000000000001008, 0x0402010080402010);
        private static readonly UInt128 _column5BitMask = new(0x0000000000002010, 0x0804020100804020);
        private static readonly UInt128 _column6BitMask = new(0x0000000000004020, 0x1008040201008040);
        private static readonly UInt128 _column7BitMask = new(0x0000000000008040, 0x2010080402010080);
        private static readonly UInt128 _column8BitMask = new(0x0000000000010080, 0x4020100804020100);

        /// <summary>
        /// 与えられたセルの列のビット集合の要素が一つだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合の要素が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSingleBit(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
            var value = (ushort)cellColumnBitSet;
#if DEBUG
            if (cellColumnBitSet is SudokuBoardCellColumnBitSet.None or > SudokuBoardCellColumnBitSet.AllColumns)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルの列のビット集合の要素の数を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる要素の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPopCount(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
            var value = (ushort)cellColumnBitSet;
#if DEBUG
            if (cellColumnBitSet > SudokuBoardCellColumnBitSet.AllColumns)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルの列のビット集合からひとつの列を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellColumn GetOneCellColumn(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
            var value = (ushort)cellColumnBitSet;
#if DEBUG
            if (cellColumnBitSet is SudokuBoardCellColumnBitSet.None or > SudokuBoardCellColumnBitSet.AllColumns)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
#endif

            return SudokuBoardCellColumn.One + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定されたセルの列のビット集合に含まれる列の全てのセルの位置を含むビット集合を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static UInt128 ToCellIndexBitSet(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var uint64Mask = (((ushort)cellColumnBitSet & (1 << 0)) != 0 ? 0ul : 1ul) - 1ul;
                var result = new UInt128(uint64Mask, uint64Mask) & _column0BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 1)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column1BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 2)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column2BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 3)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column3BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 4)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column4BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 5)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column5BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 6)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column6BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 7)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column7BitMask;
                uint64Mask = (((ushort)cellColumnBitSet & (1 << 8)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _column8BitMask;
                return result;
            }
        }

        /// <summary>
        /// 指定されたセルの列のビット集合からセルの列のシーケンスを取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellColumn> EnumerateAsCellColumns(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            if ((cellColumnBitSet & ~SudokuBoardCellColumnBitSet.AllColumns) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
#endif

            while (cellColumnBitSet != 0)
            {
                var column = cellColumnBitSet.GetOneCellColumn();
                yield return column;
                cellColumnBitSet &= ~column.ToCellColumnBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの列のビット集合からセルの列の視覚的にわかりやすい文字列のシーケンスを取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列の視覚的にわかりやすい文字列を示す <see cref="string"/> オブジェクトのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<string> EnumerateAsCellColumnTexts(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            if ((cellColumnBitSet & ~SudokuBoardCellColumnBitSet.AllColumns) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
#endif

            while (cellColumnBitSet != 0)
            {
                var column = cellColumnBitSet.GetOneCellColumn();
                yield return column.ToFriendlyString();
                cellColumnBitSet &= ~column.ToCellColumnBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの列の集合の要素を指定されたメモリに格納します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <param name="cellColumnBuffer">
        /// セルの列 <see cref="SudokuBoardCellColumn"/> 値の格納先です。
        /// </param>
        /// <returns>
        /// 実際にセルの列 <see cref="SudokuBoardCellColumn"/> が格納されたメモリを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        internal static Span<SudokuBoardCellColumn> EnumerateAsCellColumns(this SudokuBoardCellColumnBitSet cellColumnBitSet, Span<SudokuBoardCellColumn> cellColumnBuffer)
        {
#if DEBUG
            if ((cellColumnBitSet & ~SudokuBoardCellColumnBitSet.AllColumns) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellColumnBitSet));
            if (cellColumnBuffer.Length < cellColumnBitSet.GetPopCount())
                throw new ArgumentException("The destination buffer is not long enough.", nameof(cellColumnBuffer));
#endif
            var index = 0;
            while (cellColumnBitSet != 0)
            {
                var column = cellColumnBitSet.GetOneCellColumn();
                cellColumnBuffer[index++] = column;
                cellColumnBitSet &= ~column.ToCellColumnBitSet();
            }

            return cellColumnBuffer[..index];
        }
    }
}
