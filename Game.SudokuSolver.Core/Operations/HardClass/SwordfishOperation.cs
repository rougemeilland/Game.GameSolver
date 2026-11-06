using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// ソードフィッシュパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class SwordfishOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private SwordfishOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "ソードフィッシュ (Swordfish)",
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
            // ある数字 D とある重複しない行 Row1, Row2, Row3 とある重複しない列 Col1, Col2, Col3 について、
            // 1) Row1 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 3 個以下であり、かつ
            // 2) Row2 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 3 個以下であり、かつ
            // 3) Row3 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 3 個以下であり、かつ
            // 4) Col1 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 5) Col2 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 6) Col3 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち少なくとも一つに数字の候補 D が存在する場合。
            // => まず、以下のことが言える。
            //      a) Row1上において、Col1, Col2, Col3 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      b) Row2上において、Col1, Col2, Col3 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      c) Row3上において、Col1, Col2, Col3 の何れかとの交差点にあるセルに数字 D が入る。
            //    仮に、Col1 上の Row1, Row2, Row3 との何れの交差点にあるセルにも数字 D が入らないとした場合、数字 D が入ることができるセルが足りなくなってしまう。
            //    そのため、Col1 上の Row1, Row2, Row3 との何れかの交差点にあるセルには必ず数字 D が入るので、以下の条件を満たす全てのセルから数字の候補 D を排除できる。
            //      - Col1 上の Row1, Row2, Row3 の何れかの交差点にある全てのセルと何れかのハウスを共有する他のセルであり、かつ
            //      - Col1, Col2, Col3 と Row1, Row2, Row3 との何れの交差点にもないセル。
            //    なお、Col2, Col3 についても同様である。
            //     
            // ※ Row1, Row2, Row3 と Col1, Col2, Col3 の合計 9 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※上記の論拠は、行と列を言い換えても成立する。

            // 行ハウスに含まれる数字が 2 個から 3 個のみの場合
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

            // 列ハウスに含まれる数字が 2 個から 3 個のみの場合
            {
                var keyColumnsBuffer = ArrayPool<(SudokuBoardCellColumn row, int cellCount)>.Shared.Rent(9);
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
                    ArrayPool<(SudokuBoardCellColumn row, int cellCount)>.Shared.Return(keyColumnsBuffer);
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByRowHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellRow row, int cellCount)> keyRowsBuffer)
            {
                // digit を候補に持つセルが 2 個以上 3 個以下ある行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 3);

                // digit を候補に持つセルが 2 個以上 3 個以下ある行が 3 個未満である場合はソードフィッシュが成立しないので、検索を中断する。
                if (keyRows.Length < 3)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 2; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex1].cellCount is >= 2 and <= 3);
#endif
                    var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                    var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= 2 and <= 3);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length - 1; ++keyIndex2)
                    {
                        var keyRow2 = keyRows.Span[keyIndex2].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex2].cellCount is >= 2 and <= 3);
#endif
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= 2 and <= 3);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyRows.Length; ++keyIndex3)
                        {
                            var keyRow3 = keyRows.Span[keyIndex3].row;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex3].cellCount is >= 2 and <= 3);
#endif
                            var keyRowHouse3 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3);
                            var keyRowCellIndexBitSet3 = allCellIndexBitSet & keyRowHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet3) is >= 2 and <= 3);
#endif
                            var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3).GetUnionByColumn();
                            if (keyColumnBitSet.GetPopCount() == 3)
                            {
                                // 各行に 2 個以上 3 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 3 個 の列に収まっている。
                                // => ソードフィッシュの成立

                                var (keyColumn1, keyColumn2, keyColumn3) = keyColumnBitSet.SplitAsTripleCellColulmns();
                                var result =
                                    MatchPatternByHouse(
                                        digit,
                                        allCellIndexBitSet,
                                        keyRowHouse1,
                                        keyRowHouse2,
                                        keyRowHouse3,
                                        keyRowCellIndexBitSet1,
                                        keyRowCellIndexBitSet2,
                                        keyRowCellIndexBitSet3,
                                        SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1),
                                        SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2),
                                        SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3),
                                        keyRow1.ToHouseName(),
                                        keyRow2.ToHouseName(),
                                        keyRow3.ToHouseName(),
                                        keyColumn1.ToFriendlyString(),
                                        keyColumn2.ToFriendlyString(),
                                        keyColumn3.ToFriendlyString(),
                                        keyColumn1.ToHouseName(),
                                        keyColumn2.ToHouseName(),
                                        keyColumn3.ToHouseName());
                                if (result is not null)
                                    yield return result;
                            }
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn row, int cellCount)> keyColumnsBuffer)
            {
                // digit を候補に持つセルが 2 個以上 3 個以下ある列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 3);

                // digit を候補に持つセルが 2 個以上 3 個以下ある列が 3 個未満である場合はソードフィッシュが成立しないので、検索を中断する。
                if (keyColumns.Length < 3)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 2; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex1].cellCount is >= 2 and <= 3);
