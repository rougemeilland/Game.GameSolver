namespace Game.GameSolver
{
    /// <summary>
    /// 読み取り専用の一次元配列のインデクサのインターフェースです。
    /// </summary>
    /// <typeparam name="VALUE_T">
    /// 配列の要素の型です。
    /// </typeparam>
    /// <typeparam name="INDEX_T">
    /// インデックスの型です。
    /// </typeparam>
    public interface IReadOnlyVectorIndexer<VALUE_T, INDEX_T>
    {
        /// <summary>
        /// ひとつのインデックスを指定して要素を取得します。
        /// </summary>
        /// <param name="index">
        /// インデックスを示す <typeparamref name="INDEX_T"/> 値またはオブジェクトです。
        /// </param>
        /// <returns>
        /// <paramref name="index"/> に対応する <typeparamref name="VALUE_T"/> 値またはオブジェクトです。
        /// </returns>
        VALUE_T this[INDEX_T index] { get; }

        /// <summary>
        /// 要素の長さを取得します。
        /// </summary>
        /// <value>
        /// 要素の長さを示す <see cref="int"/> 値です。
        /// </value>
        int Length { get; }
    }
}
