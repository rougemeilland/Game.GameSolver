using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.GameSolver;

namespace Game.WateSortSolver.GUI
{
    internal sealed class WateSortBoardState
        : GameBoardState<WateSortBoardState, WateSortBoardOperation>
    {
        protected override GameBoardStatus CheckStatus()
        {
            // TODO: 未実装
            throw new NotImplementedException();
        }

        protected override uint[] GenerateUniqueKeySource()
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
