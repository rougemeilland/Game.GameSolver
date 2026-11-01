using System.Collections.Generic;

namespace Game.SudokuSolver
{
    internal static class LinqExtensions
    {
        /// <summary>
        /// 指定された真偽値が真のときだけ二つのシーケンスを結合します。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="first">
        /// 最初のシーケンスの列挙子です。
        /// </param>
        /// <param name="condition">
        /// 二番目のシーケンスを結合するかどうかの <see cref="bool"/> 値です。
        /// </param>
        /// <param name="second">
        /// 二番目のシーケンスの列挙子です。
        /// </param>
        /// <returns>
        /// <paramref name="condition"/> が <see langword="true"/> の場合は、<paramref name="first"/> と <paramref name="second"/> を結合したシーケンスを返します。
        /// そうではない場合は、<paramref name="first"/> を返します。
        /// </returns>
        internal static IEnumerable<ELEMENT_T> ConcatIf<ELEMENT_T>(this IEnumerable<ELEMENT_T> first, bool condition, IEnumerable<ELEMENT_T> second)
        {
            foreach (var element in first)
                yield return element;
            if (condition)
            {
                foreach (var element in second)
                    yield return element;
            }
        }

        /// <summary>
        /// 二つのシーケンスのうち最初のシーケンスが空である場合だけ二つ目のシーケンスを返す列挙子を取得します。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="first">
        /// 最初のシーケンスの列挙子です。
        /// </param>
        /// <param name="second">
        /// 二番目のシーケンスの列挙子です。
        /// </param>
        /// <returns>
        /// <paramref name="first"/> が空でなければ <paramref name="first"/> の要素が、そうではない場合は <paramref name="second"/> の要素が返る列挙子を返します。
        /// </returns>
        internal static IEnumerable<ELEMENT_T> ConcatIfNone<ELEMENT_T>(this IEnumerable<ELEMENT_T> first, IEnumerable<ELEMENT_T> second)
        {
            using var enumerator1 = first.GetEnumerator();
            if (enumerator1.MoveNext())
            {
                yield return enumerator1.Current;
                while (enumerator1.MoveNext())
                    yield return enumerator1.Current;
            }
            else
            {
                foreach (var item in second)
                    yield return item;
            }
        }

        /// <summary>
        /// 指定されたシーケンスに指定された条件付きで指定された要素を追加します。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 指定されたシーケンスです。
        /// </param>
        /// <param name="condition">
        /// <paramref name="element"/> を追加するならば <see langword="true"/>、そうでなない場合は <see langword="false"/> です。
        /// </param>
        /// <param name="element">
        /// シーケンスに追加する要素を示す <typeparamref name="ELEMENT_T"/> 型のオブジェクトまたは値です。
        /// </param>
        /// <returns>
        /// <paramref name="condition"/> が <see langword="true"/> である場合は、<paramref name="source"/> の末尾に <paramref name="element"/> が追加されたシーケンスを返します。
        /// そうではない場合は <paramref name="source"/> がそのまま返ります。
        /// </returns>
        internal static IEnumerable<ELEMENT_T> AppendIf<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, bool condition, ELEMENT_T element)
        {
            using (var enumerator1 = source.GetEnumerator())
            {
                while (enumerator1.MoveNext())
                    yield return enumerator1.Current;
            }

            if (condition)
                yield return element;
        }
    }
}
