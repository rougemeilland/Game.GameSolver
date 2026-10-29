using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// ネイキッドシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedSingleOperation
        : SudokuBoardDetermineCellOperation
    {
        private NakedSingleOperation(
            SudokuBoardCellPositionDigit determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "一択セル",
                  SudokuBoardDifficultyLevel.Easy,
                  determinedCell,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
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
                        var excludedCellNotes =
                            ws.GetCellIndexBitSetForExcludedNoteDigit(cellIndex, cellDigit)
                            .EnumerateAsCellNotes(cellDigit)
                            .ToArray();
                        var determinedCellPositionText = cellIndex.ToFriendlyString();
                        var determinedCellDigitText = cellDigit.ToCellDigitChar();
                        var description =
                            $"{determinedCellPositionText}のセルの数字の候補は '{determinedCellDigitText}' しかないため、" +
                            $"{determinedCellPositionText}の数字は '{determinedCellDigitText}' に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を排除します。" : "")}";
#if DEBUG
                        System.Diagnostics.Debug.Assert(ws.CanCetermineCell(cellIndex, cellDigit));
#endif
                        yield return
                            new NakedSingleOperation(
                                new SudokuBoardCellPositionDigit(cellIndex, cellDigit),
                                excludedCellNotes,
                                Enumerable.Empty<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)>()
                                    .ConcatIf(excludedCellNotes.Length > 0, SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                description);
                    }
                }
            }
        }
    }
}
