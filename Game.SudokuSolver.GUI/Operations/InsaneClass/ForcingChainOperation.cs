using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.InsaneClass
{
    /// <summary>
    /// 強制チェーンパターンのオペレーションのクラスです。
    /// </summary>
    internal class ForcingChainOperation
        : SudokuBoardOperation
    {
        private ForcingChainOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, string description)
            : base("強制チェーン", DifficultyLevel.Insane, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, ReadOnlyMemory<BoardCellPosition>.Empty, description)
        {
        }

        public static IEnumerable<ForcingChainOperation> MatchPattern(BoardWorkspace ws)
        {
            // 【論拠】
            // なし。
            //
            // (敢えて述べるなら)
            // ある未確定セル C とある数字 D に対して、
            // C の数字の候補に D が含まれている場合
            // => C の数字は仮に D と仮定して盤面の解決を試み、もし失敗した場合は C の数字は D ではないと判明する。

            var undterminedBitSet = ~ws.DeterminedCellBitSet & BoardWorkspace.AllCellIndexBitSet;
#if DEBUG
            System.Diagnostics.Debug.Assert(undterminedBitSet != 0);
#endif
            var foundCellIndex = undterminedBitSet.GetOneCellIndex();
            var foundCellMask = foundCellIndex.ToCellIndexBitSet();
            for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);
                if ((cellNoteBitSet & foundCellMask) != 0)
                {
                    // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                    var removedCellsNoteBitSet =
                        ~ws.DeterminedCellBitSet
                        & cellNoteBitSet
                        & BoardCellGeometry.GetVisibleCellsBitSetByCellIndex(foundCellIndex)
                        & ~foundCellMask;

                    var (determinedCellColumn, determinedCellRow, _) = BoardCellGeometry.GetColumnRowBlock(foundCellIndex);
                    var removedCellNotes = removedCellsNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                    var determinedCellDigitText = cellDigit.ToCellDigitChar();
                    var description =
                        $"{new BoardCellPosition(determinedCellColumn, determinedCellRow).ToFriendlyString()}のセルの数字を '{determinedCellDigitText}' と仮定します。" +
                        $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}";
                    yield return
                        new ForcingChainOperation(
                            new BoardCell(determinedCellColumn, determinedCellRow, cellDigit),
                            removedCellNotes,
                            description);
                }
            }
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
