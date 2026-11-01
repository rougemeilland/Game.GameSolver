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
            string description)
            : base(
                  "ジェリーフィッシュ (Jellyfish)",
                  SudokuBoardDifficultyLevel.Extreme,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
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
            //    そのため、Col1 上の Row1, Row2, Row3, Row4 との何れかの交差点にあるセルには必ず数字 D が入るので、Col1 上の Row1, Row2, Row3, Row4 との何れの交差点でもないセルの数字は D ではあり得ない。(Col2, Col3, Col4 も同様)
            //     
            // ※ Row1, Row2, Row3, Row4 と Col1, Col2, Col3, Col4 の合計 16 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※上記の論拠は、行と列を言い換えても成立する。

            for (var digit = SudokuBoardCellDigit.MinValue; digit <= SudokuBoardCellDigit.MaxValue; ++digit)
            {
                var cellIndexBitSet = ws.GetCellIndexBitSet(digit);

                // 行ハウスに含まれる数字が 2 個から 4 個のみの場合
                foreach (var operation in MatchPatternByRowHouse(digit, cellIndexBitSet))
                    yield return operation;

                // 列ハウスに含まれる数字が 2 個から 4 個のみの場合
                foreach (var operation in MatchPatternByColumnHouse(digit, cellIndexBitSet))
                    yield return operation;
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByRowHouse(SudokuBoardCellDigit digit, UInt128 cellIndexBitSet)
            {
                var keyColumnsBuffer = ArrayPool<SudokuBoardCellColumn>.Shared.Rent(4);
                try
                {
                    for (var keyRow1 = SudokuBoardCellRow.MinValue; keyRow1 <= SudokuBoardCellRow.MaxValue - 3; ++keyRow1)
                    {
                        var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                        var keyRowCellIndexBitSet1 = cellIndexBitSet & keyRowHouse1;
                        if ((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= 2 and <= 4)
                        {
                            for (var keyRow2 = keyRow1 + 1; keyRow2 <= SudokuBoardCellRow.MaxValue - 2; ++keyRow2)
                            {
                                var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                                var keyRowCellIndexBitSet2 = cellIndexBitSet & keyRowHouse2;
                                if ((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= 2 and <= 4)
                                {
                                    for (var keyRow3 = keyRow2 + 1; keyRow3 <= SudokuBoardCellRow.MaxValue - 1; ++keyRow3)
                                    {
                                        var keyRowHouse3 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3);
                                        var keyRowCellIndexBitSet3 = cellIndexBitSet & keyRowHouse3;
                                        if ((int)UInt128.PopCount(keyRowCellIndexBitSet3) is >= 2 and <= 4)
                                        {
                                            for (var keyRow4 = keyRow3 + 1; keyRow4 <= SudokuBoardCellRow.MaxValue; ++keyRow4)
                                            {
                                                var keyRowHouse4 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4);
                                                var keyRowCellIndexBitSet4 = cellIndexBitSet & keyRowHouse4;
                                                if ((int)UInt128.PopCount(keyRowCellIndexBitSet4) is >= 2 and <= 4)
                                                {
                                                    var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3 | keyRowCellIndexBitSet4).GetUnionByColumn();
                                                    if (keyColumnBitSet.GetPopCount() == 4)
                                                    {
                                                        // 各行に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4個 の列に収まっている。
                                                        // => ジェリーフィッシュの成立

                                                        var keyColumns = keyColumnBitSet.EnumerateAsCellColumns(keyColumnsBuffer);
#if DEBUG
                                                        System.Diagnostics.Debug.Assert(keyColumns.Length == 4);
#endif
                                                        var keyColumn1 = keyColumns[0];
                                                        var keyColumn2 = keyColumns[1];
                                                        var keyColumn3 = keyColumns[2];
                                                        var keyColumn4 = keyColumns[3];
                                                        var result =
                                                            MatchPatternByHouse(
                                                            digit,
                                                            cellIndexBitSet,
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
                                                            () => $"{keyRow1.ToFriendlyString()}の行",
                                                            () => $"{keyRow2.ToFriendlyString()}の行",
                                                            () => $"{keyRow3.ToFriendlyString()}の行",
                                                            () => $"{keyRow4.ToFriendlyString()}の行",
                                                            () => keyColumn1.ToFriendlyString(),
                                                            () => keyColumn2.ToFriendlyString(),
                                                            () => keyColumn3.ToFriendlyString(),
                                                            () => keyColumn4.ToFriendlyString());
                                                        if (result is not null)
                                                            yield return result;
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
                finally
                {
                    ArrayPool<SudokuBoardCellColumn>.Shared.Return(keyColumnsBuffer);

                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 cellIndexBitSet)
            {
                var keyRowsBuffer = ArrayPool<SudokuBoardCellRow>.Shared.Rent(4);
                try
                {
                    for (var keyColumn1 = SudokuBoardCellColumn.MinValue; keyColumn1 <= SudokuBoardCellColumn.MaxValue - 3; ++keyColumn1)
                    {
                        var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                        var keyColumnCellIndexBitSet1 = cellIndexBitSet & keyColumnHouse1;
                        if ((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= 2 and <= 4)
                        {
                            for (var keyColumn2 = keyColumn1 + 1; keyColumn2 <= SudokuBoardCellColumn.MaxValue - 2; ++keyColumn2)
                            {
                                var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                                var keyColumnCellIndexBitSet2 = cellIndexBitSet & keyColumnHouse2;
                                if ((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= 2 and <= 4)
                                {
                                    for (var keyColumn3 = keyColumn2 + 1; keyColumn3 <= SudokuBoardCellColumn.MaxValue - 1; ++keyColumn3)
                                    {
                                        var keyColumnHouse3 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3);
                                        var keyColumnCellIndexBitSet3 = cellIndexBitSet & keyColumnHouse3;
                                        if ((int)UInt128.PopCount(keyColumnCellIndexBitSet3) is >= 2 and <= 4)
                                        {
                                            for (var keyColumn4 = keyColumn3 + 1; keyColumn4 <= SudokuBoardCellColumn.MaxValue; ++keyColumn4)
                                            {
                                                var keyColumnHouse4 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4);
                                                var keyColumnCellIndexBitSet4 = cellIndexBitSet & keyColumnHouse4;
                                                if ((int)UInt128.PopCount(keyColumnCellIndexBitSet4) is >= 2 and <= 4)
                                                {
                                                    var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3 | keyColumnCellIndexBitSet4).GetUnionByRow();
                                                    if (keyRowBitSet.GetPopCount() == 4)
                                                    {
                                                        // 各列に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4個 の行に収まっている。
                                                        // => ジェリーフィッシュの成立

                                                        var keyRows = keyRowBitSet.EnumerateAsCellRows(keyRowsBuffer);
#if DEBUG
                                                        System.Diagnostics.Debug.Assert(keyRows.Length == 4);
#endif
                                                        var keyRow1 = keyRows[0];
                                                        var keyRow2 = keyRows[1];
                                                        var keyRow3 = keyRows[2];
                                                        var keyRow4 = keyRows[3];
                                                        var result =
                                                            MatchPatternByHouse(
                                                            digit,
                                                            cellIndexBitSet,
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
                                                            () => $"{keyColumn1.ToFriendlyString()}の列",
                                                            () => $"{keyColumn2.ToFriendlyString()}の列",
                                                            () => $"{keyColumn3.ToFriendlyString()}の列",
                                                            () => $"{keyColumn4.ToFriendlyString()}の列",
                                                            () => keyRow1.ToFriendlyString(),
                                                            () => keyRow2.ToFriendlyString(),
                                                            () => keyRow3.ToFriendlyString(),
                                                            () => keyRow4.ToFriendlyString());
                                                        if (result is not null)
                                                            yield return result;
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
                finally
                {
                    ArrayPool<SudokuBoardCellRow>.Shared.Return(keyRowsBuffer);
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                UInt128 cellIndexBitSet,
                UInt128 keyLineHouse1,
                UInt128 keyLineHouse2,
                UInt128 keyLineHouse3,
                UInt128 keyLineHouse4,
                UInt128 keyLineCellIndexBitSet1,
                UInt128 keyLineCellIndexBitSet2,
                UInt128 keyLineCellIndexBitSet3,
                UInt128 keyLineCellIndexBitSet4,
                UInt128 keyCrossLineHouse1,
                UInt128 keyCrossLineHouse2,
                UInt128 keyCrossLineHouse3,
                UInt128 keyCrossLineHouse4,
                Func<string> keyLine1HouseNameGetter,
                Func<string> keyLine2HouseNameGetter,
                Func<string> keyLine3HouseNameGetter,
                Func<string> keyLine4HouseNameGetter,
                Func<string> keyCrossLine1NameGetter,
                Func<string> keyCrossLine2NameGetter,
                Func<string> keyCrossLine3NameGetter,
                Func<string> keyCrossLine4NameGetter)
            {
                var allKeyCellIndexBitSet = keyLineCellIndexBitSet1 | keyLineCellIndexBitSet2 | keyLineCellIndexBitSet3 | keyLineCellIndexBitSet4;
                var keyCrossLineCellIndexBitSet1 = keyCrossLineHouse1 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet2 = keyCrossLineHouse2 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet3 = keyCrossLineHouse3 & allKeyCellIndexBitSet;
                var keyCrossLineCellIndexBitSet4 = keyCrossLineHouse4 & allKeyCellIndexBitSet;
                var exclucedCellIndexBitSetInCrossLine1 = cellIndexBitSet & keyCrossLineHouse1 & ~allKeyCellIndexBitSet;
                var exclucedCellIndexBitSetInCrossLine2 = cellIndexBitSet & keyCrossLineHouse2 & ~allKeyCellIndexBitSet;
                var exclucedCellIndexBitSetInCrossLine3 = cellIndexBitSet & keyCrossLineHouse3 & ~allKeyCellIndexBitSet;
                var exclucedCellIndexBitSetInCrossLine4 = cellIndexBitSet & keyCrossLineHouse4 & ~allKeyCellIndexBitSet;
                var exclucedCellIndexBitSet = exclucedCellIndexBitSetInCrossLine1 | exclucedCellIndexBitSetInCrossLine2 | exclucedCellIndexBitSetInCrossLine3 | exclucedCellIndexBitSetInCrossLine4;
                if (exclucedCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                var keyLine1HouseName = keyLine1HouseNameGetter();
                var keyLine2HouseName = keyLine2HouseNameGetter();
                var keyLine3HouseName = keyLine3HouseNameGetter();
                var keyLine4HouseName = keyLine4HouseNameGetter();
                var keyCrossLine1Name = keyCrossLine1NameGetter();
                var keyCrossLine2Name = keyCrossLine2NameGetter();
                var keyCrossLine3Name = keyCrossLine3NameGetter();
                var keyCrossLine4Name = keyCrossLine4NameGetter();
                var digitText = digit.ToCellDigitText();
                var keyLine1CellPositionsText = string.Join(", ", keyLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                var keyLine2CellPositionsText = string.Join(", ", keyLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                var keyLine3CellPositionsText = string.Join(", ", keyLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                var keyLine4CellPositionsText = string.Join(", ", keyLineCellIndexBitSet4.EnumerateAsCellPositionTexts());
                var keyCrossLine1PositionsText = string.Join(", ", keyCrossLineCellIndexBitSet1.EnumerateAsCellPositionTexts());
                var keyCrossLine2PositionsText = string.Join(", ", keyCrossLineCellIndexBitSet2.EnumerateAsCellPositionTexts());
                var keyCrossLine3PositionsText = string.Join(", ", keyCrossLineCellIndexBitSet3.EnumerateAsCellPositionTexts());
                var keyCrossLine4PositionsText = string.Join(", ", keyCrossLineCellIndexBitSet4.EnumerateAsCellPositionTexts());
                var description =
                    $"この盤面は以下の条件を満たしています。\n" +
                    $"1) {keyLine1HouseName}内の数字の候補 {digitText} を含むマス ({keyLine1CellPositionsText}) は全て {keyCrossLine1Name}, {keyCrossLine2Name}, {keyCrossLine3Name}, {keyCrossLine4Name} の何れかのみに存在し、かつ\n" +
                    $"2) {keyLine2HouseName}内の数字の候補 {digitText} を含むマス ({keyLine2CellPositionsText}) は全て {keyCrossLine1Name}, {keyCrossLine2Name}, {keyCrossLine3Name}, {keyCrossLine4Name} の何れかのみに存在し、かつ\n" +
                    $"3) {keyLine3HouseName}内の数字の候補 {digitText} を含むマス ({keyLine3CellPositionsText}) は全て {keyCrossLine1Name}, {keyCrossLine2Name}, {keyCrossLine3Name}, {keyCrossLine4Name} の何れかのみに存在し、かつ\n" +
                    $"4) {keyLine4HouseName}内の数字の候補 {digitText} を含むマス ({keyLine4CellPositionsText}) は全て {keyCrossLine1Name}, {keyCrossLine2Name}, {keyCrossLine3Name}, {keyCrossLine4Name} の何れかのみに存在する。\n" +
                    $"仮に{keyCrossLine1Name}のマス {keyCrossLine1PositionsText} の何れにも {digitText} が入らないと仮定した場合、" +
                    $"{digitText} が入ることができるマスが足りなくなってしまうので、" +
                    $"{keyCrossLine1Name}のマス {keyCrossLine1PositionsText} の何れかには必ず {digitText} が入ります。\n" +
                    $"したがって、{keyCrossLine1Name}の {keyCrossLine1PositionsText} 以外のマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を除外します。\n" +
                    $"同様の理屈で、以下のマスの数字の候補からも {digitText} を除外します。\n" +
                    $"・{keyCrossLine2Name}の {keyCrossLine2PositionsText} 以外のマス\n" +
                    $"・{keyCrossLine3Name}の {keyCrossLine3PositionsText} 以外のマス\n" +
                    $"・{keyCrossLine4Name}の {keyCrossLine4PositionsText} 以外のマス";
                return
                    new JellyfishOperation(
                        exclucedCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(((keyLineHouse1 | keyLineHouse2 | keyLineHouse3 | keyLineHouse4) & ~allKeyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(((keyCrossLineHouse1 | keyCrossLineHouse2 | keyCrossLineHouse3 | keyCrossLineHouse4) & ~(keyLineHouse1 | keyLineHouse2 | keyLineHouse3 | keyLineHouse4) & ~allKeyCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        description);
            }
        }
    }
}
