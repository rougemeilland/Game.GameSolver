using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellColumnBitSetExtensions
    {
        private static readonly UInt128 _column0BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 0);
        private static readonly UInt128 _column1BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 1);
        private static readonly UInt128 _column2BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 2);
        private static readonly UInt128 _column3BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 3);
        private static readonly UInt128 _column4BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 4);
        private static readonly UInt128 _column5BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 5);
        private static readonly UInt128 _column6BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 6);
        private static readonly UInt128 _column7BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 7);
        private static readonly UInt128 _column8BitMask = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellColumn.One + 8);

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
        /// 指定したセルの列のビット集合から 1 個のセルの列を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// <para>
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellColumnBitSet"/> は 1 のビットを 1 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellDigit"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellColumn AsSingleCellColumn(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellColumnBitSet != 0 && cellColumnBitSet.GetPopCount() == 1);
#endif

            var cellColumn = cellColumnBitSet.GetOneCellColumn();
#if DEBUG
            cellColumnBitSet &= ~cellColumn.ToCellColumnBitSet();
            System.Diagnostics.Debug.Assert(cellColumnBitSet == SudokuBoardCellColumnBitSet.None);
#endif
            return cellColumn;
        }

        /// <summary>
        /// 指定したセルの列のビット集合から 2 個のセルの列を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// <para>
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellColumnBitSet"/> は 1 のビットを 2 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> の 2 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellColumn firstCellColumn, SudokuBoardCellColumn secondCellColumn) SplitAsPairCellColulmns(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellColumnBitSet != 0 && cellColumnBitSet.GetPopCount() == 2);
#endif

            var cellColumn1 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn1.ToCellColumnBitSet();
            var cellColumn2 = cellColumnBitSet.GetOneCellColumn();
#if DEBUG
            cellColumnBitSet &= ~cellColumn2.ToCellColumnBitSet();
            System.Diagnostics.Debug.Assert(cellColumnBitSet == SudokuBoardCellColumnBitSet.None);
#endif
            return (cellColumn1, cellColumn2);
        }

        /// <summary>
        /// 指定したセルの列のビット集合から 3 個のセルの列を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// <para>
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellColumnBitSet"/> は 1 のビットを 3 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> の 3 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellColumn firstCellColumn, SudokuBoardCellColumn secondCellColumn, SudokuBoardCellColumn thirdCellColumn) SplitAsTripleCellColulmns(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellColumnBitSet != 0 && cellColumnBitSet.GetPopCount() == 3);
#endif

            var cellColumn1 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn1.ToCellColumnBitSet();
            var cellColumn2 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn2.ToCellColumnBitSet();
            var cellColumn3 = cellColumnBitSet.GetOneCellColumn();
#if DEBUG
            cellColumnBitSet &= ~cellColumn3.ToCellColumnBitSet();
            System.Diagnostics.Debug.Assert(cellColumnBitSet == SudokuBoardCellColumnBitSet.None);
#endif
            return (cellColumn1, cellColumn2, cellColumn3);
        }

        /// <summary>
        /// 指定したセルの列のビット集合から 4 個のセルの列を取得します。
        /// </summary>
        /// <param name="cellColumnBitSet">
        /// <para>
        /// セルの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellColumnBitSet"/> は 1 のビットを 4 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> の 4 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellColumn firstCellColumn, SudokuBoardCellColumn secondCellColumn, SudokuBoardCellColumn thirdCellColumn, SudokuBoardCellColumn fourthCellColumn) SplitAsQuadCellColulmns(this SudokuBoardCellColumnBitSet cellColumnBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellColumnBitSet != 0 && cellColumnBitSet.GetPopCount() == 4);
#endif

            var cellColumn1 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn1.ToCellColumnBitSet();
            var cellColumn2 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn2.ToCellColumnBitSet();
            var cellColumn3 = cellColumnBitSet.GetOneCellColumn();
            cellColumnBitSet &= ~cellColumn3.ToCellColumnBitSet();
            var cellColumn4 = cellColumnBitSet.GetOneCellColumn();
#if DEBUG
            cellColumnBitSet &= ~cellColumn4.ToCellColumnBitSet();
            System.Diagnostics.Debug.Assert(cellColumnBitSet == SudokuBoardCellColumnBitSet.None);
#endif
            return (cellColumn1, cellColumn2, cellColumn3, cellColumn4);
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
    }
}
