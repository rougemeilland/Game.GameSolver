using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// ジェリーフィッシュパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class JellyfishOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private JellyfishOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "ジェリーフィッシュ (Jellyfish)",
                  SudokuBoardDifficultyLevel.Extreme,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  descriptionBuilder)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // ある数字 D とある重複しない行 Row1, Row2, Row3, Row4 とある重複しない列 Col1, Col2, Col3, Col4 について、
            // 1) Row1 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 4 個以下であり、かつ
            // 2) Row2 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 4 個以下であり、かつ
            // 3) Row3 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 4 個以下であり、かつ
            // 4) Row4 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 4 個以下であり、かつ
            // 5) Col1 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 6) Col2 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 7) Col3 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 8) Col4 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在しする場合。
            // => まず、以下のことが言える。
            //      a) Row1上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      b) Row2上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      c) Row3上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      d) Row4上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入る。
            //    仮に、Col1 上の Row1, Row2, Row3, Row4 との何れの交差点にあるセルにも数字 D が入らないとした場合、数字 D が入ることができるセルが足りなくなってしまう。
            //    そのため、Col1 上の Row1, Row2, Row3, Row4 との何れかの交差点にあるセルには必ず数字 D が入るので、以下の条件を満たす全てのセルから数字の候補 D を排除できる。
            //      - Col1 上の Row1, Row2, Row3, Row4 の何れかの交差点にある全てのセルと何れかのハウスを共有する他のセルであり、かつ
            //      - Col1, Col2, Col3, Col4 と Row1, Row2, Row3, Row4 との何れの交差点にもないセル。
            //    なお、Col2, Col3, Col4 についても同様である。
            //     
            // ※ Row1, Row2, Row3, Row4 と Col1, Col2, Col3, Col4 の合計 16 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※上記の論拠は、行と列を言い換えても成立する。

            // 行ハウスに含まれる数字が 2 個から 4 個のみの場合
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

            // 列ハウスに含まれる数字が 2 個から 4 個のみの場合
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
                // digit を候補に持つセルが 2 個以上 4 個以下ある行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 4);

                // digit を候補に持つセルが 2 個以上 4 個以下ある行が 4 個未満である場合はジェリーフィッシュが成立しないので、検索を中断する。
                if (keyRows.Length < 4)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 3; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex1].cellCount is >= 2 and <= 4);
#endif
                    var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                    var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= 2 and <= 4);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length - 2; ++keyIndex2)
                    {
                        var keyRow2 = keyRows.Span[keyIndex2].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex2].cellCount is >= 2 and <= 4);
#endif
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= 2 and <= 4);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyRows.Length - 1; ++keyIndex3)
                        {
                            var keyRow3 = keyRows.Span[keyIndex3].row;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex3].cellCount is >= 2 and <= 4);
#endif
                            var keyRowHouse3 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3);
                            var keyRowCellIndexBitSet3 = allCellIndexBitSet & keyRowHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet3) is >= 2 and <= 4);
#endif
                            for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyRows.Length; ++keyIndex4)
                            {
                                var keyRow4 = keyRows.Span[keyIndex4].row;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex4].cellCount is >= 2 and <= 4);
#endif
                                var keyRowHouse4 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4);
                                var keyRowCellIndexBitSet4 = allCellIndexBitSet & keyRowHouse4;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet4) is >= 2 and <= 4);
