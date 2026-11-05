using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// 隠れ四国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenQuadOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private HiddenQuadOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  "隠れ四国同盟 (Hidden Quadruple)",
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
            // あるハウス H と ある四つの数字 D1, D2, D3, D4 とある四つのセル C1, C2, C3, C4 に対して、
            // H 内に数字の候補として D1, D2, D3, D4 の何れかを持つセルが C1, C2, C3, C4 のみの場合
            // => C1, C2, C3, C4 の数字はそれぞれ D1, D2, D3, D4 の何れかであるので、C1, C2, C3, C4 の D1, S2, D3, D4 以外の数字の候補を排除することが出来る。

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
                for (var keyCellDigitA = SudokuBoardCellDigit.MinValue; keyCellDigitA <= SudokuBoardCellDigit.MaxValue - 3; ++keyCellDigitA)
                {
                    var keyCellIndexBitSetA = ws.GetCellIndexBitSet(keyCellDigitA) & houseMask;
                    if (keyCellIndexBitSetA != 0)
                    {
                        // 指定されたハウスにおいて、keyCellDigitA が何れかのセルの数字の候補に存在する
                        for (var keyCellDigitB = keyCellDigitA + 1; keyCellDigitB <= SudokuBoardCellDigit.MaxValue - 2; ++keyCellDigitB)
                        {
                            var keyCellIndexBitSetB = ws.GetCellIndexBitSet(keyCellDigitB) & houseMask;
                            if (keyCellIndexBitSetB != 0)
                            {
                                // 指定されたハウスにおいて、keyCellDigitB が何れかのセルの数字の候補に存在する
                                for (var keyCellDigitC = keyCellDigitB + 1; keyCellDigitC <= SudokuBoardCellDigit.MaxValue - 1; ++keyCellDigitC)
                                {
                                    var keyCellIndexBitSetC = ws.GetCellIndexBitSet(keyCellDigitC) & houseMask;
                                    if (keyCellIndexBitSetC != 0)
                                    {
                                        // 指定されたハウスにおいて、keyCellDigitC が何れかのセルの数字の候補に存在する
                                        for (var keyCellDigitD = keyCellDigitC + 1; keyCellDigitD <= SudokuBoardCellDigit.MaxValue; ++keyCellDigitD)
                                        {
                                            var keyCellIndexBitSetD = ws.GetCellIndexBitSet(keyCellDigitD) & houseMask;
                                            if (keyCellIndexBitSetD != 0)
                                            {
                                                // 指定されたハウスにおいて、keyCellDigitD が何れかのセルの数字の候補に存在する
                                                var keyCellIndexBitSet = keyCellIndexBitSetA | keyCellIndexBitSetB | keyCellIndexBitSetC | keyCellIndexBitSetD;
                                                if (UInt128.PopCount(keyCellIndexBitSet) == 4)
                                                {
                                                    // あるハウスにおいて、ある四つの数字が四つのセルのみの候補に存在する
                                                    // => 隠れ四国同盟が成立している
                                                    // ただし、隠れ四国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                                    // 隠れ四国同盟のセルの位置を求める
                                                    var (keyCellIndex1, keyCellIndex2, keyCellIndex3, keyCellIndex4) = keyCellIndexBitSet.SplitAsQuadCellIndexes();

                                                    var keyCellDigitBitSet = keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet() | keyCellDigitC.ToCellDigitBitSet() | keyCellDigitD.ToCellDigitBitSet();
                                                    var keyCellNoteDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                                                    var keyCellNoteDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);
                                                    var keyCellNoteDigitBitSet3 = ws.GetCellNoteDigitBitSet(keyCellIndex3);
                                                    var keyCellNoteDigitBitSet4 = ws.GetCellNoteDigitBitSet(keyCellIndex4);
                                                    var excludedCellDigitBitSet1 = keyCellNoteDigitBitSet1 & ~keyCellDigitBitSet;
                                                    var excludedCellDigitBitSet2 = keyCellNoteDigitBitSet2 & ~keyCellDigitBitSet;
                                                    var excludedCellDigitBitSet3 = keyCellNoteDigitBitSet3 & ~keyCellDigitBitSet;
                                                    var excludedCellDigitBitSet4 = keyCellNoteDigitBitSet4 & ~keyCellDigitBitSet;
                                                    var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2 | excludedCellDigitBitSet3 | excludedCellDigitBitSet4;
                                                    if (excludedCellDigitBitSet != 0)
                                                    {
                                                        // この時点で、隠れ四国同盟によって排除される数字の候補が存在する

                                                        var excludedCellNotes =
                                                            excludedCellDigitBitSet1.EnumerateAsCellNotes(keyCellIndex1)
                                                            .Concat(excludedCellDigitBitSet2.EnumerateAsCellNotes(keyCellIndex2))
                                                            .Concat(excludedCellDigitBitSet3.EnumerateAsCellNotes(keyCellIndex3))
                                                            .Concat(excludedCellDigitBitSet4.EnumerateAsCellNotes(keyCellIndex4));
                                                        var highlightedCellNotes =
                                                            (keyCellNoteDigitBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex1)
                                                            .Concat((keyCellNoteDigitBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex2))
                                                            .Concat((keyCellNoteDigitBitSet3 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex3))
                                                            .Concat((keyCellNoteDigitBitSet4 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex4));
                                                        var houseName = houseNameGetter();
                                                        var excludedDigitChars = string.Join(", ", excludedCellDigitBitSet.EnumerateAsCellDigitTexts());
                                                        var keyCellDigitCharsText = string.Join(", ", keyCellDigitBitSet.EnumerateAsCellDigitTexts());
                                                        var keyCellPositionsText =
                                                            string.Join(
                                                                ", ",
                                                                (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet())
                                                                    .EnumerateAsCellPositionTexts());
                                                        var description =
                                                            $"{houseName}において、四つの数字 {keyCellDigitCharsText} を候補として含むマスは {keyCellPositionsText} の四つしかないので、" +
                                                            $"これらのマスの数字は必ず {keyCellDigitCharsText} の何れかになります。\n" +
                                                            $"したがって、四つのマス {keyCellPositionsText} の数字の候補からそれ以外の数字 {excludedDigitChars} を排除します。";
                                                        yield return
                                                            new HiddenQuadOperation(
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
    }
}
