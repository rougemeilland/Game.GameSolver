using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// フィン付きX-ウィングとサシミX-ウィングのオペレーションのクラスです。
    /// </summary>
    /// <remarks>
    /// フィン付きX-ウィングとサシミX-ウィングの論拠では以下の手法の論拠を前提にしています。
    /// <list type="bullet">
    /// <item><see cref="HardClass.XWingOperation">X-ウィング</see></item>
    /// </list>
    /// </remarks>
    internal sealed class FinnedXWingOperation
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
        private const ushort _FISH_SIZE = 2;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_BASE_LINE_CELLS =
            _MIN_WITHOUT_FIN_BASE_LINE_CELLS < _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS
            ? _MIN_WITHOUT_FIN_BASE_LINE_CELLS
            : _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS;

        /// <summary>
        /// ベースライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_BASE_LINE_CELLS =
            _MAX_WITHOUT_FIN_BASE_LINE_CELLS > _MAX_WITH_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS
            ? _MAX_WITHOUT_FIN_BASE_LINE_CELLS
            : _MAX_WITH_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS;

        /// <summary>
        /// フィンを含まないベースライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_WITHOUT_FIN_BASE_LINE_CELLS = 2;

        /// <summary>
        /// フィンを含まないベースライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_WITHOUT_FIN_BASE_LINE_CELLS = _FISH_SIZE;

        /// <summary>
        /// フィンを含むベースライン上でフィン以外に対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_WITH_FIN_BASE_LINE_CELLS = 1;

        /// <summary>
        /// フィンを含むベースライン上でフィン以外に対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_WITH_FIN_BASE_LINE_CELLS = _FISH_SIZE;

        /// <summary>
        /// フィンのセルの数の最小値
        /// </summary>
        private const ushort _MIN_FIN_CELLS = 1;

        /// <summary>
        /// フィンのセルの数の最大値
        /// </summary>
        private const ushort _MAX_FIN_CELLS = 2;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最小値
        /// </summary>
        private const ushort _MIN_COVER_LINE_CELLS = 1;

        /// <summary>
        /// カバーライン上で対象の数字を持つセルの数の最大値
        /// </summary>
        private const ushort _MAX_COVER_LINE_CELLS = _FISH_SIZE;

        private FinnedXWingOperation(
            bool isDegeneratable,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  isDegeneratable ? "サシミX-ウィング (Sashimi X-Wing)" : "フィン付きX-ウィング (Finned X-Wing)",
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
            // ある数字 D とある重複しない行 Row1, Row2 とある重複しない列 Col1, Col2 について、
            // 1) Row1 のハウスのセルには Col1, Col2 との二つの交差点のセルにのみ数字の候補 D が存在し、かつ
            // 2) Row2 のハウスのセルには Col1, Col2 との交差点に数字の候補 D を持つセルが 1 個以上 2 個以下存在し、かつ
            // 3) Col1 のハウスのセルには Row1, Row2 との交差点の何れかに 1 個以上の数字の候補 D を持つセルが存在し、かつ
            // 4) Col2 のハウスのセルには Row1, Row2 との交差点の何れかに 1 個以上の数字の候補 D を持つセルが存在し、かつ
            // 5) Row2 のハウスのうち Col1, Col2 との交差点以外に数字の候補 D を持つセル (フィン) の数が 1 個以上 2 個以下であり、かつ
            // 6) 全てのフィンのセルが同一ブロックに存在している場合。
            //
            // => フィンの何れかのセルに数字 D が入るかどうかの仮定を行ってみる。
            //    a) フィンの何れかのセルに数字 D が入る場合
            //      => 全てのフィンとハウスを共有する他のセルの数字は D ではあり得ない。
            //    b) (a)ではない、つまりフィンの何れのセルも D ではない場合
            //      => Col1 にて、Row1, Row2 との何れの交差点のセルにも数字 D が入らないと仮定すると Row1. Roow2 において数字 D が入り得るセルが足りなくなってしまうので、Col1 と Row1, Row2 の何れかの交差点のセルには数字 D が入る。
            //         したがって、Col1 の Row1, Row2 との交差点以外のセルには数字 D は入り得ない。
            //         同様の論法で、Col2 の Row1, Row2 との交差点以外のセルにも数字 D は入り得ない。
            //    (a) と (b) のどちらが成立したとしても、以下の条件を満たすセルの数字は D ではあり得ないので、それらのセルの数字の候補から D を排除できる。
            //      排除条件1) 全てのフィンとハウスを共有し (== 全てのフィンと同一ブロックにある)、かつ
            //      排除条件2) Col1, Col2 の何れかに属し、かつ
            //      排除条件3) Row1, Row2 の何れにも属さないセル。
            //
            // 注1) 同一行にある数字 D を候補に持つセルの数が 1 の場合、より発見が容易な Hidden Single が適用可能であるため、本実装では 2 個以上の場合のみ考える。
            //
            // ※ サシミパターンの条件
            //   => 以下の条件が一つでも成立すれば、サシミパターンである。
            //   1) Row1 において、Col1. Col2 との交差点にあり数字の候補に D を持つセルの数が 1 個である。(前提条件により自明的に不成立である)
            //   2) Row2 において、Col1. Col2 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            //   3) Col1 において、Row1. Row2 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            //   4) Col2 において、Row1. Row2 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            // ※ Row1, Row2 と Col1, Col2 の合計 4 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※ フィンのセルの個数が 1 個以上 2 個以下である理由は、3 個以上では排除条件を満たせないからである。
            // ※ 上記の論拠は、行と列を言い換えても成立する。

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
                // フィン付きX-ウィングを適用できる行を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した行の数が不足している場合はフィン付きX-ウィングが成立しないので、検索を中断する。
                if (keyRows.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
                    if (keyRows.Span[keyIndex1].cellCount is >= _MIN_WITHOUT_FIN_BASE_LINE_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS)
                    {
                        var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                        var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet1.GetPopCount() is >= _MIN_WITHOUT_FIN_BASE_LINE_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS);
#endif
                        var (keyColumn1, keyColumn2) = GetKeyPairColumns(keyRowCellIndexBitSet1);
                        var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                        var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                        var keyColumnHouse = keyColumnHouse1 | keyColumnHouse2;
                        for (var keyIndex2F = 0; keyIndex2F < keyRows.Length; ++keyIndex2F)
                        {
                            if (keyIndex2F != keyIndex1)
                            {
                                var keyRow2F = keyRows.Span[keyIndex2F].row;
                                if (keyRows.Span[keyIndex2F].cellCount is >= _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS)
                                {
                                    var keyRowHouse2F = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2F);
                                    var keyRowCellIndexBitSet2F = allCellIndexBitSet & keyRowHouse2F;
#if DEBUG
                                    System.Diagnostics.Debug.Assert(keyRowCellIndexBitSet2F.GetPopCount() is >= _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS);
#endif
                                    // keyRow2F に存在し得るフィンの位置のパターン (フィンの個数が規定範囲内でありかつフィン同士は同じ行・ブロックに存在する) を取得する。(全 18 通り)
                                    var finPatterns = SudokuBoardCellGeometry.GetFinCellPatterns(keyRow2F);

                                    // keyRow2F に存在し得るフィンの位置のパターンのすべてに対して、フィン付きX-ウィングの条件を満たす組み合わせを探す。
                                    for (var finPatternIndex = 0; finPatternIndex < finPatterns.Length; ++finPatternIndex)
                                    {
                                        var (finCellsBitMask, finCellsBlock) = finPatterns.Span[finPatternIndex];
#if DEBUG
                                        System.Diagnostics.Debug.Assert((finCellsBitMask & keyRowHouse2F) != 0);
                                        System.Diagnostics.Debug.Assert((finCellsBitMask & ~keyRowHouse2F) == 0);
                                        System.Diagnostics.Debug.Assert(finCellsBitMask.ExistAllCellsInOneBlock(out _));
                                        System.Diagnostics.Debug.Assert(finCellsBitMask.GetPopCount() is >= _MIN_FIN_CELLS and <= _MAX_FIN_CELLS);
#endif
                                        var finCellIndexBitSet = keyRowCellIndexBitSet2F & finCellsBitMask;
                                        if (finCellIndexBitSet != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.Assert(finCellIndexBitSet.GetPopCount() is >= _MIN_FIN_CELLS and <= _MAX_FIN_CELLS);
                                            System.Diagnostics.Debug.Assert(finCellIndexBitSet.ExistAllCellsInOneBlock(out _));
#endif
                                            var keyRowCellIndexBitSet2 = keyRowCellIndexBitSet2F & ~finCellsBitMask;
                                            if (keyRowCellIndexBitSet2.GetPopCount() is >= _MIN_WITH_FIN_BASE_LINE_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS)
                                            {
                                                if ((keyRowCellIndexBitSet2 & ~keyColumnHouse) == 0)
                                                {
                                                    // 1) フィンが keyRow2F に 存在し、かつ
                                                    // 2) フィンの数が規定範囲内であり、かつ
                                                    // 3) フィンは単一ブロックに収まっており、かつ
                                                    // 4) フィンを除く keyRow1, keyRow2F に存在する数字 digit を候補に持つセルはちょうど 2 個の列に収まっている。
                                                    // => フィンを除くとX-ウィングが成立している。
                                                    // ※ フィンを除く keyRow2F のセル数あるいは何れかの列のセル数が 1 かもしれないので、X-ウィングが成立しない可能性がある。
                                                    //   ※ finCellIndexBitSet のブロックが keyColumn1, keyColumn2 の何れかの列と交差するかについては
                                                    //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                                    var allExceptFinKeyCellIndexBitSet = keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2;
                                                    var result =
                                                        MatchPatternByHouse(
                                                            digit,
                                                            SudokuBoardHouseType.Row,
                                                            SudokuBoardHouseType.Column,
                                                            allCellIndexBitSet,
                                                            (keyRow1.ToHouseName(), keyRow1 - SudokuBoardCellRow.One, keyRowHouse1, keyRowCellIndexBitSet1),
                                                            (keyRow2F.ToHouseName(), keyRow2F - SudokuBoardCellRow.One, keyRowHouse2F, keyRowCellIndexBitSet2),
                                                            (keyColumn1.ToHouseName(), keyColumn1 - SudokuBoardCellColumn.One, SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1)),
                                                            (keyColumn2.ToHouseName(), keyColumn2 - SudokuBoardCellColumn.One, SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2)),
                                                            finCellIndexBitSet,
                                                            finCellsBlock,
                                                            allExceptFinKeyCellIndexBitSet,
                                                            new SashimiFishDegeneratableByRow(allExceptFinKeyCellIndexBitSet));
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

                static (SudokuBoardCellColumn keyColumn1, SudokuBoardCellColumn keyColumn2) GetKeyPairColumns(UInt128 cellIndexBitSet)
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
                    return (SudokuBoardCellGeometry.GetColumn(cellIndex1), SudokuBoardCellGeometry.GetColumn(cellIndex2));
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, ushort cellCount)> keyColumnsBuffer)
            {
                // フィン付きX-ウィングを適用できる列を、digit を候補に持つセルがの数の範囲によって抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, _MIN_BASE_LINE_CELLS, _MAX_BASE_LINE_CELLS);

                // 抽出した列の数が不足している場合はフィン付きX-ウィングが成立しないので、検索を中断する。
                if (keyColumns.Length < _FISH_SIZE)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
                    if (keyColumns.Span[keyIndex1].cellCount is >= _MIN_WITHOUT_FIN_BASE_LINE_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS)
                    {
                        var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                        var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet1.GetPopCount() is >= _MIN_WITHOUT_FIN_BASE_LINE_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS);
#endif
                        var (keyRow1, keyRow2) = GetKeyPairRows(keyColumnCellIndexBitSet1);
                        var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                        var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                        var keyRowHouse = keyRowHouse1 | keyRowHouse2;
                        for (var keyIndex2F = 0; keyIndex2F < keyColumns.Length; ++keyIndex2F)
                        {
                            if (keyIndex2F != keyIndex1)
                            {
                                var keyColumn2F = keyColumns.Span[keyIndex2F].column;
                                if (keyColumns.Span[keyIndex2F].cellCount is >= _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS)
                                {
                                    var keyColumnHouse2F = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2F);
                                    var keyColumnCellIndexBitSet2F = allCellIndexBitSet & keyColumnHouse2F;
#if DEBUG
                                    System.Diagnostics.Debug.Assert(keyColumnCellIndexBitSet2F.GetPopCount() is >= _MIN_WITH_FIN_BASE_LINE_CELLS + _MIN_FIN_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS + _MAX_FIN_CELLS);
#endif
                                    // keyColumn2F に存在し得るフィンの位置のパターン (フィンの個数が規定範囲内でありかつフィン同士は同じ行・ブロックに存在する) を取得する。(全 18 通り)
                                    var finPatterns = SudokuBoardCellGeometry.GetFinCellPatterns(keyColumn2F);

                                    // keyColumn2F に存在し得るフィンの位置のパターンのすべてに対して、フィン付きX-ウィングの条件を満たす組み合わせを探す。
                                    for (var finPatternIndex = 0; finPatternIndex < finPatterns.Length; ++finPatternIndex)
                                    {
                                        var (finCellsBitMask, finCellsBlock) = finPatterns.Span[finPatternIndex];
#if DEBUG
                                        System.Diagnostics.Debug.Assert((finCellsBitMask & keyColumnHouse2F) != 0);
                                        System.Diagnostics.Debug.Assert((finCellsBitMask & ~keyColumnHouse2F) == 0);
                                        System.Diagnostics.Debug.Assert(finCellsBitMask.ExistAllCellsInOneBlock(out _));
                                        System.Diagnostics.Debug.Assert(finCellsBitMask.GetPopCount() is >= _MIN_FIN_CELLS and <= _MAX_FIN_CELLS);
#endif
                                        var finCellIndexBitSet = keyColumnCellIndexBitSet2F & finCellsBitMask;
                                        if (finCellIndexBitSet != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.Assert(finCellIndexBitSet.GetPopCount() is >= _MIN_FIN_CELLS and <= _MAX_FIN_CELLS);
                                            System.Diagnostics.Debug.Assert(finCellIndexBitSet.ExistAllCellsInOneBlock(out _));
#endif
                                            var keyColumnCellIndexBitSet2 = keyColumnCellIndexBitSet2F & ~finCellsBitMask;
                                            if (keyColumnCellIndexBitSet2.GetPopCount() is >= _MIN_WITH_FIN_BASE_LINE_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS)
                                            {
                                                if ((keyColumnCellIndexBitSet2 & ~keyRowHouse) == 0)
                                                {
                                                    // 1) フィンが keyColumn2F に 存在し、かつ
                                                    // 2) フィンの数が規定範囲内であり、かつ
                                                    // 3) フィンは単一ブロックに収まっており、かつ
                                                    // 4) フィンを除く keyColumn1, keyColumn2F に存在する数字 digit を候補に持つセルはちょうど 2 個の行に収まっている。
                                                    // => フィンを除くとX-ウィングが成立している。
                                                    // ※ フィンを除く keyColumn2F のセル数あるいは何れかの行のセル数が 1 かもしれないので、X-ウィングが成立しない可能性がある。
                                                    //   ※ finCellIndexBitSet のブロックが keyRow1, keyRow2 の何れかの行と交差するかについては
                                                    //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                                    var allExceptFinKeyCellIndexBitSet = keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2;
                                                    var result =
                                                        MatchPatternByHouse(
                                                            digit,
                                                            SudokuBoardHouseType.Column,
                                                            SudokuBoardHouseType.Row,
                                                            allCellIndexBitSet,
                                                            (keyColumn1.ToHouseName(), keyColumn1 - SudokuBoardCellColumn.One, keyColumnHouse1, keyColumnCellIndexBitSet1),
                                                            (keyColumn2F.ToHouseName(), keyColumn2F - SudokuBoardCellColumn.One, keyColumnHouse2F, keyColumnCellIndexBitSet2),
                                                            (keyRow1.ToHouseName(), keyRow1 - SudokuBoardCellRow.One, SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1)),
                                                            (keyRow2.ToHouseName(), keyRow2 - SudokuBoardCellRow.One, SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2)),
                                                            finCellIndexBitSet,
                                                            finCellsBlock,
                                                            allExceptFinKeyCellIndexBitSet,
                                                            new SashimiFishDegeneratableByColumn(allExceptFinKeyCellIndexBitSet));
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

                static (SudokuBoardCellRow keyRows1, SudokuBoardCellRow keyRows2) GetKeyPairRows(UInt128 cellIndexBitSet)
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
                    return (SudokuBoardCellGeometry.GetRow(cellIndex1), SudokuBoardCellGeometry.GetRow(cellIndex2));
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                SudokuBoardHouseType baseLineHouseType,
                SudokuBoardHouseType coverLineHouseType,
                UInt128 cellIndexBitSet,
                (string houseName, int order, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse1,
                (string houseName, int order, UInt128 houseBitMask, UInt128 cellIndexBitSet) baseLineHouse2F,
                (string houseName, int order, UInt128 houseBitMask) coverLineHouse1,
                (string houseName, int order, UInt128 houseBitMask) coverLineHouse2,
                UInt128 finCellIndexBitSet,
                SudokuBoardCellBlock finCellsBlock,
                UInt128 allExceptFinKeyCellIndexBitSet,
                SashimiFishDegeneratable sashimi)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(baseLineHouse1.cellIndexBitSet.GetPopCount() is >= _MIN_WITHOUT_FIN_BASE_LINE_CELLS and <= _MAX_WITHOUT_FIN_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert(baseLineHouse2F.cellIndexBitSet.GetPopCount() is >= _MIN_WITH_FIN_BASE_LINE_CELLS and <= _MAX_WITH_FIN_BASE_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse1.houseBitMask & allExceptFinKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
                System.Diagnostics.Debug.Assert((coverLineHouse2.houseBitMask & allExceptFinKeyCellIndexBitSet).GetPopCount() is >= _MIN_COVER_LINE_CELLS and <= _MAX_COVER_LINE_CELLS);
#endif
                var baseLineHouseBitMask = baseLineHouse1.houseBitMask | baseLineHouse2F.houseBitMask;
                var coverLineHouseBitMask = coverLineHouse1.houseBitMask | coverLineHouse2.houseBitMask;
                var excludableCellsRegionBitMask = coverLineHouseBitMask & ~baseLineHouseBitMask & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(finCellsBlock);
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allExceptFinKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allExceptFinKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = cellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                const bool __IT_IS_SELF_EVIDENT_THAT_THIS_CONDITION_DOES_NOT_HOLD__ = false; // 成立し得ない条件判断を抑制する定数

                // 単一の要素しか持たないビット集合のハウスを探す
                var oneOfIsolatedCellLineHouse =
                    __IT_IS_SELF_EVIDENT_THAT_THIS_CONDITION_DOES_NOT_HOLD__ && baseLineHouse1.cellIndexBitSet.IsSingleBit()
                    ? FishLineHouseType.BaseLine1
                    : baseLineHouse2F.cellIndexBitSet.IsSingleBit()
                    ? FishLineHouseType.BaseLine2
                    : (coverLineHouse1.houseBitMask & allExceptFinKeyCellIndexBitSet).IsSingleBit()
                    ? FishLineHouseType.CoverLine1
                    : (coverLineHouse2.houseBitMask & allExceptFinKeyCellIndexBitSet).IsSingleBit()
                    ? FishLineHouseType.CoverLine2
                    : FishLineHouseType.None;
                return
                    new FinnedXWingOperation(
                        oneOfIsolatedCellLineHouse != FishLineHouseType.None,
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat((baseLineHouseBitMask & ~finCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((coverLineHouseBitMask & ~baseLineHouseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat(finCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedBlinkingType1AndType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType1)
                            .Concat(finCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType2)),
                        GetDescriptionBuilder(
                            digit,
                            baseLineHouseType,
                            coverLineHouseType,
                            finCellIndexBitSet,
                            finCellsBlock,
                            (baseLineHouse1.houseName, baseLineHouse1.order),
                            (baseLineHouse2F.houseName, baseLineHouse2F.order),
                            (coverLineHouse1.houseName, coverLineHouse1.order),
                            (coverLineHouse2.houseName, coverLineHouse2.order),
                            oneOfIsolatedCellLineHouse,
                            sashimi));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardHouseType baseLineHouseType,
                    SudokuBoardHouseType coverLineHouseType,
                    UInt128 finCellIndexBitSet,
                    SudokuBoardCellBlock finCellsBlock,
                    (string houseName, int order) baseLineHouse1,
                    (string houseName, int order) baseLineHouse2F,
                    (string houseName, int order) coverLineHouse1,
                    (string houseName, int order) coverLineHouse2,
                    FishLineHouseType oneOfIsolatedCellLineHouse,
                    SashimiFishDegeneratable sashimi)
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
                            var baseLineHouseNamesText =
                                string.Join(
                                    ", ",
                                    Enumerable.Empty<(string houseName, int order)>()
                                    .Append(baseLineHouse1)
                                    .Append(baseLineHouse2F)
                                    .OrderBy(item => item.order)
                                    .Select(item => item.houseName));
                            var coverLineHouseNamesText =
                                string.Join(
                                    ", ",
                                    Enumerable.Empty<(string houseName, int order)>()
                                    .Append(coverLineHouse1)
                                    .Append(coverLineHouse2)
                                    .OrderBy(item => item.order)
                                    .Select(item => item.houseName));
                            var finCellPositionsText = string.Join(", ", finCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var finBlockHouseName = finCellsBlock.ToHouseName();
#if DEBUG
                            System.Diagnostics.Debug.Assert(oneOfIsolatedCellLineHouse is not FishLineHouseType.BaseLine1);
#endif
                            var (isolatedCellHouseName, effectecdByIsolatedCellHouseNamesText) =
                                oneOfIsolatedCellLineHouse switch
                                {
#if false // 成立し得ない条件なので省略する
                                    FishLineHouseType.BaseLine1 => (baseLineHouse1.houseName, coverLineHouseNamesText),
#endif
                                    FishLineHouseType.BaseLine2 => (baseLineHouse2F.houseName, coverLineHouseNamesText),
                                    FishLineHouseType.CoverLine1 => (coverLineHouse1.houseName, baseLineHouseNamesText),
                                    FishLineHouseType.CoverLine2 => (coverLineHouse2.houseName, baseLineHouseNamesText),
                                    _ => (string.Empty, string.Empty),
                                };
                            return
                                FishHelper.FinnedFishDescriptionBuilder(
                                    digitText,
                                    baseLineHouseTypeName,
                                    coverLineHouseTypeName,
                                    baseLineHouseNamesText,
                                    coverLineHouseNamesText,
                                    finCellIndexBitSet,
                                    baseLineHouse2F.houseName,
                                    finCellPositionsText,
                                    finBlockHouseName,
                                    "X-ウィング",
                                    "フィン付きX-ウィング",
                                    "サシミX-ウィング",
                                    oneOfIsolatedCellLineHouse != FishLineHouseType.None,
                                    sashimi,
                                    isolatedCellHouseName,
                                    effectecdByIsolatedCellHouseNamesText);
                        };
                }
            }
        }
    }
}
