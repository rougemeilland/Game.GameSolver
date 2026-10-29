using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.InsaneClass
{
    /// <summary>
    /// 強制チェーンパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class ForcingChainOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private ForcingChainOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "強制チェーン",
                  SudokuBoardDifficultyLevel.Insane,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  [],
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるセル C と C の数字の候補のひとつ D において、
            // C の数字を仮に D と仮定して論証を続ける。
            // 論証の結果が失敗であれば D が C の数字の候補であることは排除することが出来る。

            // 【アルゴリズム】
            // 1. セル C と C の数字の候補 D を任意に選ぶ。
            // 2. C の数字を D に仮に確定して論証を続ける。
            // 3. 論証が失敗した場合、C の数字の候補から D を排除して、強制チェーンを完了する。
            // 4. 論証が成功した場合は、別の C と D の組み合わせを探して 2 から繰り返す。
            // 5. 全てのセルの全ての数字の候補に対して論証に失敗した場合、強制チェーンは失敗とする。

            // 全ての未確定セルと数字の候補を洗い出して配列化する。
            var targetItems =
                (~ws.DeterminedCellBitSet & SudokuBoardWorkspace.AllCellIndexBitSet)
                .EnumerateAsCellIndexes()
                .Select(cellIndex => (cellIndex, visibleCellsBitSet: SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex), notes: ws.GetNoteBitSet(cellIndex)))
                .SelectMany(cell => cell.notes.EnumerateAsCellDigits(), (cell, cellNoteDigit) => (cell.cellIndex, cellNoteDigit, cellNoteCount: cell.notes.GetPopCount(), excludedCellNoteCount: (int)UInt128.PopCount(ws.GetNoteBitSet(cellNoteDigit) & cell.visibleCellsBitSet)))
                .OrderBy(item => item.cellNoteCount)
                .ThenByDescending(item => item.excludedCellNoteCount)
                .ThenBy(item => item.cellIndex)
                .ThenBy(item => item.cellNoteDigit)
#if DEBUG
                .Select(item =>
                {
                    System.Diagnostics.Debug.Assert(ws.CanCetermineCell(item.cellIndex, item.cellNoteDigit));
                    return item;
                })
#endif
                .Select(item => (item.cellIndex, item.cellNoteDigit));

            // 現在の盤面を文字列の配列に変換する
            var gameBoardSource = new string[SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1];
            for (var index = 0; index < gameBoardSource.Length; ++index)
            {
                gameBoardSource[index] =
                    ws.IsDetermined(index + SudokuBoardCellIndex.FirstValue)
                    ? ws.GetDeterminedCellDigit(index + SudokuBoardCellIndex.FirstValue).ToCellDigitChar().ToString()
                    : string.Empty;
            }

            // 現在の盤面のすべての未確定セルの位置と数字の候補の組み合わせに対して仮確定を行った上で論証を試みる
            foreach (var (cellIndex, cellNoteDigit) in targetItems)
            {
                // あるセルとある数字の候補を仮に確定する
                var copyOfGameBoardSource = new string[gameBoardSource.Length];
                gameBoardSource.AsSpan().CopyTo(copyOfGameBoardSource);
                copyOfGameBoardSource[cellIndex - SudokuBoardCellIndex.FirstValue] = cellNoteDigit.ToCellDigitChar().ToString();

                // 現在の盤面に仮に数字を確定させた盤面を元に新たに数独の盤面を作成して、論証を試みる
                var gameBoard = new SudokuGameBoard(copyOfGameBoardSource);
                var solutions = gameBoard.Solve(1);
                if (solutions.ActualSolutionCount > 0)
                {
                    var solution = solutions.Solutions.Single();
                    if (solution.Status == GameBoardStatus.Failed)
                    {
                        // 論証に失敗した場合
                        var excludedCellNote = new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit);
                        var excludedCellNotePosiionText = cellIndex.ToFriendlyString();
                        var excludedCellNoteDigitText = cellNoteDigit.ToCellDigitChar();
                        var description =
                            $"{excludedCellNotePosiionText}のセルの数字が '{excludedCellNoteDigitText}' と仮定して解答を求めようとしたところ失敗しました。\n" +
                            $"失敗の原因は{excludedCellNotePosiionText}のセルの数字が '{excludedCellNoteDigitText}' と仮定したことであるため、" +
                            $"{excludedCellNotePosiionText}のセルの数字が '{excludedCellNoteDigitText}' であることはあり得ません。\n" +
                            $"したがって、{excludedCellNotePosiionText}のセルの数字の候補から '{excludedCellNoteDigitText}' を排除します。";
                        yield return
                            new ForcingChainOperation(
                                Enumerable.Empty<SudokuBoardCellPositionDigit>().Append(excludedCellNote),
                                Enumerable.Empty<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)>().Append((cellIndex, SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)),
                                description);

                        // コンピュータリソースとパフォーマンスの観点から、強制チェーンは最初のひとつの要素を返して打ち切る。
                        yield break;
                    }
                }
            }
        }
    }
}
