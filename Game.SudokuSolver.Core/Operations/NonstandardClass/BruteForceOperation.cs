using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.NonstandardClass
{
    /// <summary>
    /// 総当たりパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class BruteForceOperation
        : SudokuBoardOperation
    {
        private BruteForceOperation(
            SudokuBoardCellPositionDigit? determinedCell,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "総当たり",
                  SudokuBoardDifficultyLevel.Nonstandard,
                  determinedCell,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  removedCellNotes,
                  UInt128.Zero,
                  UInt128.Zero,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // ある C に対して、
            // C の数字の候補の全てに対して論証を試みる。
            // このパターンは、強制チェーンの手法すら適用できないケース、特に複数の解答が存在するケースを想定している。

            // 全ての未確定セルとそのセルの数字の候補を洗い出して配列化する。
            // 配列の順番は、その数字の候補が確定することによって否定される数字の候補の数が多いものを優先とする。
            // ※ removedCellNoteCount には確定される数字の候補自身も含まれているが、removedCellNoteCount 同士の比較にしか使用されないため、特に除外はしていない。
            var targetItems =
                (~ws.DeterminedCellBitSet & SudokuBoardWorkspace.AllCellIndexBitSet)
                .EnumerateAsCellIndexes()
                .Select(cellIndex => (cellIndex, visibleCellsBitSet: SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex)))
                .SelectMany(cell => ws.GetNoteBitSet(cell.cellIndex).EnumerateAsCellDigits(), (cell, cellNoteDigit) => (cell.cellIndex, cellNoteDigit, removedCellNoteCount: (int)UInt128.PopCount(ws.GetNoteBitSet(cellNoteDigit) & cell.visibleCellsBitSet)))
                .OrderByDescending(item => item.removedCellNoteCount)
                .ThenBy(item => item.cellIndex)
                .ThenBy(item => item.cellNoteDigit)
                .Select(item => (item.cellIndex, item.cellNoteDigit));
            foreach (var (cellIndex, cellNoteDigit) in targetItems)
            {
                var removedCellNotes =
                    ws.GetCellIndexBitSetForRemovedNoteDigit(cellIndex, cellNoteDigit)
                    .EnumerateAsCellIndexes()
                    .ToCells(cellNoteDigit)
                    .ToArray();
                var determinedCellPosiionText = cellIndex.ToFriendlyString();
                var undeterminedCellDigitTexts = string.Join(", ", ws.GetNoteBitSet(cellIndex).EnumerateAsCellDigits().Select(noteOnCell => $"'{noteOnCell.ToCellDigitChar()}'"));
                var determinedCellDigitText = cellNoteDigit.ToCellDigitChar();
                var description =
                    $"適用できる定石がないため、{determinedCellPosiionText}のセルの数字の候補 {undeterminedCellDigitTexts} のうち、" +
                    $"'{determinedCellDigitText}' を仮に確定します。";
#if DEBUG
                System.Diagnostics.Debug.Assert(ws.CanCetermineCell(cellIndex, cellNoteDigit));
#endif
                yield return
                    new BruteForceOperation(
                        new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit),
                        removedCellNotes,
                        description);

            }
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
