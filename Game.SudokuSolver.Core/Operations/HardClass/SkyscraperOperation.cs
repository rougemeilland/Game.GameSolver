using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// スカイスクレイパーパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class SkyscraperOperation
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
        /// 柱となるセルの数
        /// </summary>
        private const ushort _PILLAR_CELLS = 2;

        /// <summary>
        /// 土台となるセルの数
        /// </summary>
        private const ushort _BASE_CELLS = 2;

        private SkyscraperOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "スカイスクレイパー (Skyscraper)",
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
            // ある数字 D と四つのセル Cell1, Cell2, Cell3, Cell4 とある重複しない行(柱) Row1, Row2 とある重複しない列 Col(土台), Col2, Col3 について、
            // 1) Cell1 は Row1 と Column1 の交差点にあり、かつ
            // 2) Cell2 は Row2 と Column1 の交差点にあり、かつ
            // 3) Cell3 は Row1 と Column2 の交差点にあり、かつ
            // 4) Cell4 は Row2 と Column3 の交差点にあり、かつ
            // 5) Cell1 と Cell3 は Row1 において数字の候補に D を持つただ二つのセルであり、かつ
            // 6) Cell2 と Cell4 は Row2 において数字の候補に D を持つただ二つのセルである場合。
            // => 仮に、Cell3 と Cell4 のどちらにも数字 D が入らないと仮定する。
            //    Row1 で数字 D が入り得るのは Cell1 と Cell3 だけなので、Cell1 に数字 D が入る。
            //    同様に、Row2 で数字 D が入り得るのは Cell2 と Cell4 だけなので、Cell2 に数字 D が入る。
            //    その結果、Col1 の二つのセル Cell1, Cell2 に共に数字 D が入るということになってしまい、これはルール違反である。
            //    したがって、仮定が誤っており、Cell3 と Cell4 の少なくともどちらには必ずにも数字 D が入る。
            // => したがって、Cell3 と Cell4 とハウスを共有する C1, C2, C3, C4 以外のセルには数字 D は入り得ないため、それらのセルの数字の候補からは D を排除できる。
            // ※上記の論拠は、行と列を言い換えても成立する。
            // ※余談であるが、スカイスクレイパーの成立条件と排除範囲は、表現は異なるがフィンの数が 1 のサシミX-ウィングと同等である。

            // 行ハウスが柱の場合
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

            // 列ハウスが柱の場合
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
                // スカイスクレイパーを適用できる行を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, _PILLAR_CELLS, _PILLAR_CELLS);

                // 抽出した行の数が不足している場合はスカイスクレイパーが成立しないので、検索を中断する。
                if (keyRows.Length < _BASE_CELLS)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 1; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex1].cellCount == _PILLAR_CELLS);
#endif
                    var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                    var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet1.GetPopCount() == _PILLAR_CELLS);
#endif
                    var columnHouseMask1 = GetPairColumnHouseMask(keyRowCellIndexBitSet1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length; ++keyIndex2)
                    {
                        var keyRow2 = keyRows.Span[keyIndex2].row;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRows.Span[keyIndex2].cellCount == _PILLAR_CELLS);
#endif
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet2.GetPopCount() == _PILLAR_CELLS);
#endif
                        if (IsFormedSkyscraper(
                            columnHouseMask1,
                            keyRowCellIndexBitSet1,
                            keyRowCellIndexBitSet2,
                            out var baseColumn,
                            out var pillarBaseCellIndex1,
                            out var pillarBaseCellIndex2,
                            out var pillarTopCellIndex1,
                            out var pillarTopCellIndex2,
                            out var excludableRegionBitMask))
                        {
                            // スカイスクレイパーが成立している。

                            var result =
                                MatchPatternByHouse(
                                    digit,
                                    SudokuBoardHouseType.Row,
                                    SudokuBoardHouseType.Column,
                                    allCellIndexBitSet,
                                    pillarBaseCellIndex1,
                                    pillarBaseCellIndex2,
                                    pillarTopCellIndex1,
                                    pillarTopCellIndex2,
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1) | SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2),
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(baseColumn),
                                    excludableRegionBitMask,
                                    keyRow1.ToHouseName(),
                                    keyRow2.ToHouseName(),
                                    baseColumn.ToHouseName());
                            if (result is not null)
                                yield return result;
                        }
                    }
                }

                static UInt128 GetPairColumnHouseMask(UInt128 cellIndexBitSet)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(cellIndexBitSet.GetPopCount() == 2);
