using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// 隠れ三国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenTripleOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private HiddenTripleOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "隠れ三国同盟",
                  SudokuBoardDifficultyLevel.Extreme,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある三つの数字 D1, D2, D3 とある三つのセル C1, C2, C3 に対して、
            // H 内に数字の候補として D1, D2, D3 の何れかを持つセルが C1, C2, C3 のみの場合
            // => C1, C2, C3 の数字はそれぞれ D1, D2, D3 の何れかであるので、C1, C2, C3 の D1, S2, D3 以外の数字の候補を排除することが出来る。

            // ブロックハウスの検索
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                        () => $"{block.ToFriendlyString()}のブロック",
                        SudokuBoardCellGeometry.GetCellIndexes(block).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            // 行ハウスの検索
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                        () => $"{row.ToFriendlyString()}の行",
                        SudokuBoardCellGeometry.GetCellIndexes(row).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            // 列ハウスの検索
            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                        () => $"{column.ToFriendlyString()}の列",
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var cellDigitA = SudokuBoardCellDigit.MinValue; cellDigitA < SudokuBoardCellDigit.MaxValue - 2; ++cellDigitA)
                {
                    var cellNoteBitSetA = ws.GetNoteBitSet(cellDigitA) & houseMask;
                    if (cellNoteBitSetA != 0)
                    {
                        // 指定されたハウスにおいて、cellDigitA が何れかのセルの数字の候補に存在する
                        for (var cellDigitB = cellDigitA + 1; cellDigitB < SudokuBoardCellDigit.MaxValue - 1; ++cellDigitB)
                        {
                            var cellNoteBitSetB = ws.GetNoteBitSet(cellDigitB) & houseMask;
                            if (cellNoteBitSetB != 0)
                            {
                                // 指定されたハウスにおいて、cellDigitB が何れかのセルの数字の候補に存在する
                                for (var cellDigitC = cellDigitB + 1; cellDigitC < SudokuBoardCellDigit.MaxValue; ++cellDigitC)
                                {
                                    var cellNoteBitSetC = ws.GetNoteBitSet(cellDigitC) & houseMask;
                                    if (cellNoteBitSetC != 0)
                                    {
                                        // 指定されたハウスにおいて、cellDigitC が何れかのセルの数字の候補に存在する
                                        var cellNoteBitSet = cellNoteBitSetA | cellNoteBitSetB | cellNoteBitSetC;
                                        if (UInt128.PopCount(cellNoteBitSet) == 3)
                                        {
                                            // あるハウスにおいて、ある三つの数字が三つのセルのみの候補に存在する
                                            // => 隠れ三国同盟が成立している
                                            // ただし、隠れ三国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                            // 隠れ三国同盟のセルの位置を求める
                                            var cellIndexes = cellNoteBitSet.EnumerateAsCellIndexes().ToArray();
#if DEBUG
                                            System.Diagnostics.Debug.Assert(cellIndexes.Length == 3);
#endif
                                            // 隠れ三国同盟に含まれるセル 1
                                            var cellIndex1 = cellIndexes[0];
                                            // 隠れ三国同盟に含まれるセル 2
                                            var cellIndex2 = cellIndexes[1];
                                            // 隠れ三国同盟に含まれるセル 3
                                            var cellIndex3 = cellIndexes[2];

                                            var keyCellDigitBitSet = cellDigitA.ToCellNoteBitSet() | cellDigitB.ToCellNoteBitSet() | cellDigitC.ToCellNoteBitSet();
                                            var cellNoteBitSet1 = ws.GetNoteBitSet(cellIndex1);
                                            var cellNoteBitSet2 = ws.GetNoteBitSet(cellIndex2);
                                            var cellNoteBitSet3 = ws.GetNoteBitSet(cellIndex3);
                                            var excludedCellDigitBitSet1 = cellNoteBitSet1 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet2 = cellNoteBitSet2 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet3 = cellNoteBitSet3 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2 | excludedCellDigitBitSet3;
                                            if (excludedCellDigitBitSet != 0)
                                            {
                                                // この時点で、隠れ三国同盟によって排除される数字の候補が存在する

                                                var highlightedCellNote =
                                                    (cellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(cellIndex1)
                                                    .Concat((cellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(cellIndex2))
                                                    .Concat((cellNoteBitSet3 & keyCellDigitBitSet).EnumerateAsCellNotes(cellIndex3));
                                                var excludedCellNotes =
                                                    excludedCellDigitBitSet1.EnumerateAsCellNotes(cellIndex1)
                                                    .Concat(excludedCellDigitBitSet2.EnumerateAsCellNotes(cellIndex2))
                                                    .Concat(excludedCellDigitBitSet3.EnumerateAsCellNotes(cellIndex3));
                                                var houseName = houseNameGetter();
                                                var excludedDigitChars =
                                                    string.Join(
                                                        ", ",
                                                        excludedCellDigitBitSet
                                                        .EnumerateAsCellDigits()
                                                        .Select(cellDigit => $"'{cellDigit.ToCellDigitChar()}'"));
                                                var keyCellDigitCharsText = $"'{cellDigitA.ToCellDigitChar()}', '{cellDigitB.ToCellDigitChar()}', '{cellDigitC.ToCellDigitChar()}'";
                                                var keyCellPositionsText = $"{cellIndex1.ToFriendlyString()}, {cellIndex2.ToFriendlyString()}, {cellIndex3.ToFriendlyString()}";
                                                var description =
                                                    $"{houseName}において、三つの数字 {keyCellDigitCharsText} を候補として含むセルは {keyCellPositionsText} の三つしかないので、" +
                                                    $"これらのセルの数字は必ず {keyCellDigitCharsText} の何れかです。\n" +
                                                    $"したがって、三つのセル {keyCellPositionsText} の数字の候補からそれ以外の数字 {excludedDigitChars} を排除します。";
                                                yield return
                                                    new HiddenTripleOperation(
                                                        excludedCellNotes,
                                                        houseMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted),
                                                        highlightedCellNote.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                                        description);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
