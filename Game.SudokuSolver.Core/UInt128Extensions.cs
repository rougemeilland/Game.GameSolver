using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class UInt128Extensions
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
        /// 与えられた整数をセルの位置のビット集合とみなし、そのうちのひとつのセルの位置を取得します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セル位置のビット集合を示す <see cref="UInt128"/> 値です。
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
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
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
        /// 指定された整数をセルの位置を示すビット集合とみなし、個々のセルの位置を指定されたメモリに格納します。
        /// </summary>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellIndexBuffer">
        /// セルの位置 <see cref="SudokuBoardCellIndex"/> 値の格納先です。
        /// </param>
        /// <returns>
        /// 実際にセルの位置 <see cref="SudokuBoardCellIndex"/> が格納されたメモリを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        internal static Span<SudokuBoardCellIndex> EnumerateAsCellIndexes(this UInt128 cellIndexBitSet, Span<SudokuBoardCellIndex> cellIndexBuffer)
        {
#if DEBUG
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellIndexBitSet));
            if (cellIndexBuffer.Length < (int)UInt128.PopCount(cellIndexBitSet))
                throw new ArgumentException("The destination buffer is not long enough.", nameof(cellIndexBuffer));
#endif
            var index = 0;
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                cellIndexBuffer[index++] = cellIndex;
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return cellIndexBuffer[..index];
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
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
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
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
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
            if ((cellIndexBitSet & ~SudokuBoardWorkspace.AllCellIndexBitSet) != 0)
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
