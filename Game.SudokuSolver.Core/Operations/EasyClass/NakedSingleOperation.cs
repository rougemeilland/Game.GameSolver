using System;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedSingleOperation
        : SudokuBoardOperation
    {
        private NakedSingleOperation(
            SudokuBoardCellPositionDigit? determinedCell,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "一択セル",
                  SudokuBoardDifficultyLevel.Easy,
                  determinedCell,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  removedCellNotes,
                  UInt128.Zero,
                  negativelyRelatedCellIndexBitSet,
                  description)
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
                if (!ws.IsDetermined(cellIndex))
                {
                    var cellNoteBitSet = ws.GetNoteBitSet(cellIndex);
                    if (!ws.IsDetermined(cellIndex) && cellNoteBitSet.IsSingleBit())
                    {
                        var cellDigit = cellNoteBitSet.GetOneCellNoteDigit();
                        var removedCellNotes =
                            ws.GetCellIndexBitSetForRemovedNoteDigit(cellIndex, cellDigit)
                            .EnumerateAsCellIndexes()
                            .ToCells(cellDigit)
                            .ToArray();
                        var determinedCellPositionText = new SudokuBoardCellPosition(cellIndex).ToFriendlyString();
                        var determinedCellDigitText = cellDigit.ToCellDigitChar();
                        var description =
                            $"{determinedCellPositionText}のセルの数字の候補は '{determinedCellDigitText}' しかないため、" +
                            $"{determinedCellPositionText}の数字は '{determinedCellDigitText}' に確定します。\n" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
#if DEBUG
                        System.Diagnostics.Debug.Assert(!ws.IsDetermined(cellIndex));
#endif
                        return
                            new NakedSingleOperation(
                                new SudokuBoardCellPositionDigit(cellIndex, cellDigit),
                                removedCellNotes,
                                removedCellNotes.Length > 0 ? SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex) : UInt128.Zero,
                                description);
                    }
                }
            }

            return null;
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
