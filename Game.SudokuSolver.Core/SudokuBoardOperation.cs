using System;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面に適用する定石の抽象クラスです。
    /// </summary>
    public abstract class SudokuBoardOperation
        : GameBoardOperation<SudokuBoardOperation>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="title">
        /// 定石の名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="difficulty">
        /// 定石の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </param>
        /// <param name="determinedCell">
        /// 定石を適用した結果数字が確定したセルが存在する場合はそのセルの位置と数字を示す <see cref="SudokuBoardCellPositionDigit"/> 値、そうではない場合は <see langword="null"/> です。
        /// </param>
        /// <param name="highlightedCellNotes">
        /// 定石のキーとなる数字の候補とその位置を示す <see cref="SudokuBoardCellPositionDigit"/> の配列です。
        /// </param>
        /// <param name="removedCellNotes">
        /// 定石を適用した結果により否定される数字の候補とその位置を示す <see cref="SudokuBoardCellPositionDigit"/> の配列です。
        /// </param>
        /// <param name="relatedCellPositions">
        /// 定石の適用範囲のセルの位置を示す <see cref="SudokuBoardCellPosition"/> 値の配列です。
        /// </param>
        /// <param name="description">
        /// 定石の解説のテキストを示す <see cref="string"/>オブジェクトです。
        /// </param>
        protected SudokuBoardOperation(string title, SudokuBoardDifficultyLevel difficulty, SudokuBoardCellPositionDigit? determinedCell, ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
        {
#if DEBUG
            {

                // HighlightedCellNotes と RemovedCellNotes に同じ要素が含まれていてはならない
                for (var index1 = 0; index1 < highlightedCellNotes.Length; ++index1)
                {
                    var highlitedCellNote = highlightedCellNotes.Span[index1];
                    for (var index2 = 0; index2 < removedCellNotes.Length; ++index2)
                    {
                        var removedCellNote = removedCellNotes.Span[index2];
                        if (highlitedCellNote == removedCellNote)
                            throw new ApplicationException();
                    }
                }
            }
#endif

            Title = title;
            Difficulty = difficulty;
            DeterminedCell = determinedCell;
            HighlightedCellNotes = highlightedCellNotes;
            RemovedCellNotes = removedCellNotes;
            RelatedCellPositions = relatedCellPositions;
            Description = description;
        }

        /// <summary>
        /// 定石の名前を取得します。
        /// </summary>
        /// 定石の名前を示す <see cref="string"/> オブジェクトです。
        public string Title { get; }

        /// <summary>
        /// 定石の難易度を取得します。
        /// </summary>
        /// <value>
        /// 定石の難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </value>
        public SudokuBoardDifficultyLevel Difficulty { get; }

        /// <summary>
        /// オペレーションにより確定されるセルを取得します。
        /// </summary>
        /// <value>
        /// 確定されたセルが存在する場合はそのセルを示す <see cref="SudokuBoardCellPositionDigit"/> 値です。そうではない場合は <see langword="null"/> です。
        /// </value>
        /// <remarks>
        /// </remarks>
        public SudokuBoardCellPositionDigit? DeterminedCell { get; }

        /// <summary>
        /// オペレーションの説明に役立つ肯定的なセルの数字の候補のコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="SudokuBoardCellPositionDigit"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、セルの数字の候補の絞り込みの根拠となったセルのメモのコレクションを意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<SudokuBoardCellPositionDigit> HighlightedCellNotes { get; }

        /// <summary>
        /// オペレーションの説明に役立つ否定的なセルの数字の候補のコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="SudokuBoardCellPositionDigit"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションの結果により削除されるセルの数字の候補のコレクションを意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<SudokuBoardCellPositionDigit> RemovedCellNotes { get; }

        /// <summary>
        /// オペレーションの説明に役立つ関連セルのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルの位置を示す <see cref="SudokuBoardCellPosition"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションによるセルの数字の候補の絞り込みの説明に役立つ範囲 (行、列、ブロック、任意のセルからの歌詞範囲、など) を意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<SudokuBoardCellPosition> RelatedCellPositions { get; }

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
        /// </remarks>
        public string Description { get; }

        /// <summary>
        /// 定石を盤面に適用します。
        /// </summary>
        /// <param name="cells">
        /// 盤面の状態を示す <see cref="SudokuBoardCellsSet"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// オペレーションを実行した結果の盤面の状態を示す <see cref="SudokuBoardCellsSet"/> オブジェクトを返します。
        /// </returns>
        internal abstract SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells);
    }
}
