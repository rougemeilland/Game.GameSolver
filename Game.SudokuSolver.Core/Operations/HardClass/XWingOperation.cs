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
        /// <summary>
        /// 行の数の最大値
        /// </summary>
        private const int _MAX_ROWS = 9;

        /// <summary>
        /// 列の数の最大値
        /// </summary>
        private const int _MAX_COLUMNS = 9;

        /// <summary>
        /// フィッシュの大きさ
        /// </summary>
        private const int _FISH_SIZE = 2;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const int _MIN_BASE_LINE_CELLS = 2;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const int _MAX_BASE_LINE_CELLS = _FISH_SIZE;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const int _MIN_COVER_LINE_CELLS = 1;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const int _MAX_COVER_LINE_CELLS = _FISH_SIZE;

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
            // ある数字 D とある重複しない行 Row1, Row2 とある重複しない列 Col1, Col2 について、
            // 1) Row1 のハウスのセルには Col1, Col2 の何れの交差点のセルにも数字の候補 D が存在し、かつ
            // 2) Row2 のハウスのセルには Col1, Col2 の何れの交差点のセルにも数字の候補 D が存在する場合。
            // => まず、以下のことが言える。
            //      a) Row1 上において、Col1, Col2 の何れかの交差点のセルに数字 D が入り、かつ
            //      b) Row2 上において、Col1, Col2 の何れかの交差点のセルに数字 D が入る。
            //    仮に、Col1 上の Row1, Row2 との何れの交差点にあるセルにも数字 D が入らないとした場合、数字 D が入ることができるセルが足りなくなってしまう。
            //    そのため、Col1 上の Row1, Row2 との何れかの交差点にあるセルには必ず数字 D が入るので、交差点以外の全てのセルから数字の候補 D を排除できる。
            //    なお、Col2 についても同様である。
            //
            // 注1) 同一行にある数字 D を候補に持つセルの数が 1 の場合、より発見が容易な Hidden Single が適用可能であるため、本実装では 2 個以上の場合のみ考える。
            //
            // ※ Row1, Row2 と Col1, Col2 の合計 4 個の交差点にあるセルの数字の候補には必ず D がある。
            // ※上記の論拠は、行と列を言い換えても成立する。

            // 行ハウスがベースラインの場合
            {
                var keyRowsBuffer = ArrayPool<(SudokuBoardCellRow row, int cellCount)>.Shared.Rent(_MAX_ROWS);
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

            // 列行ハウスがベースラインの場合
            {
                var keyColumnsBuffer = ArrayPool<(SudokuBoardCellColumn column, int cellCount)>.Shared.Rent(_MAX_COLUMNS);
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
                // X-ウィングを適用できる行を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した行の数が不足している場合はX-ウィングが成立しないので、検索を中断する。
                if (keyRows.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 1; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex1].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                    var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length; ++keyIndex2)
                    {
                        var keyRow2 = keyRows.Span[keyIndex2].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex2].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2).GetUnionByColumn();
                        if (keyColumnBitSet.GetPopCount() == _FISH_SIZE)
                        {
                            // 各行に 2 個の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 2 個の列に収まっている。
                            // => X-ウィングの成立

                            var (keyColumn1, keyColumn2) = keyColumnBitSet.SplitAsPairCellColulmns();
                            var result =
                                MatchPatternByHouse(
                                    digit,
                                    SudokuBoardHouseType.Row,
                                    SudokuBoardHouseType.Column,
                                    allCellIndexBitSet,
                                    (keyRow1.ToHouseName(), keyRowHouse1, keyRowCellIndexBitSet1),
                                    (keyRow2.ToHouseName(), keyRowHouse2, keyRowCellIndexBitSet2),
                                    (keyColumn1.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1)),
                                    (keyColumn2.ToHouseName(), SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2)));
                            if (result is not null)
                                yield return result;
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> keyColumnsBuffer)
            {
                // X-ウィングを適用できる行を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した行の数が不足している場合はX-ウィングが成立しないので、検索を中断する。
                if (keyColumns.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 1; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex1].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                    var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length; ++keyIndex2)
                    {
                        var keyColumn2 = keyColumns.Span[keyIndex2].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex2].cellCount is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
