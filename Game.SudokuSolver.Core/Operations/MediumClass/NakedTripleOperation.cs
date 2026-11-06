using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// 三国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedTripleOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private NakedTripleOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "三国同盟 (Naked Triple)",
                  SudokuBoardDifficultyLevel.Medium,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある三つの数字 D1, D2, D3 と H 内にある三つのセル C1, C2, C3 に対して、
            // C1, C2, C3 が数字の候補として D1, S2, D3 の何れかだけを持つ場合
            // => C1, C2, C3 の数字はそれぞれ D1, D2, D3 の何れかであるので、C1, C2, C3 の全てが共有するハウス内の C1, C2, C3 以外のセルは D1, D2, D3 の何れでもあり得ない。

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
                            block.ToHouseName(),
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
                            row.ToHouseName(),
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
                            column.ToHouseName(),
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

            static IEnumerable<NakedTripleOperation> MatchPatternByHouse(
                SudokuBoardWorkspace ws,
                UInt128 houseMask,
                string houseName,
                ReadOnlyMemory<SudokuBoardCellIndex> houseCellIndexes,
                Memory<SudokuBoardCellIndex> keyCellIIndexesBuffer)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(houseCellIndexes.Length <= keyCellIIndexesBuffer.Length);
#endif
                var houseNoteMask = ~ws.DeterminedCellBitSet & houseMask;

                // ハウス内の未確定セルの数が 4 未満の場合、三国同盟が成立しないかあるいは仮に成立したとしても数字の候補を排除すべき他の未確定セルが存在しないため、検索を中断する
                if (UInt128.PopCount(houseNoteMask) < 4)
                    yield break;

                // 三国同盟の可能性があるセルを抽出する
                var keyCellIIndexes = ws.ExtractMembersOfAlliance(3, houseCellIndexes, keyCellIIndexesBuffer);

                // ハウス内の未確定かつ数字の候補が 3 個以下のセルが 3 個未満の場合、三国同盟が成立する組み合わせは存在しないため、検索を中断する
                if (keyCellIIndexes.Length < 3)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 2; ++keyIndex1)
                {
                    var keyCellIndex1 = keyCellIIndexes.Span[keyIndex1];
                    var keyCellDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 1; ++keyIndex2)
                    {
                        var keyCellIndex2 = keyCellIIndexes.Span[keyIndex2];
                        var keyCellDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length; ++keyIndex3)
                        {
                            var keyCellIndex3 = keyCellIIndexes.Span[keyIndex3];
                            var keyCellDigitBitSet3 = ws.GetCellNoteDigitBitSet(keyCellIndex3);

                            var keyCellDigitBitSet = keyCellDigitBitSet1 | keyCellDigitBitSet2 | keyCellDigitBitSet3;
                            if (keyCellDigitBitSet.GetPopCount() == 3)
                            {
                                // この時点で、keyCellIndex1 と keyCellIndex2 と cellIndex3 が示す三つのセルの数字の候補の和集合の大きさが 3 である、つまり三国同盟が成立している。
                                // ただし、三国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                // 三国同盟に含まれるセルの数字を求める
                                var (keyCellDigitA, keyCellDigitB, keyCellDigitC) = keyCellDigitBitSet.SplitAsTripleCellDigits();

                                // 三国同盟によって排除出来るセルの数字の候補を求める
                                var keyCellIndexBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet();
                                var excludedCellHouseMask =
                                    SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex1)
                                    & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex2)
                                    & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex3);
                                var excludedCellIndexBitSetA = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitA) & ~keyCellIndexBitSet;
                                var excludedCellIndexBitSetB = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitB) & ~keyCellIndexBitSet;
                                var excludedCellIndexBitSetC = excludedCellHouseMask & ws.GetCellIndexBitSet(keyCellDigitC) & ~keyCellIndexBitSet;

                                var excludedCellIndexBitSet = excludedCellIndexBitSetA | excludedCellIndexBitSetB | excludedCellIndexBitSetC;
                                if (excludedCellIndexBitSet != 0)
                                {
                                    // この時点で、三国同盟によって排除出来るセルの数字の候補が存在する

                                    // 三国同盟に含まれるセルの位置と数字を求める
                                    var highlightedCellIndexBitSetA = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitA) & keyCellIndexBitSet;
                                    var highlightedCellIndexBitSetB = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitB) & keyCellIndexBitSet;
                                    var highlightedCellIndexBitSetC = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitC) & keyCellIndexBitSet;
                                    var highlightedCellNotes =
                                        highlightedCellIndexBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                        .Concat(highlightedCellIndexBitSetB.EnumerateAsCellNotes(keyCellDigitB))
                                        .Concat(highlightedCellIndexBitSetC.EnumerateAsCellNotes(keyCellDigitC));
#if DEBUG
                                    System.Diagnostics.Debug.Assert(highlightedCellNotes.Count() >= 3);
#endif

                                    // 三国同盟によって排除出来るセルの位置と数字の候補を求める
                                    var excludedCellNotes =
                                        excludedCellIndexBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                        .Concat(excludedCellIndexBitSetB.EnumerateAsCellNotes(keyCellDigitB))
                                        .Concat(excludedCellIndexBitSetC.EnumerateAsCellNotes(keyCellDigitC));
#if DEBUG
                                    System.Diagnostics.Debug.Assert(excludedCellNotes.Any());
#endif

                                    yield return
                                        new NakedTripleOperation(
                                            excludedCellNotes,
                                            keyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                                                .Concat((excludedCellHouseMask & ~keyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                            highlightedCellNotes.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                            GetDescriptionBuilder(
                                                houseName,
                                                keyCellDigitA,
                                                keyCellDigitB,
                                                keyCellDigitC,
                                                keyCellIndexBitSet));
                                }
                            }
                        }
                    }
                }

                static Func<string> GetDescriptionBuilder(
                    string houseName,
                    SudokuBoardCellDigit keyCellDigitA,
                    SudokuBoardCellDigit keyCellDigitB,
                    SudokuBoardCellDigit keyCellDigitC,
                    UInt128 keyCellIndexBitSet)
                {
                    return
                        () =>
                        {
                            var keyCellPositionsText = string.Join(", ", keyCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var keyCellDigitCharsText =
                                string.Join(
                                    ", ",
                                    (keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet() | keyCellDigitC.ToCellDigitBitSet())
                                        .EnumerateAsCellDigitTexts());
                            return
                                $"{houseName}において、{keyCellPositionsText} の三つのマスの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                $"これら三つの数字はこれら三つのマスの何れかにしか入り得ません。、\n" +
                                $"したがって、{keyCellPositionsText} の三つのマスを共に含むハウスの他のマスは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                $"それらのマスの数字の候補から {keyCellDigitCharsText} を排除します。";
                        };
                }
            }
        }
    }
}
