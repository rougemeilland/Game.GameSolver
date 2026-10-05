namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面に対するオペレーションを示す抽象クラスです。
    /// </summary>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public abstract class GameBoardOperation<OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
    }
}
