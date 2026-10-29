using System;
using System.Buffers;
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
                var keyCellIndexBuffer = ArrayPool<SudokuBoardCellIndex>.Shared.Rent(9);
                try
                {
                    for (var keyCellDigitA = SudokuBoardCellDigit.MinValue; keyCellDigitA < SudokuBoardCellDigit.MaxValue - 2; ++keyCellDigitA)
                    {
                        var keyCellNoteBitSetA = ws.GetNoteBitSet(keyCellDigitA) & houseMask;
                        if (keyCellNoteBitSetA != 0)
                        {
                            // 指定されたハウスにおいて、keyCellDigitA が何れかのセルの数字の候補に存在する
                            for (var keyCellDigitB = keyCellDigitA + 1; keyCellDigitB < SudokuBoardCellDigit.MaxValue - 1; ++keyCellDigitB)
                            {
                                var keyCellNoteBitSetB = ws.GetNoteBitSet(keyCellDigitB) & houseMask;
                                if (keyCellNoteBitSetB != 0)
                                {
                                    // 指定されたハウスにおいて、keyCellDigitB が何れかのセルの数字の候補に存在する
                                    for (var cellDigitC = keyCellDigitB + 1; cellDigitC < SudokuBoardCellDigit.MaxValue; ++cellDigitC)
                                    {
                                        var keyCellNoteBitSetC = ws.GetNoteBitSet(cellDigitC) & houseMask;
                                        if (keyCellNoteBitSetC != 0)
                                        {
                                            // 指定されたハウスにおいて、keyCellDigitC が何れかのセルの数字の候補に存在する
                                            var keyCellNoteBitSet = keyCellNoteBitSetA | keyCellNoteBitSetB | keyCellNoteBitSetC;
                                            if (UInt128.PopCount(keyCellNoteBitSet) == 3)
                                            {
                                                // あるハウスにおいて、ある三つの数字が三つのセルのみの候補に存在する
                                                // => 隠れ三国同盟が成立している
                                                // ただし、隠れ三国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                                // 隠れ三国同盟のセルの位置を求める
                                                var keyCellIndexes = keyCellNoteBitSet.EnumerateAsCellIndexes(keyCellIndexBuffer);
#if DEBUG
                                                System.Diagnostics.Debug.Assert(keyCellIndexes.Length == 3);
#endif
                                                // 隠れ三国同盟に含まれるセル 1
                                                var keyCellIndex1 = keyCellIndexes[0];
                                                // 隠れ三国同盟に含まれるセル 2
                                                var keyCellIndex2 = keyCellIndexes[1];
                                                // 隠れ三国同盟に含まれるセル 3
                                                var keyCellIndex3 = keyCellIndexes[2];

                                                var keyCellDigitBitSet = keyCellDigitA.ToCellNoteBitSet() | keyCellDigitB.ToCellNoteBitSet() | cellDigitC.ToCellNoteBitSet();
                                                var keyCellNoteBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                                                var keyCellNoteBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                                                var keyCellNoteBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                                                var excludedCellDigitBitSet1 = keyCellNoteBitSet1 & ~keyCellDigitBitSet;
                                                var excludedCellDigitBitSet2 = keyCellNoteBitSet2 & ~keyCellDigitBitSet;
                                                var excludedCellDigitBitSet3 = keyCellNoteBitSet3 & ~keyCellDigitBitSet;
                                                var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2 | excludedCellDigitBitSet3;
                                                if (excludedCellDigitBitSet != 0)
                                                {
                                                    // この時点で、隠れ三国同盟によって排除される数字の候補が存在する

                                                    var highlightedCellNote =
                                                        (keyCellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex1)
                                                        .Concat((keyCellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex2))
                                                        .Concat((keyCellNoteBitSet3 & keyCellDigitBitSet).EnumerateAsCellNotes(keyCellIndex3));
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
                finally
                {
                    ArrayPool<SudokuBoardCellIndex>.Shared.Return(keyCellIndexBuffer);
                }
            }
        }
    }
}
