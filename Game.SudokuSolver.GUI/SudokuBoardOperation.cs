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
        protected SudokuBoardOperation(string title, DifficultyLevel difficulty, BoardCell? determinedCell, ReadOnlyMemory<BoardCell> hilightedCellNotes, ReadOnlyMemory<BoardCell> removedCellNotes, ReadOnlyMemory<BoardCellPosition> relatedCellPositions, string description)
        {
#if DEBUG
            {

                // HilightedCellNotes と RemovedCellNotes に同じ要素が含まれていてはならない
                for (var index1 = 0; index1 < hilightedCellNotes.Length; ++index1)
                {
                    var pos1 = hilightedCellNotes.Span[index1];
                    for (var index2 = 0; index2 < removedCellNotes.Length; ++index2)
                    {
                        var pos2 = removedCellNotes.Span[index2];
                        if (pos1 == pos2)
                            throw new ApplicationException();
                    }
                }
            }
#endif

            Title = title;
            Difficulty = difficulty;
            DeterminedCell = determinedCell;
            HilightedCellNotes = hilightedCellNotes;
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
        /// </remarks>
        public BoardCell? DeterminedCell { get; }

        /// <summary>
        /// オペレーションの説明に役立つ肯定的なセルの数字の候補のコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="BoardCell"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、セルの数字の候補の絞り込みの根拠となったセルのメモのコレクションを意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<BoardCell> HilightedCellNotes { get; }

        /// <summary>
        /// オペレーションの説明に役立つ否定的なセルの数字の候補のコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルを示す <see cref="BoardCell"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションの結果により削除されるセルの数字の候補のコレクションを意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<BoardCell> RemovedCellNotes { get; }

        /// <summary>
        /// オペレーションの説明に役立つ関連セルのコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルの位置を示す <see cref="BoardCellPosition"/> 値のコレクションです。
        /// </value>
        /// <remarks>
        /// <para>
        /// このプロパティは、例えば、オペレーションによるセルの数字の候補の絞り込みの説明に役立つ範囲 (行、列、ブロック、任意のセルからの歌詞範囲、など) を意味します。
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<BoardCellPosition> RelatedCellPositions;

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
        /// 盤面の状態を示す <see cref="BoardCellsSet"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// オペレーションを実行した結果の盤面の状態を示す <see cref="BoardCellsSet"/> オブジェクトを返します。
        /// </returns>
        public abstract BoardCellsSet Execute(BoardCellsSet cells);
    }
}
