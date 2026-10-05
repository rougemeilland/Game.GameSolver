using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal class BoardCellsSet
    {
        private const ushort _IS_DETERMINED = 0x0200;
        private const ushort _ALL_NOTE_BIT_MASK = 0x01ff;
        private const ushort _ALL_BITS_MASK = _IS_DETERMINED | _ALL_NOTE_BIT_MASK;

        // セル毎の、各セルが属しているハウスの別のセルのインデックスの配列
        private static readonly byte[][] _visibleCellsIndexes =
        [
            [ 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 27, 36, 45, 54, 63, 72 ],
            [ 00, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 29, 38, 47, 56, 65, 74 ],
            [ 00, 01, 02, 04, 05, 06, 07, 08, 12, 13, 14, 21, 22, 23, 30, 39, 48, 57, 66, 75 ],
            [ 00, 01, 02, 03, 05, 06, 07, 08, 12, 13, 14, 21, 22, 23, 31, 40, 49, 58, 67, 76 ],
            [ 00, 01, 02, 03, 04, 06, 07, 08, 12, 13, 14, 21, 22, 23, 32, 41, 50, 59, 68, 77 ],
            [ 00, 01, 02, 03, 04, 05, 07, 08, 15, 16, 17, 24, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 00, 01, 02, 03, 04, 05, 06, 08, 15, 16, 17, 24, 25, 26, 34, 43, 52, 61, 70, 79 ],
            [ 00, 01, 02, 03, 04, 05, 06, 07, 15, 16, 17, 24, 25, 26, 35, 44, 53, 62, 71, 80 ],
            [ 00, 01, 02, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 27, 36, 45, 54, 63, 72 ],
            [ 00, 01, 02, 09, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 02, 09, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 29, 38, 47, 56, 65, 74 ],
            [ 03, 04, 05, 09, 10, 11, 13, 14, 15, 16, 17, 21, 22, 23, 30, 39, 48, 57, 66, 75 ],
            [ 03, 04, 05, 09, 10, 11, 12, 14, 15, 16, 17, 21, 22, 23, 31, 40, 49, 58, 67, 76 ],
            [ 03, 04, 05, 09, 10, 11, 12, 13, 15, 16, 17, 21, 22, 23, 32, 41, 50, 59, 68, 77 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 16, 17, 24, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 17, 24, 25, 26, 34, 43, 52, 61, 70, 79 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 24, 25, 26, 35, 44, 53, 62, 71, 80 ],
            [ 00, 01, 02, 09, 10, 11, 19, 20, 21, 22, 23, 24, 25, 26, 27, 36, 45, 54, 63, 72 ],
            [ 00, 01, 02, 09, 10, 11, 18, 20, 21, 22, 23, 24, 25, 26, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 02, 09, 10, 11, 18, 19, 21, 22, 23, 24, 25, 26, 29, 38, 47, 56, 65, 74 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 22, 23, 24, 25, 26, 30, 39, 48, 57, 66, 75 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 21, 23, 24, 25, 26, 31, 40, 49, 58, 67, 76 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 21, 22, 24, 25, 26, 32, 41, 50, 59, 68, 77 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 26, 34, 43, 52, 61, 70, 79 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 35, 44, 53, 62, 71, 80 ],
            [ 00, 09, 18, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 54, 63, 72 ],
            [ 01, 10, 19, 27, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 56, 65, 74 ],
            [ 03, 12, 21, 27, 28, 29, 31, 32, 33, 34, 35, 39, 40, 41, 48, 49, 50, 57, 66, 75 ],
            [ 04, 13, 22, 27, 28, 29, 30, 32, 33, 34, 35, 39, 40, 41, 48, 49, 50, 58, 67, 76 ],
            [ 05, 14, 23, 27, 28, 29, 30, 31, 33, 34, 35, 39, 40, 41, 48, 49, 50, 59, 68, 77 ],
            [ 06, 15, 24, 27, 28, 29, 30, 31, 32, 34, 35, 42, 43, 44, 51, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 27, 28, 29, 30, 31, 32, 33, 35, 42, 43, 44, 51, 52, 53, 61, 70, 79 ],
            [ 08, 17, 26, 27, 28, 29, 30, 31, 32, 33, 34, 42, 43, 44, 51, 52, 53, 62, 71, 80 ],
            [ 00, 09, 18, 27, 28, 29, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 54, 63, 72 ],
            [ 01, 10, 19, 27, 28, 29, 36, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 29, 36, 37, 39, 40, 41, 42, 43, 44, 45, 46, 47, 56, 65, 74 ],
            [ 03, 12, 21, 30, 31, 32, 36, 37, 38, 40, 41, 42, 43, 44, 48, 49, 50, 57, 66, 75 ],
            [ 04, 13, 22, 30, 31, 32, 36, 37, 38, 39, 41, 42, 43, 44, 48, 49, 50, 58, 67, 76 ],
            [ 05, 14, 23, 30, 31, 32, 36, 37, 38, 39, 40, 42, 43, 44, 48, 49, 50, 59, 68, 77 ],
            [ 06, 15, 24, 33, 34, 35, 36, 37, 38, 39, 40, 41, 43, 44, 51, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 51, 52, 53, 61, 70, 79 ],
            [ 08, 17, 26, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 51, 52, 53, 62, 71, 80 ],
            [ 00, 09, 18, 27, 28, 29, 36, 37, 38, 46, 47, 48, 49, 50, 51, 52, 53, 54, 63, 72 ],
            [ 01, 10, 19, 27, 28, 29, 36, 37, 38, 45, 47, 48, 49, 50, 51, 52, 53, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 29, 36, 37, 38, 45, 46, 48, 49, 50, 51, 52, 53, 56, 65, 74 ],
            [ 03, 12, 21, 30, 31, 32, 39, 40, 41, 45, 46, 47, 49, 50, 51, 52, 53, 57, 66, 75 ],
            [ 04, 13, 22, 30, 31, 32, 39, 40, 41, 45, 46, 47, 48, 50, 51, 52, 53, 58, 67, 76 ],
            [ 05, 14, 23, 30, 31, 32, 39, 40, 41, 45, 46, 47, 48, 49, 51, 52, 53, 59, 68, 77 ],
            [ 06, 15, 24, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 53, 61, 70, 79 ],
            [ 08, 17, 26, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 62, 71, 80 ],
            [ 00, 09, 18, 27, 36, 45, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 01, 10, 19, 28, 37, 46, 54, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 03, 12, 21, 30, 39, 48, 54, 55, 56, 58, 59, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 04, 13, 22, 31, 40, 49, 54, 55, 56, 57, 59, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 05, 14, 23, 32, 41, 50, 54, 55, 56, 57, 58, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 06, 15, 24, 33, 42, 51, 54, 55, 56, 57, 58, 59, 61, 62, 69, 70, 71, 78, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 54, 55, 56, 57, 58, 59, 60, 62, 69, 70, 71, 78, 79, 80 ],
            [ 08, 17, 26, 35, 44, 53, 54, 55, 56, 57, 58, 59, 60, 61, 69, 70, 71, 78, 79, 80 ],
            [ 00, 09, 18, 27, 36, 45, 54, 55, 56, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 01, 10, 19, 28, 37, 46, 54, 55, 56, 63, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 56, 63, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 03, 12, 21, 30, 39, 48, 57, 58, 59, 63, 64, 65, 67, 68, 69, 70, 71, 75, 76, 77 ],
            [ 04, 13, 22, 31, 40, 49, 57, 58, 59, 63, 64, 65, 66, 68, 69, 70, 71, 75, 76, 77 ],
            [ 05, 14, 23, 32, 41, 50, 57, 58, 59, 63, 64, 65, 66, 67, 69, 70, 71, 75, 76, 77 ],
            [ 06, 15, 24, 33, 42, 51, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 78, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 78, 79, 80 ],
            [ 08, 17, 26, 35, 44, 53, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 78, 79, 80 ],
            [ 00, 09, 18, 27, 36, 45, 54, 55, 56, 63, 64, 65, 73, 74, 75, 76, 77, 78, 79, 80 ],
            [ 01, 10, 19, 28, 37, 46, 54, 55, 56, 63, 64, 65, 72, 74, 75, 76, 77, 78, 79, 80 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 56, 63, 64, 65, 72, 73, 75, 76, 77, 78, 79, 80 ],
            [ 03, 12, 21, 30, 39, 48, 57, 58, 59, 66, 67, 68, 72, 73, 74, 76, 77, 78, 79, 80 ],
            [ 04, 13, 22, 31, 40, 49, 57, 58, 59, 66, 67, 68, 72, 73, 74, 75, 77, 78, 79, 80 ],
            [ 05, 14, 23, 32, 41, 50, 57, 58, 59, 66, 67, 68, 72, 73, 74, 75, 76, 78, 79, 80 ],
            [ 06, 15, 24, 33, 42, 51, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 80 ],
            [ 08, 17, 26, 35, 44, 53, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79 ],
        ];

        private readonly ushort[] _cells = new ushort[81];

        public BoardCellsSet(int[] cells)
        {
            ArgumentNullException.ThrowIfNull(cells);
            if (cells.Length != _cells.Length)
                throw new ArgumentException("The size of the cell array must be 81.", nameof(cells));

#if DEBUG
            var returned = true;
            try
            {
#endif
                for (var index = 0; index < cells.Length; ++index)
                {
                    if (cells[index] > 0)
                    {
                        if (!DetermineCellCore(index % 9, index / 9, cells[index]))
                            throw new ApplicationException("Failed to initialize the board. There is likely a duplicate digit in one of the houses (row, column, or block).");
                    }
                }
#if DEBUG
            }
            catch (Exception)
            {
                returned = false;
                throw;
            }
            finally
            {
                if (returned)
                    ValidateInternalState();
            }
#endif
        }

        private BoardCellsSet(ushort[] cells)
        {
            ArgumentNullException.ThrowIfNull(cells);
            if (cells.Length != 9)
                throw new ArgumentException("The size of the cell array must be 81.", nameof(cells));

#if DEBUG
            var returned = true;
            try
            {
#endif
                cells.CopyTo(_cells, 0);
#if DEBUG
            }
            catch (Exception)
            {
                returned = false;
                throw;
            }
            finally
            {
                if (returned)
                    ValidateInternalState();
            }
#endif
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
        public bool IsDetermined(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
#endif
            return (_cells[row * 9 + column] & _IS_DETERMINED) != 0;
        }

        /// <summary>
        /// 値が確定している指定されたセルの値を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列です。列は 0 から 8 までの整数です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行です。行は 0 から 8 までの整数です。
        /// </param>
        /// <value>
        /// 指定されたセルの値を示す <see cref="ushort"/> 値を返します。この値は 1 から 9 の整数です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetDeterminedCellValue(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            System.Diagnostics.Debug.Assert(IsDetermined(column, row) == true);
#endif
            return (ushort)(_cells[row * 9 + column] & ~_IS_DETERMINED);
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetNoteBits(int column, int row)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            System.Diagnostics.Debug.Assert(IsDetermined(column, row) == false);
#endif
            return (ushort)(_cells[row * 9 + column] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 指定されたセルを指定された数字で確定します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列です。列は 0 から 8 までの整数です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行です。行は 0 から 8 までの整数です。
        /// </param>
        /// <param name="digit">
        /// 指定された数字です。数字は 1 から 9 までの整数です。
        /// </param>
        /// <returns>
        /// セルの数字の確定に成功した場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// </returns>
        /// <remarks>
        /// <para>
        /// 以下のような場合に確定に失敗します。
        /// <list type="bullet">
        /// <item>指定されたセルの数字が既に確定している場合</item>
        /// <item>指定されたセルの何れかのハウスに同じ数字で確定しているセルが存在する場合</item>
        /// </list>
        /// </para>
        /// </remarks>
        public bool DetermineCell(int column, int row, int digit)
        {
#if DEBUG
            var returned = true;
            try
            {
#endif
                return DetermineCellCore(column, row, digit);
#if DEBUG
            }
            catch (Exception)
            {
                returned = false;
                throw;
            }
            finally
            {
                if (returned)
                    ValidateInternalState();
            }
#endif
        }

        /// <summary>
        /// 指定されたセルのメモから指定された値を削除します。
        /// </summary>
        /// <param name="column">
        /// セルの列を示す 0 から 8 までの <see cref="int"/> 値です。
        /// </param>
        /// <param name="row">
        /// セルの行を示す 0 から 8 までの <see cref="int"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す 1 から 9 までの <see cref="int"/> 値です。
        /// </param>
        /// <returns>
        /// メモからの値の削除に成功した場合は <see langword="true"/>、そうではない場合は <see cref="false"/> を返します。
        /// <para>
        /// 以下のような場合にメモの削除に失敗します。
        /// <list type="bullet">
        /// <item>
        /// 確定済みのセルからメモの削除を試みた場合
        /// </item>
        /// <item>
        /// メモにない数字の削除を試みた場合
        /// </item>
        /// </list>
        /// </para>
        /// </returns>
        public bool RemoveNote(int column, int row, int digit)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            ArgumentOutOfRangeException.ThrowIfLessThan(digit, 1, nameof(digit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9, nameof(digit));
#endif

#if DEBUG
            var returned = true;
            try
            {
#endif
                var determinedCellValue = (ushort)(_IS_DETERMINED | (ushort)digit);
                var targetCellIndex = row * 9 + column;

                // 指定されたセルまだ確定していないことの確認
                if ((_cells[targetCellIndex] & _IS_DETERMINED) != 0)
                    return false;

                var targetDigitMask = 1 << (digit - 1);

                // 削除対象の数字がメモに存在することの確認
                if ((_cells[targetCellIndex] & targetDigitMask) == 0)
                    return false;

                // メモから数字を削除
                _cells[targetCellIndex] &= (ushort)~targetDigitMask;

                return true;
#if DEBUG
            }
            catch (Exception)
            {
                returned = false;
                throw;
            }
            finally
            {
                if (returned)
                    ValidateInternalState();
            }
#endif
        }

        /// <summary>
        /// 盤面が完了しているかどうかを調べます。
        /// </summary>
        /// <returns>
        /// 盤面が完了していれば <see langword="true"/>、そうではないのなら <see langword="false"/> を返します。
        /// </returns>
        public bool CheckIfSuccess()
        {
            for (var index = 0; index < _cells.Length; ++index)
            {
                if ((_cells[index] & _IS_DETERMINED) == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 盤面が失敗しているかどうかを調べます。
        /// </summary>
        /// <returns>
        /// 盤面が失敗していれば <see langword="true"/>、そうではないのなら <see langword="false"/> を返します。
        /// </returns>
        /// <remarks>
        /// 確定しておらずかつ候補が一つもないセルが一つでも存在する場合、その盤面は失敗とみなされます。
        /// </remarks>
        public bool CheckIfFailed()
        {
            for (var index = 0; index < _cells.Length; ++index)
            {
                if (_cells[index] == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 盤面の状態を 32bbit 配列にパックします。
        /// </summary>
        /// <returns>
        /// 盤面の状態がパックされた <see cref="uint"/> の配列です。
        /// </returns>
        public uint[] Pack()
        {
            var packedData = PackCore(_cells);

#if DEBUG
            System.Diagnostics.Debug.Assert(UnpackCore(packedData).SequenceEqual(_cells));
#endif
            return packedData;
        }

        /// <summary>
        /// オブジェクトの複製を作成します。
        /// </summary>
        /// <returns>
        /// 複製された <see cref="BoardWorkspace"/> オブジェクトです。
        /// </returns>
        public BoardCellsSet Clone() => new(_cells);

        private bool DetermineCellCore(int column, int row, int digit)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfLessThan(column, 0, nameof(column));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(column, 8, nameof(column));
            ArgumentOutOfRangeException.ThrowIfLessThan(row, 0, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(row, 8, nameof(row));
            ArgumentOutOfRangeException.ThrowIfLessThan(digit, 1, nameof(digit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9, nameof(digit));
#endif
            var cellIndex = row * 9 + column;
            if ((_cells[cellIndex] & _IS_DETERMINED) != 0)
                return false;
            var determinedCellValue = (ushort)(_IS_DETERMINED | (ushort)digit);
            var cellNoteBitMask = (ushort)(1 << (digit - 1));
            var otherCellsIndexes = _visibleCellsIndexes[cellIndex];
            for (var index = 0; index < otherCellsIndexes.Length; ++index)
            {
                var otherCellIndex = otherCellsIndexes[index];
                if ((_cells[otherCellIndex] & _IS_DETERMINED) != 0)
                {
                    if (_cells[otherCellIndex] == determinedCellValue)
                        return false;
                }
                else
                {
                    if ((_cells[otherCellIndex] & cellNoteBitMask) != 0)
                        _cells[otherCellIndex] &= (ushort)~cellNoteBitMask;
                }
            }

            _cells[cellIndex] = determinedCellValue;
            return true;
        }

        private void ValidateInternalState()
        {
#if DEBUG
            // 未使用ビットが 0 であることの確認
            for (var index = 9; index < _cells.Length; ++index)
                System.Diagnostics.Debug.Assert((_cells[index] & ~_ALL_BITS_MASK) == 0);

            // 確定している数字の範囲が正しいことの確認
            for (var index = 9; index < _cells.Length; ++index)
            {
                var digit = _cells[index];
                if ((digit & _IS_DETERMINED) != 0)
                {
                    System.Diagnostics.Debug.Assert(digit is >= 1 and <= 9);
                }
            }

            // 各ハウスで確定済みの値が重複していないことの確認
            for (var cellIndex = 0; cellIndex < _visibleCellsIndexes.Length; ++cellIndex)
            {
                var cellValue = _cells[cellIndex];
                if ((cellValue & _IS_DETERMINED) != 0)
                {
                    var otherCellsIndexes = _visibleCellsIndexes[cellIndex];
                    for (var index = 0; index < otherCellsIndexes.Length; ++index)
                    {
                        var otherCellValue = _cells[otherCellsIndexes[index]];
                        System.Diagnostics.Debug.Assert((otherCellValue & _IS_DETERMINED) == 0 || otherCellValue != cellValue);
                    }
                }
            }

            // TODO: 各ハウスで確定済みのセルの数字がメモにないことの確認
            for (var cellIndex = 0; cellIndex < _visibleCellsIndexes.Length; ++cellIndex)
            {
                var cellValue = _cells[cellIndex];
                var otherCellsIndexes = _visibleCellsIndexes[cellIndex];
                if ((cellValue & _IS_DETERMINED) != 0)
                {
                    cellValue &= unchecked((ushort)~_IS_DETERMINED);
                    for (var index = 0; index < otherCellsIndexes.Length; ++index)
                    {
                        var otherCellValue = _cells[otherCellsIndexes[index]];
                        System.Diagnostics.Debug.Assert((otherCellValue & _IS_DETERMINED) != 0 || (otherCellValue & (1u << cellValue)) == 0);
                    }
                }
            }
#endif
        }

        private static uint[] PackCore(ushort[] source)
        {
            const int SOURCE_BIT_LENGTH = 10;
            const int DESTINATION_BIT_LENGTH = 32;
            var buffer = new uint[26];
            var destinationIndex = 0;
            var shiftCount = 0;
            for (var sourceIndex = 0; sourceIndex < source.Length; ++sourceIndex)
            {
                buffer[destinationIndex] |= (uint)(source[sourceIndex] << shiftCount);
                shiftCount += SOURCE_BIT_LENGTH;
                if (shiftCount >= DESTINATION_BIT_LENGTH)
                {
                    shiftCount -= DESTINATION_BIT_LENGTH;
                    ++destinationIndex;
                    if (shiftCount > 0)
                        buffer[destinationIndex] = (uint)(source[sourceIndex] >> (SOURCE_BIT_LENGTH - shiftCount));
                }
            }

            return buffer;
        }

        private static ushort[] UnpackCore(uint[] source)
        {
            const int SOURCE_BIT_LENGTH = 32;
            const int DESTINATION_BIT_LENGTH = 10;
            var buffer = new ushort[81];
            var sourceIndex = 0;
            var shiftCount = 0;
            for (var destinationIndex = 0; destinationIndex < buffer.Length; ++destinationIndex)
            {
                buffer[destinationIndex] |= (ushort)(source[destinationIndex] >> shiftCount);
                shiftCount += DESTINATION_BIT_LENGTH;
                if (shiftCount >= SOURCE_BIT_LENGTH)
                {
                    shiftCount -= SOURCE_BIT_LENGTH;
                    ++sourceIndex;
                    if (shiftCount > 0)
                        buffer[sourceIndex] = (ushort)(source[destinationIndex] >> (SOURCE_BIT_LENGTH - shiftCount));
                }
            }

            return buffer;
        }
    }
}
