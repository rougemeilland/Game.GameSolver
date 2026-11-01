using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class CharExtensions
    {
        /// <summary>
        /// セルの数字を示す <see cref="char"/> 値を <see cref="SudokuBoardCellDigit"/> 値に変換します。
        /// </summary>
        /// <param name="digitChar">
        /// セルの数字を示す <see cref="char"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字を示す <see cref="SudokuBoardCellDigit"/> を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SudokuBoardCellDigit ToCellDigit(this char digitChar)
        {
#if DEBUG
            if (digitChar is < '1' or > '9')
                throw new ArgumentOutOfRangeException(nameof(digitChar));
#endif

            return SudokuBoardCellDigit.One + (digitChar - '1');
        }
    }
}
