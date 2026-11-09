using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// X-ウィングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class XWingOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private XWingOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "X-ウィング (X-Wing)",
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
            // ある数字 D について、
            // 1) ある行にセル C1, C2 の何れの候補にも D が含まれており、かつ
            // 2) C1, C2 と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 3) 別の行のセル  C3, C4 の何れの候補にも D が含まれており、かつ
            // 4) C3, C4 と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 5) C1, C3 は同じ列に存在し、かつ
            // 6) C2, C4 は同じ列に存在し、かつ
            // 7) C1 と C4 が共通に属するハウスは存在せず (注1) 、かつ
            // 8) C2 と C3 が共通に属するハウスは存在しない (注2) 場合。
            // => C1, C2 の数字は何れかだけが必ず D であり、C3, C4 の数字も何れかだけが必ず D である。
            //    そして、C1, C3 は同時には D になり得ないので、C2, C4 の何れかだけが必ず D となる。そのため、C1, C3 も何れかだけが D となる。
            //    したがって、以下のセルの数字の候補から D を排除できる。
            //    a) C1, C3 と同じ列の C1, C3 以外のセル
            //    b) C2, C4 と同じ列の C2, C4 以外のセル
            // 
            // 注1) C1 と C4 は異なる行かつ異なる列にあるため共通に属するハウスがあるとすればブロックハウスしかない。したがって「C1 と C4 は異なるブロックに存在する」と言い換えることもできる。
            // 注2) C2 と C3 は異なる行かつ異なる列にあるため共通に属するハウスがあるとすればブロックハウスしかない。したがって「C2 と C3 は異なるブロックに存在する」と言い換えることもできる。
            //
            // ※上記の論拠は、行と列を言い換えても成立する。

            // 行ハウスに含まれる数字が 2 個のみの場合
            {
                var keyRowsBuffer = ArrayPool<(SudokuBoardCellRow row, int cellCount)>.Shared.Rent(9);
                try
                {
                    for (var digit = SudokuBoardCellDigit.MinValue; digit <= SudokuBoardCellDigit.MaxValue; ++digit)
                    {
                        foreach (var operation in MatchPatternByRowHouse(digit, ws.GetCellIndexBitSet(digit), keyRowsBuffer))
                            yield return operation;
                    }
                }
                finally
                {
                    ArrayPool<(SudokuBoardCellRow row, int cellCount)>.Shared.Return(keyRowsBuffer);
                }
            }

            // 列ハウスに含まれる数字が 2 個のみの場合
            {
                var keyColumnsBuffer = ArrayPool<(SudokuBoardCellColumn column, int cellCount)>.Shared.Rent(9);
                try
                {
                    for (var digit = SudokuBoardCellDigit.MinValue; digit <= SudokuBoardCellDigit.MaxValue; ++digit)
                    {
                        foreach (var operation in MatchPatternByColumnHouse(digit, ws.GetCellIndexBitSet(digit), keyColumnsBuffer))
                            yield return operation;
                    }
                }
                finally
                {
                    ArrayPool<(SudokuBoardCellColumn column, int cellCount)>.Shared.Return(keyColumnsBuffer);
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByRowHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellRow row, int cellCount)> keyRowsBuffer)
            {
                // digit を候補に持つセルが 2 個以上ある行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 2);

                // digit を候補に持つセルが 2 個ある行が 2 個未満である場合は X-Wing が成立しないので、検索を中断する。
                if (keyRows.Length < 2)
                    yield break;

                for (var keyIndex12 = 0; keyIndex12 < keyRows.Length - 1; ++keyIndex12)
                {
                    var row12 = keyRows.Span[keyIndex12].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex12].cellCount == 2);
#endif
                    var rowHouse12 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row12);
                    var keyCellIndexBitSet12 = allCellIndexBitSet & rowHouse12;

#if DEBUG
                    System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet12) == 2);
#endif
                    // 行 row12 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex2 である。
                    var (keyCellIndex1, keyCellIndex2) = keyCellIndexBitSet12.SplitAsPairCellIndexes();

                    for (var keyIndex34 = keyIndex12 + 1; keyIndex34 < keyRows.Length; ++keyIndex34)
                    {
                        var row34 = keyRows.Span[keyIndex34].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex34].cellCount == 2);
