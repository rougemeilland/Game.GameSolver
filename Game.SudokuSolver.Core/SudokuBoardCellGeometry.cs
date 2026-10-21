using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の一般的な計算・検索のヘルパークラスです。
    /// </summary>
    internal partial class SudokuBoardCellGeometry
    {
        /// <summary>
        /// 指定された行の行ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// 列を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetRowCellsBitSet(SudokuBoardCellRow row)
        {
#if DEBUG
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return _rowHouseCellsBitSetsByRow[row - SudokuBoardCellRow.One];
        }

        /// <summary>
        /// 指定されたセルの行ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの番号を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetRowCellsBitSet(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex < SudokuBoardCellIndex.MinValue | cellIndex > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _rowHouseCellsBitSetsByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定された行の列ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="column">
        /// 行を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetColumnCellsBitSet(SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return _columnHouseCellsBitSetsByColumn[column - SudokuBoardCellColumn.One];
        }

        /// <summary>
        /// 指定されたセルの列ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの行を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetColumnCellsBitSet(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _columnHouseCellsBitSetsByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="block">
        /// ブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif

            return _blockHouseCellsBitSetsByBlock[block - SudokuBoardCellBlock.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属するブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属するブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(SudokuBoardCellColumn column, SudokuBoardCellRow row)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            var cellIndex = _cellIndexByRowColumn[((row - SudokuBoardCellRow.One) << 4) | (column - SudokuBoardCellColumn.One)];
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
#endif
            return _blockHouseCellsBitSetsByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属するブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属するブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _blockHouseCellsBitSetsByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属する全てのハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属する全てのハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetVisibleCellsBitSet(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _visibleCellsBitSetsByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたブロックと重なる列のうちひとつの列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="columnOffset">
        /// ブロックと重なる列のうちのひとつを示す <see cref="SudokuBoardCellBlockColumn"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックと重なる列を示す <see cref="SudokuBoardCellColumn"/>値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellColumn GetColumn(SudokuBoardCellBlock block, SudokuBoardCellBlockColumn columnOffset)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (columnOffset is < SudokuBoardCellBlockColumn.MinValue or > SudokuBoardCellBlockColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(columnOffset));
#endif

            return _columnsByBlock[block - SudokuBoardCellBlock.FirstValue][columnOffset - SudokuBoardCellBlockColumn.Zero];
        }

        /// <summary>
        /// 指定されたブロックと重なる行のうちひとつの列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="rowOffset">
        /// ブロックと重なる行のうちのひとつを示す <see cref="SudokuBoardCellBlockRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックと重なる列を示す <see cref="SudokuBoardCellColumn"/>値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellRow GetRow(SudokuBoardCellBlock block, SudokuBoardCellBlockRow rowOffset)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (rowOffset is < SudokuBoardCellBlockRow.MinValue or > SudokuBoardCellBlockRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rowOffset));
#endif

            return _rowsByBlock[block - SudokuBoardCellBlock.FirstValue][rowOffset - SudokuBoardCellBlockRow.Zero];
        }

        /// <summary>
        /// 指定された列と重なるブロックの配列を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定された列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// ブロックを示す <see cref="SudokuBoardCellBlock"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<SudokuBoardCellBlock> GetBlocks(SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return new ReadOnlyMemory<SudokuBoardCellBlock>(_blocksByColumn[column - SudokuBoardCellColumn.One]);
        }

        /// <summary>
        /// 指定された行と重なるブロックの配列を取得します。
        /// </summary>
        /// <param name="row">
        /// 指定された行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// ブロックを示す <see cref="SudokuBoardCellBlock"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<SudokuBoardCellBlock> GetBlocks(SudokuBoardCellRow row)
        {
#if DEBUG
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return new ReadOnlyMemory<SudokuBoardCellBlock>(_blocksByRow[row - SudokuBoardCellRow.One]);
        }

        /// <summary>
        /// 指定された列に含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定された列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<SudokuBoardCellIndex> GetCellIndexes(SudokuBoardCellColumn column)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return new ReadOnlyMemory<SudokuBoardCellIndex>(_cellIndexesByColumn[column - SudokuBoardCellColumn.One]);
        }

        /// <summary>
        /// 指定された行に含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="row">
        /// 指定された行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<SudokuBoardCellIndex> GetCellIndexes(SudokuBoardCellRow row)
        {
#if DEBUG
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return new ReadOnlyMemory<SudokuBoardCellIndex>(_cellIndexesByRow[row - SudokuBoardCellRow.One]);
        }

        /// <summary>
        /// 指定されたブロックに含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<SudokuBoardCellIndex> GetCellIndexes(SudokuBoardCellBlock block)
        {
#if DEBUG
            if (block is < SudokuBoardCellBlock.MinValue or > SudokuBoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif

            return new ReadOnlyMemory<SudokuBoardCellIndex>(_cellIndexesByBlock[block - SudokuBoardCellBlock.FirstValue]);
        }

        /// <summary>
        /// 指定されたセルの行と列に対応するセルの番号を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// セルの番号を示す <see cref="int"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellIndex GetCellIndex(SudokuBoardCellColumn column, SudokuBoardCellRow row)
        {
#if DEBUG
            if (column is < SudokuBoardCellColumn.MinValue or > SudokuBoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < SudokuBoardCellRow.MinValue or > SudokuBoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            var cellIndex = _cellIndexByRowColumn[((row - SudokuBoardCellRow.One) << 4) | (column - SudokuBoardCellColumn.One)];
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndex is >= SudokuBoardCellIndex.MinValue and <= SudokuBoardCellIndex.MaxValue);
#endif
            return cellIndex;
        }

        /// <summary>
        /// 指定されたセルの列を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="SudokuBoardCellColumn"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellColumn GetColumn(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue].column;
        }

        /// <summary>
        /// 指定されたセルの行を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellRow"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellRow GetRow(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue].row;
        }

        /// <summary>
        /// 指定されたセルのブロックを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="SudokuBoardCellBlock"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static SudokuBoardCellBlock GetBlock(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue].block;
        }

        /// <summary>
        /// 指定されたセルの行と列、ブロックを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="int"/> 値と行を示す <see cref="int"/> 値、ブロックを示す <see cref="SudokuBoardCellBlock"/> 値の組を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (SudokuBoardCellColumn column, SudokuBoardCellRow row, SudokuBoardCellBlock block) GetColumnRowBlock(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - SudokuBoardCellIndex.FirstValue];
        }
    }
}