#endif
                    var cellIndex1 = cellIndexBitSet.GetOneCellIndex();
                    cellIndexBitSet &= ~cellIndex1.ToCellIndexBitSet();
                    var cellIndex2 = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
                    cellIndexBitSet &= ~cellIndex2.ToCellIndexBitSet();
                    System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
                    return SudokuBoardCellGeometry.GetColumnCellIndexBitSet(cellIndex1) | SudokuBoardCellGeometry.GetColumnCellIndexBitSet(cellIndex2);
                }

                static bool IsFormedSkyscraper(
                    UInt128 columnHouseMask1,
                    UInt128 keyRowCellIndexBitSet1,
                    UInt128 keyRowCellIndexBitSet2,
                    out SudokuBoardCellColumn baseColumn,
                    out SudokuBoardCellIndex pillarBaseCellIndex1,
                    out SudokuBoardCellIndex pillarBaseCellIndex2,
                    out SudokuBoardCellIndex pillarTopCellIndex1,
                    out SudokuBoardCellIndex pillarTopCellIndex2,
                    out UInt128 excludableRegionBitMask)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet1.GetPopCount() == 2);
                    System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet2.GetPopCount() == 2);
                    System.Diagnostics.Debug.Assert((keyRowCellIndexBitSet1 & ~columnHouseMask1) == 0);
#endif
                    // 一本目の柱と二本目の柱の共通の列 (土台) を求める
                    var pillarBaseCellBitSet2 = keyRowCellIndexBitSet2 & columnHouseMask1;
                    if (!pillarBaseCellBitSet2.IsSingleBit())
                    {
                        // 一本目の柱と二本目の柱のただ一つの共通の列 (土台) がただ一つに定まらない。(存在しないかまたは複数存在する)

                        baseColumn = SudokuBoardCellColumn.MinValue;
                        pillarBaseCellIndex1 = SudokuBoardCellIndex.Undefined;
                        pillarBaseCellIndex2 = SudokuBoardCellIndex.Undefined;
                        pillarTopCellIndex1 = SudokuBoardCellIndex.Undefined;
                        pillarTopCellIndex2 = SudokuBoardCellIndex.Undefined;
                        excludableRegionBitMask = 0;
                        return false;
                    }

                    // 一本目の柱と二本目の柱でただ一つの共通の列 (土台) が存在する。
                    // => スカイスクレイパーの成立

                    // 二本目の柱の土台となるセルの位置を求める。
                    pillarBaseCellIndex2 = pillarBaseCellBitSet2.AsSingleCellIndex();

                    // 土台の列を求める。
                    baseColumn = SudokuBoardCellGeometry.GetColumn(pillarBaseCellIndex2);

                    // 一本目の柱の土台となるセルの位置を求める。
                    var pillarBaseCellBitSet1 = keyRowCellIndexBitSet1 & SudokuBoardCellGeometry.GetColumnCellIndexBitSet(baseColumn);
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarBaseCellBitSet1.IsSingleBit());
#endif
                    pillarBaseCellIndex1 = pillarBaseCellBitSet1.AsSingleCellIndex();

                    // 一本目の柱の頂点となるセルの位置を求める。
                    var pillarTopCellBitSet1 = keyRowCellIndexBitSet1 & ~pillarBaseCellBitSet1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarTopCellBitSet1.IsSingleBit());
#endif
                    pillarTopCellIndex1 = pillarTopCellBitSet1.AsSingleCellIndex();

                    // 二本目の柱の頂点となるセルの位置を求める。
                    var pillarTopCellBitSet2 = keyRowCellIndexBitSet2 & ~columnHouseMask1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarTopCellBitSet2.IsSingleBit());
#endif
                    pillarTopCellIndex2 = pillarTopCellBitSet2.AsSingleCellIndex();

                    // 排除可能な他のセルの領域を求める。
                    excludableRegionBitMask =
                        SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(pillarTopCellIndex1)
                        & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(pillarTopCellIndex2);
