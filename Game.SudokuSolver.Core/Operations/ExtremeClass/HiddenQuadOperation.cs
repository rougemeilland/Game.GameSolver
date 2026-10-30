using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

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
                  GameBoardStatus.Solving,
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
                var keyCellIndexBuffer = ArrayPool<SudokuBoardCellIndex>.Shared.Rent(9);
                try
                {
                    for (var keyCellDigitA = SudokuBoardCellDigit.MinValue; keyCellDigitA <= SudokuBoardCellDigit.MaxValue - 3; ++keyCellDigitA)
                    {
                        var keyCellBitSetA = ws.GetNoteBitSet(keyCellDigitA) & houseMask;
                        if (keyCellBitSetA != 0)
                        {
                            // 指定されたハウスにおいて、keyCellDigitA が何れかのセルの数字の候補に存在する
                            for (var keyCellDigitB = keyCellDigitA + 1; keyCellDigitB <= SudokuBoardCellDigit.MaxValue - 2; ++keyCellDigitB)
                            {
                                var keyCellBitSetB = ws.GetNoteBitSet(keyCellDigitB) & houseMask;
                                if (keyCellBitSetB != 0)
                                {
                                    // 指定されたハウスにおいて、keyCellDigitB が何れかのセルの数字の候補に存在する
                                    for (var keyCellDigitC = keyCellDigitB + 1; keyCellDigitC <= SudokuBoardCellDigit.MaxValue - 1; ++keyCellDigitC)
                                    {
                                        var keyCellBitSetC = ws.GetNoteBitSet(keyCellDigitC) & houseMask;
                                        if (keyCellBitSetC != 0)
                                        {
                                            // 指定されたハウスにおいて、keyCellDigitC が何れかのセルの数字の候補に存在する
                                            for (var keyCellDigitD = keyCellDigitC + 1; keyCellDigitD <= SudokuBoardCellDigit.MaxValue; ++keyCellDigitD)
                                            {
                                                var keyCellBitSetD = ws.GetNoteBitSet(keyCellDigitD) & houseMask;
                                                if (keyCellBitSetD != 0)
                                                {
                                                    // 指定されたハウスにおいて、keyCellDigitD が何れかのセルの数字の候補に存在する
                                                    var keyCellBitSet = keyCellBitSetA | keyCellBitSetB | keyCellBitSetC | keyCellBitSetD;
                                                    if (UInt128.PopCount(keyCellBitSet) == 4)
                                                    {
                                                        // あるハウスにおいて、ある四つの数字が四つのセルのみの候補に存在する
                                                        // => 隠れ四国同盟が成立している
                                                        // ただし、隠れ四国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                                        // 隠れ四国同盟のセルの位置を求める
                                                        var keyCellIndexes = keyCellBitSet.EnumerateAsCellIndexes(keyCellIndexBuffer);
#if DEBUG
                                                        System.Diagnostics.Debug.Assert(keyCellIndexes.Length == 4);
#endif
                                                        // 隠れ四国同盟に含まれるセル 1
                                                        var keyCellIndex1 = keyCellIndexes[0];
                                                        // 隠れ四国同盟に含まれるセル 2
                                                        var keyCellIndex2 = keyCellIndexes[1];
                                                        // 隠れ四国同盟に含まれるセル 3
                                                        var keyCellIndex3 = keyCellIndexes[2];
                                                        // 隠れ四国同盟に含まれるセル 4
                                                        var keyCellIndex4 = keyCellIndexes[3];

                                                        var keyCellDigitBitSet = keyCellDigitA.ToCellNoteBitSet() | keyCellDigitB.ToCellNoteBitSet() | keyCellDigitC.ToCellNoteBitSet() | keyCellDigitD.ToCellNoteBitSet();
                                                        var keyCellNoteBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                                                        var keyCellNoteBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                                                        var keyCellNoteBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                                                        var keyCellNoteBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                                                        var excludedCellDigitBitSet1 = keyCellNoteBitSet1 & ~keyCellDigitBitSet;
                                                        var excludedCellDigitBitSet2 = keyCellNoteBitSet2 & ~keyCellDigitBitSet;
                                                        var excludedCellDigitBitSet3 = keyCellNoteBitSet3 & ~keyCellDigitBitSet;
                                                        var excludedCellDigitBitSet4 = keyCellNoteBitSet4 & ~keyCellDigitBitSet;
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
                                                                (keyCellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex1)
                                                                .Concat((keyCellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex2))
                                                                .Concat((keyCellNoteBitSet3 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex3))
                                                                .Concat((keyCellNoteBitSet4 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex4));
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
                                                                    keyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                                                                        .Concat((houseMask & ~keyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2)),
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
                finally
                {
                    ArrayPool<SudokuBoardCellIndex>.Shared.Return(keyCellIndexBuffer);
                }
            }
        }
    }
}
