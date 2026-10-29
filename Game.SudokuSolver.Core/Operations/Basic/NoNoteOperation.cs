using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.Basic
{
    /// <summary>
    /// 数字の候補が存在しないセルが存在するパターン(失敗)のオペレーションのクラスです。
    /// </summary>
    internal sealed class NoNoteOperation
        : SudokuBoardOperation
    {
        private NoNoteOperation(
            IEnumerable<SudokuBoardCellIndex> noNoteCellIndexes,
            string description)
            : base(
                  GameBoardStatus.Failed,
                  "数字の候補が存在しない",
                  SudokuBoardDifficultyLevel.Basic,
                  null,
                  [],
                  noNoteCellIndexes.Select(cellIndex => (cellIndex, SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                  [],
                  description)
        {
        }

        public static SudokuBoardOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // ある任意のセルにおいて、数字の候補が存在しない場合
            // => その探索は失敗する。

            var notNoteCellIndexBitSet = ws.GetNoNoteCellIndexBitSet();
            if (notNoteCellIndexBitSet == 0)
                return null;

            var notNoteCellIndexes = notNoteCellIndexBitSet.EnumerateAsCellIndexes();
            var cellPositionListText = string.Join(", ", notNoteCellIndexes.ToCellPositionTexts());
            var description =
                $"{cellPositionListText}のセルの数字の候補が存在しないため、" +
                $"解答は存在しません。";
            return new NoNoteOperation(notNoteCellIndexes, description);
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cells.GetNoNoteCellIndexBitSet() != 0);
#endif
            return cells.Clone();
        }
    }
}
