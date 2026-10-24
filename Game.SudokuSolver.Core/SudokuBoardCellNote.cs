using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの数字の候補の表示方法を示す構造体です。
    /// </summary>
    public readonly struct SudokuBoardCellNote
    {
        internal SudokuBoardCellNote(SudokuBoardCellNoteEffect effect)
        {
            Effect = effect;
        }

        /// <summary>
        /// セルの数字の候補を表示する際のエフェクトの方法を取得します。
        /// </summary>
        /// <value>
        /// 数字の候補を表示する際のエフェクトの方法を示す <see cref="SudokuBoardCellNoteEffect"/> 値です。
        /// </value>
        public SudokuBoardCellNoteEffect Effect { get; }
    }
}
