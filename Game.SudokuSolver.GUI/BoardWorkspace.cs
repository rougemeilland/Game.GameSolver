using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal class BoardWorkspace
    {
        public static readonly UInt128 AllCellIndexBitSet = new(0x000000000001ffff, 0xffffffffffffffff);
        private readonly BoardCellsSet _cellsSource;
        private readonly UInt128[] _cellIndexBittSetByDigit;

        public BoardWorkspace(BoardCellsSet cells)
        {
            _cellsSource = cells;
            DeterminedCellBitSet = 0;
            _cellIndexBittSetByDigit = new UInt128[9];
            for (var cellIndex = BoardCellIndex.MinValue; cellIndex <= BoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellIndexBitSet = cellIndex.ToCellIndexBitSet();
                if (cells.IsDetermined(cellIndex))
                {
                    DeterminedCellBitSet |= cellIndexBitSet;
                }
                else
                {
                    foreach (var cellDigit in cells.GetNoteBitSet(cellIndex).EnumerateAsCellDigits())
                        _cellIndexBittSetByDigit[cellDigit - BoardCellDigit.One] |= cellIndexBitSet;
                }
            }
        }

        /// <summary>
        /// 確定済みのセルのビット集合を取得します。
        /// </summary>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        public UInt128 DeterminedCellBitSet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get;
            private set;
        } = 0;

        /// <summary>
        /// 指定されたセルの数字が確定されているかどうかを調べます。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの数字が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(BoardCellIndex cellIndex) => _cellsSource.IsDetermined(cellIndex);

        /// <summary>
        /// 指定されたセルの数字が確定されているかどうかを調べます。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの数字が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(BoardCellColumn column, BoardCellRow row) => _cellsSource.IsDetermined(column, row);

        /// <summary>
        /// 未確定のセルに対して、指定された数字の候補のビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetNoteBitSet(BoardCellDigit digit)
        {
#if DEBUG
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            return _cellIndexBittSetByDigit[digit - BoardCellDigit.One];
        }

        /// <summary>
        /// 数字が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの数字の候補を示す <see cref="BoardCellNoteBitSet"/> 値を返します。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoardCellNoteBitSet GetNoteBitSet(BoardCellIndex cellIndex) => _cellsSource.GetNoteBitSet(cellIndex);

        /// <summary>
        /// 数字が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの数字の候補を示す <see cref="BoardCellNoteBitSet"/> 値を返します。
        /// この値は 9 ビットのビット列で、0 ビットが 数字 1、8 ビットが 数字 9 を意味し、各ビットが 1 の場合に対応する数字が候補に含まれます。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoardCellNoteBitSet GetNoteBitSet(BoardCellColumn column, BoardCellRow row) => _cellsSource.GetNoteBitSet(column, row);
    }
}
