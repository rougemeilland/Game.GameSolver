using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のクラスです。
    /// </summary>
    public class SudokuGameBoard
        : GameBoard<SudokuBoardState, SudokuBoardOperation>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="initialCellDigits">
        /// <para>
        /// 数独の初期盤面のセルの数字を示す <see cref="string"/> オブジェクトの配列です。
        /// </para>
        /// <para>
        /// <paramref name="initialCellDigits"/> のサイズは 81 でなければなりません。
        /// </para>
        /// <para>
        /// 数独の盤面のあるセルの位置が R 行 C 列である場合、それに対応する <paramref name="initialCellDigits"/> のインデックス値は R * 9 + C です。
        /// </para>
        /// <para>
        /// 配列の要素は以下の何れかでなければなりません。
        /// <list type="bullet">
        /// <item>
        /// <term>空文字列 (<see cref="string.Empty"/>)</term>
        /// <description>その位置のセルの数字が不明であることを示します。</description>
        /// </item>
        /// <item>
        /// <term>"1" から "9" までの数字の長さ 1 の文字列</term>
        /// <description>その位置のセルはその数字に確定していることを示します。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </param>
        public SudokuGameBoard(ReadOnlySpan<string> initialCellDigits)
            : base(CreateInitialState(initialCellDigits))
        {
        }

        /// <summary>
        /// 与えられた盤面の数独の解を探します。
        /// </summary>
        /// <returns>
        /// <para>
        /// 見つかった数独の解を示す <see cref="SudokuSolution"/> オブジェクトを返します。
        /// </para>
        /// <para>
        /// <see cref="SudokuSolution"/> オブジェクトの <see cref="SudokuSolution.Status"/> プロパティは必ず確認してください。
        /// </para>
        /// </returns>
        public IEnumerable<SudokuSolution> Solve()
            => SolveCore().Select(solution => new SudokuSolution(solution));

        private static Lazy<SudokuBoardState> CreateInitialState(ReadOnlySpan<string> initialCellDigits)
        {
            var copyOfInitialCellDigits = initialCellDigits.ToArray();
            return new Lazy<SudokuBoardState>(() => new SudokuBoardState(copyOfInitialCellDigits));
        }
    }
}
