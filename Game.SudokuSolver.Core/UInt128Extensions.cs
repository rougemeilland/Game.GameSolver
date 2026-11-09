using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class UInt128Extensions
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

        private static readonly UInt128 _row0BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 0);
        private static readonly UInt128 _row1BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 1);
        private static readonly UInt128 _row2BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 2);
        private static readonly UInt128 _row3BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 3);
        private static readonly UInt128 _row4BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 4);
        private static readonly UInt128 _row5BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 5);
        private static readonly UInt128 _row6BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 6);
        private static readonly UInt128 _row7BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 7);
        private static readonly UInt128 _row8BitMask = SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellRow.One + 8);

        private static readonly UInt128 _block0BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 0);
        private static readonly UInt128 _block1BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 1);
        private static readonly UInt128 _block2BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 2);
        private static readonly UInt128 _block3BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 3);
        private static readonly UInt128 _block4BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 4);
        private static readonly UInt128 _block5BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 5);
        private static readonly UInt128 _block6BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 6);
        private static readonly UInt128 _block7BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 7);
        private static readonly UInt128 _block8BitMask = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(SudokuBoardCellBlock.FirstValue + 8);

        /// <summary>
        /// 与えられた整数をセルの位置のビット集合とみなし、そのうちのひとつのセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// <para>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellIndexBitSet"/> は 1 のビットを 3 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellIndex GetOneCellIndex(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(cellIndexBitSet, UInt128.One << 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cellIndexBitSet, UInt128.One << (SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1));
#endif

            return SudokuBoardCellIndex.FirstValue + (int)UInt128.TrailingZeroCount(cellIndexBitSet);
        }

        /// <summary>
        /// 指定したセルの位置のビット集合から 1 個のセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// <para>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellIndexBitSet"/> は 1 のビットを 1 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellIndex AsSingleCellIndex(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndexBitSet != 0 && UInt128.PopCount(cellIndexBitSet) == 1);
#endif

            var cellIndex = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
            cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
            return cellIndex;
        }

        /// <summary>
        /// 指定したセルの位置のビット集合から 2 個のセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// <para>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellIndexBitSet"/> は 1 のビットを 2 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の 3 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellIndex firstCellIndex, SudokuBoardCellIndex secondCellIndex) SplitAsPairCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndexBitSet != 0 && UInt128.PopCount(cellIndexBitSet) == 2);
#endif

            var cellIndex1 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex1.ToCellIndexBitSet();
            var cellIndex2 = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
            cellIndexBitSet &= ~cellIndex2.ToCellIndexBitSet();
            System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
            return (cellIndex1, cellIndex2);
        }

        /// <summary>
        /// 指定したセルの位置のビット集合から 3 個のセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// <para>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellIndexBitSet"/> は 1 のビットを 3 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の 3 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellIndex firstCellIndex, SudokuBoardCellIndex secondCellIndex, SudokuBoardCellIndex thirdCellIndex) SplitAsTripleCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndexBitSet != 0 && UInt128.PopCount(cellIndexBitSet) == 3);
#endif

            var cellIndex1 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex1.ToCellIndexBitSet();
            var cellIndex2 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex2.ToCellIndexBitSet();
            var cellIndex3 = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
            cellIndexBitSet &= ~cellIndex3.ToCellIndexBitSet();
            System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
            return (cellIndex1, cellIndex2, cellIndex3);
        }

        /// <summary>
        /// 指定したセルの位置のビット集合から 4 個のセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// <para>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellIndexBitSet"/> は 1 のビットを 4 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の 4 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellIndex firstCellIndex, SudokuBoardCellIndex secondCellIndex, SudokuBoardCellIndex thirdCellIndex, SudokuBoardCellIndex fourthCellIndex) SplitAsQuadCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndexBitSet != 0 && UInt128.PopCount(cellIndexBitSet) == 4);
