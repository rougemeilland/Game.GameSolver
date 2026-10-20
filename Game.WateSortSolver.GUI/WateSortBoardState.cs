using System;
using System.Collections.Generic;
using Game.GameSolver;

namespace Game.WateSortSolver.GUI
{
    internal sealed class WateSortBoardState
        : GameBoardState<WateSortBoardState, WateSortBoardOperation>
    {
        protected override ReadOnlyMemory<uint> GenerateUniqueKeySource()
        {
            // TODO: 未実装
            throw new NotImplementedException();
        }

        protected override IEnumerable<WateSortBoardOperation> EnumerateNextOperations()
        {
            // TODO: 未実装
            throw new NotImplementedException();
        }

        protected override WateSortBoardState GenerateNextState(WateSortBoardOperation operation)
        {
            // TODO: 未実装
            throw new NotImplementedException();
        }
    }
}
