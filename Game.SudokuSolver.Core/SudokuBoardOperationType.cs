namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の定石の種類を示す列挙体です。
    /// </summary>
    public enum SudokuBoardOperationType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        #region BeginnerClass

        /// <summary>
        /// Full House
        /// </summary>
        FullHouse,

        #endregion

        #region EasyClass

        /// <summary>
        ///  Hidden Single
        /// </summary>
        HiddenSingle,

        /// <summary>
        /// Naked Single
        /// </summary>
        NakedSingle,

        #endregion

        #region MediumClass

        /// <summary>
        /// Box Line Reduction
        /// </summary>
        BoxLineReduction,

        /// <summary>
        ///  Naked Pair
        /// </summary>
        NakedPair,

        /// <summary>
        /// Naked Triple
        /// </summary>
        NakedTriple,

        /// <summary>
        /// Pointing
        /// </summary>
        Pointing,

        #endregion

        #region HardClass

        /// <summary>
        /// Hidden Pair
        /// </summary>
        HiddenPair,

        /// <summary>
        /// Naked Quad
        /// </summary>
        NakedQuad,

        /// <summary>
        /// Swordfish
        /// </summary>
        Swordfish,

        /// <summary>
        /// X-Wing
        /// </summary>
        XWing,

        #endregion

        #region ExtremeClass

        /// <summary>
        /// Hidden Quad
        /// </summary>
        HiddenQuad,

        /// <summary>
        /// Hidden Triple
        /// </summary>
        HiddenTriple,

        /// <summary>
        /// Jellyfish
        /// </summary>
        Jellyfish,

        #endregion

        #region InsaneClass

        /// <summary>
        /// Su de Coq
        /// </summary>
        SueDeCoq,

        #endregion
    }
}
