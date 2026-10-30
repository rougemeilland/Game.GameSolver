using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

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
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "二国同盟 (Naked Pair)",
                  SudokuBoardDifficultyLevel.Medium,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある二つの数字 D1, D2 と H 内にある二つのセル C1, C2 に対して、
            // C1, C2 が数字の候補として D1, S2 の何れかだけを持つ場合
            // => C1, C2 の数字はそれぞれ D1, D2 の何れかであるので、H 内の C1, C2 以外のセルは D1, D2 の何れでもあり得ない。

            // ブロックハウスの検索
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                        () => $"{block.ToFriendlyString()}のブロック",
                        SudokuBoardCellGeometry.GetCellIndexes(block).Span);
                if (operation is not null)
                    yield return operation;
            }

            // 行ハウスの検索
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                        () => $"{row.ToFriendlyString()}の行",
                        SudokuBoardCellGeometry.GetCellIndexes(row).Span);
                if (operation is not null)
                    yield return operation;
            }

            // 列ハウスの検索
            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                        () => $"{column.ToFriendlyString()}の列",
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                if (operation is not null)
                    yield return operation;
            }

            static NakedPairOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                // 二国同盟の可能性があるセルを抽出する
                Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[houseCellIndexes.Length];
                var keyCellIIndexes = CopyKeyCellIndexes(ws, houseCellIndexes, keyCellIIndexesBuffer);

                // ハウス内の未確定かつ数字の候補が 2 個以下のセルが 2 個未満の場合、二国同盟が成立する組み合わせは存在しないため、検索を中断する
                if (keyCellIIndexes.Length < 2)
                    return null;

                var houseNoteMask = ~ws.DeterminedCellBitSet & houseMask;

                // ハウス内の未確定セルの数が 3 未満の場合、二国同盟が成立しないかあるいは仮に成立したとしても数字の候補を排除可能な他の未確定セルが存在しないため、検索を中断する
                if (UInt128.PopCount(houseNoteMask) < 3)
                    return null;

                Span<SudokuBoardCellDigit> keyCellDigitBuffer = stackalloc SudokuBoardCellDigit[9];
                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 1; ++keyIndex1)
                {
                    var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                    var keyCellDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length; ++keyIndex2)
                    {
                        var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                        var keyCellDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);

                        var keyCellDigitBitSet = keyCellDigitBitSet1 | keyCellDigitBitSet2;
                        if (keyCellDigitBitSet.GetPopCount() == 2)
                        {
                            // この時点で、keyCellIndex1 と keyCellIndex2 が示す二つのセルの数字の候補の和集合の大きさが 2 である、つまり二国同盟が成立している。
                            // ただし、二国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                            // 二国同盟に含まれるセルの数字を求める
                            var foundCellDigits = keyCellDigitBitSet.EnumerateAsCellDigits(keyCellDigitBuffer);
#if DEBUG
                            System.Diagnostics.Debug.Assert(foundCellDigits.Length == 2);
#endif
                            // 二国同盟に含まれるセルの数字 A
                            var keyCellDigitA = foundCellDigits[0];
                            // 二国同盟に含まれるセルの数字 B
                            var keyCellDigitB = foundCellDigits[1];

                            // 二国同盟によって排除可能なセルの数字の候補を求める
                            var keyCellBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet();
                            var excludedCellNoteBitSetA = houseMask & ws.GetNoteBitSet(keyCellDigitA) & ~keyCellBitSet;
                            var excludedCellNoteBitSetB = houseMask & ws.GetNoteBitSet(keyCellDigitB) & ~keyCellBitSet;

                            var excludedCellNoteBitSet = excludedCellNoteBitSetA | excludedCellNoteBitSetB;
                            if (excludedCellNoteBitSet != 0)
                            {
                                // この時点で、二国同盟によって排除可能なセルの候補が存在する

                                // 二国同盟に含まれるセルの位置と数字を求める
                                var highlightedCellNoteBitSetA = houseNoteMask & ws.GetNoteBitSet(keyCellDigitA) & keyCellBitSet;
                                var highlightedCellNoteBitSetB = houseNoteMask & ws.GetNoteBitSet(keyCellDigitB) & keyCellBitSet;
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

                                var houseName = houseNameGetter();
                                var keyCellPositionsText = string.Join(", ", keyCellBitSet.EnumerateAsCellPositionTexts());
                                var keyCellDigitCharsText =
                                    string.Join(
                                        ", ",
                                        (keyCellDigitA.ToCellNoteBitSet() | keyCellDigitB.ToCellNoteBitSet())
                                            .EnumerateAsCellDigitTexts());
                                var excludedCellPositionsText = string.Join(", ", excludedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings());
                                var description =
                                    $"{houseName}において、{keyCellPositionsText} の二つのマスの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                    $"これら二つの数字はこれら二つのマスの何れかにしか入り得ません。、\n" +
                                    $"したがって、{houseName}の他のマスは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                    $"他のマス ({excludedCellPositionsText}) の数字の候補から {keyCellDigitCharsText} を排除します。";
                                return
                                    new NakedPairOperation(
                                        excludedCellNotes,
                                        keyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted)
                                            .Concat((houseMask & ~keyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                        highlightedCellNotes.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                        description);
                            }
                        }
                    }
                }

                return null;

                static Span<SudokuBoardCellIndex> CopyKeyCellIndexes(SudokuBoardWorkspace ws, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes, Span<SudokuBoardCellIndex> keyCellIIndexesBuffer)
                {
                    // ハウス内のセルの番号の配列から、二国同盟の対象となる可能性があるセル (未確定であり、かつ含まれる数字の候補の数が 2 以下) の番号を抽出する
                    var length = 0;
                    for (var index = 0; index < houseCellIndexes.Length; ++index)
                    {
                        if (!ws.IsDetermined(houseCellIndexes[index]) && ws.GetNoteBitSet(houseCellIndexes[index]).GetPopCount() <= 2)
                            keyCellIIndexesBuffer[length++] = houseCellIndexes[index];
                    }

                    return keyCellIIndexesBuffer[..length];
                }
            }
        }
    }
}
