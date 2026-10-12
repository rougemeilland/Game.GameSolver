using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal partial class BoardCellGeometry
    {
        /// <summary>
        /// 指定された行の行ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="row">
        /// 列を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetRowCellsBitSet(BoardCellRow row)
        {
#if DEBUG
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return _rowHouseCellsBitSetsByRow[row - BoardCellRow.One];
        }

        /// <summary>
        /// 指定されたセルの行ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの番号を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetRowCellsBitSet(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex < BoardCellIndex.MinValue | cellIndex > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _rowHouseCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定された行の列ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="column">
        /// 行を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetColumnCellsBitSet(BoardCellColumn column)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return _columnHouseCellsBitSetsByColumn[column - BoardCellColumn.One];
        }

        /// <summary>
        /// 指定されたセルの列ハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの行を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定された行ハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetColumnCellsBitSet(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _columnHouseCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="block">
        /// ブロックを示す <see cref="BoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(BoardCellBlock block)
        {
#if DEBUG
            if (block is < BoardCellBlock.MinValue or > BoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif

            return _blockHouseCellsBitSetsByBlock[block - BoardCellBlock.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属するブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属するブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            var cellIndex = _cellIndexByRowColumn[((row - BoardCellRow.One) << 4) | (column - BoardCellColumn.One)];
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndex is >= BoardCellIndex.MinValue and <= BoardCellIndex.MaxValue);
#endif
            return _blockHouseCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属するブロックハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属するブロックハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetBlockCellsBitSet(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _blockHouseCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属する全てのハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属する全てのハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetVisibleCellsBitSet(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            var cellIndex = _cellIndexByRowColumn[((row - BoardCellRow.One) << 4) | (column - BoardCellColumn.One)];
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndex is >= BoardCellIndex.MinValue and <= BoardCellIndex.MaxValue);
#endif
            return _visibleCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたセルが属する全てのハウスのビットマスクを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルが属する全てのハウスのビットマスクを示す <see cref="UInt128"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt128 GetVisibleCellsBitSetByCellIndex(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif

            return _visibleCellsBitSetsByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }

        /// <summary>
        /// 指定されたブロックと重なる列のうちひとつの列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="BoardCellBlock"/> 値です。
        /// </param>
        /// <param name="columnOffset">
        /// ブロックと重なる列のうちのひとつを示す <see cref="BoardCellBlockColumn"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックと重なる列を示す <see cref="BoardCellColumn"/>値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellColumn GetColumn(BoardCellBlock block, BoardCellBlockColumn columnOffset)
        {
#if DEBUG
            if (block is < BoardCellBlock.MinValue or > BoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (columnOffset is < BoardCellBlockColumn.MinValue or > BoardCellBlockColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(columnOffset));
#endif

            return _columnsByBlock[block - BoardCellBlock.FirstValue][columnOffset - BoardCellBlockColumn.Zero];
        }

        /// <summary>
        /// 指定されたブロックと重なる行のうちひとつの列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="BoardCellBlock"/> 値です。
        /// </param>
        /// <param name="rowOffset">
        /// ブロックと重なる行のうちのひとつを示す <see cref="BoardCellBlockRow"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたブロックと重なる列を示す <see cref="BoardCellColumn"/>値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellRow GetRow(BoardCellBlock block, BoardCellBlockRow rowOffset)
        {
#if DEBUG
            if (block is < BoardCellBlock.MinValue or > BoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (rowOffset is < BoardCellBlockRow.MinValue or > BoardCellBlockRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rowOffset));
#endif

            return _rowsByBlock[block - BoardCellBlock.FirstValue][rowOffset - BoardCellBlockRow.Zero];
        }

        /// <summary>
        /// 指定された列と重なるブロックの配列を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定された列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// ブロックを示す <see cref="BoardCellBlock"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<BoardCellBlock> GetBlocks(BoardCellColumn column)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return new ReadOnlyMemory<BoardCellBlock>(_blocksByColumn[column - BoardCellColumn.One]);
        }

        /// <summary>
        /// 指定された行と重なるブロックの配列を取得します。
        /// </summary>
        /// <param name="row">
        /// 指定された行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// ブロックを示す <see cref="BoardCellBlock"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<BoardCellBlock> GetBlocks(BoardCellRow row)
        {
#if DEBUG
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return new ReadOnlyMemory<BoardCellBlock>(_blocksByRow[row - BoardCellRow.One]);
        }

        /// <summary>
        /// 指定された列に含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定された列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<BoardCellIndex> GetCellIndexes(BoardCellColumn column)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
#endif

            return new ReadOnlyMemory<BoardCellIndex>(_cellIndexesByColumn[column - BoardCellColumn.One]);
        }

        /// <summary>
        /// 指定された行に含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="row">
        /// 指定された行を示す <see cref="BoardCellRown"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<BoardCellIndex> GetCellIndexes(BoardCellRow row)
        {
#if DEBUG
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return new ReadOnlyMemory<BoardCellIndex>(_cellIndexesByRow[row - BoardCellRow.One]);
        }

        /// <summary>
        /// 指定されたブロックに含まれるセルの位置の配列を取得します。
        /// </summary>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="BoardCellBlock"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="BoardCellIndex"/> の配列です。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ReadOnlyMemory<BoardCellIndex> GetCellIndexes(BoardCellBlock block)
        {
#if DEBUG
            if (block is < BoardCellBlock.MinValue or > BoardCellBlock.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(block));
#endif

            return new ReadOnlyMemory<BoardCellIndex>(_cellIndexesByBlock[block - BoardCellBlock.FirstValue]);
        }

        /// <summary>
        /// 指定されたセルの行と列に対応するセルの番号を取得します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <returns>
        /// セルの番号を示す <see cref="int"/> 値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellIndex GetCellIndex(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            var cellIndex = _cellIndexByRowColumn[((row - BoardCellRow.One) << 4) | (column - BoardCellColumn.One)];
#if DEBUG
            System.Diagnostics.Debug.Assert(cellIndex is >= BoardCellIndex.MinValue and <= BoardCellIndex.MaxValue);
#endif
            return cellIndex;
        }

        /// <summary>
        /// 指定されたセルの列を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="BoardCellColumn"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellColumn GetColumn(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - BoardCellIndex.FirstValue].column;
        }

        /// <summary>
        /// 指定されたセルの行を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="BoardCellRow"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellRow GetRow(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - BoardCellIndex.FirstValue].row;
        }

        /// <summary>
        /// 指定されたセルのブロックを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの行を示す <see cref="BoardCellBlock"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static BoardCellBlock GetBlock(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - BoardCellIndex.FirstValue].block;
        }

        /// <summary>
        /// 指定されたセルの行と列、ブロックを取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// セルの列を示す <see cref="int"/> 値と行を示す <see cref="int"/> 値、ブロックを示す <see cref="BoardCellBlock"/> 値の組を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (BoardCellColumn column, BoardCellRow row, BoardCellBlock block) GetColumnRowBlock(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return _columnRowBlockByCellIndex[cellIndex - BoardCellIndex.FirstValue];
        }
    }
}
