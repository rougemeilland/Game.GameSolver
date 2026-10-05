using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal class BoardWorkspace
    {
        public static readonly UInt128 AllCellsBits = new(0x000000000001ffff, 0xffffffffffffffff);
        private readonly BoardCellsSet _cellsSource;
        private readonly UInt128[] _cellBittsByDigit;
        private readonly UInt128[] _cellNotBitsByDigit;

        public BoardWorkspace(BoardCellsSet cells)
        {
            _cellsSource = cells;
            DeterminedCellBits = 0;
            _cellBittsByDigit = new UInt128[9];
            _cellNotBitsByDigit = new UInt128[9];
            for (var cellIndex = 0; cellIndex < 81; ++cellIndex)
            {
                var cellRow = cellIndex / 9;
                var cellColumn = cellIndex % 9;
                var cellBitMask = UInt128.One << cellIndex;
                if (cells.IsDetermined(cellColumn, cellRow))
                {
                    DeterminedCellBits |= cellBitMask;
                }
                else
                {
                    var cellValue = cells.GetNoteBits(cellColumn, cellRow);
                    for (var digitMinusOne = 0; digitMinusOne < 9; ++digitMinusOne)
                    {
                        if ((cellValue & (1 << digitMinusOne)) != 0)
                            _cellBittsByDigit[digitMinusOne] |= cellBitMask;
                    }
                }
            }

            for (var index = 0; index < 9; ++index)
                _cellNotBitsByDigit[index] = ~_cellBittsByDigit[index];
        }

        /// <summary>
        /// 確定済みのセルのビット集合を取得します。
        /// </summary>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        public UInt128 DeterminedCellBits
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get;
            private set;
        } = 0;

        /// <summary>
        /// 指定された値のセルのビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの値を示す 1 から 9 までの <see cref="int"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        /// <remarks>
        /// 返るビット集合には確定済み及び未確定の両方のセルのビット集合が含まれます。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetBitsByDigit(int digit)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(digit, 1, nameof(digit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9, nameof(digit));
#endif
            return _cellBittsByDigit[digit - 1];
        }

        /// <summary>
        /// 指定された値ではないセルのビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの値を示す 1 から 9 までの <see cref="int"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        /// <remarks>
        /// 返るビット集合には確定済み及び未確定の両方のセルのビット集合が含まれます。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetNotBitsByDigit(int digit)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(digit, 1, nameof(digit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9, nameof(digit));
#endif
            return _cellNotBitsByDigit[digit - 1];
        }

        /// <summary>
        /// 指定されたセルの値が確定されているかどうかを調べます。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列です。列は 0 から 8 までの整数です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行です。行は 0 から 8 までの整数です。
        /// </param>
        /// <returns>
        /// 指定されたセルの値が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(int column, int row) => _cellsSource.IsDetermined(column, row);

        /// <summary>
        /// 値が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列です。列は 0 から 8 までの整数です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行です。行は 0 から 8 までの整数です。
        /// </param>
        /// <value>
        /// 指定されたセルの候補を示す <see cref="ushort"/> 値を返します。
        /// この値は 9 ビットのビット列で、0 ビットが 数字 1、8 ビットが 数字 9 を意味し、各ビットが 1 の場合に対応する数字が候補に含まれます。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNoteBits(int column, int row) => _cellsSource.GetNoteBits(column, row);
    }
}
