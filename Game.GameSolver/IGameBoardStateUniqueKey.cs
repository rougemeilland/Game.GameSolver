using System;

namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面を識別する短いデータ列の公開インターフェースです。
    /// </summary>
    public interface IGameBoardStateUniqueKey
        : IEquatable<IGameBoardStateUniqueKey>
    {
    }
}
