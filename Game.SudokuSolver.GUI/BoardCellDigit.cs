namespace Game.SudokuSolver.GUI
{
    /// <summary>
    /// セルの数字を示す列挙体です。
    /// </summary>
    internal enum BoardCellDigit
    {
        /// <summary>
        /// 数字 1
        /// </summary>
        One = 1 - 1,

        /// <summary>
        /// 数字 2
        /// </summary>
        Two = 2 - 1,

        /// <summary>
        /// 数字 3
        /// </summary>
        Three = 3 - 1,

        /// <summary>
        /// 数字 4
        /// </summary>
        Four = 4 - 1,

        /// <summary>
        /// 数字 5
        /// </summary>
        Five = 5 - 1,

        /// <summary>
        /// 数字 6
        /// </summary>
        Six = 6 - 1,

        /// <summary>
        /// 数字 7
        /// </summary>
        Seven = 7 - 1,

        /// <summary>
        /// 数字 8
        /// </summary>
        Eight = 8 - 1,

        /// <summary>
        /// 数字 9
        /// </summary>
        Nine = 9 - 1,

        /// <summary>
        /// 最小値
        /// </summary>
        MinValue = One,

        /// <summary>
        /// 最大値
        /// </summary>
        MaxValue = Nine,

        /// <summary>
        /// 未定義
        /// </summary>
        Undefined = -1,
    }
}