#endif
                    var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                    var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= 2 and <= 3);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length - 1; ++keyIndex2)
                    {
                        var keyColumn2 = keyColumns.Span[keyIndex2].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex2].cellCount is >= 2 and <= 3);
#endif
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= 2 and <= 3);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyColumns.Length; ++keyIndex3)
                        {
                            var keyColumn3 = keyColumns.Span[keyIndex3].column;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex3].cellCount is >= 2 and <= 3);
#endif
                            var keyColumnHouse3 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3);
                            var keyColumnCellIndexBitSet3 = allCellIndexBitSet & keyColumnHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet3) is >= 2 and <= 3);
#endif
                            var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3).GetUnionByRow();
                            if (keyRowBitSet.GetPopCount() == 3)
                            {
                                // 各列に 2 個以上 3 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 3 個 の行に収まっている。
                                // => ソードフィッシュの成立

                                var (keyRow1, keyRow2, keyRow3) = keyRowBitSet.SplitAsTripleCellRows();
                                var result =
                                    MatchPatternByHouse(
                                        digit,
                                        allCellIndexBitSet,
                                        keyColumnHouse1,
                                        keyColumnHouse2,
                                        keyColumnHouse3,
                                        keyColumnCellIndexBitSet1,
                                        keyColumnCellIndexBitSet2,
                                        keyColumnCellIndexBitSet3,
                                        SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1),
                                        SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2),
                                        SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3),
                                        keyColumn1.ToHouseName(),
                                        keyColumn2.ToHouseName(),
                                        keyColumn3.ToHouseName(),
                                        keyRow1.ToFriendlyString(),
                                        keyRow2.ToFriendlyString(),
                                        keyRow3.ToFriendlyString(),
                                        keyRow1.ToHouseName(),
                                        keyRow2.ToHouseName(),
                                        keyRow3.ToHouseName());
                                if (result is not null)
                                    yield return result;
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
                UInt128 keyLineCellIndexBitSet1,
                UInt128 keyLineCellIndexBitSet2,
                UInt128 keyLineCellIndexBitSet3,
                UInt128 keyCrossLineHouseBitMask1,
                UInt128 keyCrossLineHouseBitMask2,
                UInt128 keyCrossLineHouseBitMask3,
                string keyLineHouseName1,
                string keyLineHouseName2,
                string keyLineHouseName3,
                string keyCrossLineName1,
                string keyCrossLineName2,
                string keyCrossLineName3,
                string keyCrossLineHouseName1,
                string keyCrossLineHouseName2,
                string keyCrossLineHouseName3)
            {
                var allKeyCellIndexBitSet = keyLineCellIndexBitSet1 | keyLineCellIndexBitSet2 | keyLineCellIndexBitSet3;
                var keyCrossLineCellIndexBitSet1 = keyCrossLineHouseBitMask1 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet2 = keyCrossLineHouseBitMask2 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet3 = keyCrossLineHouseBitMask3 & allKeyCellIndexBitSet;
                var excludedCellsBitMask =
                    keyCrossLineCellIndexBitSet1.ToVisibleCellsIntersectionBitSet()
                    | keyCrossLineCellIndexBitSet2.ToVisibleCellsIntersectionBitSet()
                    | keyCrossLineCellIndexBitSet3.ToVisibleCellsIntersectionBitSet();
                var exclucedCellIndexBitSet = excludedCellsBitMask & ~allKeyCellIndexBitSet & cellIndexBitSet;
                if (exclucedCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                var keyLineHouseBitMask = keyLineHouseBitMask1 | keyLineHouseBitMask2 | keyLineHouseBitMask3;
                return
                    new SwordfishOperation(
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
                            keyLineHouseName1,
                            keyLineHouseName2,
                            keyLineHouseName3,
                            keyCrossLineName1,
                            keyCrossLineName2,
                            keyCrossLineName3,
                            keyCrossLineHouseName1,
                            keyCrossLineHouseName2,
                            keyCrossLineHouseName3,
                            keyCrossLineCellIndexBitSet1,
                            keyCrossLineCellIndexBitSet2,
                            keyCrossLineCellIndexBitSet3));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    UInt128 keyLineCellIndexBitSet1,
                    UInt128 keyLineCellIndexBitSet2,
                    UInt128 keyLineCellIndexBitSet3,
                    string keyLineHouseName1,
                    string keyLineHouseName2,
                    string keyLineHouseName3,
                    string keyCrossLineName1,
                    string keyCrossLineName2,
                    string keyCrossLineName3,
                    string keyCrossLineHouseName1,
                    string keyCrossLineHouseName2,
                    string keyCrossLineHouseName3,
                    UInt128 keyCrossLineCellIndexBitSet1,
                    UInt128 keyCrossLineCellIndexBitSet2,
                    UInt128 keyCrossLineCellIndexBitSet3)
                {
                    return
                        () =>
                        {
                            var keyCrossLineBlockBitSet1 = keyCrossLineCellIndexBitSet1.GetUnionByBlock();
                            var keyCrossLineBlockBitSet2 = keyCrossLineCellIndexBitSet2.GetUnionByBlock();
                            var keyCrossLineBlockBitSet3 = keyCrossLineCellIndexBitSet3.GetUnionByBlock();
                            var digitText = digit.ToCellDigitText();
                            var keyLineCellPositionsText1 = string.Join(", ", keyLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                            var keyLineCellPositionsText2 = string.Join(", ", keyLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                            var keyLineCellPositionsText3 = string.Join(", ", keyLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText1 = string.Join(", ", keyCrossLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText2 = string.Join(", ", keyCrossLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                            var keyCrossLinePositionsText3 = string.Join(", ", keyCrossLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                            return
                                $"この盤面は以下の条件を満たしています。\n" +
                                $"1) {keyLineHouseName1}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText1}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3} の何れかのみに存在し、かつ\n" +
                                $"2) {keyLineHouseName2}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText2}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3} の何れかのみに存在し、かつ\n" +
                                $"3) {keyLineHouseName3}内の数字の候補 {digitText} を含むマス ({keyLineCellPositionsText3}) は全て {keyCrossLineName1}, {keyCrossLineName2}, {keyCrossLineName3} の何れかのみに存在する。\n" +
                                $"仮に{keyCrossLineName1}のマス {keyCrossLinePositionsText1} の何れにも {digitText} が入らないと仮定した場合、" +
                                $"{digitText} が入ることができるマスが足りなくなってしまうので、" +
                                $"{keyCrossLineName1}のマス {keyCrossLinePositionsText1} の何れかには必ず {digitText} が入ります。\n" +
                                $"したがって、{(keyCrossLineBlockBitSet1.IsSingleBit() ? $"{keyCrossLinePositionsText1} が共に属するハウス ({keyCrossLineHouseName1}および{keyCrossLineBlockBitSet1.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName1)}の {keyCrossLinePositionsText1} 以外のマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を除外します。\n" +
                                $"同様の論法で、以下のマスの数字の候補からも {digitText} を除外します。\n" +
                                $"・{(keyCrossLineBlockBitSet2.IsSingleBit() ? $"{keyCrossLinePositionsText2} が共に属するハウス ({keyCrossLineHouseName2}および{keyCrossLineBlockBitSet2.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName2)}の {keyCrossLinePositionsText2} 以外のマス\n" +
                                $"・{(keyCrossLineBlockBitSet3.IsSingleBit() ? $"{keyCrossLinePositionsText3} が共に属するハウス ({keyCrossLineHouseName3}および{keyCrossLineBlockBitSet3.AsSingleCellBlock().ToHouseName()}) " : keyCrossLineName3)}の {keyCrossLinePositionsText3} 以外のマス";
                        };
                }
            }
        }
    }
}
