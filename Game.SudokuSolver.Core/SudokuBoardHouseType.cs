namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のハウスの種類を示す列挙体です。
    /// </summary>
    internal enum SudokuBoardHouseType
    {
        Unknown,
        Row,
        Column,
        Block,
    }
}