#endif
                        var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2).GetUnionByRow();
                        if (keyRowBitSet.GetPopCount() == _FISH_SIZE)
                        {
                            // 各列に 2 個の数字の候補 D を持つセルがあり、かつそれらの全てのセルはちょうど 2 個の行に収まっている。
                            // => X-ウィングの成立

                            var (keyRow1, keyRow2) = keyRowBitSet.SplitAsPairCellRows();
                            var result =
                                MatchPatternByHouse(
                                    digit,
                                    SudokuBoardHouseType.Column,
                                    SudokuBoardHouseType.Row,
                                    allCellIndexBitSet,
                                    (keyColumn1.ToHouseName(), keyColumnHouse1, keyColumnCellIndexBitSet1),
                                    (keyColumn2.ToHouseName(), keyColumnHouse2, keyColumnCellIndexBitSet2),
                                    (keyRow1.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1)),
                                    (keyRow2.ToHouseName(), SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2)));
                            if (result is not null)
                                yield return result;
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
                (string houseName, UInt128 houseBitMask) coverLineHouse1,
                (string houseName, UInt128 houseBitMask) coverLineHouse2)
            {
                var allKeyCellIndexBitSet = baseLineHouse1.cellIndexBitSet | baseLineHouse2.cellIndexBitSet;
#if DEBUG
                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(baseLineHouse1.cellIndexBitSet) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(baseLineHouse2.cellIndexBitSet) is >= _MIN_BASE_LINE_CELLS and <= _MAX_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(coverLineHouse1.houseBitMask & allKeyCellIndexBitSet) is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(coverLineHouse2.houseBitMask & allKeyCellIndexBitSet) is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
#endif
                var baseLineHouseBitMask = baseLineHouse1.houseBitMask | baseLineHouse2.houseBitMask;
                var coverLineHouseBitMask = coverLineHouse1.houseBitMask | coverLineHouse2.houseBitMask;
                var excludableCellsRegionBitMask = coverLineHouseBitMask & ~baseLineHouseBitMask;
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new XWingOperation(
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
                            coverLineHouse1.houseName,
                            coverLineHouse2.houseName));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardHouseType baseLineHouseType,
                    SudokuBoardHouseType coverLineHouseType,
                    string baseLineHouseName1,
                    string baseLineHouseName2,
                    string coverLineHouseName1,
                    string coverLineHouseName2)
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
                            var baseLineHouseNamesText = $"{baseLineHouseName1}, {baseLineHouseName2}";
                            var coverLineHouseNamesText = $"{coverLineHouseName1}, {coverLineHouseName2}";
                            return
                                $"数字 {digitText} に着目します。\n" +
                                $"{baseLineHouseNamesText} の二つの{baseLineHouseTypeName}のどちらにおいても、" +
                                $"数字の候補 {digitText} を含むマスは二つの{coverLineHouseTypeName} {coverLineHouseNamesText} との交差点にしか存在しません。\n" +
                                $"実際に {digitText} が入るマスは {baseLineHouseNamesText} の各{baseLineHouseTypeName}の間で各{coverLineHouseTypeName}に重複せずに分散する必要があるので、" +
                                $"{coverLineHouseNamesText} のどちらの{coverLineHouseTypeName}でも {baseLineHouseNamesText} との交差点にあるどちらかのマスには必ず {digitText} が入ります。\n" +
                                $"したがって、" +
                                $"{coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} との交差点以外にあるマスには {digitText} は入り得ないので、" +
                                $"それらのマスの数字の候補から {digitText} を排除します。";
                        };
                }
            }
        }
    }
}
