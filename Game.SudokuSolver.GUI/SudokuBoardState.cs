using System;
using System.Collections.Generic;
using Game.GameSolver;
using Game.SudokuSolver.GUI.Operations.BeginnerClass;
using Game.SudokuSolver.GUI.Operations.EasyClass;
using Game.SudokuSolver.GUI.Operations.InsaneClass;
using Game.SudokuSolver.GUI.Operations.MediumClass;

namespace Game.SudokuSolver.GUI
{
    internal class SudokuBoardState
        : GameBoardState<SudokuBoardState, SudokuBoardOperation>
    {
        private readonly BoardCellsSet _cells;

        public SudokuBoardState(char[] initialCellDigits)
        {
            ArgumentNullException.ThrowIfNull(initialCellDigits);
            if (initialCellDigits.Length != 0)
                throw new ArgumentException("The size of the Sudoku cell array must be 81.", nameof(initialCellDigits));

            _cells = new BoardCellsSet(initialCellDigits);
        }

        public SudokuBoardState(SudokuBoardState state, SudokuBoardOperation operation)
            : base(state)
        {
            _cells = operation.Execute(state._cells);
        }

        protected override GameBoardStatus CheckStatus()
        {
            if (_cells.CheckIfSuccess())
                return GameBoardStatus.Success;
            else if (_cells.CheckIfFailed())
                return GameBoardStatus.Failed;
            else
                return GameBoardStatus.Solving;
        }

        protected override uint[] GenerateUniqueKeySource() => _cells.Pack();

        protected override IEnumerable<SudokuBoardOperation> EnumerateNextOperations()
        {
            var workspace = new BoardWorkspace(_cells);
            var operation = (SudokuBoardOperation?)null;
            operation = FullHouseOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = NakedSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = HiddenSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = PointingOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = BoxLineReductionOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            // TODO: 定石の追加
#error

            return ForcingChainOperation.MatchPattern(workspace);
        }

        /// <inheritdoc/>
        protected override SudokuBoardState GenerateNextState(SudokuBoardOperation operation) => new(this, operation);
    }
}
