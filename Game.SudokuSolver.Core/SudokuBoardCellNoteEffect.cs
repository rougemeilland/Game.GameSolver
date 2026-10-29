namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの数字の候補の表示方法を示す列挙体です。
    /// </summary>
    /// <remarks>
    /// 同じセルの数字の候補に異なる <see cref="SudokuBoardCellNoteEffect"/> 値が指定されている場合、値が大きい方が優先して適用されます。
    /// </remarks>
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
        /// 数字の候補を肯定的に強調表示します。(タイプ3)
        /// </summary>
        PositivelyHilightedType3,

        /// <summary>
        /// 数字の候補を肯定的に強調表示します。(タイプ2)
        /// </summary>
        PositivelyHilightedType2,

        /// <summary>
        /// 数字の候補を肯定的に強調表示します。(タイプ1)
        /// </summary>
        PositivelyHilightedType1,

        /// <summary>
        /// 数字の候補を肯定的に強調表示します。
        /// </summary>
        PrimaryPositivelyHighlighted = PositivelyHilightedType1,

        /// <summary>
        /// 数字の候補を否定的に強調表示します。
        /// </summary>
        NegativelyHighlighted,
    }
}
