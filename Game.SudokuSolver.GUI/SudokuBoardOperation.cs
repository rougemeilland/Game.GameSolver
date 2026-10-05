using System;
using Game.GameSolver;

namespace Game.SudokuSolver.GUI
{
    /// <summary>
    /// 数独の盤面に適用するオペレーションの抽象クラスです。
    /// </summary>
    internal abstract class SudokuBoardOperation
        : GameBoardOperation<SudokuBoardOperation>
    {
        protected SudokuBoardOperation(DifficultyLevel difficulty)
        {
            Difficulty = difficulty;
        }

        /// <summary>
        /// 定石の難易度を取得します。
        /// </summary>
        /// <value>
        /// 定石の難易度を示す <see cref="DifficultyLevel"/> 値です。
        /// </value>
        public DifficultyLevel Difficulty { get; }

        /// <summary>
        /// オペレーションにより確定されるセルを取得します。
        /// </summary>
        /// <value>
        /// 確定されたセルが存在する場合はそのセルを示す <see cref="BoardCell"/> 値です。そうではない場合は <see langword="null"/> です。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは継承可能です。
        /// </para>
        /// </remarks>
        public virtual BoardCell? DeterminedCell => null;

        /// <summary>
        /// オペレーションの説明に役立つ肯定的なセルのメモのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="BoardCell"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、セルのメモの絞り込みの根拠となったセルのメモのコレクションを意味します。
        /// </para>
        /// <para>
        /// このプロパティは継承可能です。
        /// </para>
        /// </remarks>
        public virtual ReadOnlyMemory<BoardCell> HilightedCellNotes => ReadOnlyMemory<BoardCell>.Empty;

        /// <summary>
        /// オペレーションの説明に役立つ否定的なセルのメモのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="BoardCell"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションの結果により削除されるセルのメモのコレクションを意味します。
        /// </para>
        /// <para>
        /// このプロパティは継承可能です。
        /// </para>
        /// </remarks>
        public virtual ReadOnlyMemory<BoardCell> RemovedCellNotes => ReadOnlyMemory<BoardCell>.Empty;

        /// <summary>
        /// オペレーションの説明に役立つ関連セルのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションによるセルメモの絞り込みの説明に役立つ範囲 (行、列、ブロック、任意のセルからの歌詞範囲、など) を意味します。
        /// </para>
        /// <para>
        /// このプロパティは継承可能です。
        /// </para>
        /// </remarks>
        public virtual ReadOnlyMemory<BoardCellPosition> RelatedCells => ReadOnlyMemory<BoardCellPosition>.Empty;

        /// <summary>
        /// オペレーションの動作内容を示す文字列を取得します。
        /// </summary>
        /// <value>
        /// オペレーションの動作内容を示す <see cref="string"/> です。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティが返す文字列は以下の内容を含まなければなりません。
        /// <list type="bullet">
        /// <item>このオペレーションが採用された根拠</item>
        /// <item>このオペレーションが盤面に及ぼす影響</item>
        /// </list>
        /// </para>
        /// <para>
        /// このプロパティは継承必須です。
        /// </para>
        /// </remarks>
        public abstract string Description { get; }

        /// <summary>
        /// 定石を盤面に適用します。
        /// </summary>
        /// <param name="cells">
        /// 盤面の状態を示す <see cref="BoardCellsSet"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// オペレーションを実行した結果の盤面の状態を示す <see cref="BoardCellsSet"/> オブジェクトを返します。
        /// </returns>
        public abstract BoardCellsSet Execute(BoardCellsSet cells);
    }
}
