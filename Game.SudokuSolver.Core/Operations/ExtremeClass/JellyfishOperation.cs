using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// ジェリーフィッシュのオペレーションのクラスです。
    /// </summary>
    internal sealed class JellyfishOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        /// <summary>
        /// 行の数の最大値
        /// </summary>
        private const ushort _MAX_ROWS = 9;

        /// <summary>
        /// 列の数の最大値
        /// </summary>
        private const ushort _MAX_COLUMNS = 9;

        /// <summary>
        /// フィッシュの大きさ
        /// </summary>
        private const ushort _FISH_SIZE = 4;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_BASE_LINE_CELLS = 2;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_BASE_LINE_CELLS = _FISH_SIZE;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_COVER_LINE_CELLS = 1;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_COVER_LINE_CELLS = _FISH_SIZE;

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
            // 1) Row1 のハウスのセルには Col1, Col2, Col3, Col4 との何れかの交差点のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 4 個以下であり、かつ
            // 1) Row2 のハウスのセルには Col1, Col2, Col3, Col4 との何れかの交差点のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 4 個以下であり、かつ
            // 1) Row3 のハウスのセルには Col1, Col2, Col3, Col4 との何れかの交差点のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 4 個以下であり、かつ
            // 1) Row4 のハウスのセルには Col1, Col2, Col3, Col4 との何れかの交差点のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 4 個以下であり、かつ
            // 5) Col1 のハウスのセルには Row1, Row2, Row3, Row4 との何れかの交差点のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 5) Col2 のハウスのセルには Row1, Row2, Row3, Row4 との何れかの交差点のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 5) Col3 のハウスのセルには Row1, Row2, Row3, Row4 との何れかの交差点のセルのうち少なくとも一つに数字の候補 D が存在し、かつ
            // 5) Col4 のハウスのセルには Row1, Row2, Row3, Row4 との何れかの交差点のセルのうち少なくとも一つに数字の候補 D が存在する場合。
            // => まず、以下のことが言える。
            //      a) Row1 上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      b) Row2 上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      c) Row3 上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入り、かつ
            //      d) Row4 上において、Col1, Col2, Col3, Col4 の何れかとの交差点にあるセルに数字 D が入る。
            //    仮に、Col1 上の Row1, Row2, Row3, Row4 との何れの交差点にあるセルにも数字 D が入らないとした場合、数字 D が入ることができるセルが足りなくなってしまう。
            //    そのため、Col1 上の Row1, Row2, Row3, Row4 との何れかの交差点にあるセルには必ず数字 D が入るので、交差点以外の全てのセルから数字の候補 D を排除できる。
            //    なお、Col2, Col3, Col4 についても同様である。
            //
            // 注1) 同一行にある数字 D を候補に持つセルの数が 1 の場合、より発見が容易な Hidden Single が適用可能であるため、本実装では 2 個以上の場合のみ考える。
            //
            // ※ Row1, Row2, Row3, Row4 と Col1, Col2, Col3, Col4 の合計 16 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※上記の論拠は、行と列を言い換えても成立する。

            // 行ハウスがベースラインの場合
            {
                var keyRowsBuffer = ArrayPool<(SudokuBoardCellRow row, ushort cellCount)>.Shared.Rent(_MAX_ROWS);
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
                    ArrayPool<(SudokuBoardCellRow row, ushort cellCount)>.Shared.Return(keyRowsBuffer);
                }
            }

            // 列ハウスがベースラインの場合
            {
                var keyColumnsBuffer = ArrayPool<(SudokuBoardCellColumn column, ushort cellCount)>.Shared.Rent(_MAX_COLUMNS);
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
                    ArrayPool<(SudokuBoardCellColumn column, ushort cellCount)>.Shared.Return(keyColumnsBuffer);
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByRowHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellRow row, ushort cellCount)> keyRowsBuffer)
            {
                // ジェリーフィッシュを適用できる行を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した行の数が不足している場合はジェリーフィッシュが成立しないので、検索を中断する。
                if (keyRows.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 3; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex1].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                    var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet1.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length - 2; ++keyIndex2)
                    {
                        var keyRow2 = keyRows.Span[keyIndex2].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex2].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet2.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyRows.Length - 1; ++keyIndex3)
                        {
                            var keyRow3 = keyRows.Span[keyIndex3].row;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex3].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                            var keyRowHouse3 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3);
                            var keyRowCellIndexBitSet3 = allCellIndexBitSet & keyRowHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet3.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                            for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyRows.Length; ++keyIndex4)
                            {
                                var keyRow4 = keyRows.Span[keyIndex4].row;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex4].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                                var keyRowHouse4 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4);
                                var keyRowCellIndexBitSet4 = allCellIndexBitSet & keyRowHouse4;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet4.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                                var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3 | keyRowCellIndexBitSet4).GetUnionByColumn();
                                if (keyColumnBitSet.GetPopCount() == _FISH_SIZE)
                                {
                                    // 各行に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4 個の列に収まっている。
                                    // => ジェリーフィッシュの成立

                                    var (keyColumn1, keyColumn2, keyColumn3, keyColumn4) = keyColumnBitSet.SplitAsQuadCellColulmns();
                                    var result =
                                        MatchPatternByHouse(
                                            digit,
                                            SudokuBoardHouseType.Row,
                                            SudokuBoardHouseType.Column,
                                            allCellIndexBitSet,
                                            (keyRow1.ToHouseName(), keyRowHouse1, keyRowCellIndexBitSet1),
                                            (keyRow2.ToHouseName(), keyRowHouse2, keyRowCellIndexBitSet2),
                                            (keyRow3.ToHouseName(), keyRowHouse3, keyRowCellIndexBitSet3),
                                            (keyRow4.ToHouseName(), keyRowHouse4, keyRowCellIndexBitSet4),
                                            (keyColumn1.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1)),
                                            (keyColumn2.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2)),
                                            (keyColumn3.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3)),
                                            (keyColumn4.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4)));
                                    if (result is not null)
                                        yield return result;
                                }
                            }
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, ushort cellCount)> keyColumnsBuffer)
            {
                // ジェリーフィッシュを適用できる列を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した列の数が不足している場合はジェリーフィッシュが成立しないので、検索を中断する。
                if (keyColumns.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 3; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex1].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                    var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet1.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length - 2; ++keyIndex2)
                    {
                        var keyColumn2 = keyColumns.Span[keyIndex2].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex2].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet2.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyColumns.Length - 1; ++keyIndex3)
                        {
                            var keyColumn3 = keyColumns.Span[keyIndex3].column;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex3].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                            var keyColumnHouse3 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3);
                            var keyColumnCellIndexBitSet3 = allCellIndexBitSet & keyColumnHouse3;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet3.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                            for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyColumns.Length; ++keyIndex4)
                            {
                                var keyColumn4 = keyColumns.Span[keyIndex4].column;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex4].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                                var keyColumnHouse4 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4);
                                var keyColumnCellIndexBitSet4 = allCellIndexBitSet & keyColumnHouse4;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet4.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                                var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3 | keyColumnCellIndexBitSet4).GetUnionByRow();
                                if (keyRowBitSet.GetPopCount() == _FISH_SIZE)
                                {
                                    // 各列に 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 4 個の行に収まっている。
                                    // => ジェリーフィッシュの成立

                                    var (keyRow1, keyRow2, keyRow3, keyRow4) = keyRowBitSet.SplitAsQuadCellRows();
                                    var result =
                                        MatchPatternByHouse(
                                            digit,
                                            SudokuBoardHouseType.Column,
                                            SudokuBoardHouseType.Row,
                                            allCellIndexBitSet,
                                            (keyColumn1.ToHouseName(), keyColumnHouse1, keyColumnCellIndexBitSet1),
                                            (keyColumn2.ToHouseName(), keyColumnHouse2, keyColumnCellIndexBitSet2),
                                            (keyColumn3.ToHouseName(), keyColumnHouse3, keyColumnCellIndexBitSet3),
                                            (keyColumn4.ToHouseName(), keyColumnHouse4, keyColumnCellIndexBitSet4),
                                            (keyRow1.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1)),
                                            (keyRow2.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2)),
                                            (keyRow3.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3)),
                                            (keyRow4.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4)));
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
                SudokuBoardHouseType baseLineHouseType,
                SudokuBoardHouseType coverLineHouseType,
                UInt128 allCellIndexBitSet,
                (string houseName, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse1,
                (string houseName, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse2,
                (string houseName, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse3,
                (string houseName, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse4,
                (string houseName, UInt128 houseBitMask) coverLineHouse1,
                (string houseName, UInt128 houseBitMask) coverLineHouse2,
                (string houseName, UInt128 houseBitMask) coverLineHouse3,
                (string houseName, UInt128 houseBitMask) coverLineHouse4)
            {
                var allKeyCellIndexBitSet = baseLineHouse1.cellIndexBitSet | baseLineHouse2.cellIndexBitSet | baseLineHouse3.cellIndexBitSet | baseLineHouse4.cellIndexBitSet;
#if DEBUG
                System.Diagnostics.Debug.Assert(baseLineHouse1.cellIndexBitSet.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert(baseLineHouse2.cellIndexBitSet.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert(baseLineHouse3.cellIndexBitSet.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert(baseLineHouse4.cellIndexBitSet.GetPopCount() is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse1.houseBitMask & allKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse2.houseBitMask & allKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse3.houseBitMask & allKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse4.houseBitMask & allKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
#endif
                var baseLineHouseBitMask = baseLineHouse1.houseBitMask | baseLineHouse2.houseBitMask | baseLineHouse3.houseBitMask | baseLineHouse4.houseBitMask;
                var coverLineHouseBitMask = coverLineHouse1.houseBitMask | coverLineHouse2.houseBitMask | coverLineHouse3.houseBitMask | coverLineHouse4.houseBitMask;
                var excludableCellsRegionBitMask = coverLineHouseBitMask & ~baseLineHouseBitMask;
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new JellyfishOperation(
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(baseLineHouseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            baseLineHouseType,
                            coverLineHouseType,
                            baseLineHouse1.houseName,
                            baseLineHouse2.houseName,
                            baseLineHouse3.houseName,
                            baseLineHouse4.houseName,
                            coverLineHouse1.houseName,
                            coverLineHouse2.houseName,
                            coverLineHouse3.houseName,
                            coverLineHouse4.houseName));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardHouseType baseLineHouseType,
                    SudokuBoardHouseType coverLineHouseType,
                    string baseLineHouseName1,
                    string baseLineHouseName2,
                    string baseLineHouseName3,
                    string baseLineHouseName4,
                    string coverLineHouseName1,
                    string coverLineHouseName2,
                    string coverLineHouseName3,
                    string coverLineHouseName4)
                {
                    return
                        () =>
                        {
                            var baseLineHouseTypeName =
                                baseLineHouseType switch
                                {
                                    SudokuBoardHouseType.Row => "行",
                                    SudokuBoardHouseType.Column => "列",
                                    _ => throw new ApplicationException(),
                                };
                            var coverLineHouseTypeName =
                                coverLineHouseType switch
                                {
                                    SudokuBoardHouseType.Row => "行",
                                    SudokuBoardHouseType.Column => "列",
                                    _ => throw new ApplicationException(),
                                };
                            var digitText = digit.ToCellDigitText();
                            var baseLineHouseNamesText = $"{baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4}";
                            var coverLineHouseNamesText = $"{coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4}";
                            return
                                $"数字 {digitText} に着目します。\n" +
                                $"{baseLineHouseNamesText} の四つのどの{baseLineHouseTypeName}においても、" +
                                $"数字の候補 {digitText} を含むマスは四つの{coverLineHouseTypeName} {coverLineHouseNamesText} との交差点にしか存在しません。\n" +
                                $"実際に {digitText} が入るマスは {baseLineHouseNamesText} の各{baseLineHouseTypeName}の間で各{coverLineHouseTypeName}に重複せずに分散する必要があるので、" +
                                $"{coverLineHouseNamesText} のどの{coverLineHouseTypeName}でも {baseLineHouseNamesText} との交差点にある何れかのマスには必ず {digitText} が入ります。\n" +
                                $"したがって、" +
                                $"{coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} との交差点以外にあるマスには {digitText} は入り得ないので、" +
                                $"それらのマスの数字の候補から {digitText} を排除します。";
                        };
                }
            }
        }
    }
}
