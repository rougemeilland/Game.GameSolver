using System;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.BeginnerClass
{
    /// <summary>
    /// 数独の盤面が完了するパターン(成功)のオペレーションのクラスです。
    /// </summary>
    internal sealed class CompleteOperation
        : SudokuBoardOperation
    {
        private CompleteOperation(string description)
            : base(
                  GameBoardStatus.Success,
                  "完了",
                  SudokuBoardDifficultyLevel.Beginner,
                  null,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty,
                  UInt128.Zero,
                  UInt128.Zero,
                  description)
        {
        }

        public static CompleteOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // すべてのセルにおいて、セルの数字が確定している。
            // => その探索は完了する。

            if (!ws.IsCompleted())
                return null;

            var description = "盤面のすべてのセルが確定しました。";
            return new CompleteOperation(description);
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
#if DEBUG
            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
            {
                if (!cells.IsDetermined(cellIndex))
                    throw new ApplicationException();

            }
#endif
            return cells.Clone();
        }
    }
}
