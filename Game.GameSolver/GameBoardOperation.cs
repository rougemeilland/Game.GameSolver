namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面に対するオペレーションを示す抽象クラスです。
    /// </summary>
    /// <typeparam name="OPERATION_T">ゲームの盤面に対するオペレーションを示すクラスです。</typeparam>
    public abstract class GameBoardOperation<OPERATION_T>
        where OPERATION_T : GameBoardOperation<OPERATION_T>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="status">
        /// このオペレーションを適用した結果の盤面の状態を示す <see cref="GameBoardStatus"/> 値です。
        /// </param>
        protected GameBoardOperation(GameBoardStatus status)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(this is OPERATION_T);
#endif
            Status = status;
        }

        /// <summary>
        /// このオペレーションを適用した結果の盤面の状態を取得します。
        /// </summary>
        /// <value>
        /// このオペレーションを適用した結果の盤面の状態をを示す <see cref="GameBoardStatus"/> 値です。
        /// </value>
        public GameBoardStatus Status { get; }
    }
}
