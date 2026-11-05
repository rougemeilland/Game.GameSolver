using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellRowBitSetExtension
    {
        private static readonly UInt128 _row0BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 0);
        private static readonly UInt128 _row1BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 1);
        private static readonly UInt128 _row2BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 2);
        private static readonly UInt128 _row3BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 3);
        private static readonly UInt128 _row4BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 4);
        private static readonly UInt128 _row5BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 5);
        private static readonly UInt128 _row6BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 6);
        private static readonly UInt128 _row7BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 7);
        private static readonly UInt128 _row8BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 8);

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
        /// 指定したセルの行のビット集合から 1 個のセルの行を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// <para>
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellColumnBitSet"/> は 1 のビットを 1 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellRow AsSingleCellRow(this SudokuBoardCellRowBitSet cellColumnBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellColumnBitSet != 0 && cellColumnBitSet.GetPopCount() == 1);
#endif

            var cellRow = cellColumnBitSet.GetOneCellRow();
#if DEBUG
            cellColumnBitSet &= ~cellRow.ToCellRowBitSet();
            System.Diagnostics.Debug.Assert(cellColumnBitSet == SudokuBoardCellRowBitSet.None);
#endif
            return cellRow;
        }

        /// <summary>
        /// 指定したセルの行のビット集合から 2 個のセルの行を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// <para>
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellRowBitSet"/> は 1 のビットを 2 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> の 2 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellRow firstCellRow, SudokuBoardCellRow secondCellRow) SplitAsPairCellRows(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellRowBitSet != 0 && cellRowBitSet.GetPopCount() == 2);
#endif

            var cellRow1 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow1.ToCellRowBitSet();
            var cellRow2 = cellRowBitSet.GetOneCellRow();
#if DEBUG
            cellRowBitSet &= ~cellRow2.ToCellRowBitSet();
            System.Diagnostics.Debug.Assert(cellRowBitSet == SudokuBoardCellRowBitSet.None);
#endif
            return (cellRow1, cellRow2);
        }

        /// <summary>
        /// 指定したセルの行のビット集合から 3 個のセルの行を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// <para>
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellRowBitSet"/> は 1 のビットを 3 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> の 3 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellRow firstCellRow, SudokuBoardCellRow secondCellRow, SudokuBoardCellRow thirdCellRow) SplitAsTripleCellRows(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellRowBitSet != 0 && cellRowBitSet.GetPopCount() == 3);
#endif

            var cellRow1 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow1.ToCellRowBitSet();
            var cellRow2 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow2.ToCellRowBitSet();
            var cellRow3 = cellRowBitSet.GetOneCellRow();
#if DEBUG
            cellRowBitSet &= ~cellRow3.ToCellRowBitSet();
            System.Diagnostics.Debug.Assert(cellRowBitSet == SudokuBoardCellRowBitSet.None);
#endif
            return (cellRow1, cellRow2, cellRow3);
        }

        /// <summary>
        /// 指定したセルの行のビット集合から 4 個のセルの行を取得します。
        /// </summary>
        /// <param name="cellRowBitSet">
        /// <para>
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellRowBitSet"/> は 1 のビットを 4 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> の 4 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellRow firstCellRow, SudokuBoardCellRow secondCellRow, SudokuBoardCellRow thirdCellRow, SudokuBoardCellRow fourthCellRow) SplitAsQuadCellRows(this SudokuBoardCellRowBitSet cellRowBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellRowBitSet != 0 && cellRowBitSet.GetPopCount() == 4);
#endif

            var cellRow1 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow1.ToCellRowBitSet();
            var cellRow2 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow2.ToCellRowBitSet();
            var cellRow3 = cellRowBitSet.GetOneCellRow();
            cellRowBitSet &= ~cellRow3.ToCellRowBitSet();
            var cellRow4 = cellRowBitSet.GetOneCellRow();
#if DEBUG
            cellRowBitSet &= ~cellRow4.ToCellRowBitSet();
            System.Diagnostics.Debug.Assert(cellRowBitSet == SudokuBoardCellRowBitSet.None);
#endif
            return (cellRow1, cellRow2, cellRow3, cellRow4);
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
    }
}
