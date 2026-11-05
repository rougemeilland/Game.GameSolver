using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// 四国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedQuadOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private NakedQuadOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  "四国同盟 (Naked Quadruple)",
                  SudokuBoardDifficultyLevel.Hard,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある四つの数字 D1, D2, D3, D4 と H 内にある四つのセル C1, C2, C3, C4 に対して、
            // C1, C2, C3, C4 が数字の候補として D1, S2, D3, D4 の何れかだけを持つ場合
            // => C1, C2, C3, C4 の数字はそれぞれ D1, D2, D3, D4 の何れかであるので、H 内の C1, C2, C3, C4 以外のセルは D1, D2, D3, D4 の何れでもあり得ない。
            // => C1, C2, C3, C4 の数字はそれぞれ D1, D2, D3, D4 の何れかであるので、C1, C2, C3, C4 の全てが共有するハウス (つまりハウス H) 内の C1, C2, C3, C4 以外のセルは D1, D2, D3, D4 の何れでもあり得ない。

            var keyCellIIndexesBuffer = ArrayPool<SudokuBoardCellIndex>.Shared.Rent(9);
            try
            {
                // ブロックハウスの検索
                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var operations =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block),
                            () => $"{block.ToFriendlyString()}のブロック",
                            SudokuBoardCellGeometry.GetCellIndexes(block),
                            keyCellIIndexesBuffer);
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
                            SudokuBoardCellGeometry.GetCellIndexes(row),
                            keyCellIIndexesBuffer);
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
                            SudokuBoardCellGeometry.GetCellIndexes(column),
                            keyCellIIndexesBuffer);
                    foreach (var operation in operations)
                        yield return operation;
                }
            }
            finally
            {
                ArrayPool<SudokuBoardCellIndex>.Shared.Return(keyCellIIndexesBuffer);
            }

            static IEnumerable<NakedQuadOperation> MatchPatternByHouse(
                SudokuBoardWorkspace ws,
                UInt128 houseMask,
                Func<string> houseNameGetter,
                ReadOnlyMemory<SudokuBoardCellIndex> houseCellIndexes,
                Memory<SudokuBoardCellIndex> keyCellIIndexesBuffer)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(houseCellIndexes.Length <= keyCellIIndexesBuffer.Length);
#endif
                var houseNoteMask = ~ws.DeterminedCellBitSet & houseMask;

                // ハウス内の未確定セルの数が 5 未満の場合、四国同盟が成立しないかあるいは仮に成立したとしても数字の候補を排除可能な他の未確定セルが存在しないため、検索を中断する
                if (UInt128.PopCount(houseNoteMask) < 5)
                    yield break;

                // 四国同盟の可能性があるセルを抽出する
                var keyCellIIndexes = ws.ExtractMembersOfAlliance(4, houseCellIndexes, keyCellIIndexesBuffer);

                // ハウス内の未確定かつ数字の候補が 4 個以下のセルが 4 個未満の場合、四国同盟が成立する組み合わせは存在しないため、検索を中断する
                if (keyCellIIndexes.Length < 4)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 3; ++keyIndex1)
                {
                    var keyCellIndex1 = keyCellIIndexes.Span[keyIndex1];
                    var keyCellDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 2; ++keyIndex2)
                    {
                        var keyCellIndex2 = keyCellIIndexes.Span[keyIndex2];
                        var keyCellDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 1; ++keyIndex3)
                        {
                            var keyCellIndex3 = keyCellIIndexes.Span[keyIndex3];
                            var keyCellDigitBitSet3 = ws.GetCellNoteDigitBitSet(keyCellIndex3);
                            for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length; ++keyIndex4)
                            {
                                var keyCellIndex4 = keyCellIIndexes.Span[keyIndex4];
                                var keyCellDigitBitSet4 = ws.GetCellNoteDigitBitSet(keyCellIndex4);

                                var keyCellDigitBitSet = keyCellDigitBitSet1 | keyCellDigitBitSet2 | keyCellDigitBitSet3 | keyCellDigitBitSet4;
                                if (keyCellDigitBitSet.GetPopCount() == 4)
                                {
                                    // この時点で、keyCellIndex1 と keyCellIndex2 と keyCellIndex3 と keyCellIndex4 が示す四つのセルの数字の候補の和集合の大きさが 4 である、つまり四国同盟が成立している。
                                    // ただし、四国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                    // 四国同盟に含まれるセルの数字を求める
                                    var (keyCellDigitA, keyCellDigitB, keyCellDigitC, keyCellDigitD) = keyCellDigitBitSet.SplitAsQuadCellDigits();

                                    // 四国同盟によって排除されるセルの数字の候補を求める
                                    var keyCellIndexBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet();
                                    var excludedCellHouseMask =
                                        SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex1)
                                        & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex2)
                                        & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex3)
                                        & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex4);
