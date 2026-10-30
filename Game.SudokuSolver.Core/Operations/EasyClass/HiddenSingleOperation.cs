using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// 隠れシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenSingleOperation
        : SudokuBoardDetermineCellOperation
    {
        private HiddenSingleOperation(
            SudokuBoardCellPositionDigit determinedCell,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "隠れシングル (Hidden Single)",
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
            // あるハウス H と H 内のセル C とある数字 D に対して、
            // H 内の C の数字の候補に D が含まれており、かつ C 以外のすべてのセルの数字の候補に D が含まれていない場合
            // => C の数字は D で確定する。

            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; ++cellDigit)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);

                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Block,
                            SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                            cellDigit,
                            cellNoteBitSet);
                    if (operation is not null)
                        yield return operation;
                }

                for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Column,
                            SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                            cellDigit,
                            cellNoteBitSet);
                    if (operation is not null)
                        yield return operation;
                }

                for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Row,
                            SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                            cellDigit,
                            cellNoteBitSet);
                    if (operation is not null)
                        yield return operation;
                }
            }

            static HiddenSingleOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, SudokuBoardHouseType house, UInt128 houseBitMask, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var foundBitSet = cellNoteBitSet & houseBitMask;
                if (UInt128.PopCount(foundBitSet) != 1)
                    return null;
                var determinedCellIndex = foundBitSet.GetOneCellIndex();
                var excludedCellNotes =
                    ws.GetCellIndexBitSetForExcludedNoteDigit(determinedCellIndex, cellDigit)
                    .EnumerateAsCellNotes(cellDigit)
                    .ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                var determinedCellPosiionText = determinedCellIndex.ToFriendlyString();
                var determinedCellDigitText = cellDigit.ToCellDigitText();

                var description =
                    house switch
                    {
                        SudokuBoardHouseType.Row =>
                            $"{determinedCellRow.ToFriendlyString()}のマスのうち {determinedCellDigitText} であり得るマスが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のマスの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したマスと同じハウスに属する別のマス ({string.Join(", ", excludedCellNotes.Select(cellNote => new SudokuBoardCellPosition(cellNote.CellIndex).ToFriendlyString()))}) の数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        SudokuBoardHouseType.Column =>
                            $"{determinedCellColumn.ToFriendlyString()}のマスのうち {determinedCellDigitText} であり得るマスが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のマスの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したマスと同じハウスに属する別のマス ({string.Join(", ", excludedCellNotes.Select(cellNote => new SudokuBoardCellPosition(cellNote.CellIndex).ToFriendlyString()))}) の数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        SudokuBoardHouseType.Block =>
                            $"{determinedCellBlock.ToFriendlyString()}のマスが属しているブロックのうち {determinedCellDigitText} であり得るマスが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のマスの数字は {determinedCellDigitText} に確定します。\n" +
                            $"{(excludedCellNotes.Length > 0 ? $"それとともに、確定したマスと同じハウスに属する別のマス ({string.Join(", ", excludedCellNotes.Select(cellNote => new SudokuBoardCellPosition(cellNote.CellIndex).ToFriendlyString()))}) の数字の候補から {determinedCellDigitText} を排除します。" : "")}",
                        _ => throw new ArgumentOutOfRangeException(nameof(house)),
                    };

#if DEBUG
                if (!ws.CanCetermineCell(determinedCellIndex, cellDigit))
                    throw new ApplicationException();
                System.Diagnostics.Debug.Assert(ws.CanCetermineCell(determinedCellIndex, cellDigit));
#endif
                return
                    new HiddenSingleOperation(
                        new SudokuBoardCellPositionDigit(determinedCellIndex, cellDigit),
                        excludedCellNotes,
                        houseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                            .ConcatIf(excludedCellNotes.Length > 0, (SudokuBoardCellGeometry.GetVisibleCellsBitSet(determinedCellIndex) & ~houseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        description);
            }
        }
    }
}
