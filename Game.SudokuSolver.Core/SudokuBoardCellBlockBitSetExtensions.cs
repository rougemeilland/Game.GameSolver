using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellBlockBitSetExtensions
    {
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
        /// 与えられたセルのブロックのビット集合の要素が一つだけであるかどうかを調べます。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合の要素が一つだけであるならば <see langword="true"/>、そうではないなら <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSingleBit(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
            var value = (ushort)cellBlockBitSet;
#if DEBUG
            if (cellBlockBitSet is SudokuBoardCellBlockBitSet.None or > SudokuBoardCellBlockBitSet.AllBlocks)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
#endif

            return ushort.IsPow2(value);
        }

        /// <summary>
        /// 与えられたセルのブロックのビット集合の要素の数を取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// ビット集合に含まれる要素の数を示す <see cref="int"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPopCount(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
            var value = (ushort)cellBlockBitSet;
#if DEBUG
            if (cellBlockBitSet > SudokuBoardCellBlockBitSet.AllBlocks)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
#endif

            return ushort.PopCount(value);
        }

        /// <summary>
        /// 与えられたセルのブロックのビット集合からひとつの列を取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックを示す <see cref="SudokuBoardCellBlock"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellBlock GetOneCellBlock(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
            var value = (ushort)cellBlockBitSet;
#if DEBUG
            if (cellBlockBitSet is SudokuBoardCellBlockBitSet.None or > SudokuBoardCellBlockBitSet.AllBlocks)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
#endif

            return SudokuBoardCellBlock.FirstValue + ushort.TrailingZeroCount(value);
        }

        /// <summary>
        /// 指定したセルのブロックのビット集合から 1 個のセルのブロックを取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// <para>
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </para>
        /// <para>
        /// <paramref name="cellBlockBitSet"/> は 1 のビットを 1 個だけ含んでいなければなりません。
        /// </para>
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellDigit"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellBlock AsSingleCellBlock(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellBlockBitSet != 0 && cellBlockBitSet.GetPopCount() == 1);
#endif

            var cellBlock = cellBlockBitSet.GetOneCellBlock();
#if DEBUG
            cellBlockBitSet &= ~cellBlock.ToCellBlockBitSet();
            System.Diagnostics.Debug.Assert(cellBlockBitSet == SudokuBoardCellBlockBitSet.None);
#endif
            return cellBlock;
        }

        /// <summary>
        /// 指定されたセルのブロックのビット集合に含まれる列の全てのセルの位置を含むビット集合を取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルの行のビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static UInt128 ToCellIndexBitSet(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
            unchecked
            {
                // 三項演算子の "xxx ? 1 : 0" あるいは "xxx ? 0 : 1" は非常に高速 (アーキテクチャ依存)
                var uint64Mask = (((ushort)cellBlockBitSet & (1 << 0)) != 0 ? 0ul : 1ul) - 1ul;
                var result = new UInt128(uint64Mask, uint64Mask) & _block0BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 1)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block1BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 2)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block2BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 3)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block3BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 4)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block4BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 5)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block5BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 6)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block6BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 7)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block7BitMask;
                uint64Mask = (((ushort)cellBlockBitSet & (1 << 8)) != 0 ? 0ul : 1ul) - 1ul;
                result |= new UInt128(uint64Mask, uint64Mask) & _block8BitMask;
                return result;
            }
        }

        /// <summary>
        /// 指定されたセルのブロックのビット集合からセルのブロックのシーケンスを取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックを示す <see cref="SudokuBoardCellBlock"/> 値のシーケンスを返します。
        /// </returns>
        internal static IEnumerable<SudokuBoardCellBlock> EnumerateAsCellColumns(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
#if DEBUG
            if ((cellBlockBitSet & ~SudokuBoardCellBlockBitSet.AllBlocks) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
#endif

            while (cellBlockBitSet != 0)
            {
                var block = cellBlockBitSet.GetOneCellBlock();
                yield return block;
                cellBlockBitSet &= ~block.ToCellBlockBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルのブロックのビット集合からセルのブロックの視覚的にわかりやすい文字列のシーケンスを取得します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <returns>
        /// セルのブロックの視覚的にわかりやすい文字列を示す <see cref="string"/> オブジェクトのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<string> EnumerateAsBlockTexts(this SudokuBoardCellBlockBitSet cellBlockBitSet)
        {
#if DEBUG
            if ((cellBlockBitSet & ~SudokuBoardCellBlockBitSet.AllBlocks) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
#endif

            while (cellBlockBitSet != 0)
            {
                var block = cellBlockBitSet.GetOneCellBlock();
                yield return block.ToFriendlyString();
                cellBlockBitSet &= ~block.ToCellBlockBitSet();
            }
        }

        /// <summary>
        /// 指定されたセルのブロックの集合の要素を指定されたメモリに格納します。
        /// </summary>
        /// <param name="cellBlockBitSet">
        /// セルのブロックのビット集合を示す <see cref="SudokuBoardCellBlockBitSet"/> 値です。
        /// </param>
        /// <param name="cellBlockBuffer">
        /// セルのブロック <see cref="SudokuBoardCellBlock"/> 値の格納先です。
        /// </param>
        /// <returns>
        /// 実際にセルのブロック <see cref="SudokuBoardCellBlock"/> が格納されたメモリを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        internal static Span<SudokuBoardCellBlock> EnumerateAsCellBlocks(this SudokuBoardCellBlockBitSet cellBlockBitSet, Span<SudokuBoardCellBlock> cellBlockBuffer)
        {
#if DEBUG
            if ((cellBlockBitSet & ~SudokuBoardCellBlockBitSet.AllBlocks) != 0)
                throw new ArgumentOutOfRangeException(nameof(cellBlockBitSet));
            if (cellBlockBuffer.Length < cellBlockBitSet.GetPopCount())
                throw new ArgumentException("The destination buffer is not long enough.", nameof(cellBlockBuffer));
#endif
            var index = 0;
            while (cellBlockBitSet != 0)
            {
                var block = cellBlockBitSet.GetOneCellBlock();
                cellBlockBuffer[index++] = block;
                cellBlockBitSet &= ~block.ToCellBlockBitSet();
            }

            return cellBlockBuffer[..index];
        }
    }
}
