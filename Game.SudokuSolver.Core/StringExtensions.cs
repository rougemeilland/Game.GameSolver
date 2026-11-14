using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.SudokuSolver
{
    internal static class StringExtensions
    {
#if DEBUG
        /// <summary>
        /// 指定した文字列を盤面上のセルの配置と見做して、セル位置のビット集合に変換します。
        /// </summary>
        /// <param name="cellLayout">
        /// 盤面上のセルの位置を示す <see cref="string"/> オブジェクトです。
        /// <paramref name="cellLayout"/> の長さは 81 でなければなりません。
        /// </param>
        /// <returns>
        /// セル位置のビット集合を示す <see cref="UInt128"/> 値を返します。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="cellLayout"/> のフォーマットについて</term>
        /// <description>
        /// <list type="bullet">
        /// <item><paramref name="cellLayout"/> の長さは 81 でなければなりません。</item>
        /// <item><paramref name="cellLayout"/> に許容される文字は ' ' (空白) と '*' (アスタリスク) のみです。空白はその位置にセルが存在しないことを示し、アスタリスクはその位置にセルが存在することを示します。</item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public static UInt128 ToCellIndexBitSet(this string cellLayout)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(cellLayout.Length == 81);
#endif
            var result = UInt128.Zero;
            for (var index = 0; index < cellLayout.Length; ++index)
            {
                switch (cellLayout[index])
                {
                    case ' ':
                        break;
                    case '*':
                        result |= UInt128.One << index;
                        break;
                    default:
                        throw new ApplicationException();
                }
            }

            return result;
        }
#endif
    }
}
