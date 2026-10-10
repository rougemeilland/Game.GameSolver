using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.GUI
{
    internal partial class BoardCellsSet
    {
        private const ushort _IS_DETERMINED = 0x0200;
        private const ushort _ALL_NOTE_BIT_MASK = 0x01ff;
        private const ushort _ALL_BITS_MASK = _IS_DETERMINED | _ALL_NOTE_BIT_MASK;

        private readonly ushort[] _cells = new ushort[81];

        public BoardCellsSet(char[] cells)
        {
            ArgumentNullException.ThrowIfNull(cells);
            if (cells.Length != _cells.Length)
                throw new ArgumentException("The size of the cell array must be 81.", nameof(cells));

#if DEBUG
            var returned = true;
            try
            {
#endif
                for (var cellIndex = BoardCellIndex.MinValue; cellIndex <= BoardCellIndex.MaxValue; ++cellIndex)
                {
                    var cellDigitChar = cells[cellIndex - BoardCellIndex.FirstValue];
                    if (cellDigitChar is not ' ' and not '\0')
                        throw new ApplicationException();

                    if (cellDigitChar is >= '0' and <= '9')
                    {
                        if (!DetermineCellCore(cellIndex, cellDigitChar.ToCellDigit()))
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
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの値が確定されていれば <see langword="true"、そうではないのなら <see langword="false"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool IsDetermined(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
#endif
            return (_cells[cellIndex - BoardCellIndex.FirstValue] & _IS_DETERMINED) != 0;
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
        public bool IsDetermined(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
#endif

            return (_cells[BoardCellGeometry.GetCellIndex(column, row) - BoardCellIndex.FirstValue] & _IS_DETERMINED) != 0;
        }

        /// <summary>
        /// 値が確定している指定されたセルの値を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの値を示す <see cref="ushort"/> 値を返します。この値は 1 から 9 の整数です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetDeterminedCellDigit(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            System.Diagnostics.Debug.Assert(IsDetermined(cellIndex));
#endif
            return (ushort)(_cells[cellIndex - BoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 値が確定している指定されたセルの値を取得します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <value>
        /// 指定されたセルの値を示す <see cref="ushort"/> 値を返します。この値は 1 から 9 の整数です。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetDeterminedCellDigit(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            System.Diagnostics.Debug.Assert(IsDetermined(column, row));
#endif
            return (ushort)(_cells[BoardCellGeometry.GetCellIndex(column, row) - BoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 値が確定していない指定されたセルの数字の候補を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの番号です。列は 0 から 80 までの整数です。
        /// </param>
        /// <value>
        /// 指定されたセルの候補を示す <see cref="ushort"/> 値を返します。
        /// この値は 9 ビットのビット列で、0 ビットが 数字 1、8 ビットが 数字 9 を意味し、各ビットが 1 の場合に対応する数字が候補に含まれます。
        /// </value>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetNoteBits(BoardCellIndex cellIndex)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            System.Diagnostics.Debug.Assert(!IsDetermined(cellIndex));
#endif
            return (ushort)(_cells[cellIndex - BoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ushort GetNoteBits(BoardCellColumn column, BoardCellRow row)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            System.Diagnostics.Debug.Assert(IsDetermined(column, row) == false);
#endif
            return (ushort)(_cells[BoardCellGeometry.GetCellIndex(column, row) - BoardCellIndex.FirstValue] & ~_IS_DETERMINED);
        }

        /// <summary>
        /// 指定されたセルを指定された数字で確定します。
        /// </summary>
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
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
        public bool DetermineCell(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
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
        /// 指定されたセルを指定された数字で確定します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
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
        public bool DetermineCell(BoardCellColumn column, BoardCellRow row, BoardCellDigit digit)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
            var returned = true;
            try
            {
#endif
                return DetermineCellCore(BoardCellGeometry.GetCellIndex(column, row), digit);
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
        /// <param name="cellIndex">
        /// 指定されたセルの位置を示す <see cref="BoardCellIndex"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
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
        public bool RemoveNote(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
#if DEBUG
            if (cellIndex is < BoardCellIndex.MinValue or > BoardCellIndex.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(cellIndex));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return RemoveNoteCore(cellIndex, digit);
        }

        /// <summary>
        /// 指定されたセルのメモから指定された値を削除します。
        /// </summary>
        /// <param name="column">
        /// 指定されたセルの列を示す <see cref="BoardCellColumn"/> 値です。
        /// </param>
        /// <param name="row">
        /// 指定されたセルの行を示す <see cref="BoardCellRow"/> 値です。
        /// </param>
        /// <param name="digit">
        /// セルの数字を示す <see cref="BoardCellDigit"/> 値です。
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
        public bool RemoveNote(BoardCellColumn column, BoardCellRow row, BoardCellDigit digit)
        {
#if DEBUG
            if (column is < BoardCellColumn.MinValue or > BoardCellColumn.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(column));
            if (row is < BoardCellRow.MinValue or > BoardCellRow.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (digit is < BoardCellDigit.MinValue or > BoardCellDigit.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(digit));
#endif

            return RemoveNoteCore(BoardCellGeometry.GetCellIndex(column, row), digit);
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

        private bool DetermineCellCore(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
            if ((_cells[cellIndex - BoardCellIndex.FirstValue] & _IS_DETERMINED) != 0)
                return false;
            var determinedCellValue = (ushort)(_IS_DETERMINED | (ushort)digit);
            var cellNoteBitMask = (ushort)digit.ToBitMask();
            var otherCellsIndexes = _visibleCellsIndexes[cellIndex - BoardCellIndex.FirstValue];
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

            _cells[cellIndex - BoardCellIndex.FirstValue] = determinedCellValue;
            return true;
        }

        private bool RemoveNoteCore(BoardCellIndex cellIndex, BoardCellDigit digit)
        {
#if DEBUG
            var returned = true;
            try
            {
#endif
                var determinedCellValue = (ushort)(_IS_DETERMINED | (ushort)digit);

                // 指定されたセルまだ確定していないことの確認
                if ((_cells[cellIndex - BoardCellIndex.FirstValue] & _IS_DETERMINED) != 0)
                    return false;

                var targetDigitMask = digit.ToBitMask();

                // 削除対象の数字がメモに存在することの確認
                if ((_cells[cellIndex - BoardCellIndex.FirstValue] & targetDigitMask) == 0)
                    return false;

                // メモから数字を削除
                _cells[cellIndex - BoardCellIndex.FirstValue] &= (ushort)~targetDigitMask;

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
    }
}