#if DEBUG
                    System.Diagnostics.Debug.Assert(((pillarBaseCellIndex1.ToCellIndexBitSet() | pillarBaseCellIndex2.ToCellIndexBitSet()) & ~SudokuBoardCellGeometry.GetColumnCellIndexBitSet(baseColumn)) == 0);
                    System.Diagnostics.Debug.Assert((pillarBaseCellIndex1.ToCellIndexBitSet() | pillarTopCellIndex1.ToCellIndexBitSet()) == keyRowCellIndexBitSet1);
                    System.Diagnostics.Debug.Assert((pillarBaseCellIndex2.ToCellIndexBitSet() | pillarTopCellIndex2.ToCellIndexBitSet()) == keyRowCellIndexBitSet2);
#endif
                    // 排除可能な他のセルの領域が空でないときだけ成功する。
                    return excludableRegionBitMask != 0;
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, ushort cellCount)> keyColumnsBuffer)
            {
                // スカイスクレイパーを適用できる列を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, _PILLAR_CELLS, _PILLAR_CELLS);

                // 抽出した行の数が不足している場合はスカイスクレイパーが成立しないので、検索を中断する。
                if (keyColumns.Length < _BASE_CELLS)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 1; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex1].cellCount == _PILLAR_CELLS);
#endif
                    var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                    var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet1.GetPopCount() == _PILLAR_CELLS);
#endif
                    var rowHouseMask1 = GetPairRowHouseMask(keyColumnCellIndexBitSet1);
                    for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length; ++keyIndex2)
                    {
                        var keyColumn2 = keyColumns.Span[keyIndex2].column;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumns.Span[keyIndex2].cellCount == _PILLAR_CELLS);
#endif
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet2.GetPopCount() == _PILLAR_CELLS);
#endif
                        if (IsFormedSkyscraper(
                            rowHouseMask1,
                            keyColumnCellIndexBitSet1,
                            keyColumnCellIndexBitSet2,
                            out var baseRow,
                            out var pillarBaseCellIndex1,
                            out var pillarBaseCellIndex2,
                            out var pillarTopCellIndex1,
                            out var pillarTopCellIndex2,
                            out var excludableRegionBitMask))
                        {
                            // スカイスクレイパーが成立している。

                            var result =
                                MatchPatternByHouse(
                                    digit,
                                    SudokuBoardHouseType.Column,
                                    SudokuBoardHouseType.Row,
                                    allCellIndexBitSet,
                                    pillarBaseCellIndex1,
                                    pillarBaseCellIndex2,
                                    pillarTopCellIndex1,
                                    pillarTopCellIndex2,
                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1) | SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2),
                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(baseRow),
                                    excludableRegionBitMask,
                                    keyColumn1.ToHouseName(),
                                    keyColumn2.ToHouseName(),
                                    baseRow.ToHouseName());
                            if (result is not null)
                                yield return result;
                        }
                    }
                }

                static UInt128 GetPairRowHouseMask(UInt128 cellIndexBitSet)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(cellIndexBitSet.GetPopCount() == 2);
#endif
                    var cellIndex1 = cellIndexBitSet.GetOneCellIndex();
                    cellIndexBitSet &= ~cellIndex1.ToCellIndexBitSet();
                    var cellIndex2 = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
                    cellIndexBitSet &= ~cellIndex2.ToCellIndexBitSet();
                    System.Diagnostics.Debug.Assert(cellIndexBitSet == 0);
#endif
                    return SudokuBoardCellGeometry.GetRowCellIndexBitSet(cellIndex1) | SudokuBoardCellGeometry.GetRowCellIndexBitSet(cellIndex2);
                }

                static bool IsFormedSkyscraper(
                    UInt128 rowHouseMask1,
                    UInt128 keyColumnCellIndexBitSet1,
                    UInt128 keyColumnCellIndexBitSet2,
                    out SudokuBoardCellRow baseRow,
                    out SudokuBoardCellIndex pillarBaseCellIndex1,
                    out SudokuBoardCellIndex pillarBaseCellIndex2,
                    out SudokuBoardCellIndex pillarTopCellIndex1,
                    out SudokuBoardCellIndex pillarTopCellIndex2,
                    out UInt128 excludableRegionBitMask)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet1.GetPopCount() == 2);
                    System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet2.GetPopCount() == 2);
                    System.Diagnostics.Debug.Assert((keyColumnCellIndexBitSet1 & ~rowHouseMask1) == 0);