#endif
                                var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3 | keyRowCellIndexBitSet4).GetUnionByColumn();
                                if (keyColumnBitSet.GetPopCount() == 4)
                                {
                                    // 各行に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4個 の列に収まっている。
                                    // => ジェリーフィッシュの成立

                                    var (keyColumn1, keyColumn2, keyColumn3, keyColumn4) = keyColumnBitSet.SplitAsQuadCellColulmns();
                                    var result =
                                        MatchPatternByHouse(
                                            digit,
                                            allCellIndexBitSet,
                                            keyRowHouse1,
                                            keyRowHouse2,
                                            keyRowHouse3,
                                            keyRowHouse4,
                                            keyRowCellIndexBitSet1,
                                            keyRowCellIndexBitSet2,
                                            keyRowCellIndexBitSet3,
                                            keyRowCellIndexBitSet4,
                                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1),
                                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2),
                                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3),
                                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4),
                                            keyRow1.ToHouseName(),
                                            keyRow2.ToHouseName(),
                                            keyRow3.ToHouseName(),
                                            keyRow4.ToHouseName(),
                                            keyColumn1.ToFriendlyString(),
                                            keyColumn2.ToFriendlyString(),
                                            keyColumn3.ToFriendlyString(),
                                            keyColumn4.ToFriendlyString(),
                                            keyColumn1.ToHouseName(),
                                            keyColumn2.ToHouseName(),
                                            keyColumn3.ToHouseName(),
                                            keyColumn4.ToHouseName());
                                    if (result is not null)
                                        yield return result;
                                }
                            }
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> keyColumnsBuffer)
            {
                // digit を候補に持つセルが 2 個以上 4 個以下ある列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 4);

                // digit を候補に持つセルが 2 個以上 4 個以下ある列が 4 個未満である場合はジェリーフィッシュが成立しないので、検索を中断する。
                if (keyColumns.Length < 4)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 3; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex1].cellCount is >= 2 and <= 4);
#endif
                    var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                    var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= 2 and <= 4);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length - 2; ++keyIndex2)
                    {
                        var keyColumn2 = keyColumns.Span[keyIndex2].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex2].cellCount is >= 2 and <= 4);
#endif
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= 2 and <= 4);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyColumns.Length - 1; ++keyIndex3)
                        {
                            var keyColumn3 = keyColumns.Span[keyIndex3].column;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex3].cellCount is >= 2 and <= 4);
#endif
                            var keyColumnHouse3 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3);
                            var keyColumnCellIndexBitSet3 = allCellIndexBitSet & keyColumnHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet3) is >= 2 and <= 4);
#endif
                            for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyColumns.Length; ++keyIndex4)
                            {
                                var keyColumn4 = keyColumns.Span[keyIndex4].column;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex4].cellCount is >= 2 and <= 4);
#endif
                                var keyColumnHouse4 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4);
                                var keyColumnCellIndexBitSet4 = allCellIndexBitSet & keyColumnHouse4;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet4) is >= 2 and <= 4);
