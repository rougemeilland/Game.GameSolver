using System;
using System.Collections.Generic;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面に適用するセルの数字を確定する定石の抽象クラスです。
    /// </summary>
    internal abstract class SudokuBoardDetermineCellOperation
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
        /// <param name="determinedCell">
        /// 定石を適用した結果数字が確定したセルが存在する場合はそのセルの位置と数字を示す <see cref="SudokuBoardCellPositionDigit"/> 値です。
        /// </param>
        /// <param name="excludedCellNotes">
        /// 定石を適用した結果排除されるセルの数字の候補とそのセルの位置を示す <see cref="SudokuBoardCellPositionDigit"/> 値のシーケンスです。
        /// </param>
        /// <param name="cellBackgroundEffects">
        /// 定石の説明において背景効果を必要とするセルの位置を <see cref="SudokuBoardCellIndex"/> 値とその背景効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値のペアのシーケンスです。
        /// </param>
        /// <param name="description">
        /// 定石の説明のテキストを示す <see cref="string"/>オブジェクトです。
        /// </param>
        protected SudokuBoardDetermineCellOperation(
            GameBoardStatus status,
            string title,
            SudokuBoardDifficultyLevel difficulty,
            SudokuBoardCellPositionDigit determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            string description)
            : base(status,
                   title,
                   difficulty,
                   determinedCell,
                   excludedCellNotes,
                   cellBackgroundEffects,
                   [],
                   description)
        {
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.CellIndex, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