#endif
                    // 一本目の柱と二本目の柱の共通の行 (土台) を求める
                    var pillarBaseCellBitSet2 = keyColumnCellIndexBitSet2 & rowHouseMask1;
                    if (!pillarBaseCellBitSet2.IsSingleBit())
                    {
                        // 一本目の柱と二本目の柱のただ一つの共通の行 (土台) がただ一つに定まらない。(存在しないかまたは複数存在する)

                        baseRow = SudokuBoardCellRow.MinValue;
                        pillarBaseCellIndex1 = SudokuBoardCellIndex.Undefined;
                        pillarBaseCellIndex2 = SudokuBoardCellIndex.Undefined;
                        pillarTopCellIndex1 = SudokuBoardCellIndex.Undefined;
                        pillarTopCellIndex2 = SudokuBoardCellIndex.Undefined;
                        excludableRegionBitMask = 0;
                        return false;
                    }

                    // 一本目の柱と二本目の柱でただ一つの共通の行 (土台) が存在する。
                    // => スカイスクレイパーの成立

                    // 二本目の柱の土台となるセルの位置を求める。
                    pillarBaseCellIndex2 = pillarBaseCellBitSet2.AsSingleCellIndex();

                    // 土台の行を求める。
                    baseRow = SudokuBoardCellGeometry.GetRow(pillarBaseCellIndex2);

                    // 一本目の柱の土台となるセルの位置を求める。
                    var pillarBaseCellBitSet1 = keyColumnCellIndexBitSet1 & SudokuBoardCellGeometry.GetRowCellIndexBitSet(baseRow);
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarBaseCellBitSet1.IsSingleBit());
#endif
                    pillarBaseCellIndex1 = pillarBaseCellBitSet1.AsSingleCellIndex();

                    // 一本目の柱の頂点となるセルの位置を求める。
                    var pillarTopCellBitSet1 = keyColumnCellIndexBitSet1 & ~pillarBaseCellBitSet1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarTopCellBitSet1.IsSingleBit());
#endif
                    pillarTopCellIndex1 = pillarTopCellBitSet1.AsSingleCellIndex();

                    // 二本目の柱の頂点となるセルの位置を求める。
                    var pillarTopCellBitSet2 = keyColumnCellIndexBitSet2 & ~rowHouseMask1;
#if DEBUG
                    System.Diagnostics.Debug.Assert(pillarTopCellBitSet2.IsSingleBit());
#endif
                    pillarTopCellIndex2 = pillarTopCellBitSet2.AsSingleCellIndex();

                    // 排除可能な他のセルの領域を求める。
                    excludableRegionBitMask =
                        SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(pillarTopCellIndex1)
                        & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(pillarTopCellIndex2);

#if DEBUG
                    System.Diagnostics.Debug.Assert(((pillarBaseCellIndex1.ToCellIndexBitSet() | pillarBaseCellIndex2.ToCellIndexBitSet()) & ~SudokuBoardCellGeometry.GetRowCellIndexBitSet(baseRow)) == 0);
                    System.Diagnostics.Debug.Assert((pillarBaseCellIndex1.ToCellIndexBitSet() | pillarTopCellIndex1.ToCellIndexBitSet()) == keyColumnCellIndexBitSet1);
                    System.Diagnostics.Debug.Assert((pillarBaseCellIndex2.ToCellIndexBitSet() | pillarTopCellIndex2.ToCellIndexBitSet()) == keyColumnCellIndexBitSet2);
