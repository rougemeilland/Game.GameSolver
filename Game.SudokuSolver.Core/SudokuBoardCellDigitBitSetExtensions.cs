using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellDigitBitSetExtensions
    {
        /// <summary>
        /// 与えられたセルの数字のビット集合の要素が一つだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合の要素が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSingleBit(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
            var value = (ushort)cellDigitBitSet;
#if DEBUG
            if (cellDigitBitSet is SudokuBoardCellDigitBitSet.None or > SudokuBoardCellDigitBitSet.AllCellDigits)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合の要素の数を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合の要素の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPopCount(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
            var value = (ushort)cellDigitBitSet;
#if DEBUG
            if (cellDigitBitSet > SudokuBoardCellDigitBitSet.AllCellDigits)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルの数字のビット集合からひとつの数字を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellDigit GetOneCellDigit(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
            var value = (ushort)cellDigitBitSet;
#if DEBUG
            if (cellDigitBitSet is SudokuBoardCellDigitBitSet.None or > SudokuBoardCellDigitBitSet.AllCellDigits)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            return SudokuBoardCellDigit.One + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定したセルの数字のビット集合から 1 個のセルの数字を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// <para>
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellDigitBitSet"/> は 1 のビットを 1 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellDigit AsSingleCellDigit(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellDigitBitSet != 0 && cellDigitBitSet.GetPopCount() == 1);
#endif

            var cellDigit = cellDigitBitSet.GetOneCellDigit();
#if DEBUG
            cellDigitBitSet &= ~cellDigit.ToCellDigitBitSet();
            System.Diagnostics.Debug.Assert(cellDigitBitSet == SudokuBoardCellDigitBitSet.None);
#endif
            return cellDigit;
        }

        /// <summary>
        /// 指定したセルの数字のビット集合から 2 個のセルの数字を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// <para>
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellDigitBitSet"/> は 1 のビットを 2 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> の 2 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellDigit firstCellDigit, SudokuBoardCellDigit secondCellDigit) SplitAsPairCellDigits(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellDigitBitSet != 0 && cellDigitBitSet.GetPopCount() == 2);
#endif

            var cellDigit1 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit1.ToCellDigitBitSet();
            var cellDigit2 = cellDigitBitSet.GetOneCellDigit();
#if DEBUG
            cellDigitBitSet &= ~cellDigit2.ToCellDigitBitSet();
            System.Diagnostics.Debug.Assert(cellDigitBitSet == SudokuBoardCellDigitBitSet.None);
#endif
            return (cellDigit1, cellDigit2);
        }

        /// <summary>
        /// 指定したセルの数字のビット集合から 3 個のセルの数字を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// <para>
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellDigitBitSet"/> は 1 のビットを 3 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> の 3 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellDigit firstCellDigit, SudokuBoardCellDigit secondCellDigit, SudokuBoardCellDigit thirdCellDigit) SplitAsTripleCellDigits(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellDigitBitSet != 0 && cellDigitBitSet.GetPopCount() == 3);
#endif

            var cellDigit1 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit1.ToCellDigitBitSet();
            var cellDigit2 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit2.ToCellDigitBitSet();
            var cellDigit3 = cellDigitBitSet.GetOneCellDigit();
#if DEBUG
            cellDigitBitSet &= ~cellDigit3.ToCellDigitBitSet();
            System.Diagnostics.Debug.Assert(cellDigitBitSet == SudokuBoardCellDigitBitSet.None);
#endif
            return (cellDigit1, cellDigit2, cellDigit3);
        }

        /// <summary>
        /// 指定したセルの数字のビット集合から 4 個のセルの数字を取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// <para>
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellDigitBitSet"/> は 1 のビットを 4 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> の 4 個組のタプルです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellDigit firstCellDigit, SudokuBoardCellDigit secondCellDigit, SudokuBoardCellDigit thirdCellDigit, SudokuBoardCellDigit fourthCellDigit) SplitAsQuadCellDigits(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellDigitBitSet != 0 && cellDigitBitSet.GetPopCount() == 4);
#endif

            var cellDigit1 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit1.ToCellDigitBitSet();
            var cellDigit2 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit2.ToCellDigitBitSet();
            var cellDigit3 = cellDigitBitSet.GetOneCellDigit();
            cellDigitBitSet &= ~cellDigit3.ToCellDigitBitSet();
            var cellDigit4 = cellDigitBitSet.GetOneCellDigit();
#if DEBUG
            cellDigitBitSet &= ~cellDigit4.ToCellDigitBitSet();
            System.Diagnostics.Debug.Assert(cellDigitBitSet == SudokuBoardCellDigitBitSet.None);
#endif
            return (cellDigit1, cellDigit2, cellDigit3, cellDigit4);
        }

        /// <summary>
        /// 指定されたセルの数字のビット集合からセルの数字のシーケンスを取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellDigit> EnumerateAsCellDigits(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            if ((cellDigitBitSet & ~SudokuBoardCellDigitBitSet.AllCellDigits) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            while (cellDigitBitSet != 0)
            {
                var cellDigit = cellDigitBitSet.GetOneCellDigit();
                yield return cellDigit;
                cellDigitBitSet &= ~cellDigit.ToCellDigitBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの数字のビット集合からセルの数字の視覚的にわかりやすい文字列のシーケンスを取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字の視覚的にわかりやすい文字列を示す <see cref="string"/> オブジェクトのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<string> EnumerateAsCellDigitTexts(this SudokuBoardCellDigitBitSet cellDigitBitSet)
        {
#if DEBUG
            if ((cellDigitBitSet & ~SudokuBoardCellDigitBitSet.AllCellDigits) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            while (cellDigitBitSet != 0)
            {
                var cellDigit = cellDigitBitSet.GetOneCellDigit();
                yield return $"'{cellDigit.ToCellDigitChar()}'";
                cellDigitBitSet &= ~cellDigit.ToCellDigitBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルの数字のビット集合とセルの位置からセルの数字の候補のシーケンスを取得します。
        /// </summary>
        /// <param name="cellDigitBitSet">
        /// セルの数字のビット集合を示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellPositionDigit> EnumerateAsCellNotes(this SudokuBoardCellDigitBitSet cellDigitBitSet, SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if ((cellDigitBitSet & ~SudokuBoardCellDigitBitSet.AllCellDigits) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellDigitBitSet));
#endif

            while (cellDigitBitSet != 0)
            {
                var cellDigit = cellDigitBitSet.GetOneCellDigit();
                yield return new SudokuBoardCellPositionDigit(cellIndex, cellDigit);
                cellDigitBitSet &= ~cellDigit.ToCellDigitBitSet();
            }
        }

#if DEBUG

        internal static void PrintToDebugConsole(this SudokuBoardCellDigitBitSet cellNoteDigitBitSet, string title)
        {
            var mask = (SudokuBoardCellDigitBitSet)1;
            System.Diagnostics.Debug.WriteLine($"[CellNoteDigitBitSet-{title}]");
            System.Diagnostics.Debug.Write("  => [");
            var first = true;
            for (var count = 0; count < 9; ++count)
            {
                if ((cellNoteDigitBitSet & mask) != 0)
                {
                    if (!first)
                        System.Diagnostics.Debug.Write(", ");
                    System.Diagnostics.Debug.Write($"'{count + 1}'");
                    first = false;
                }

                mask = (SudokuBoardCellDigitBitSet)((int)mask << 1);
            }

            System.Diagnostics.Debug.WriteLine("]");
        }
#endif
    }
}
