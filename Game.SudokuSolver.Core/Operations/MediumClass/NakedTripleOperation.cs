using System;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.MediumClass
{
    /// <summary>
    /// 三国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class NakedTripleOperation
        : SudokuBoardOperation
    {
        private NakedTripleOperation(ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base(GameBoardStatus.Solving, "三国同盟", SudokuBoardDifficultyLevel.Medium, null, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, highlightedCellNotes, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static NakedTripleOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある三つの数字 D1, D2, D3 と H 内にある三つのセル C1, C2, C3 に対して、
            // C1, C2, C3 が数字の候補として D1, S2, D3 の何れかだけを持つ場合
            // => C1, C2, C3 の数字はそれぞれ D1, D2, D3 の何れかであるので、H 内の C1, C2, C3 以外のセルは C1, C2, C3 の何れでもあり得ない。

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
                    return null;
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
                    return null;
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
                    return null;
            }

            return null;

            static NakedTripleOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                // 三国同盟の可能性があるセルを抽出する
                Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[houseCellIndexes.Length];
                var keyCellIIndexes = CopyKeyCellIndexes(ws, houseCellIndexes, keyCellIIndexesBuffer);

                // ハウス内の未確定かつ数字の候補が 3 個以下のセルが 3 個未満の場合、三国同盟が成立する組み合わせは存在しないため、検索を中断する
                if (keyCellIIndexes.Length < 3)
                    return null;

                var houseNoteMask = ~ws.DeterminedCellBitSet & houseMask;

                // ハウス内の未確定セルの数が 4 未満の場合、三国同盟が成立しないかあるいは仮に成立したとしても数字の候補を否定するべき他の未確定セルが存在しないため、検索を中断する
                if (UInt128.PopCount(houseNoteMask) < 4)
                    return null;

                for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 2; ++keyIndex1)
                {
                    var cellIndex1 = keyCellIIndexes[keyIndex1];
                    var cellDigitBitSet1 = ws.GetNoteBitSet(cellIndex1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 1; ++keyIndex2)
                    {
                        var cellIndex2 = keyCellIIndexes[keyIndex2];
                        var cellDigitBitSet2 = ws.GetNoteBitSet(cellIndex2);
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length; ++keyIndex3)
                        {
                            var cellIndex3 = keyCellIIndexes[keyIndex3];
                            var cellDigitBitSet3 = ws.GetNoteBitSet(cellIndex3);

                            var cellDigitBitSet = cellDigitBitSet1 | cellDigitBitSet2 | cellDigitBitSet3;
                            if (cellDigitBitSet.GetPopCount() == 3)
                            {
                                // この時点で、cellIndex1 と cellIndex2 と cellIndex3 が示す三つのセルの数字の候補の和集合の大きさが 3 である、つまり三国同盟が成立している。
                                // ただし、三国同盟によって否定可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                // 三国同盟に含まれるセルの数字を求める
                                var foundCellDigits = cellDigitBitSet.EnumerateAsCellDigits().ToArray();
#if DEBUG
                                System.Diagnostics.Debug.Assert(foundCellDigits.Length == 3);
#endif
                                // 三国同盟に含まれるセルの数字 A
                                var cellDigitA = foundCellDigits[0];
                                // 三国同盟に含まれるセルの数字 B
                                var cellDigitB = foundCellDigits[1];
                                // 三国同盟に含まれるセルの数字 C
                                var cellDigitC = foundCellDigits[2];

                                // 三国同盟によって否定されるセルの数字の候補を求める
                                var keyCellBitSet = cellIndex1.ToCellIndexBitSet() | cellIndex2.ToCellIndexBitSet() | cellIndex3.ToCellIndexBitSet();
                                var removedCellNoteBitSetA = houseMask & ws.GetNoteBitSet(cellDigitA) & ~keyCellBitSet;
                                var removedCellNoteBitSetB = houseMask & ws.GetNoteBitSet(cellDigitB) & ~keyCellBitSet;
                                var removedCellNoteBitSetC = houseMask & ws.GetNoteBitSet(cellDigitC) & ~keyCellBitSet;

                                var removedCellNoteBitSet = removedCellNoteBitSetA | removedCellNoteBitSetB | removedCellNoteBitSetC;
                                if (removedCellNoteBitSet != 0)
                                {
                                    // この時点で、三国同盟によって否定されるセルの数字の候補が存在する

                                    // 三国同盟に含まれるセルの位置と数字を求める
                                    var highlightedCellNoteBitSetA = houseNoteMask & ws.GetNoteBitSet(cellDigitA) & keyCellBitSet;
                                    var highlightedCellNoteBitSetB = houseNoteMask & ws.GetNoteBitSet(cellDigitB) & keyCellBitSet;
                                    var highlightedCellNoteBitSetC = houseNoteMask & ws.GetNoteBitSet(cellDigitC) & keyCellBitSet;
                                    var highlightedCellNotes =
                                        highlightedCellNoteBitSetA.EnumerateAsCellIndexes().ToCells(cellDigitA)
                                        .Concat(highlightedCellNoteBitSetB.EnumerateAsCellIndexes().ToCells(cellDigitB))
                                        .Concat(highlightedCellNoteBitSetC.EnumerateAsCellIndexes().ToCells(cellDigitC))
                                        .ToArray();
#if DEBUG
                                    System.Diagnostics.Debug.Assert(highlightedCellNotes.Length >= 3);
#endif

                                    // 三国同盟によって否定されるセルの位置と数字の候補を求める
                                    var removedCellNotes =
                                        removedCellNoteBitSetA.EnumerateAsCellIndexes().ToCells(cellDigitA)
                                        .Concat(removedCellNoteBitSetB.EnumerateAsCellIndexes().ToCells(cellDigitB))
                                        .Concat(removedCellNoteBitSetC.EnumerateAsCellIndexes().ToCells(cellDigitC))
                                        .ToArray();
#if DEBUG
                                    System.Diagnostics.Debug.Assert(removedCellNotes.Length > 0);
#endif

                                    var houseName = houseNameGetter();
                                    var relatedCellPositions = houseMask.EnumerateAsCellIndexes().ToCellPositions().ToArray();
                                    var cellPositionText1 = cellIndex1.ToFriendlyString();
                                    var cellPositionText2 = cellIndex2.ToFriendlyString();
                                    var cellPositionText3 = cellIndex3.ToFriendlyString();
                                    var cellDigitCharA = cellDigitA.ToCellDigitChar();
                                    var cellDigitCharB = cellDigitB.ToCellDigitChar();
                                    var cellDigitCharC = cellDigitC.ToCellDigitChar();
                                    var keyCellPositionsText = $"{cellPositionText1}, {cellPositionText2}, {cellPositionText3}";
                                    var keyCellDigitCharsText = $"'{cellDigitCharA}', '{cellDigitCharB}', '{cellDigitCharC}'";
                                    var removedCellPositionsText = string.Join(", ", removedCellNoteBitSet.EnumerateAsCellIndexes().ToFriendlyStrings());
                                    var description =
                                        $"{houseName}において、{keyCellPositionsText} の三つのセルの数字の候補は {keyCellDigitCharsText} の何れかのみであり、" +
                                        $"これら三つのセルの数字は必ずこれら三つの数字の何れかです。" +
                                        $"したがって、{houseName}の他のセルは数字 {keyCellDigitCharsText} の何れでもあり得ないので、" +
                                        $"他のセル ({removedCellPositionsText}) の数字の候補から {keyCellDigitCharsText} を削除します。";
                                    return
                                        new NakedTripleOperation(
                                            highlightedCellNotes,
                                            removedCellNotes,
                                            relatedCellPositions,
                                            description);
                                }
                            }
                        }
                    }
                }

                return null;

                static Span<SudokuBoardCellIndex> CopyKeyCellIndexes(SudokuBoardWorkspace ws, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes, Span<SudokuBoardCellIndex> keyCellIIndexesBuffer)
                {
                    // ハウス内のセルの番号の配列から、三国同盟の対象となる可能性があるセル (未確定であり、かつ含まれる数字の候補の数が 3 以下) の番号を抽出する
                    var length = 0;
                    for (var index = 0; index < houseCellIndexes.Length; ++index)
                    {
                        if (!ws.IsDetermined(houseCellIndexes[index]) && ws.GetNoteBitSet(houseCellIndexes[index]).GetPopCount() <= 3)
                            keyCellIIndexesBuffer[length++] = houseCellIndexes[index];
                    }

                    return keyCellIIndexesBuffer[..length];
                }
            }
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is not null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            var removedCellNotes = RemovedCellNotes.Span;
            for (var index = 0; index < RemovedCellNotes.Length; ++index)
            {
                var removedCellNote = removedCellNotes[index];
                if (!newCellSet.RemoveNote(removedCellNote.CellIndex, removedCellNote.Digit))
                    throw new ApplicationException();
            }

            return newCellSet;
        }
    }
}
