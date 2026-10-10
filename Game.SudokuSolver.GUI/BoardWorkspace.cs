using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal class BoardWorkspace
    {
        public static readonly UInt128 AllCellBits = new(0x000000000001ffff, 0xffffffffffffffff);
        private readonly BoardCellsSet _cellsSource;
        private readonly UInt128[] _cellBittsByDigit;
        private readonly UInt128[] _cellNotBitsByDigit;

        public BoardWorkspace(BoardCellsSet cells)
        {
            _cellsSource = cells;
            DeterminedCellBits = 0;
            _cellBittsByDigit = new UInt128[9];
            _cellNotBitsByDigit = new UInt128[9];
            for (var cellIndex = BoardCellIndex.MinValue; cellIndex <= BoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellBitMask = cellIndex.ToBitMask();
                if (cells.IsDetermined(cellIndex))
                {
                    DeterminedCellBits |= cellBitMask;
                }
                else
                {
                    var cellNoteBits = cells.GetNoteBits(cellIndex);
                    for (var digitMinusOne = 0; digitMinusOne < 9; ++digitMinusOne)
                    {
                        if ((cellNoteBits & (1 << digitMinusOne)) != 0)
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
        /// 指定されたセルの値が確定されているかどうかを調べます。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの値が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(BoardCellIndex cellIndex) => _cellsSource.IsDetermined(cellIndex);

        /// <summary>
        /// 指定されたセルの値が確定されているかどうかを調べます。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの値が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(BoardCellColumn column, BoardCellRow row) => _cellsSource.IsDetermined(column, row);

        /// <summary>
        /// 指定された数字のメモを含む未確定セルのビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetNoteBits(BoardCellDigit digit)
        {
#if DEBUG
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            return _cellBittsByDigit[digit - BoardCellDigit.One];
        }

        /// <summary>
        /// 値が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの候補を示す <see cref="ushort"/> 値を返します。
        /// この値は 9 ビットのビット列で、0 ビットが 数字 1、8 ビットが 数字 9 を意味し、各ビットが 1 の場合に対応する数字が候補に含まれます。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNoteBits(BoardCellIndex cellIndex) => _cellsSource.GetNoteBits(cellIndex);

        /// <summary>
        /// 値が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの候補を示す <see cref="ushort"/> 値を返します。
        /// この値は 9 ビットのビット列で、0 ビットが 数字 1、8 ビットが 数字 9 を意味し、各ビットが 1 の場合に対応する数字が候補に含まれます。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetNoteBits(BoardCellColumn column, BoardCellRow row) => _cellsSource.GetNoteBits(column, row);

        /// <summary>
        /// 指定された数字のメモを含まない未確定セルのビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetNotNoteBits(BoardCellDigit digit)
        {
#if DEBUG
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            return _cellNotBitsByDigit[digit - BoardCellDigit.One];
        }
    }
}