#if DEBUG
                                    System.Diagnostics.Debug.Assert(excludedCellHouseMask == houseMask, "四国同盟を構成する四つのセルが共有できるハウスは元々のハウスしかない");
#endif
                                    var excludedCellIndexBitSetA = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitA) & ~keyCellIndexBitSet;
                                    var excludedCellIndexBitSetB = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitB) & ~keyCellIndexBitSet;
                                    var excludedCellIndexBitSetC = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitC) & ~keyCellIndexBitSet;
                                    var excludedCellIndexBitSetD = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitD) & ~keyCellIndexBitSet;

                                    var excludedCellNoteBitSet = excludedCellIndexBitSetA | excludedCellIndexBitSetB | excludedCellIndexBitSetC | excludedCellIndexBitSetD;
                                    if (excludedCellNoteBitSet != 0)
                                    {
                                        // この時点で、四国同盟によって排除されるセルの数字の候補が存在する

                                        // 四国同盟に含まれるセルの位置と数字を求める
                                        var highlightedCellNoteBitSetA = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitA) & keyCellIndexBitSet;
                                        var highlightedCellNoteBitSetB = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitB) & keyCellIndexBitSet;
                                        var highlightedCellNoteBitSetC = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitC) & keyCellIndexBitSet;
                                        var highlightedCellNoteBitSetD = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitD) & keyCellIndexBitSet;
                                        var highlightedCellNotes =
                                            highlightedCellNoteBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                            .Concat(highlightedCellNoteBitSetB.EnumerateAsCellNotes(keyCellDigitB))
                                            .Concat(highlightedCellNoteBitSetC.EnumerateAsCellNotes(keyCellDigitC))
                                            .Concat(highlightedCellNoteBitSetD.EnumerateAsCellNotes(keyCellDigitD));
#if DEBUG
                                        System.Diagnostics.Debug.Assert(highlightedCellNotes.Count() >= 4);
#endif

                                        // 四国同盟によって排除されるセルの位置と数字の候補を求める
                                        var excludedCellNotes =
                                            excludedCellIndexBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                            .Concat(excludedCellIndexBitSetB.EnumerateAsCellNotes(keyCellDigitB))
                                            .Concat(excludedCellIndexBitSetC.EnumerateAsCellNotes(keyCellDigitC))
                                            .Concat(excludedCellIndexBitSetD.EnumerateAsCellNotes(keyCellDigitD));
#if DEBUG
                                        System.Diagnostics.Debug.Assert(excludedCellNotes.Any());
#endif

                                        var houseName = houseNameGetter();
                                        var keyCellPositionsText = string.Join(", ", keyCellIndexBitSet.EnumerateAsCellPositionTexts());
                                        var keyCellDigitCharsText =
                                            string.Join(
                                                ", ",
                                                (keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet() | keyCellDigitC.ToCellDigitBitSet() | keyCellDigitD.ToCellDigitBitSet())
                                                    .EnumerateAsCellDigitTexts());
                                        var description =
                                            $"{houseName}において、{keyCellPositionsText} の四つのマスの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                            $"これら四つの数字はこれら四つのマスの何れかにしか入り得ません。、\n" +
                                            // 二国同盟や三国同盟と異なり、四個のマスが全て共有するハウスは一種類しかありえないので、排除対象のハウスは houseName のみと限定してよい。
                                            $"したがって、{houseName}の他のマスは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                            $"それらのマスの数字の候補から {keyCellDigitCharsText} を排除します。";
                                        yield return
                                            new NakedQuadOperation(
                                                excludedCellNotes,
                                                keyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                                                    .Concat((excludedCellHouseMask & ~keyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
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
