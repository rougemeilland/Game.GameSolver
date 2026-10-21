using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.InsaneClass
{
    /// <summary>
    /// 強制チェーンパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class ForcingChainOperation
        : SudokuBoardOperation
    {
        private ForcingChainOperation(
            SudokuBoardCellPositionDigit? determinedCell,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "強制チェーン",
                  SudokuBoardDifficultyLevel.Insane,
                  determinedCell,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  removedCellNotes,
                  UInt128.Zero,
                  negativelyRelatedCellIndexBitSet,
                  description)
        {
        }

        public static IEnumerable<ForcingChainOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // なし。
            //
            // (敢えて述べるなら)
            // ある未確定セル C とある数字 D に対して、
            // C の数字の候補に D が含まれている場合
            // => C の数字は仮に D と仮定して盤面の解答を試み、もし失敗した場合は C の数字は D ではないと判明する。

            var undterminedBitSet = ~ws.DeterminedCellBitSet & SudokuBoardWorkspace.AllCellIndexBitSet;
#if DEBUG
            System.Diagnostics.Debug.Assert(undterminedBitSet != 0);
#endif
            var foundCellIndex = undterminedBitSet.GetOneCellIndex();
            var foundCellMask = foundCellIndex.ToCellIndexBitSet();
            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; ++cellDigit)
            {
                if ((ws.GetNoteBitSet(cellDigit) & foundCellMask) != 0)
                {
                    var removedCellNotes =
                        ws.GetCellIndexBitSetForRemovedNoteDigit(foundCellIndex, cellDigit)
                        .EnumerateAsCellIndexes()
                        .ToCells(cellDigit)
                        .ToArray();
                    var determinedCellDigitText = cellDigit.ToCellDigitChar();
                    var description =
                        $"{new SudokuBoardCellPosition(foundCellIndex).ToFriendlyString()}のセルの数字を '{determinedCellDigitText}' と仮定します。\n" +
                        $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
#if DEBUG
                    System.Diagnostics.Debug.Assert(!ws.IsDetermined(foundCellIndex));
#endif
                    yield return
                        new ForcingChainOperation(
                            new SudokuBoardCellPositionDigit(foundCellIndex, cellDigit),
                            removedCellNotes,
                            removedCellNotes.Length > 0 ? SudokuBoardCellGeometry.GetVisibleCellsBitSet(foundCellIndex) : UInt128.Zero,
                            description);
                }
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
