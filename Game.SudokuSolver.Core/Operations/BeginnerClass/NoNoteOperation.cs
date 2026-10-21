using System;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.BeginnerClass
{
    /// <summary>
    /// 数字の候補が存在しないセルが存在するパターン(失敗)のオペレーションのクラスです。
    /// </summary>
    internal sealed class NoNoteOperation
        : SudokuBoardOperation
    {
        private NoNoteOperation(ReadOnlyMemory<SudokuBoardCellPosition> noNoteCells, string description)
            : base(GameBoardStatus.Failed, "数字の候補が存在しない", SudokuBoardDifficultyLevel.Beginner, null, noNoteCells, ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty, ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, description)
        {
        }

        public static NoNoteOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // ある任意のセルにおいて、数字の候補が存在しない場合
            // => その探索は失敗する。

            var notNoteCellIndexBitSet = ws.GetNoNoteCellIndexBitSet();
            if (notNoteCellIndexBitSet == 0)
                return null;

            var cellPositionList = notNoteCellIndexBitSet.EnumerateAsCellIndexes().Select(cellIndex => new SudokuBoardCellPosition(cellIndex)).ToArray();
            var cellPositionListText = string.Join(", ", cellPositionList.Select(cellPosition => cellPosition.ToFriendlyString()));
            var description =
                $"{cellPositionListText}のセルの数字の候補が存在しないため、" +
                $"解答は存在しません。";
            return new NoNoteOperation(cellPositionList, description);
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
