namespace Game.GameSolver
{
    /// <summary>
    /// ゲームの盤面の状態の概要を示す列挙体です。
    /// </summary>
    public enum GameBoardStatus
    {
        /// <summary>
        /// 未知の状態
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 初期状態
        /// </summary>
        Initial,

        /// <summary>
        /// 解答作業中
        /// </summary>
        Solving,

        /// <summary>
        /// 解答が完了した
        /// </summary>
        Success,

        /// <summary>
        /// 解答が存在しない
        /// </summary>
        Failed,
    }
}
