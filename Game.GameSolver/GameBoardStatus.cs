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
        /// 解決中
        /// </summary>
        Solving,

        /// <summary>
        /// 解決に成功した
        /// </summary>
        Success,

        /// <summary>
        /// 解決に成功した
        /// </summary>
        Failed,
    }
}