#endif
                        var rowHouse34 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row34);
                        var keyCellIndexBitSet34 = allCellIndexBitSet & rowHouse34;

#if DEBUG
                        System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet34) == 2);
#endif
                        // 行 row34 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex1 である。
                        var (keyCellIndex3, keyCellIndex4) = keyCellIndexBitSet34.SplitAsPairCellIndexes();

                        if (IsFormedXWingByRowHouse(keyCellIndex1, keyCellIndex2, keyCellIndex3, keyCellIndex4, out var column13, out var column24))
                        {
                            var operation =
                                MatchPatternByHouse(
                                    digit,
                                    allCellIndexBitSet,
                                    rowHouse12,
                                    rowHouse34,
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column13),
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column24),
                                    row12.ToHouseName(),
                                    row34.ToHouseName(),
                                    column13.ToHouseName(),
                                    column24.ToHouseName(),
                                    keyCellIndex1,
                                    keyCellIndex2,
                                    keyCellIndex3,
                                    keyCellIndex4);
                            if (operation is not null)
                                yield return operation;
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> keyColumnsBuffer)
            {
                // digit を候補に持つセルが 2 個以上ある列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 2);

                // digit を候補に持つセルが 2 個ある列が 2 個未満である場合は X-Wing が成立しないので、検索を中断する。
                if (keyColumns.Length < 2)
                    yield break;

                for (var keyIndex12 = 0; keyIndex12 < keyColumns.Length - 1; ++keyIndex12)
                {
                    var column12 = keyColumns.Span[keyIndex12].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex12].cellCount == 2);
#endif
                    var columnHouse12 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column12);
                    var keyCellIndexBitSet12 = allCellIndexBitSet & columnHouse12;

#if DEBUG
                    System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet12) == 2);
#endif
                    // 列 column12 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex2 である。
                    var (keyCellIndex1, keyCellIndex2) = keyCellIndexBitSet12.SplitAsPairCellIndexes();

                    for (var keyIndex34 = keyIndex12 + 1; keyIndex34 < keyColumns.Length; ++keyIndex34)
                    {
                        var column34 = keyColumns.Span[keyIndex34].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex34].cellCount == 2);
#endif
                        var columnHouse34 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column34);
                        var keyCellIndexBitSet34 = allCellIndexBitSet & columnHouse34;

#if DEBUG
                        System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet34) == 2);
