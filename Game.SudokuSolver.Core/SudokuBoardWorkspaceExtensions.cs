using System;
using System.Collections.Generic;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardWorkspaceExtensions
    {
        /// <summary>
        /// 指定されたセル位置のビット集合と数字のマスクからセルの文字の数字の候補を抽出して、セルの数字の候補の表示効果とのペアのシーケンスを取得します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellNoteBitMask">
        /// セルの数字の候補を抽出するためのビットマスクを示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <param name="effect"></param>
        /// セルの数字の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値です。
        /// <returns>
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値とセルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> のペアのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> EnumerateAsCellNoteEffects(this SudokuBoardWorkspace ws, UInt128 cellIndexBitSet, SudokuBoardCellDigitBitSet cellNoteBitMask, SudokuBoardCellNoteEffect effect)
        {
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(!ws.IsDetermined(cellIndex));
#endif
                var cellNoteDigitBitSet = ws.GetCellNoteDigitBitSet(cellIndex) & cellNoteBitMask;
                while (cellNoteDigitBitSet != 0)
                {
                    var cellNoteDigit = cellNoteDigitBitSet.GetOneCellDigit();
                    yield return (new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit), effect);

                    cellNoteDigitBitSet &= ~cellNoteDigit.ToCellDigitBitSet();
                }

                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }
    }
}
