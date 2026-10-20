using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの数字および数字の候補の状態を示すクラスです。
    /// </summary>
    internal sealed partial class SudokuBoardCellsSet
    {
        private const int _CELL_COUNT = SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1;
        private const ushort _IS_DETERMINED = 1 << 9;
        private const ushort _ALL_NOTE_BIT_MASK = (1 << 9) - 1;
        private const ushort _ALL_BIT_MASK = _IS_DETERMINED | _ALL_NOTE_BIT_MASK;

        private readonly ushort[] _cells = new ushort[_CELL_COUNT];

        public SudokuBoardCellsSet(ReadOnlySpan<string> cellDigits)
        {
            if (cellDigits.Length != _cells.Length)
                throw new ArgumentException($"The size of the cell array must be {_cells.Length}.", nameof(cellDigits));

            _cells.AsSpan().Fill(_ALL_NOTE_BIT_MASK);

#if DEBUG
            var returned = true;
            try
            {
#endif
                for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
                {
                    var cellDigitString = cellDigits[cellIndex - SudokuBoardCellIndex.FirstValue];
                    if (cellDigitString.Length > 1)
                        throw new ApplicationException();
                    if (cellDigitString.Length > 0)
                    {
                        if (cellDigitString[0] is < '1' or > '9')
                            throw new ApplicationException();
                        if (!DetermineCellCore(cellIndex, cellDigitString[0].ToCellDigit()))
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

        private SudokuBoardCellsSet(ushort[] cells)
        {
            ArgumentNullException.ThrowIfNull(cells);
            if (cells.Length != _CELL_COUNT)
                throw new ArgumentException($"The size of the cell array must be {_CELL_COUNT}.", nameof(cells));

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
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return (_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & _IS_DETERMINED) != 0;
        }

        /// <summary>
        /// 数字が確定している指定されたセルの値を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの値を示す <see cref="SudokuBoardCellDigit"/> 値を返します。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public SudokuBoardCellDigit GetDeterminedCellDigit(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            System.Diagnostics.Debug.Assert(IsDetermined(cellIndex));
#endif
            return SudokuBoardCellDigit.One + (_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 数字が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの番号です。列は 0 から 80 までの整数です。
        /// </param>
        /// <value>
        /// 指定されたセルの数字の候補の集合をを示す <see cref="SudokuBoardCellNoteBitSet"/> 値を返します。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public SudokuBoardCellNoteBitSet GetNoteBitSet(SudokuBoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            System.Diagnostics.Debug.Assert(!IsDetermined(cellIndex));
#endif
            return (SudokuBoardCellNoteBitSet)(_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 盤面に数字の候補が存在しないセルの位置を取得します。
        /// </summary>
        /// <returns>
        /// 数字の候補が存在しないセルの位置のビット集合である <see cref="UInt128"/> 値を返します。
        /// </returns>
        public UInt128 GetNoNoteCellIndexBitSet()
        {
            var result = UInt128.Zero;
            for (var index = 0; index < _cells.Length; ++index)
            {
                if (_cells[index] == 0)
                    result |= (SudokuBoardCellIndex.FirstValue + index).ToCellIndexBitSet();
            }

            return result;
        }

        /// <summary>
        /// 指定されたセルを指定された数字で確定します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
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
        public bool DetermineCell(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));

            var returned = true;
            try
            {
#endif
                return DetermineCellCore(cellIndex, digit);
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
        /// 指定されたセルの数字の候補から指定された数字を削除します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// 数字の候補の削除に成功した場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// <para>
        /// 以下のような場合に数字の候補の削除に失敗します。
        /// <list type="bullet">
        /// <item>
        /// 確定済みのセルから数字の候補の削除を試みた場合
        /// </item>
        /// <item>
        /// 候補にない数字の削除を試みた場合
        /// </item>
        /// </list>
        /// </para>
        /// </returns>
        public bool RemoveNote(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return RemoveNoteCore(cellIndex, digit);
        }

        /// <summary>
        /// 盤面の状態を 32bbit 配列にパックします。
        /// </summary>
        /// <returns>
        /// 盤面の状態がパックされた <see cref="uint"/> の配列です。
        /// </returns>
        /// <remarks>
        /// 配列中の有効なビットの長さは 810 ビット (10 * 81) です。
        /// これを <see cref="uint"/> にパックするため、パック先の配列のサイズは 26 で、最後の要素は 22 ビットだけ余ります。
        /// </remarks>
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
        /// 複製された <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </returns>
        public SudokuBoardCellsSet Clone() => new(_cells);

        private bool DetermineCellCore(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit digit)
        {
            if ((_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & _IS_DETERMINED) != 0)
                return false;
            var determinedCellElement = (ushort)(_IS_DETERMINED | (ushort)digit);
            var cellNoteBitMask = digit.ToCellNoteBitSet();
            var otherCellsIndexes = _visibleCellsIndexes[cellIndex - SudokuBoardCellIndex.FirstValue];
            for (var index = 0; index < otherCellsIndexes.Length; ++index)
            {
                var otherCellIndex = otherCellsIndexes[index];
                if ((_cells[otherCellIndex] & _IS_DETERMINED) != 0)
                {
                    if (_cells[otherCellIndex] == determinedCellElement)
                        return false;
                }
                else
                {
                    if ((_cells[otherCellIndex] & (ushort)cellNoteBitMask) != 0)
                        _cells[otherCellIndex] &= (ushort)~cellNoteBitMask;
                }
            }

            _cells[cellIndex - SudokuBoardCellIndex.FirstValue] = determinedCellElement;
            return true;
        }

        private bool RemoveNoteCore(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit digit)
        {
#if DEBUG
            var returned = true;
            try
            {
#endif
                var determinedCellValue = (ushort)(_IS_DETERMINED | (ushort)digit);

                // 指定されたセルがまだ確定していないことの確認
                if ((_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & _IS_DETERMINED) != 0)
                    return false;

                var targetDigitMask = digit.ToCellNoteBitSet();

                // 削除対象の数字が候補に存在することの確認
                if ((_cells[cellIndex - SudokuBoardCellIndex.FirstValue] & (ushort)targetDigitMask) == 0)
                    return false;

                // 数字の候補から数字を削除
                _cells[cellIndex - SudokuBoardCellIndex.FirstValue] &= (ushort)~targetDigitMask;

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
                        buffer[destinationIndex] |= (uint)(source[sourceIndex] >> (SOURCE_BIT_LENGTH - shiftCount));
                }
            }

            return buffer;
        }

        private static ushort[] UnpackCore(uint[] source)
        {
            const int SOURCE_BIT_LENGTH = 32;
            const int DESTINATION_BIT_LENGTH = 10;
            const ushort DESTINATION_BIT_MASK = (1 << DESTINATION_BIT_LENGTH) - 1;
            var buffer = new ushort[_CELL_COUNT];
            var sourceIndex = 0;
            var shiftCount = 0;
            for (var destinationIndex = 0; destinationIndex < buffer.Length; ++destinationIndex)
            {
                buffer[destinationIndex] |= (ushort)((source[sourceIndex] >> shiftCount) & DESTINATION_BIT_MASK);
                shiftCount += DESTINATION_BIT_LENGTH;
                if (shiftCount >= SOURCE_BIT_LENGTH)
                {
                    shiftCount -= SOURCE_BIT_LENGTH;
                    ++sourceIndex;
                    if (shiftCount > 0)
                        buffer[destinationIndex] |= (ushort)((source[sourceIndex] << (DESTINATION_BIT_LENGTH - shiftCount)) & DESTINATION_BIT_MASK);
                }
            }

            return buffer;
        }

        private void ValidateInternalState()
        {
#if DEBUG
            // 未使用ビットが 0 であることの確認
            for (var index = 9; index < _cells.Length; ++index)
                System.Diagnostics.Debug.Assert((_cells[index] & ~_ALL_BIT_MASK) == 0);

            // 確定している数字の範囲が正しいことの確認
            for (var index = 9; index < _cells.Length; ++index)
            {
                var digit = _cells[index];
                if ((digit & _IS_DETERMINED) != 0)
                {
                    System.Diagnostics.Debug.Assert((digit & ~_IS_DETERMINED) is >= 0 and < 9);
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

            // 各ハウスで確定済みのセルの数字が候補にないことの確認
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
    }
}