#endif
                        // 列 column34 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex1 である。
                        var (keyCellIndex3, keyCellIndex4) = keyCellIndexBitSet34.SplitAsPairCellIndexes();

                        if (IsFormedXWingByColumnHouse(keyCellIndex1, keyCellIndex2, keyCellIndex3, keyCellIndex4, out var row13, out var row24))
                        {
                            var operation =
                                MatchPatternByHouse(
                                    digit,
                                    allCellIndexBitSet,
                                    columnHouse12,
                                    columnHouse34,
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(row13),
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(row24),
                                    column12.ToHouseName(),
                                    column34.ToHouseName(),
                                    row13.ToHouseName(),
                                    row24.ToHouseName(),
                                    keyCellIndex1,
                                    keyCellIndex2,
                                    keyCellIndex3,
                                    keyCellIndex4);
                            if (operation is not null)
                                yield return operation;
                        }
                    }
                }
            }

            static bool IsFormedXWingByRowHouse(
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4,
                out SudokuBoardCellColumn column13,
                out SudokuBoardCellColumn column24)
            {
                // 1) keyCellIndex1 のセルの列と keyCellIndex3 のセルの列は等しく、かつ
                // 2) keyCellIndex2 のセルの列と keyCellIndex4 のセルの列は等しく、かつ
                // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくない。
                // => X-Wing が成立している。

                var (column1, _, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                var (column2, _, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                var (column3, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                var (column4, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                column13 = column1;
                column24 = column2;
                return column1 == column3 && column2 == column4 && block1 != block4 && block2 != block3;
            }

            static bool IsFormedXWingByColumnHouse(
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4,
                out SudokuBoardCellRow row13,
                out SudokuBoardCellRow row24)
            {
                // 1) keyCellIndex1 のセルの行と keyCellIndex3 のセルの行は等しく、かつ
                // 2) keyCellIndex2 のセルの行と keyCellIndex4 のセルの行は等しく、かつ
                // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくない。
                // => X-Wing が成立している。
                var (_, row1, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                var (_, row2, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                var (_, row3, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                var (_, row4, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                row13 = row1;
                row24 = row2;
                return row1 == row3 && row2 == row4 && block1 != block4 && block2 != block3;
            }

            static XWingOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                UInt128 allCellIndexBitSet,
                UInt128 baseLineHouseMask12,
                UInt128 baseLineHouseMask34,
                UInt128 coverLineHouseMask13,
                UInt128 coverLineHouseMask24,
                string baseLineHouseName12,
                string baseLineHouseName34,
                string coverLineHouseName13,
                string coverLineHouseName24,
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4)
            {
                var allKeyCellBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet();
                var excludableCellsRegionBitMask = (coverLineHouseMask13 | coverLineHouseMask24) & ~(baseLineHouseMask12 | baseLineHouseMask34);
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allKeyCellBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allKeyCellBitSet)} を除去する必要はない。");
#endif
                var excludedCellsBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludedCellsBitSet == 0)
                    return null;

                // X-Wing により排除されるセルの数字の候補が存在する。

                return
                    new XWingOperation(
                        excludedCellsBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(((baseLineHouseMask12 | baseLineHouseMask34) & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            baseLineHouseName12,
                            baseLineHouseName34,
                            coverLineHouseName13,
                            coverLineHouseName24,
                            keyCellIndex1,
                            keyCellIndex2,
                            keyCellIndex3,
                            keyCellIndex4));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    string baseLineHouseName12,
                    string baseLineHouseName34,
                    string coverLineHouseName13,
                    string coverLineHouseName24,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    SudokuBoardCellIndex keyCellIndex4)
                {
                    return
                        () =>
                        {
                            var digitText = digit.ToCellDigitText();
                            var cell1PositionText = keyCellIndex1.ToFriendlyString();
                            var cell2PositionText = keyCellIndex2.ToFriendlyString();
                            var cell3PositionText = keyCellIndex3.ToFriendlyString();
                            var cell4PositionText = keyCellIndex4.ToFriendlyString();
                            return
                                $"{baseLineHouseName12}と{baseLineHouseName34}では、数字 {digitText} を候補として持つマスはそれぞれ 2 個しか存在せず、" +
                                $"かつそれらのマスは {coverLineHouseName13} と {coverLineHouseName24} に収まっています。\n" +
                                $"仮に「{coverLineHouseName13}のマス {cell1PositionText}, {cell3PositionText} の何れにも {digitText} が入らない」と仮定します。\n" +
                                $"{cell1PositionText}に {digitText} が入らないので、{baseLineHouseName12}で {digitText} が入ることが出来るマスは{cell2PositionText}しかありません。\n" +
                                $"同様に、" +
                                $"{cell3PositionText}に {digitText} が入らないので、{baseLineHouseName34}で {digitText} が入ることが出来るマスは{cell4PositionText}しかありません。\n" +
                                $"すると、{coverLineHouseName24}内の二つのマス {cell2PositionText}, {cell4PositionText} の両方に {digitText} が入ることになってしまうので、仮定が誤っていることになります。\n" +
                                $"つまり、" +
                                $"{cell1PositionText}, {cell3PositionText} の何れかには {digitText} が入ります。\n" +
                                $"同様の論法で、" +
                                $"{cell2PositionText}, {cell4PositionText} の何れかにも {digitText} が入ります。\n" +
                                $"したがって、以下に述べるマスには {digitText} は入り得ませんので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                $"・{cell1PositionText} と {cell3PositionText} を除く{coverLineHouseName13}の全てのマス\n" +
                                $"・{cell2PositionText} と {cell4PositionText} を除く{coverLineHouseName24}の全てのマス";
                        };
                }
            }
        }
    }
}
