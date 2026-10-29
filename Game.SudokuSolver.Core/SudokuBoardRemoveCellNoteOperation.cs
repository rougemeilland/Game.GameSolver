using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面に適用するセルの数字の候補を排除する定石の抽象クラスです。
    /// </summary>
    internal abstract class SudokuBoardRemoveCellNoteOperation
        : SudokuBoardOperation
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="status">
        /// 定石を適用した結果の盤面の状態を示す <see cref="GameBoardStatus"/> 値です。
        /// </param>
        /// <param name="title">
        /// 定石の名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="difficulty">
        /// 定石の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </param>
        /// <param name="excludedCellNotes">
        /// 定石を適用した結果排除されるセルの数字の候補とそのセルの位置を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスです。
        /// </param>
        /// <param name="cellBackgroundEffects">
        /// 定石の説明において背景効果を必要とするセルの位置を <see cref="SudokuBoardCellIndex"/> 値とその背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値のペアのシーケンスです。
        /// </param>
        /// <param name="cellNoteEffects">
        /// 定石の説明において強調効果を必要とするセルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値とその強調効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値のペアのシーケンスです。
        /// </param>
        /// <param name="description">
        /// 定石の説明のテキストを示す <see cref="string"/>オブジェクトです。
        /// </param>
        protected SudokuBoardRemoveCellNoteOperation(
            GameBoardStatus status,
            string title,
            SudokuBoardDifficultyLevel difficulty,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(status,
                   title,
                   difficulty,
                   null,
                   excludedCellNotes,
                   cellBackgroundEffects,
                   cellNoteEffects
#if DEBUG
                    .Select(item =>
                    {
                        switch (item.effect)
                        {
                            case SudokuBoardCellNoteEffect.PositivelyHilightedType3:
                            case SudokuBoardCellNoteEffect.PositivelyHilightedType2:
                            case SudokuBoardCellNoteEffect.PositivelyHilightedType1:
                                break;
                            default:
                                throw new ApplicationException();
                        }

                        return item;
                    })
#endif
                    .Concat(excludedCellNotes.Select(cellNote => (cellNote, SudokuBoardCellNoteEffect.NegativelyHighlighted))),
                   description)
        {
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is not null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            for (var index = 0; index < ExcludedCellNotes.Length; ++index)
            {
                var excludedCellNote = ExcludedCellNotes[index];
                if (!newCellSet.RemoveNote(excludedCellNote.CellIndex, excludedCellNote.Digit))
                    throw new ApplicationException();
            }

            return newCellSet;
        }
    }
}
