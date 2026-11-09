using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// 二国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedPairOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private NakedPairOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "二国同盟 (Naked Pair)",
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
            // あるハウス H と ある二つの数字 D1, D2 と H 内にある二つのセル C1, C2 に対して、
            // C1, C2 が数字の候補として D1, S2 の何れかだけを持つ場合
            // => C1, C2 の数字はそれぞれ D1, D2 の何れかであるので、H 内の C1, C2 以外のセルは D1, D2 の何れでもあり得ない。

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

            static IEnumerable<NakedPairOperation> MatchPatternByHouse(
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

                // ハウス内の未確定セルの数が 3 未満の場合、二国同盟が成立しないかあるいは仮に成立したとしても数字の候補を排除可能な他の未確定セルが存在しないため、検索を中断する
                if (UInt128.PopCount(houseNoteMask) < 3)
                    yield break;

                // 二国同盟の可能性があるセルを抽出する
                var keyCellIIndexes = ws.ExtractMembersOfAlliance(2, houseCellIndexes, keyCellIIndexesBuffer);

                // ハウス内の未確定かつ数字の候補が 2 個以下のセルが 2 個未満の場合、二国同盟が成立する組み合わせは存在しないため、検索を中断する
                if (keyCellIIndexes.Length < 2)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 1; ++keyIndex1)
                {
                    var keyCellIndex1 = keyCellIIndexes.Span[keyIndex1];
                    var keyCellDigitBitSet1 = ws.GetCellNoteDigitBitSet(keyCellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length; ++keyIndex2)
                    {
                        var keyCellIndex2 = keyCellIIndexes.Span[keyIndex2];
                        var keyCellDigitBitSet2 = ws.GetCellNoteDigitBitSet(keyCellIndex2);

                        var keyCellDigitBitSet = keyCellDigitBitSet1 | keyCellDigitBitSet2;
                        if (keyCellDigitBitSet.GetPopCount() == 2)
                        {
                            // この時点で、keyCellIndex1 と keyCellIndex2 が示す二つのセルの数字の候補の和集合の大きさが 2 である、つまり二国同盟が成立している。
                            // ただし、二国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                            // 二国同盟に含まれるセルの数字を求める
                            var (keyCellDigitA, keyCellDigitB) = keyCellDigitBitSet.SplitAsPairCellDigits();

                            // 二国同盟によって排除可能なセルの数字の候補を求める
                            var keyCellIndexBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet();
                            var excludedCellNoteBitSetA = houseMask & ws.GetCellIndexBitSet(keyCellDigitA) & ~keyCellIndexBitSet;
                            var excludedCellNoteBitSetB = houseMask & ws.GetCellIndexBitSet(keyCellDigitB) & ~keyCellIndexBitSet;

                            var excludedCellNoteBitSet = excludedCellNoteBitSetA | excludedCellNoteBitSetB;
                            if (excludedCellNoteBitSet != 0)
                            {
                                // この時点で、二国同盟によって排除可能なセルの候補が存在する

                                // 二国同盟に含まれるセルの位置と数字を求める
                                var highlightedCellNoteBitSetA = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitA) & keyCellIndexBitSet;
                                var highlightedCellNoteBitSetB = houseNoteMask & ws.GetCellIndexBitSet(keyCellDigitB) & keyCellIndexBitSet;
                                var highlightedCellNotes =
                                    highlightedCellNoteBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                    .Concat(highlightedCellNoteBitSetB.EnumerateAsCellNotes(keyCellDigitB));
#if DEBUG
                                System.Diagnostics.Debug.Assert(highlightedCellNotes.Count() >= 2);
#endif

                                // 二国同盟によって排除可能なセルの位置と数字の候補を求める
                                var excludedCellNotes =
                                    excludedCellNoteBitSetA.EnumerateAsCellNotes(keyCellDigitA)
                                    .Concat(excludedCellNoteBitSetB.EnumerateAsCellNotes(keyCellDigitB));
#if DEBUG
                                System.Diagnostics.Debug.Assert(highlightedCellNotes.Any());
#endif

                                yield return
                                    new NakedPairOperation(
                                        excludedCellNotes,
                                        keyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                                            .Concat((houseMask & ~keyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                        highlightedCellNotes.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                        GetDescriptionBuilder(
                                            houseName,
                                            keyCellDigitA,
                                            keyCellDigitB,
                                            keyCellIndexBitSet));
                            }
                        }
                    }
                }

                static Func<string> GetDescriptionBuilder(
                    string houseName,
                    SudokuBoardCellDigit keyCellDigitA,
                    SudokuBoardCellDigit keyCellDigitB,
                    UInt128 keyCellIndexBitSet)
                {
                    return
                        () =>
                        {
                            var keyCellPositionsText = string.Join(", ", keyCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var keyCellDigitCharsText =
                                string.Join(
                                    ", ",
                                    (keyCellDigitA.ToCellDigitBitSet() | keyCellDigitB.ToCellDigitBitSet())
                                        .EnumerateAsCellDigitTexts());
                            return
                                $"{houseName}において、{keyCellPositionsText} の二つのマスの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                $"これら二つの数字はこれら二つのマスの何れかにしか入り得ません。、\n" +
                                $"したがって、{houseName}の他のマスは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                $"それらのマスの数字の候補から {keyCellDigitCharsText} を排除します。";
                        };
                }
            }
        }
    }
}
