using System.Collections.Generic;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardCellPositionDigitExtensions
    {
        /// <summary>
        /// 指定されたセルの数字の候補のシーケンスとセルの文字の候補の背景表示効果から、セルの数字の候補と背景表示効果のペアのシーケンスを取得します。
        /// </summary>
        /// <param name="cellNotes">
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスです。
        /// </param>
        /// <param name="effect">
        /// セルの数字の候補の背景表示効果です。
        /// </param>
        /// <returns>
        /// セルの数字の候補と背景表示効果のペアのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> ToCellNoteEffects(this IEnumerable<SudokuBoardCellPositionDigit> cellNotes, SudokuBoardCellNoteEffect effect)
        {
            foreach (var cellNote in cellNotes)
                yield return (cellNote, effect);
        }
    }
}
