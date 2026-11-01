using System;
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
        /// <param name="highPriorityOperation">
        /// 優先的に適用される手順を示す <see cref="SudokuBoardOperationType"/> 値です。
        /// 通常は、このパラメタには <see cref="SudokuBoardOperationType.None"/> を指定します。
        /// 省略時の既定値は <see cref="SudokuBoardOperationType.None"/> です。
        /// </param>
        /// <param name="initialExcludedCellNotes">
        /// <para>
        /// 初期配置と同時に排除しておかなければならないセルの数字の候補のリストを示す <see cref="string"/> オブジェクトです。省略時の既定値は空文字列です。
        /// </para>
        /// <para>
        /// この文字列のフォーマットは、"&lt;行&gt;&lt;列&gt;&lt;数字&gt;" の三つの数字をそのまま連結したものをセミコロン ';' で区切ったものです。
        /// 行、列、数字は何れも 1 から 9 までの値です。
        /// 指定するセルの数字の候補が特にない場合には空文字列を指定します。
        /// </para>
        /// </param>
        public SudokuGameBoard(ReadOnlySpan<string> initialCellDigits,
            SudokuBoardOperationType highPriorityOperation = SudokuBoardOperationType.None,
            string initialExcludedCellNotes = "")
            : base(CreateInitialState(initialCellDigits, highPriorityOperation, initialExcludedCellNotes))
        {
        }

        /// <summary>
        /// 数独の盤面の解を求めます。
        /// </summary>
        /// <param name="maxSolutionCount">
        /// 求める解答の数の最大値です。
        /// </param>
        /// <returns>
        /// 数独の解を示す <see cref="SudokuSolutionCollection"/> オブジェクトを返します。
        /// </returns>
        public SudokuSolutionCollection Solve(int maxSolutionCount = 100)
            => new(SolveCore(maxSolutionCount));

        private static Lazy<SudokuBoardState> CreateInitialState(ReadOnlySpan<string> initialCellDigits,
            SudokuBoardOperationType highPriorityOperation = SudokuBoardOperationType.None,
            string initialExcludedCellNotes = "")
        {
            var copyOfInitialCellDigits = initialCellDigits.ToArray();
            return new Lazy<SudokuBoardState>(() => new SudokuBoardState(copyOfInitialCellDigits, highPriorityOperation, initialExcludedCellNotes));
        }
    }
}
