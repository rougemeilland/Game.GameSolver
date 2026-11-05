using System;
using System.Collections.Generic;
using System.Linq;

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
                  "隠れ三国同盟 (Hidden Triple)",
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
                        SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block),
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
                        SudokuBoardCellGeometry.GetRowCellIndexBitSet(row),
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
                        SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column),
                        () => $"{column.ToFriendlyString()}の列",
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var keyCellDigitA = SudokuBoardCellDigit.MinValue; keyCellDigitA <= SudokuBoardCellDigit.MaxValue - 2; ++keyCellDigitA)
                {
                    var keyCellIndexBitSetA = ws.GetCellIndexBitSet(keyCellDigitA) & houseMask;
                    if (keyCellIndexBitSetA != 0)
                    {
                        // 指定されたハウスにおいて、keyCellDigitA が何れかのセルの数字の候補に存在する
                        for (var keyCellDigitB = keyCellDigitA + 1; keyCellDigitB <= SudokuBoardCellDigit.MaxValue - 1; ++keyCellDigitB)
                        {
                            var keyCellIndexBitSetB = ws.GetCellIndexBitSet(keyCellDigitB) & houseMask;
                            if (keyCellIndexBitSetB != 0)
                            {
                                // 指定されたハウスにおいて、keyCellDigitB が何れかのセルの数字の候補に存在する
                                for (var keyCellDigitC = keyCellDigitB + 1; keyCellDigitC <= SudokuBoardCellDigit.MaxValue; ++keyCellDigitC)
                                {
                                    var keyCellIndexBitSetC = ws.GetCellIndexBitSet(keyCellDigitC) & houseMask;
                                    if (keyCellIndexBitSetC != 0)
                                    {
                                        // 指定されたハウスにおいて、keyCellDigitC が何れかのセルの数字の候補に存在する
                                        var keyCellIndexBitSet = keyCellIndexBitSetA | keyCellIndexBitSetB | keyCellIndexBitSetC;
                                        if (UInt128.PopCount(keyCellIndexBitSet) == 3)
                                        {
                                            // あるハウスにおいて、ある三つの数字が三つのセルのみの候補に存在する
                                            // => 隠れ三国同盟が成立している
                                            // ただし、隠れ三国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                            // 隠れ三国同盟のセルの位置を求める
                                            var (keyCellIndex1, keyCellIndex2, keyCellIndex3) = keyCellIndexBitSet.SplitAsTripleCellIndexes();

                                            var keyCellDigitBitSet = keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet() | keyCellDigitC.ToCellDigitBitSet();
                                            var keyCellNoteDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                                            var keyCellNoteDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);
                                            var keyCellNoteDigitBitSet3 = ws.GetCellNoteDigitBitSet(keyCellIndex3);
                                            var excludedCellDigitBitSet1 = keyCellNoteDigitBitSet1 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet2 = keyCellNoteDigitBitSet2 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet3 = keyCellNoteDigitBitSet3 & ~keyCellDigitBitSet;
                                            var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2 | excludedCellDigitBitSet3;
                                            if (excludedCellDigitBitSet != 0)
                                            {
                                                // この時点で、隠れ三国同盟によって排除される数字の候補が存在する

                                                var highlightedCellNotes =
                                                    (keyCellNoteDigitBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex1)
                                                    .Concat((keyCellNoteDigitBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex2))
                                                    .Concat((keyCellNoteDigitBitSet3 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex3));
                                                var excludedCellNotes =
                                                    excludedCellDigitBitSet1.EnumerateAsCellNotes(keyCellIndex1)
                                                    .Concat(excludedCellDigitBitSet2.EnumerateAsCellNotes(keyCellIndex2))
                                                    .Concat(excludedCellDigitBitSet3.EnumerateAsCellNotes(keyCellIndex3));
                                                var houseName = houseNameGetter();
                                                var excludedDigitChars = string.Join(", ", excludedCellDigitBitSet.EnumerateAsCellDigitTexts());
                                                var keyCellDigitCharsText = string.Join(", ", keyCellDigitBitSet.EnumerateAsCellDigitTexts());
                                                var keyCellPositionsText =
                                                    string.Join(
                                                        ", ",
                                                        (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet())
                                                            .EnumerateAsCellPositionTexts());
                                                var description =
                                                    $"{houseName}において、三つの数字 {keyCellDigitCharsText} を候補として含むマスは {keyCellPositionsText} の三つしかないので、" +
                                                    $"これらのマスの数字は必ず {keyCellDigitCharsText} の何れかになります。\n" +
                                                    $"したがって、三つのマス {keyCellPositionsText} の数字の候補からそれ以外の数字 {excludedDigitChars} を排除します。";
                                                yield return
                                                    new HiddenTripleOperation(
                                                        excludedCellNotes,
                                                        keyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                                                            .Concat((houseMask & ~keyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2)),
                                                        highlightedCellNotes.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
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
