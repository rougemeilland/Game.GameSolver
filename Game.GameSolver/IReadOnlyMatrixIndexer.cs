namespace Game.GameSolver
{
    /// <summary>
    /// 読み取り専用の行列のインデクサのインターフェースです。
    /// </summary>
    /// <typeparam name="VALUE_T">
    /// 配列の要素の型です。
    /// </typeparam>
    /// <typeparam name="COLUMN_T">
    /// 最初のインデックス (列) の型です。
    /// </typeparam>
    /// <typeparam name="ROW_T">
    /// 2番目のインデックス (行) の型です。
    /// </typeparam>
    public interface IReadOnlyMatrixIndexer<VALUE_T, COLUMN_T, ROW_T>
    {
        /// <summary>
        /// 行と列を指定して要素を取得します。
        /// </summary>
        /// <param name="column">
        /// 列を示す <typeparamref name="COLUMN_T"/> 値またはオブジェクトです。
        /// </param>
        /// <param name="row">
        /// 行を示す <typeparamref name="ROW_T"/> 値またはオブジェクトです。
        /// </param>
        /// <returns>
        /// <paramref name="column"/> と <paramref name="row"/> に対応する要素を示す <typeparamref name="VALUE_T"/> 値またはオブジェクトです。
        /// </returns>
        VALUE_T this[COLUMN_T column, ROW_T row] { get; }

        /// <summary>
        /// 列の長さを取得します。
        /// </summary>
        /// <value>
        /// 列の長さを示す <see cref="int"/> 値です。
        /// </value>
        int ColumnLength { get; }

        /// <summary>
        /// 行の長さを取得します。
        /// </summary>
        /// <value>
        /// 行の長さを示す <see cref="int"/> 値です。
        /// </value>
        int RowLength { get; }
    }
}
