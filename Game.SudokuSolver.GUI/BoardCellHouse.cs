using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal class BoardCellHouse
    {
        private static readonly UInt128[] _rowCellBitMasks =
        [
            // TODO: 配列のコードを別プログラムから生成する
#error
            CreateBits(00, 01, 02, 03, 04, 05, 06, 07, 08),
            CreateBits(09, 10, 11, 12, 13, 14, 15, 16, 17),
            CreateBits(18, 19, 20, 21, 22, 23, 24, 25, 26),
            CreateBits(27, 28, 29, 30, 31, 32, 33, 34, 35),
            CreateBits(36, 37, 38, 39, 40, 41, 42, 43, 44),
            CreateBits(45, 46, 47, 48, 49, 50, 51, 52, 53),
            CreateBits(54, 55, 56, 57, 58, 59, 60, 61, 62),
            CreateBits(63, 64, 65, 66, 67, 68, 69, 70, 71),
            CreateBits(72, 73, 74, 75, 76, 77, 78, 79, 80),
        ];

        private static readonly UInt128[] _columnCellBitMasks =
        [
            CreateBits(00, 09, 18, 27, 36, 45, 54, 63, 72),
            CreateBits(01, 10, 19, 28, 37, 46, 55, 64, 73),
            CreateBits(02, 11, 20, 29, 38, 47, 56, 65, 74),
            CreateBits(03, 12, 21, 30, 39, 48, 57, 66, 75),
            CreateBits(04, 13, 22, 31, 40, 49, 58, 67, 76),
            CreateBits(05, 14, 23, 32, 41, 50, 59, 68, 77),
            CreateBits(06, 15, 24, 33, 42, 51, 60, 69, 78),
            CreateBits(07, 16, 25, 34, 43, 52, 61, 70, 79),
            CreateBits(08, 17, 26, 35, 44, 53, 62, 71, 80),

        ];

        private static readonly int[] _blockOfCell =
        [
            0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 1, 1, 1, 2, 2, 2,
            3, 3, 3, 4, 4, 4, 5, 5, 5,
            3, 3, 3, 4, 4, 4, 5, 5, 5,
            3, 3, 3, 4, 4, 4, 5, 5, 5,
            6, 7, 8, 6, 7, 8, 6, 7, 8,
            6, 7, 8, 6, 7, 8, 6, 7, 8,
            6, 7, 8, 6, 7, 8, 6, 7, 8,
        ];

        private static readonly UInt128[] _blockCellGroup =
        [
            CreateBits(00, 01, 02, 09, 10, 11, 18, 19, 20),
            CreateBits(03, 04, 05, 12, 13, 14, 21, 22, 23),
            CreateBits(06, 07, 08, 15, 16, 17, 24, 25, 26),
            CreateBits(27, 28, 29, 36, 37, 38, 45, 46, 47),
            CreateBits(30, 31, 32, 39, 40, 41, 48, 49, 50),
            CreateBits(33, 34, 35, 42, 43, 44, 51, 52, 53),
            CreateBits(54, 55, 56, 63, 64, 65, 72, 73, 74),
            CreateBits(57, 58, 59, 66, 67, 68, 75, 76, 77),
            CreateBits(60, 61, 62, 69, 70, 71, 78, 79, 80),
        ];

        /// <summary>
        /// 指定された行のビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// 行番号です。行番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <returns>
        /// 指定された行のビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetRowCellsBitMask(int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
#endif

            return _rowCellBitMasks[row];
        }

        /// <summary>
        /// 指定された列のビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// 列番号です。列番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <returns>
        /// 指定された列のビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetColumnCellsBitMask(int column)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
#endif

            return _columnCellBitMasks[column];
        }

        /// <summary>
        /// 指定されたブロックのビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// ブロック番号です。ブロック番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <returns>
        /// 指定されたブロックのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitMask(int block)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(block, 0, nameof(block));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(block, 8, nameof(block));
#endif

            return _blockCellGroup[block];
        }

        /// <summary>
        /// 指定されたセルが属するブロックのビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// セルの行番号です。行番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <param name="row">
        /// セルの列番号です。列番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属するブロックのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitMask(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
#endif

            // TODO: 81個の配列に展開
#error

            return _blockCellGroup[_blockOfCell[row * 9 + column]];
        }

        /// <summary>
        /// 指定されたセルが属する全てのハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// セルの行番号です。行番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <param name="row">
        /// セルの列番号です。列番号は 0 から 8 までの間の整数です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属する全てのハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetVisibleCellsBitMask(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
#endif

            // TODO: 81個の配列に展開
#error

            return _columnCellBitMasks[column] | _rowCellBitMasks[row] | _blockCellGroup[_blockOfCell[row * 9 + column]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static UInt128 CreateBits(int pos0, int pos1, int pos2, int pos3, int pos4, int pos5, int pos6, int pos7, int pos8)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(pos0, 0, nameof(pos0));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos0, 80, nameof(pos0));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos1, 0, nameof(pos1));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos1, 80, nameof(pos1));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos2, 0, nameof(pos2));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos2, 80, nameof(pos2));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos3, 0, nameof(pos3));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos3, 80, nameof(pos3));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos4, 0, nameof(pos4));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos4, 80, nameof(pos4));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos5, 0, nameof(pos5));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos5, 80, nameof(pos5));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos6, 0, nameof(pos6));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos6, 80, nameof(pos6));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos7, 0, nameof(pos7));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos7, 80, nameof(pos7));
            ArgumentOutOfRangeException.ThrowIfLessThan(pos8, 0, nameof(pos8));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(pos8, 80, nameof(pos8));
#endif

            return (UInt128.One << pos0)
                    | (UInt128.One << pos1)
                    | (UInt128.One << pos2)
                    | (UInt128.One << pos3)
                    | (UInt128.One << pos4)
                    | (UInt128.One << pos5)
                    | (UInt128.One << pos6)
                    | (UInt128.One << pos7)
                    | (UInt128.One << pos8);
        }
    }
}}
