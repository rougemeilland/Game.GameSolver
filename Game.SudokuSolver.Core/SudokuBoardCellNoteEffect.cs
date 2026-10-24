namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの数字の候補の表示方法を示す列挙体です。
    /// </summary>
    public enum SudokuBoardCellNoteEffect
    {
        /// <summary>
        /// 数字の候補を表示しません。
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// 数字の候補を通常表示します。
        /// </summary>
        Normal,

        /// <summary>
        /// 数字の候補を強く肯定的に強調表示します。
        /// </summary>
        PrimaryPositivelyHighlighted,

        /// <summary>
        /// 数字の候補を弱く肯定的に強調表示します。
        /// </summary>
        SecondaryPositivelyHilighted,

        /// <summary>
        /// 数字の候補を否定的に強調表示します。
        /// </summary>
        NegativelyHighlighted,
    }
}
