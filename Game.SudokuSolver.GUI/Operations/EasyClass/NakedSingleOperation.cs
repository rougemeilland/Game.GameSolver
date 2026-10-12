using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal class NakedSingleOperation
        : SudokuBoardOperation
    {
        private NakedSingleOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, string description)
            : base("一択セル", DifficultyLevel.Easy, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, ReadOnlyMemory<BoardCellPosition>.Empty, description)
        {
        }

        public static NakedSingleOperation? MatchPattern(BoardWorkspace ws)
        {
            // 【論拠】
            // あるセル C とある数字 D に対して、
            // C の数字の候補が D ただ一つである場合
            // => C の数字は D で確定する。

            for (var cellIndex = BoardCellIndex.MinValue; cellIndex <= BoardCellIndex.MaxValue; ++cellIndex)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellIndex);
                if (!ws.IsDetermined(cellIndex) && cellNoteBitSet.IsSingleBit())
                {
                    var cellDigit = cellNoteBitSet.GetOneCellNoteDigit();

                    // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellNoteBitSet =
                        ~ws.DeterminedCellBitSet
                        & ws.GetNoteBitSet(cellDigit)
                        & BoardCellGeometry.GetVisibleCellsBitSetByCellIndex(cellIndex)
                        & ~cellIndex.ToCellIndexBitSet();

                    var removedCellNotes = removedCellNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                    var (determinedCellColumn, determinedCellRow, _) = BoardCellGeometry.GetColumnRowBlock(cellIndex);
                    var determinedCellPositionText = new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString();
                    var determinedCellDigitText = cellDigit.ToCellDigitChar();
                    var description =
                        $"{determinedCellPositionText}のセルの数字の候補は '{determinedCellDigitText}' しかないため、" +
                        $"{determinedCellPositionText}の数字は '{determinedCellDigitText}' に確定します。" +
                        $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
                    return
                        new NakedSingleOperation(
                            new BoardCell(determinedCellColumn, determinedCellRow, cellDigit),
                            removedCellNotes,
                            description);
                }
            }

            return null;
        }

        public override BoardCellsSet Execute(BoardCellsSet cells)
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
