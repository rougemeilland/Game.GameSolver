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
                  "二国同盟",
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

                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 1; ++keyIndex1)
                {
                    var cellIndex1 = keyCellIIndexes[keyIndex1];
                    var cellDigitBitSet1 = ws.GetNoteBitSet(cellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length; ++keyIndex2)
                    {
                        var cellIndex2 = keyCellIIndexes[keyIndex2];
                        var cellDigitBitSet2 = ws.GetNoteBitSet(cellIndex2);

                        var cellDigitBitSet = cellDigitBitSet1 | cellDigitBitSet2;
                        if (cellDigitBitSet.GetPopCount() == 2)
                        {
                            // この時点で、cellIndex1 と cellIndex2 が示す二つのセルの数字の候補の和集合の大きさが 2 である、つまり二国同盟が成立している。
                            // ただし、二国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                            // 二国同盟に含まれるセルの数字を求める
                            var foundCellDigits = cellDigitBitSet.EnumerateAsCellDigits().ToArray();
#if DEBUG
                            System.Diagnostics.Debug.Assert(foundCellDigits.Length == 2);
#endif
                            // 二国同盟に含まれるセルの数字 A
                            var cellDigitA = foundCellDigits[0];
                            // 二国同盟に含まれるセルの数字 B
                            var cellDigitB = foundCellDigits[1];

                            // 二国同盟によって排除可能なセルの数字の候補を求める
                            var keyCellBitSet = cellIndex1.ToCellIndexBitSet() | cellIndex2.ToCellIndexBitSet();
                            var excludedCellNoteBitSetA = houseMask & ws.GetNoteBitSet(cellDigitA) & ~keyCellBitSet;
                            var excludedCellNoteBitSetB = houseMask & ws.GetNoteBitSet(cellDigitB) & ~keyCellBitSet;

                            var excludedCellNoteBitSet = excludedCellNoteBitSetA | excludedCellNoteBitSetB;
                            if (excludedCellNoteBitSet != 0)
                            {
                                // この時点で、二国同盟によって排除可能なセルの候補が存在する

                                // 二国同盟に含まれるセルの位置と数字を求める
                                var highlightedCellNoteBitSetA = houseNoteMask & ws.GetNoteBitSet(cellDigitA) & keyCellBitSet;
                                var highlightedCellNoteBitSetB = houseNoteMask & ws.GetNoteBitSet(cellDigitB) & keyCellBitSet;
                                var highlightedCellNotes =
                                    highlightedCellNoteBitSetA.EnumerateAsCellNotes(cellDigitA)
                                    .Concat(highlightedCellNoteBitSetB.EnumerateAsCellNotes(cellDigitB))
                                    .ToArray();
#if DEBUG
                                System.Diagnostics.Debug.Assert(highlightedCellNotes.Length >= 2);
#endif

                                // 二国同盟によって排除可能なセルの位置と数字の候補を求める
                                var excludedCellNotes =
                                    excludedCellNoteBitSetA.EnumerateAsCellNotes(cellDigitA)
                                    .Concat(excludedCellNoteBitSetB.EnumerateAsCellNotes(cellDigitB))
                                    .ToArray();
#if DEBUG
                                System.Diagnostics.Debug.Assert(highlightedCellNotes.Length > 0);
#endif

                                var houseName = houseNameGetter();
                                var cellPositionText1 = cellIndex1.ToFriendlyString();
                                var cellPositionText2 = cellIndex2.ToFriendlyString();
                                var cellDigitCharA = cellDigitA.ToCellDigitChar();
                                var cellDigitCharB = cellDigitB.ToCellDigitChar();
                                var keyCellPositionsText = $"{cellPositionText1}, {cellPositionText2}";
                                var keyCellDigitCharsText = $"'{cellDigitCharA}', '{cellDigitCharB}'";
                                var excludedCellPositionsText = string.Join(", ", excludedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings());
                                var description =
                                    $"{houseName}において、{keyCellPositionsText} の二つのセルの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                    $"これら二つのセルの数字は必ずこれら二つの数字の何れかです。\n" +
                                    $"したがって、{houseName}の他のセルは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                    $"他のセル ({excludedCellPositionsText}) の数字の候補から {keyCellDigitCharsText} を排除します。";
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
