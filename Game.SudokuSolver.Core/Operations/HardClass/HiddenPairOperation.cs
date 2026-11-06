using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// 隠れ二国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenPairOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private HiddenPairOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "隠れ二国同盟 (Hidden Pair)",
                  SudokuBoardDifficultyLevel.Hard,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある二つの数字 D1, D2 とある二つのセル C1, C2 に対して、
            // H 内に数字の候補として D1, D2 の何れかを持つセルが C1, C2 のみの場合
            // => C1, C2 の数字はそれぞれ D1, D2 の何れかであるので、C1, C2 の D1, S2 以外の数字の候補は排除することが出来る。

            // ブロックハウスの検索
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block),
                        block.ToHouseName(),
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
                        row.ToHouseName(),
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
                        column.ToHouseName(),
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, string houseName, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var keyCellDigitA = SudokuBoardCellDigit.MinValue; keyCellDigitA <= SudokuBoardCellDigit.MaxValue - 1; ++keyCellDigitA)
                {
                    var keyCellIndexBitSetA = ws.GetCellIndexBitSet(keyCellDigitA) & houseMask;
                    if (keyCellIndexBitSetA != 0)
                    {
                        // 指定されたハウスにおいて、keyCellDigitA が何れかのセルの数字の候補に存在する
                        for (var keyCellDigitB = keyCellDigitA + 1; keyCellDigitB <= SudokuBoardCellDigit.MaxValue; ++keyCellDigitB)
                        {
                            var keyCellIndexBitSetB = ws.GetCellIndexBitSet(keyCellDigitB) & houseMask;
                            if (keyCellIndexBitSetB != 0)
                            {
                                // 指定されたハウスにおいて、keyCellDigitB が何れかのセルの数字の候補に存在する
                                var keyCellIndexBitSet = keyCellIndexBitSetA | keyCellIndexBitSetB;
                                if (UInt128.PopCount(keyCellIndexBitSet) == 2)
                                {
                                    // あるハウスにおいて、ある二つの数字が二つのセルのみの候補に存在する
                                    // => 隠れ二国同盟が成立している
                                    // ただし、隠れ二国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                    // 隠れ二国同盟のセルの位置を求める
                                    var (keyCellIndex1, keyCellIndex2) = keyCellIndexBitSet.SplitAsPairCellIndexes();

                                    var keyCellDigitBitSet = keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet();
                                    var keyCellNoteDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                                    var keyCellNoteDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);
                                    var excludedCellDigitBitSet1 = keyCellNoteDigitBitSet1 & ~keyCellDigitBitSet;
                                    var excludedCellDigitBitSet2 = keyCellNoteDigitBitSet2 & ~keyCellDigitBitSet;
                                    var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2;
                                    if (excludedCellDigitBitSet != 0)
                                    {
                                        // この時点で、隠れ二国同盟によって排除される数字の候補が存在する

                                        var highlightedCellNotes =
                                            (keyCellNoteDigitBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex1)
                                            .Concat((keyCellNoteDigitBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex2));
                                        var excludedCellNotes =
                                            excludedCellDigitBitSet1.EnumerateAsCellNotes(keyCellIndex1)
                                            .Concat(excludedCellDigitBitSet2.EnumerateAsCellNotes(keyCellIndex2));
                                        yield return
                                            new HiddenPairOperation(
                                                excludedCellNotes,
                                                keyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                                                    .Concat((houseMask & ~keyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2)),
                                                highlightedCellNotes.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                                GetDescriptionBuilder(houseName, keyCellIndex1, keyCellIndex2, keyCellDigitBitSet, excludedCellDigitBitSet));
                                    }
                                }
                            }
                        }
                    }
                }

                static Func<string> GetDescriptionBuilder(
                    string houseName,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellDigitBitSet keyCellDigitBitSet,
                    SudokuBoardCellDigitBitSet excludedCellDigitBitSet)
                {
                    return
                        () =>
                        {
                            var excludedDigitChars = string.Join(", ", excludedCellDigitBitSet.EnumerateAsCellDigitTexts());
                            var keyCellDigitCharsText = string.Join(", ", keyCellDigitBitSet.EnumerateAsCellDigitTexts());
                            var keyCellPositionsText =
                                string.Join(
                                    ", ",
                                    (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet())
                                        .EnumerateAsCellPositionTexts());
                            return
                                $"{houseName}において、二つの数字 {keyCellDigitCharsText} を候補として含むマスは {keyCellPositionsText} の二つしかないので、" +
                                $"これらのマスの数字は必ず {keyCellDigitCharsText} の何れかになります。\n" +
                                $"したがって、二つのマス {keyCellPositionsText} の数字の候補からそれ以外の数字 {excludedDigitChars} を排除します。";
                        };
                }
            }
        }
    }
}
