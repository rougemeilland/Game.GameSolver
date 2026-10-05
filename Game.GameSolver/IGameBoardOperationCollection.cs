using System.Collections.Generic;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面に対するオペレーションのコレクションの公開インターフェースです。
    /// </summary>
    /// <typeparam name="OPERATION_T"></typeparam>
    public interface IGameBoardOperationCollection<OPERATION_T>
        : IEnumerable<OPERATION_T>
    {
    }
}
