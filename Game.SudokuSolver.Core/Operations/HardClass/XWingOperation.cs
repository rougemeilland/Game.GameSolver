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
            //    したがって、数字の候補を排除できる。
            //    a) C1, C2 と同じハウスのセル (注3) に数字の候補 D
            //    b) C1, C3 と同じハウスのセル (注4) に数字の候補 D
            //    c) C2, C4 と同じハウスのセル (注5) に数字の候補 D
            //    d) C3, C4 と同じハウスのセル (注6) に数字の候補 D
            // 
            // 注1) C1 と C4 は異なる行かつ異なる列にあるため共通に属するハウスがあるとすればブロックハウスしかない。したがって「C1 と C4 は異なるブロックに存在する」と言い換えることもできる。
            // 注2) C2 と C3 は異なる行かつ異なる列にあるため共通に属するハウスがあるとすればブロックハウスしかない。したがって「C2 と C3 は異なるブロックに存在する」と言い換えることもできる。
            // 注3) C1 と C2 は同じ行にあり、かつその行に数字 D を持つセルは他に存在しないため、「C1 と C2 の共通のブロックに存在するセル」と言い換えることもできる。ただし、C1 と C2 が必ずしも同じブロックにあるわけではないことに注意すること。
            // 注4) C1 と C3 は同じ列にあるため、「C1 と C3 と共通の行またはブロックに存在するセル」と言い換えることもできる。ただし、C1 と C3 が必ずしも同じブロックにあるわけではないことに注意すること。
            // 注5) C2 と C4 は同じ列にあるため、「C2 と C4 と共通の行またはブロックに存在するセル」と言い換えることもできる。ただし、C2 と C4 が必ずしも同じブロックにあるわけではないことに注意すること。
            // 注6) C3 と C4 は同じ行にあり、かつその行に数字 D を持つセルは他に存在しないため、「C3 と C4 の共通のブロックに存在するセル」と言い換えることもできる。ただし、C3 と C4 が必ずしも同じブロックにあるわけではないことに注意すること。
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
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column13),
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column24),
                                    rowHouse34,
                                    row12.ToHouseName(),
                                    column13.ToHouseName(),
                                    column24.ToHouseName(),
                                    row34.ToHouseName(),
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
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(row13),
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(row24),
                                    columnHouse34,
                                    column12.ToHouseName(),
                                    row13.ToHouseName(),
                                    row24.ToHouseName(),
                                    column34.ToHouseName(),
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
                UInt128 houseMask12,
                UInt128 houseMask13,
                UInt128 houseMask24,
                UInt128 houseMask34,
                string houseName12,
                string houseName13,
                string houseName24,
                string houseName34,
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4)
            {
                var allKeyCellBitSet = keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet();

                var (_, _, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                var (_, _, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                var (_, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
#if DEBUG
#pragma warning disable IDE0047 // 不要なかっこの削除
                System.Diagnostics.Debug.Assert(block1 != block4);
                System.Diagnostics.Debug.Assert(block2 != block3);
                System.Diagnostics.Debug.Assert((block1 == block2) == (block3 == block4));
                System.Diagnostics.Debug.Assert((block1 == block3) == (block2 == block4));
#pragma warning restore IDE0047 // 不要なかっこの削除
#endif
                var excludableCellsRegionBitMask =
                    GetExcludableCellsRegionBitMask(keyCellIndex1, keyCellIndex2, keyCellIndex3, keyCellIndex4, allKeyCellBitSet)
                    & ~houseMask12
                    & ~houseMask34;
                var excludedCellsBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludedCellsBitSet == 0)
                    return null;

                // X-Wing により排除されるセルの数字の候補が存在する。

                return
                    new XWingOperation(
                        excludedCellsBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(((houseMask12 | houseMask34) & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            houseMask13,
                            houseMask24,
                            houseName12,
                            houseName13,
                            houseName24,
                            houseName34,
                            keyCellIndex1,
                            keyCellIndex2,
                            keyCellIndex3,
                            keyCellIndex4,
                            block1,
                            block2,
                            block3,
                            block4));

                static UInt128 GetExcludableCellsRegionBitMask(
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    SudokuBoardCellIndex keyCellIndex4,
                    UInt128 allKeyCellBitSet)
                {
                    var visibleRegionFromCellIndex1 = SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex1);
                    var visibleRegionFromCellIndex2 = SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex2);
                    var visibleRegionFromCellIndex3 = SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex3);
                    var visibleRegionFromCellIndex4 = SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(keyCellIndex4);
                    var edcludableCellsRegionBitMask12 = visibleRegionFromCellIndex1 & visibleRegionFromCellIndex2;
                    var edcludableCellsRegionBitMask13 = visibleRegionFromCellIndex1 & visibleRegionFromCellIndex3;
                    var edcludableCellsRegionBitMask24 = visibleRegionFromCellIndex2 & visibleRegionFromCellIndex4;
                    var edcludableCellsRegionBitMask34 = visibleRegionFromCellIndex3 & visibleRegionFromCellIndex4;
                    return (edcludableCellsRegionBitMask12 | edcludableCellsRegionBitMask13 | edcludableCellsRegionBitMask24 | edcludableCellsRegionBitMask34) & ~allKeyCellBitSet;
                }

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    UInt128 houseMask13,
                    UInt128 houseMask24,
                    string houseName12,
                    string houseName13,
                    string houseName24,
                    string houseName34,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    SudokuBoardCellIndex keyCellIndex4,
                    SudokuBoardCellBlock block1,
                    SudokuBoardCellBlock block2,
                    SudokuBoardCellBlock block3,
                    SudokuBoardCellBlock block4)
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
                                $"{houseName12}では数字 {digitText} の候補は {cell1PositionText} と {cell2PositionText} のマスにしか存在しないので、数字 {digitText} は必ずどちらかのマスだけに入ります。 - (1)\n" +
                                $"{houseName34}では数字 {digitText} の候補は {cell3PositionText} と {cell4PositionText} のマスにしか存在しないので、数字 {digitText} は必ずどちらかのマスだけに入ります。 - (2)\n" +
                                $"{cell1PositionText} と {cell3PositionText} のマスは同じ{houseName13}に存在するので、両方のマスに数字 {digitText} が入ることはあり得ません。 - (3)\n" +
                                $"(1), (2), (3) から、{cell2PositionText} と {cell4PositionText} のマスは必ずどちらかだけが数字 {digitText} になります。 - (4)\n" +
                                $"(2), (3), (4) から、{cell1PositionText} と {cell3PositionText} のマスも必ずどちらかだけが数字 {digitText} になります。 - (5)\n" +
                                $"したがって、以下に述べるマスの数字は {digitText} にはなり得ませんので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                $"・(5) から、{cell1PositionText} と {cell3PositionText} を除く{(block1 == block3 ? $"{houseMask13}および{block1.ToHouseName()}" : houseName13)}の全てのマスは {digitText} になり得ない。\n" +
                                $"・(4) から、{cell2PositionText} と {cell4PositionText} を除く{(block2 == block4 ? $"{houseMask24}および{block2.ToHouseName()}" : houseName24)}の全てのマスは {digitText} になり得ない。" +
                                $"{(block1 == block2 ? $"\n・(1) から、{cell1PositionText} と {cell2PositionText} を除く{block1.ToHouseName()}の全てのマスは {digitText} になり得ない。" : "")}" +
                                $"{(block3 == block4 ? $"\n・(2) から、{cell3PositionText} と {cell4PositionText} を除く{block3.ToHouseName()}の全てのマスは {digitText} になり得ない。" : "")}";
                        };
                }
            }
        }
    }
}
