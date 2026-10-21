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
        /// <param name="status">
        /// 定石を適用した結果の盤面の状態を示す <see cref="GameBoardStatus"/> 値です。
        /// </param>
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
        /// <param name="positivelyRelatedCellIndexBitSet">
        /// 定石の適用において肯定的な意味を持つセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="negativelyRelatedCellIndexBitSet">
        /// 定石の適用において否定的な意味を持つセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="description">
        /// 定石の解説のテキストを示す <see cref="string"/>オブジェクトです。
        /// </param>
        protected SudokuBoardOperation(
            GameBoardStatus status,
            string title,
            SudokuBoardDifficultyLevel difficulty,
            SudokuBoardCellPositionDigit? determinedCell,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes,
            ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes,
            UInt128 positivelyRelatedCellIndexBitSet,
            UInt128 negativelyRelatedCellIndexBitSet,
            string description)
            : base(status)
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
            PositivelyRelatedCellIndexBitSet = positivelyRelatedCellIndexBitSet;
            NegativelyRelatedCellIndexBitSet = negativelyRelatedCellIndexBitSet;
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
        /// オペレーションの説明に肯定的に役立つ関連セルの位置のビット集合を取得します。
        /// </summary>
        /// <value>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションによるセルの数字の候補の絞り込みのキーとなるセルの位置などを意味します。
        /// </para>
        /// </remarks>
        public UInt128 PositivelyRelatedCellIndexBitSet { get; }

        /// <summary>
        /// オペレーションの説明に否定的に役立つ関連セルの位置のビット集合を取得します。
        /// </summary>
        /// <value>
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションによるセルの数字の候補の絞り込みの結果、否定される数字の候補のセルの位置などを意味します。
        /// </para>
        /// </remarks>
        public UInt128 NegativelyRelatedCellIndexBitSet { get; }

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