#endif
                    // 排除可能な他のセルの領域が空でないときだけ成功する。
                    return excludableRegionBitMask != 0;
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                SudokuBoardHouseType pillarHouseType,
                SudokuBoardHouseType baseHouseType,
                UInt128 allCellIndexBitSet,
                SudokuBoardCellIndex pillarBaseCellIndex1,
                SudokuBoardCellIndex pillarBaseCellIndex2,
                SudokuBoardCellIndex pillarTopCellIndex1,
                SudokuBoardCellIndex pillarTopCellIndex2,
                UInt128 pillarHouseBitMask,
                UInt128 baseHouseBitMask,
                UInt128 excludableRegionBitMask,
                string pillarHouseName1,
                string pillarHouseName2,
                string baseHouseName)
            {
                var allKeyCellIndexBitSet =
                    pillarBaseCellIndex1.ToCellIndexBitSet()
                    | pillarBaseCellIndex2.ToCellIndexBitSet()
                    | pillarTopCellIndex1.ToCellIndexBitSet()
                    | pillarTopCellIndex2.ToCellIndexBitSet();
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableRegionBitMask & ~allKeyCellIndexBitSet;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new SkyscraperOperation(
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(pillarHouseBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((baseHouseBitMask & ~pillarHouseBitMask & ~excludableRegionBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat((excludableRegionBitMask & ~pillarHouseBitMask & ~baseHouseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            pillarHouseType,
                            baseHouseType,
                            pillarBaseCellIndex1,
                            pillarBaseCellIndex2,
                            pillarTopCellIndex1,
                            pillarTopCellIndex2,
                            pillarHouseName1,
                            pillarHouseName2,
                            baseHouseName));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardHouseType pillarHouseType,
                    SudokuBoardHouseType baseHouseType,
                    SudokuBoardCellIndex pillarBaseCellIndex1,
                    SudokuBoardCellIndex pillarBaseCellIndex2,
                    SudokuBoardCellIndex pillarTopCellIndex1,
                    SudokuBoardCellIndex pillarTopCellIndex2,
                    string pillarHouseName1,
                    string pillarHouseName2,
                    string baseHouseName)
                {
                    return
                        () =>
                        {
                            var pillarHouseTypeName =
                                pillarHouseType switch
                                {
                                    SudokuBoardHouseType.Row => "行",
                                    SudokuBoardHouseType.Column => "列",
                                    _ => throw new ApplicationException(),
                                };
                            var baseHouseTypeName =
                                baseHouseType switch
                                {
                                    SudokuBoardHouseType.Row => "行",
                                    SudokuBoardHouseType.Column => "列",
                                    _ => throw new ApplicationException(),
                                };
                            var digitText = digit.ToCellDigitText();
                            var pillarBaseCellPositionText1 = pillarBaseCellIndex1.ToFriendlyString();
                            var pillarBaseCellPositionText2 = pillarBaseCellIndex2.ToFriendlyString();
                            var pillarTopCellPositionText1 = pillarTopCellIndex1.ToFriendlyString();
                            var pillarTopCellPositionText2 = pillarTopCellIndex2.ToFriendlyString();
                            var pillarCellPositionsText1 = $"{pillarBaseCellPositionText1}, {pillarTopCellPositionText1}";
                            var pillarCellPositionsText2 = $"{pillarBaseCellPositionText2}, {pillarTopCellPositionText2}";
                            var baseCellPositionsText = $"{pillarBaseCellPositionText1}, {pillarBaseCellPositionText1}";
                            var topCellPositionsText = $"{pillarTopCellPositionText1}, {pillarTopCellPositionText2}";
                            return
                                $"数字 {digitText} に着目します。\n" +
                                $"{pillarHouseName1}で数字の候補 {digitText} があるマスは {pillarCellPositionsText1} の二つだけで、" +
                                $"{pillarHouseName2}で数字の候補 {digitText} があるマスは {pillarCellPositionsText2} の二つだけです。\n" +
                                $"そして、" +
                                $"{baseCellPositionsText} のマスは共に{baseHouseName}にあり、{topCellPositionsText} のマスは互いに別の{baseHouseTypeName}にあるので、" +
                                $"スカイスクレイパーが成立しています。\n" +
                                $"\n" +
                                $"仮に、「{topCellPositionsText} のどちらのマスにも {digitText} が入らない」と仮定します。\n" +
                                $"{pillarHouseName1}で{pillarTopCellPositionText1}の他に {digitText} が入り得るマスは{pillarBaseCellPositionText1}だけなので、{pillarBaseCellPositionText1}のマスに {digitText} が入ります。\n" +
                                $"同様に、" +
                                $"{pillarHouseName2}で{pillarTopCellPositionText2}の他に {digitText} が入り得るマスは{pillarBaseCellPositionText2}だけなので、{pillarBaseCellPositionText2}のマスに {digitText} が入ります。\n" +
                                $"その結果、{baseHouseName}の二つのマス {baseCellPositionsText} の数字が共に {digitText} ということになってしまうので、" +
                                $"「{topCellPositionsText} のどちらのマスにも {digitText} が入らない」という仮定が誤っていることになり、つまり {topCellPositionsText} の少なくともどちらかのマスには必ず {digitText} が入ります。\n" +
                                $"\n" +
                                $"したがって、二つのマス {topCellPositionsText} とハウスを共有する他のマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。";
                        };
                }
            }
        }
    }
}