#endif

            var cellIndex1 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex1.ToCellIndexBitSet();
            var cellIndex2 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex2.ToCellIndexBitSet();
            var cellIndex3 = cellIndexBitSet.GetOneCellIndex();
            cellIndexBitSet &= ~cellIndex3.ToCellIndexBitSet();
            var cellIndex4 = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
            cellIndexBitSet &= ~cellIndex4.ToCellIndexBitSet();
            System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
            return (cellIndex1, cellIndex2, cellIndex3, cellIndex4);
        }

        /// <summary>
        /// 指定されたセル位置のビット集合から、列毎の和集合を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セル位置の列毎の和集合を示す <see cref="SudokuBoardCellColumnBitSet"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellColumnBitSet GetUnionByColumn(this UInt128 cellIndexBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var result = ((((cellIndexBitSet & _column0BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 0);
                result |= ((((cellIndexBitSet & _column1BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 1);
                result |= ((((cellIndexBitSet & _column2BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 2);
                result |= ((((cellIndexBitSet & _column3BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 3);
                result |= ((((cellIndexBitSet & _column4BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 4);
                result |= ((((cellIndexBitSet & _column5BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 5);
                result |= ((((cellIndexBitSet & _column6BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 6);
                result |= ((((cellIndexBitSet & _column7BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 7);
                result |= ((((cellIndexBitSet & _column8BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 8);
                return (SudokuBoardCellColumnBitSet)result;
            }
        }

        /// <summary>
        /// 指定されたセル位置のビット集合から、行毎の和集合を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セル位置の行毎の和集合を示す <see cref="SudokuBoardCellRowBitSet"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellRowBitSet GetUnionByRow(this UInt128 cellIndexBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var result = ((((cellIndexBitSet & _row0BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 0);
                result |= ((((cellIndexBitSet & _row1BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 1);
                result |= ((((cellIndexBitSet & _row2BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 2);
                result |= ((((cellIndexBitSet & _row3BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 3);
                result |= ((((cellIndexBitSet & _row4BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 4);
                result |= ((((cellIndexBitSet & _row5BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 5);
                result |= ((((cellIndexBitSet & _row6BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 6);
                result |= ((((cellIndexBitSet & _row7BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 7);
                result |= ((((cellIndexBitSet & _row8BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 8);
                return (SudokuBoardCellRowBitSet)result;
            }
        }

        /// <summary>
        /// 指定されたセル位置のビット集合から、ブロック毎の和集合を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セル位置のブロック毎の和集合を示す <see cref="SudokuBoardCellBlockBitSet"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellBlockBitSet GetUnionByBlock(this UInt128 cellIndexBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var result = ((((cellIndexBitSet & _block0BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 0);
                result |= ((((cellIndexBitSet & _block1BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 1);
                result |= ((((cellIndexBitSet & _block2BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 2);
                result |= ((((cellIndexBitSet & _block3BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 3);
                result |= ((((cellIndexBitSet & _block4BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 4);
                result |= ((((cellIndexBitSet & _block5BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 5);
                result |= ((((cellIndexBitSet & _block6BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 6);
                result |= ((((cellIndexBitSet & _block7BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 7);
                result |= ((((cellIndexBitSet & _block8BitMask) != 0) ? 0u : 1u) - 1u) & (1u << 8);
                return (SudokuBoardCellBlockBitSet)result;
            }
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、各セルから可視である他のセルの位置の積集合を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置の積集合を示す <see cref="UInt128"/> 値を返します。
        /// ただし、<paramref name="cellIndexBitSet"/> が空集合である場合は、全ての要素のビットが 1 である集合を示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        public static UInt128 ToVisibleCellsIntersectionBitSet(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            var result = SudokuBoardCellGeometry.AllCellIndexBitSet;
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                result &= SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(cellIndex);
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return result;
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、各セルが全て同じ列に存在するかどうかを調べます。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// この値は 0 であってはなりません。
        /// </param>
        /// <returns>
        /// <paramref name="cellIndexBitSet"/> で指定された集合の要素のセルが全て同じ列に存在する場合は <see langword="true"/> を返します。
        /// そうではない場合は <see langword="true"/> を返します。
        /// </returns>
        public static bool ExistAllCellsInOneColumn(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfZero(cellIndexBitSet);
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            var cellIndex = cellIndexBitSet.GetOneCellIndex();
            var column = SudokuBoardCellGeometry.GetColumn(cellIndex);
            cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            while (cellIndexBitSet != 0)
            {
                cellIndex = cellIndexBitSet.GetOneCellIndex();
                var otherCellColumn = SudokuBoardCellGeometry.GetColumn(cellIndex);
                if (otherCellColumn != column)
                    return false;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return true;
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、各セルが全て同じ行に存在するかどうかを調べます。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// この値は 0 であってはなりません。
        /// </param>
        /// <returns>
        /// <paramref name="cellIndexBitSet"/> で指定された集合の要素のセルが全て同じ行に存在する場合は <see langword="true"/> を返します。
        /// そうではない場合は <see langword="true"/> を返します。
        /// </returns>
        public static bool ExistAllCellsInOneRow(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfZero(cellIndexBitSet);
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            var cellIndex = cellIndexBitSet.GetOneCellIndex();
            var row = SudokuBoardCellGeometry.GetRow(cellIndex);
            cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            while (cellIndexBitSet != 0)
            {
                cellIndex = cellIndexBitSet.GetOneCellIndex();
                var otherCellRow = SudokuBoardCellGeometry.GetRow(cellIndex);
                if (otherCellRow != row)
                    return false;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return true;
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、各セルが全て同じブロックに存在するかどうかを調べます。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// この値は 0 であってはなりません。
        /// </param>
        /// <returns>
        /// <paramref name="cellIndexBitSet"/> で指定された集合の要素のセルが全て同じブロックに存在する場合は <see langword="true"/> を返します。
        /// そうではない場合は <see langword="true"/> を返します。
        /// </returns>
        public static bool ExistAllCellsInOneBlock(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfZero(cellIndexBitSet);
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            var cellIndex = cellIndexBitSet.GetOneCellIndex();
            var block = SudokuBoardCellGeometry.GetBlock(cellIndex);
            cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            while (cellIndexBitSet != 0)
            {
                cellIndex = cellIndexBitSet.GetOneCellIndex();
                var otherCellBlock = SudokuBoardCellGeometry.GetBlock(cellIndex);
                if (otherCellBlock != block)
                    return false;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return true;
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置のシーケンスを取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellIndex> EnumerateAsCellIndexes(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return cellIndex;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、列のセル数が指定された最小値と最大値の範囲内である列について、列と列のセル数のタプルを指定されたバッファに格納します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="columnsBuffer">
        /// 列を示す <see cref="SudokuBoardCellColumn"/> 値と列のセル数を示す <see cref="int"/>のタプルの格納先です。
        /// </param>
        /// <param name="minCount">
        /// 抽出条件のうち、列に含まれるセル数の最小値を示す <see cref="int"/> 値です。既定値は 0 です。
        /// </param>
        /// <param name="maxCount">
        /// 抽出条件のうち、列に含まれるセル数の最大値を示す <see cref="int"/> 値です。既定値は 9 です。
        /// </param>
        /// <returns>
        /// <paramref name="columnsBuffer"/> に格納された実際の要素の範囲を示す <see cref="ReadOnlyMemory{T}"/> 値です。
        /// </returns>
        public static ReadOnlyMemory<(SudokuBoardCellColumn column, int cellCount)> ExtractColumnsByCellCount(this UInt128 cellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> columnsBuffer, [ConstantExpected(Min = 0, Max = 9)] int minCount = 0, [ConstantExpected(Min = 0, Max = 9)] int maxCount = 9)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(columnsBuffer.Length >= 9);
#endif
            var destinationBuffer = columnsBuffer.Span;
            var length = 0;
            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var cellCount = (int)UInt128.PopCount(cellIndexBitSet & SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column));
                if (cellCount >= minCount && cellCount <= maxCount)
                    destinationBuffer[length++] = (column, cellCount);
            }

            return columnsBuffer[..length];
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、行のセル数が指定された最小値と最大値の範囲内である行について、行と行のセル数のタプルを指定されたバッファに格納します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="rowsBuffer">
        /// 行を示す <see cref="SudokuBoardCellRow"/> 値と行のセル数を示す <see cref="int"/>のタプルの格納先です。
        /// </param>
        /// <param name="minCount">
        /// 抽出条件のうち、行に含まれるセル数の最小値を示す <see cref="int"/> 値です。既定値は 0 です。
        /// </param>
        /// <param name="maxCount">
        /// 抽出条件のうち、行に含まれるセル数の最大値を示す <see cref="int"/> 値です。既定値は 9 です。
        /// </param>
        /// <returns>
        /// <paramref name="rowsBuffer"/> に格納された実際の要素の範囲を示す <see cref="ReadOnlyMemory{T}"/> 値です。
        /// </returns>
        public static ReadOnlyMemory<(SudokuBoardCellRow row, int cellCount)> ExtractRowsByCellCount(this UInt128 cellIndexBitSet, Memory<(SudokuBoardCellRow row, int cellCount)> rowsBuffer, [ConstantExpected(Min = 0, Max = 9)] int minCount = 0, [ConstantExpected(Min = 0, Max = 9)] int maxCount = 9)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(rowsBuffer.Length >= 9);
#endif
            var destinationBuffer = rowsBuffer.Span;
            var length = 0;
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var cellCount = (int)UInt128.PopCount(cellIndexBitSet & SudokuBoardCellGeometry.GetRowCellIndexBitSet(row));
                if (cellCount >= minCount && cellCount <= maxCount)
                    destinationBuffer[length++] = (row, cellCount);
            }

            return rowsBuffer[..length];
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、ブロックのセル数が指定された最小値と最大値の範囲内であるブロックについて、ブロックとブロックのセル数のタプルを指定されたバッファに格納します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blocksBuffer">
        /// ブロックを示す <see cref="SudokuBoardCellBlock"/> 値とブロックのセル数を示す <see cref="int"/>のタプルの格納先です。
        /// </param>
        /// <param name="minCount">
        /// 抽出条件のうち、ブロックに含まれるセル数の最小値を示す <see cref="int"/> 値です。既定値は 0 です。
        /// </param>
        /// <param name="maxCount">
        /// 抽出条件のうち、ブロックに含まれるセル数の最大値を示す <see cref="int"/> 値です。既定値は 9 です。
        /// </param>
        /// <returns>
        /// <paramref name="blocksBuffer"/> に格納された実際の要素の範囲を示す <see cref="ReadOnlyMemory{T}"/> 値です。
        /// </returns>
        public static ReadOnlyMemory<(SudokuBoardCellBlock block, int cellCount)> ExtractBlocksByCellCount(this UInt128 cellIndexBitSet, Memory<(SudokuBoardCellBlock block, int cellCount)> blocksBuffer, [ConstantExpected(Min = 0, Max = 9)] int minCount = 0, [ConstantExpected(Min = 0, Max = 9)] int maxCount = 9)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(blocksBuffer.Length >= 9);
#endif
            var destinationBuffer = blocksBuffer.Span;
            var length = 0;
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var cellCount = (int)UInt128.PopCount(cellIndexBitSet & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block));
                if (cellCount >= minCount && cellCount <= maxCount)
                    destinationBuffer[length++] = (block, cellCount);
            }

            return blocksBuffer[..length];
        }

        /// <summary>
        /// 指定された数字のビット集合とセルの位置からセルの数字の候補のシーケンスを取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellNoteDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellPositionDigit> EnumerateAsCellNotes(this UInt128 cellIndexBitSet, SudokuBoardCellDigit cellNoteDigit)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
            if (cellNoteDigit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellNoteDigit));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit);
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、セルの位置を示す文字列のシーケンスに変換します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの番号のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトの列挙子を返します。
        /// </returns>
        internal static IEnumerable<string> EnumerateAsCellPositionTexts(this UInt128 cellIndexBitSet)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return cellIndex.ToFriendlyString();
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

        /// <summary>
        /// 指定された整数をセルの位置を示すビット集合とみなし、各セルの背景表示効果のシーケンスを取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="effect">
        /// セルの背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> と セルの背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値のペアのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> EnumerateAsCellBackgroundEffects(this UInt128 cellIndexBitSet, SudokuBoardCellBackgroundEffect effect)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardCellGeometry.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
#endif

            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return (cellIndex, effect);
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

        /// <summary>
        /// 指定されたセル位置のビット集合と数字から、セルの数字の候補の表示効果とのペアのシーケンスを取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellNoteDigit">
        /// セルの数字の候補を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <param name="effect"></param>
        /// セルの数字の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値です。
        /// <returns>
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値とセルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> のペアのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> EnumerateAsCellNoteEffects(this UInt128 cellIndexBitSet, SudokuBoardCellDigit cellNoteDigit, SudokuBoardCellNoteEffect effect)
        {
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                yield return (new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit), effect);
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }

#if DEBUG

        internal static void PrintToDebugConsole(this UInt128 cellIndexBitSet, string title)
        {
            var mask = UInt128.One;
            System.Diagnostics.Debug.WriteLine($"[CellIndexBitSet-{title}]");
            for (var count = 0; count < 81; ++count)
            {
                System.Diagnostics.Debug.Write((cellIndexBitSet & mask) != 0 ? "*" : "_");
                if ((count + 1) % 9 == 0)
                    System.Diagnostics.Debug.WriteLine("");
                mask <<= 1;
            }
        }
#endif
    }
}
