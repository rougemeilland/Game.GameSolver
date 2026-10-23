namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の難易度を示す列挙体です。
    /// </summary>
    public enum SudokuBoardDifficultyLevel
    {
        /// <summary>
        /// 不明 (内部制御のみに使用)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 基本 (内部制御のみに使用)
        /// </summary>
        Basic,

        /// <summary>
        /// 入門
        /// </summary>
        Beginner,

        /// <summary>
        /// 初級
        /// </summary>
        Easy,

        /// <summary>
        /// 中級
        /// </summary>
        Medium,

        /// <summary>
        /// 上級
        /// </summary>
        Hard,

        /// <summary>
        /// 超上級
        /// </summary>
        Extreme,

        /// <summary>
        /// 究極
        /// </summary>
        Insane,

        /// <summary>
        /// 規格外
        /// </summary>
        Nonstandard = int.MaxValue,
    }
}
