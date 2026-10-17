using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedSingleOperation
        : SudokuBoardOperation
    {
        private NakedSingleOperation(SudokuBoardCellPositionDigit? determinedCell, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, string description)
            : base("一択セル", SudokuBoardDifficultyLevel.Easy, determinedCell, ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty, removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, description)
        {
        }

        public static NakedSingleOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるセル C とある数字 D に対して、
            // C の数字の候補が D ただ一つである場合
            // => C の数字は D で確定する。

            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellIndex);
                if (!ws.IsDetermined(cellIndex) && cellNoteBitSet.IsSingleBit())
                {
                    var cellDigit = cellNoteBitSet.GetOneCellNoteDigit();

                    // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellNoteBitSet =
                        ~ws.DeterminedCellBitSet
                        & ws.GetNoteBitSet(cellDigit)
                        & SudokuBoardCellGeometry.GetVisibleCellsBitSetByCellIndex(cellIndex)
                        & ~cellIndex.ToCellIndexBitSet();

                    var removedCellNotes = removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                    var determinedCellPositionText = new SudokuBoardCellPosition(cellIndex).ToFriendlyString();
                    var determinedCellDigitText = cellDigit.ToCellDigitChar();
                    var description =
                        $"{determinedCellPositionText}のセルの数字の候補は '{determinedCellDigitText}' しかないため、" +
                        $"{determinedCellPositionText}の数字は '{determinedCellDigitText}' に確定します。" +
                        $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
                    return
                        new NakedSingleOperation(
                            new SudokuBoardCellPositionDigit(cellIndex, cellDigit),
                            removedCellNotes,
                            description);
                }
            }

            return null;
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.Column, DeterminedCell.Value.Row, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
