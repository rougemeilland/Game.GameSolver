using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellPositionExtensions
    {
        /// <summary>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="position">
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値です。
        /// </param>
        /// <returns>
        /// セルの位置を示す <see cref="string"/> オブジェクトを返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this SudokuBoardCellPosition position)
            => (position.Column, position.Row).ToFriendlyString();
    }
}
