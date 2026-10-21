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
        private ForcingChainOperation(SudokuBoardCellPositionDigit? determinedCell, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, string description)
            : base(GameBoardStatus.Solving, "強制チェーン", SudokuBoardDifficultyLevel.Insane, determinedCell, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty, removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, description)
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
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);
                if ((cellNoteBitSet & foundCellMask) != 0)
                {
                    // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellsNoteBitSet =
                        ~ws.DeterminedCellBitSet
                        & cellNoteBitSet
                        & SudokuBoardCellGeometry.GetVisibleCellsBitSetByCellIndex(foundCellIndex)
                        & ~foundCellMask;

                    var removedCellNotes = removedCellsNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                    var determinedCellDigitText = cellDigit.ToCellDigitChar();
                    var description =
                        $"{new SudokuBoardCellPosition(foundCellIndex).ToFriendlyString()}のセルの数字を '{determinedCellDigitText}' と仮定します。\n" +
                        $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
                    yield return
                        new ForcingChainOperation(
                            new SudokuBoardCellPositionDigit(foundCellIndex, cellDigit),
                            removedCellNotes,
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
