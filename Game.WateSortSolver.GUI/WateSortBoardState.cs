using System;
using System.Collections.Generic;
using Game.GameSolver;

namespace Game.WateSortSolver.GUI
{
    internal sealed class WateSortBoardState
        : GameBoardState<WateSortBoardState, WateSortBoardOperation>
    {
        // TODO: 未実装
        protected override ReadOnlyMemory<uint> GenerateUniqueKeySource() => throw new NotImplementedException();

        // TODO: 未実装
        protected override IEnumerable<WateSortBoardOperation> EnumerateNextOperations() => throw new NotImplementedException();

        // TODO: 未実装
        protected override WateSortBoardState GenerateNextState(WateSortBoardOperation operation) => throw new NotImplementedException();
    }
}