#endif
                                var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3 | keyColumnCellIndexBitSet4).GetUnionByRow();
                                if (keyRowBitSet.GetPopCount() == 4)
                                {
                                    // 各列に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4個 の行に収まっている。
                                    // => ジェリーフィッシュの成立

                                    var (keyRow1, keyRow2, keyRow3, keyRow4) = keyRowBitSet.SplitAsQuadCellRows();
                                    var result =
                                        MatchPatternByHouse(
                                            digit,
                                            allCellIndexBitSet,
                                            keyColumnHouse1,
                                            keyColumnHouse2,
                                            keyColumnHouse3,
                                            keyColumnHouse4,
                                            keyColumnCellIndexBitSet1,
                                            keyColumnCellIndexBitSet2,
                                            keyColumnCellIndexBitSet3,
                                            keyColumnCellIndexBitSet4,
                                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1),
                                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2),
                                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3),
                                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4),
                                            keyColumn1.ToHouseName(),
                                            keyColumn2.ToHouseName(),
                                            keyColumn3.ToHouseName(),
                                            keyColumn4.ToHouseName(),
                                            keyRow1.ToFriendlyString(),
                                            keyRow2.ToFriendlyString(),
                                            keyRow3.ToFriendlyString(),
                                            keyRow4.ToFriendlyString(),
                                            keyRow1.ToHouseName(),
                                            keyRow2.ToHouseName(),
                                            keyRow3.ToHouseName(),
                                            keyRow4.ToHouseName());
                                    if (result is not null)
                                        yield return result;
                                }
                            }
                        }
                    }
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                UInt128 cellIndexBitSet,
                UInt128 keyLineHouseBitMask1,
                UInt128 keyLineHouseBitMask2,
                UInt128 keyLineHouseBitMask3,
                UInt128 keyLineHouseBitMask4,
                UInt128 keyLineCellIndexBitSet1,
                UInt128 keyLineCellIndexBitSet2,
                UInt128 keyLineCellIndexBitSet3,
                UInt128 keyLineCellIndexBitSet4,
                UInt128 keyCrossLineHouseBitMask1,
                UInt128 keyCrossLineHouseBitMask2,
                UInt128 keyCrossLineHouseBitMask3,
                UInt128 keyCrossLineHouseBitMask4,
                string keyLineHouseName1,
                string keyLineHouseName2,
                string keyLineHouseName3,
                string keyLineHouseName4,
                string keyCrossLineName1,
                string keyCrossLineName2,
                string keyCrossLineName3,
                string keyCrossLineName4,
                string keyCrossLineHouseName1,
                string keyCrossLineHouseName2,
                string keyCrossLineHouseName3,
                string keyCrossLineHouseName4)
            {
                var allKeyCellIndexBitSet = keyLineCellIndexBitSet1 | keyLineCellIndexBitSet2 | keyLineCellIndexBitSet3 | keyLineCellIndexBitSet4;
                var keyCrossLineCellIndexBitSet1 = keyCrossLineHouseBitMask1 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet2 = keyCrossLineHouseBitMask2 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet3 = keyCrossLineHouseBitMask3 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet4 = keyCrossLineHouseBitMask4 & allKeyCellIndexBitSet;
                var excludedCellsBitMask =
                    keyCrossLineCellIndexBitSet1.ToVisibleCellsIntersectionBitSet()
                    | keyCrossLineCellIndexBitSet2.ToVisibleCellsIntersectionBitSet()
                    | keyCrossLineCellIndexBitSet3.ToVisibleCellsIntersectionBitSet()
                    | keyCrossLineCellIndexBitSet4.ToVisibleCellsIntersectionBitSet();
                var exclucedCellIndexBitSet = excludedCellsBitMask & ~allKeyCellIndexBitSet & cellIndexBitSet;
                if (exclucedCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                var keyLineHouseBitMask = keyLineHouseBitMask1 | keyLineHouseBitMask2 | keyLineHouseBitMask3 | keyLineHouseBitMask4;
                return
                    new JellyfishOperation(
                        exclucedCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(keyLineHouseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((excludedCellsBitMask & ~keyLineHouseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            keyLineCellIndexBitSet1,
                            keyLineCellIndexBitSet2,
                            keyLineCellIndexBitSet3,
                            keyLineCellIndexBitSet4,
                            keyLineHouseName1,
                            keyLineHouseName2,
                            keyLineHouseName3,
                            keyLineHouseName4,
                            keyCrossLineName1,
                            keyCrossLineName2,
                            keyCrossLineName3,
                            keyCrossLineName4,
                            keyCrossLineHouseName1,
                            keyCrossLineHouseName2,
                            keyCrossLineHouseName3,
                            keyCrossLineHouseName4,
                            keyCrossLineCellIndexBitSet1,
                            keyCrossLineCellIndexBitSet2,
                            keyCrossLineCellIndexBitSet3,
                            keyCrossLineCellIndexBitSet4));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    UInt128 keyLineCellIndexBitSet1,
                    UInt128 keyLineCellIndexBitSet2,
                    UInt128 keyLineCellIndexBitSet3,
                    UInt128 keyLineCellIndexBitSet4,
                    string keyLineHouseName1,
                    string keyLineHouseName2,
                    string keyLineHouseName3,
                    string keyLineHouseName4,
                    string keyCrossLineName1,
                    string keyCrossLineName2,
                    string keyCrossLineName3,
                    string keyCrossLineName4,
                    string keyCrossLineHouseName1,
                    string keyCrossLineHouseName2,
                    string keyCrossLineHouseName3,
                    string keyCrossLineHouseName4,
                    UInt128 keyCrossLineCellIndexBitSet1,
                    UInt128 keyCrossLineCellIndexBitSet2,
                    UInt128 keyCrossLineCellIndexBitSet3,
                    UInt128 keyCrossLineCellIndexBitSet4)
                {
                    return
                        () =>
                        {
                            var keyCrossLineBlockBitSet1 = keyCrossLineCellIndexBitSet1.GetUnionByBlock();
                            var keyCrossLineBlockBitSet2 = keyCrossLineCellIndexBitSet2.GetUnionByBlock();
                            var keyCrossLineBlockBitSet3 = keyCrossLineCellIndexBitSet3.GetUnionByBlock();
                            var keyCrossLineBlockBitSet4 = keyCrossLineCellIndexBitSet4.GetUnionByBlock();
                            var digitText = digit.ToCellDigitText();
                            var keyLineCellPositionsText1 = string.Join(", ", keyLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                            var keyLineCellPositionsText2 = string.Join(", ", keyLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                            var keyLineCellPositionsText3 = string.Join(", ", keyLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                            var keyLineCellPositionsText4 = string.Join(", ", keyLineCellIndexBitSet4.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText1 = string.Join(", ", keyCrossLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText2 = string.Join(", ", keyCrossLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText3 = string.Join(", ", keyCrossLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText4 = string.Join(", ", keyCrossLineCellIndexBitSet4.EnumerateAsCellPositionTexts());
                            return
                                $"この盤面は以下の条件を満たしています。\n" +
                                $"1) {keyLineHouseName1}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText1}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3}, {keyCrossLineName4} の何れかのみに存在し、かつ\n" +
                                $"2) {keyLineHouseName2}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText2}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3}, {keyCrossLineName4} の何れかのみに存在し、かつ\n" +
                                $"3) {keyLineHouseName3}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText3}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3}, {keyCrossLineName4} の何れかのみに存在し、かつ\n" +
                                $"4) {keyLineHouseName4}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText4}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3}, {keyCrossLineName4} の何れかのみに存在する。\n" +
                                $"仮に{keyCrossLineName1}のマス {keyCrossLinePositionsText1} の何れにも {digitText} が入らないと仮定した場合、" +
                                $"{digitText} が入ることができるマスが足りなくなってしまうので、" +
                                $"{keyCrossLineName1}のマス {keyCrossLinePositionsText1} の何れかには必ず {digitText} が入ります。\n" +
                                $"したがって、{(keyCrossLineBlockBitSet1.IsSingleBit() ? $"{keyCrossLinePositionsText1} が共に属するハウス ({keyCrossLineHouseName1}および{keyCrossLineBlockBitSet1.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName1)}の {keyCrossLinePositionsText1} 以外のマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を除外します。\n" +
                                $"同様の論法で、以下のマスの数字の候補からも {digitText} を除外します。\n" +
                                $"・{(keyCrossLineBlockBitSet2.IsSingleBit() ? $"{keyCrossLinePositionsText2} が共に属するハウス ({keyCrossLineHouseName2}および{keyCrossLineBlockBitSet2.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName2)}の {keyCrossLinePositionsText2} 以外のマス\n" +
                                $"・{(keyCrossLineBlockBitSet3.IsSingleBit() ? $"{keyCrossLinePositionsText3} が共に属するハウス ({keyCrossLineHouseName3}および{keyCrossLineBlockBitSet3.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName3)}の {keyCrossLinePositionsText3} 以外のマス\n" +
                                $"・{(keyCrossLineBlockBitSet4.IsSingleBit() ? $"{keyCrossLinePositionsText4} が共に属するハウス ({keyCrossLineHouseName4}および{keyCrossLineBlockBitSet4.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName4)}の {keyCrossLinePositionsText4} 以外のマス";
                        };
                }
            }
        }
    }
}
