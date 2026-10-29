using System;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の分析情報を保持するクラスです。
    /// </summary>
    internal sealed class SudokuBoardWorkspace
    {
        public static readonly UInt128 AllCellIndexBitSet = new(0x000000000001ffff, 0xffffffffffffffff);
        private readonly SudokuBoardCellsSet _cellsSource;
        private readonly UInt128[] _cellIndexBittSetByDigit;

        public SudokuBoardWorkspace(SudokuBoardCellsSet cells)
        {
            _cellsSource = cells;
            DeterminedCellBitSet = 0;
            _cellIndexBittSetByDigit = new UInt128[9];
            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellIndexBitSet = cellIndex.ToCellIndexBitSet();
                if (cells.IsDetermined(cellIndex))
                {
                    DeterminedCellBitSet |= cellIndexBitSet;
                }
                else
                {
                    foreach (var cellDigit in cells.GetNoteBitSet(cellIndex).EnumerateAsCellDigits())
                        _cellIndexBittSetByDigit[cellDigit - SudokuBoardCellDigit.One] |= cellIndexBitSet;
                }
            }
        }

        /// <summary>
        /// 盤面が完了しているかどうか調べます。
        /// </summary>
        /// <returns>
        /// 盤面が完了しているならば <see langword="true"/>、そうではないならば <see langword="false"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsCompleted() => DeterminedCellBitSet == AllCellIndexBitSet;

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
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの数字が確定されていれば <see langword="true"/>、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(SudokuBoardCellIndex cellIndex)
        {

            var resut = (DeterminedCellBitSet & cellIndex.ToCellIndexBitSet()) != 0;
#if DEBUG
            System.Diagnostics.Debug.Assert(resut == _cellsSource.IsDetermined(cellIndex));
#endif
            return resut;
        }

        /// <summary>
        /// 数字が確定している指定されたセルの数字を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの値を示す <see cref="SudokuBoardCellDigit"/> 値を返します。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public SudokuBoardCellDigit GetDeterminedCellDigit(SudokuBoardCellIndex cellIndex)
            => _cellsSource.GetDeterminedCellDigit(cellIndex);

        /// <summary>
        /// 指定された数字を候補に持つセルのビット集合を取得します。
        /// </summary>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <value>
        /// セルのビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public UInt128 GetNoteBitSet(SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif
            return _cellIndexBittSetByDigit[digit - SudokuBoardCellDigit.One];
        }

        /// <summary>
        /// 数字が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの数字の候補を示す <see cref="SudokuBoardCellNoteBitSet"/> 値を返します。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SudokuBoardCellNoteBitSet GetNoteBitSet(SudokuBoardCellIndex cellIndex) => _cellsSource.GetNoteBitSet(cellIndex);

        /// <summary>
        /// 盤面に数字の候補が存在しないセルの位置を取得します。
        /// </summary>
        /// <returns>
        /// 数字の候補が存在しないセルの位置のビット集合である <see cref="UInt128"/> 値を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt128 GetNoNoteCellIndexBitSet() => _cellsSource.GetNoNoteCellIndexBitSet();

        /// <summary>
        /// 指定された位置のセルが指定された数字に確定した場合に数字の候補を持つ別のセルの位置のビット集合を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="cellNoteDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// <para>
        /// 以下の条件を全て満たすセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </para>
        /// <list type="number">
        /// <item>未確定のセルである</item>
        /// <item><paramref name="cellIndex"/> で指定されたセルと何れかのハウスを共有する</item>
        /// <item><paramref name="cellNoteDigit"/> で指定された数字を候補に持つ</item>
        /// <item><paramref name="cellIndex"/> 自身ではない</item>
        /// </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt128 GetCellIndexBitSetForExcludedNoteDigit(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellNoteDigit)
            => ~DeterminedCellBitSet
                & GetNoteBitSet(cellNoteDigit)
                & SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex)
                & ~cellIndex.ToCellIndexBitSet();

        /// <summary>
        /// 指定されたセルを指定された数字で確定可能かどうかを調べます。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="cellDigit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字の確定が可能である場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// </returns>
        /// <remarks>
        /// <para>
        /// 以下のような場合は確定不可能です。
        /// <list type="bullet">
        /// <item>指定されたセルの数字が既に確定している場合</item>
        /// <item>指定されたセルの何れかのハウスに同じ数字で確定しているセルが存在する場合</item>
        /// </list>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCetermineCell(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellDigit)
            => _cellsSource.CanCetermineCell(cellIndex, cellDigit);

        /// <summary>
        /// 現在の盤面の配置をデバッグコンソールに出力します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrintToDebugConsole()
        {
#if DEBUG
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                if ((row - SudokuBoardCellRow.One) % 3 == 0)
                    System.Diagnostics.Debug.WriteLine("+---+---+---+");
                for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
                {
                    if ((column - SudokuBoardCellColumn.One) % 3 == 0)
                        System.Diagnostics.Debug.Write("|");
                    var cellIndex = SudokuBoardCellGeometry.GetCellIndex(column, row);
                    System.Diagnostics.Debug.Write(IsDetermined(cellIndex) ? GetDeterminedCellDigit(cellIndex).ToCellDigitChar() : ' ');
                }

                System.Diagnostics.Debug.WriteLine("|");
            }

            System.Diagnostics.Debug.WriteLine("+---+---+---+");
            System.Diagnostics.Debug.WriteLine("");
#endif
        }
    }
}
