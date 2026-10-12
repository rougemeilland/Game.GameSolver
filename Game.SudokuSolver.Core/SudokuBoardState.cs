using System;
using System.Collections.Generic;
using Game.GameSolver;
using Game.SudokuSolver.Operations.BeginnerClass;
using Game.SudokuSolver.Operations.EasyClass;
using Game.SudokuSolver.Operations.ExtremeClass;
using Game.SudokuSolver.Operations.HardClass;
using Game.SudokuSolver.Operations.InsaneClass;
using Game.SudokuSolver.Operations.MediumClass;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の状態のクラスです。
    /// </summary>
    public sealed class SudokuBoardState
        : GameBoardState<SudokuBoardState, SudokuBoardOperation>
    {
        private readonly SudokuBoardCellsSet _cells;

        public SudokuBoardState(char[] initialCellDigits)
        {
            ArgumentNullException.ThrowIfNull(initialCellDigits);
            if (initialCellDigits.Length != 0)
                throw new ArgumentException("The size of the Sudoku cell array must be 81.", nameof(initialCellDigits));

            _cells = new SudokuBoardCellsSet(initialCellDigits);
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
            var workspace = new SudokuBoardWorkspace(_cells);

            #region BeginnerClass

            var operation = (SudokuBoardOperation?)FullHouseOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            #endregion

            #region EasyClass

            operation = NakedSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = HiddenSingleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region MediumClass

            operation = PointingOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = BoxLineReductionOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = NakedPairOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };
            operation = NakedTripleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region HardClass

            operation = NakedQuadOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = HiddenPairOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = XWingOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region ExtremeClass

            operation = HiddenTripleOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            operation = HiddenQuadOperation.MatchPattern(workspace);
            if (operation is not null)
                return new[] { operation };

            #endregion

            #region InsaneClass

            return ForcingChainOperation.MatchPattern(workspace);

            #endregion

        }

        /// <inheritdoc/>
        protected override SudokuBoardState GenerateNextState(SudokuBoardOperation operation) => new(this, operation);
    }
}
