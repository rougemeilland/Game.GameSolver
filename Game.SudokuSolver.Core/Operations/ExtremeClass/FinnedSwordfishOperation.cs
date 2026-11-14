using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// フィン付きソードフィッシュとサシミソードフィッシュのパターンのオペレーションのクラスです。
    /// </summary>
    /// <remarks>
    /// フィン付きソードフィッシュとサシミソードフィッシュの論拠では以下の手法の論拠を前提にしています。
    /// <list type="bullet">
    /// <item><see cref="HardClass.XWingOperation">X-ウィング</see></item>
    /// <item><see cref="HardClass.SwordfishOperation">ソードフィッシュ</see></item>
    /// </list>
    /// </remarks>
    internal sealed class FinnedSwordfishOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private FinnedSwordfishOperation(
            bool isDegeneratable,
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  isDegeneratable ? "サシミソードフィッシュ (Sashimi Swordfish)" : "フィン付きソードフィッシュ (Finned Swordfish)",
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
            // ある数字 D とある重複しない行 Row1, Row2, Row3 とある重複しない列 Col1, Col2, Col3 について、
            // 1) Row1 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 3 個以下であり、かつ
            // 2) Row2 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注Ⅰ) 3 個以下であり、かつ
            // 3) Row3 のハウスのセルには Col1, Col2, Col3 の何れかと交わる場所の数字の候補 D を持つセルが 1 個以上 3 個以下存在し、かつ
            // 4) Col1 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち 1 個以上のセルに数字の候補 D が存在し、かつ
            // 5) Col2 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち 1 個以上のセルに数字の候補 D が存在し、かつ
            // 6) Col3 のハウスのセルには Row1, Row2, Row3 の何れかと交わる場所のセルのうち 1 個以上のセルに数字の候補 D が存在し、かつ
            // 7) Row3 のハウスのうち Col1, Col2, Col3 の何れとも交差しないセル (フィン) が 1 個以上 2 個以下存在し、かつ
            // 8) 全てのフィンのセルが同一ブロックに存在している場合。
            //
            // => フィンの何れかのセルに数字 D が入るかどうかの仮定を行ってみる。
            //    a) フィンの何れかのセルに数字 D が入る場合
            //      => 全てのフィンとハウスを共有する他のマスの数字は D ではあり得ない。
            //    b) (a)ではない、つまりフィンの何れのセルも D ではない場合
            //      => Row1, Row2, Row3 と Col1. Col2. Col3 でソードフィッシュが成立するので、Col1, Col2, Col3, Col3 の何れの列も Row1, Row2, Row3 との交差点以外のセルに数字 D は入り得ない。
            //    (a) と (b) のどちらが成立したとしても、以下の条件を満たすマスの数字は D ではあり得ないので、それらのマスの数字の候補から D を排除できる。
            //      排除条件1) 全てのフィンとハウスを共有し (== 全てのフィンと同一ブロックにある)、かつ
            //      排除条件2) Col1, Col2, Col3 の何れかに属し、かつ
            //      排除条件3) Row1, Row2, Row3 の何れにも属さないマス。
            //
            // 注1) 同一行にある数字 D を候補に持つセルの数が 1 の場合、より発見が容易な Hidden Single が適用可能であるため、本実装では 2 個以上の場合のみ考える。
            // 注2) この条件がないと、フィン付きソードフィッシュと成立条件が重複してしまう。
            //
            // ※ サシミパターンの条件
            //   => 以下の条件が一つでも成立すれば、サシミパターンである。
            //   1) Row1 において、Col1. Col2. Col3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。(前提条件により自明的に不成立である)
            //   2) Row2 において、Col1. Col2. Col3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。(前提条件により自明的に不成立である)
            //   3) Row3 において、Col1. Col2. Col3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            //   4) Col1 において、Row1. Row2. Row3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            //   5) Col2 において、Row1. Row2. Row3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            //   6) Col3 において、Row1. Row2. Row3 との交差点にあり数字の候補に D を持つセルの数が 1 個である。
            // ※ Row1, Row2, Row3 と Col1, Col2, Col3 の合計 9 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
            // ※ フィンのマスの個数が 1 個以上 2 個以下である理由は、3 個以上では排除条件を満たせないからである。
            // ※ 上記の論拠は、行と列を言い換えても成立する。

            // フィンが行ハウスにある場合
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

            // フィンが列ハウスにある場合
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
                // digit を候補に持つセルが 2 個以上 5 個以下ある行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 5);

                // digit を候補に持つセルが 2 個以上 5 個以下ある行が 3 個未満である場合はフィン付きソードフィッシュが成立しないので、検索を中断する。
                if (keyRows.Length < 3)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 1; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
                    if (keyRows.Span[keyIndex1].cellCount is >= 2 and <= 3)
                    {
                        var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                        var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= 2 and <= 3);
#endif
                        for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length; ++keyIndex2)
                        {
                            var keyRow2 = keyRows.Span[keyIndex2].row;
                            if (keyRows.Span[keyIndex2].cellCount is >= 2 and <= 3)
                            {
                                var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                                var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= 2 and <= 3);
#endif
                                for (var keyIndex3F = 0; keyIndex3F < keyRows.Length; ++keyIndex3F)
                                {
                                    if (keyIndex3F != keyIndex1 && keyIndex3F != keyIndex2)
                                    {
                                        var keyRow3F = keyRows.Span[keyIndex3F].row;
                                        if (keyRows.Span[keyIndex3F].cellCount is >= 2 and <= 5)
                                        {
                                            var keyRowHouse3F = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3F);
                                            var keyRowCellIndexBitSet3F = allCellIndexBitSet & keyRowHouse3F;
#if DEBUG
                                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet3F) is >= 2 and <= 5);
#endif
                                            // keyRow3F に存在し得るフィンの位置のパターン (フィンの個数は1～2個でありフィン同士は同じ行・ブロックに存在する) を取得する。(全 18 通り)
                                            var finPatterns = SudokuBoardCellGeometry.GetFinCellPatterns(keyRow3F);

                                            // keyRow3F に存在し得るフィンの位置のパターンのすべてに対して、フィン付きソードフィッシュの条件を満たす組み合わせを探す。
                                            for (var finPatternIndex = 0; finPatternIndex < finPatterns.Length; ++finPatternIndex)
                                            {
                                                var (finCellsBitMask, finCellsBlock) = finPatterns.Span[finPatternIndex];
#if DEBUG
                                                System.Diagnostics.Debug.Assert((finCellsBitMask & keyRowHouse3F) != 0);
                                                System.Diagnostics.Debug.Assert((finCellsBitMask & ~keyRowHouse3F) == 0);
                                                System.Diagnostics.Debug.Assert(finCellsBitMask.ExistAllCellsInOneBlock(out _));
                                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellsBitMask) is >= 1 and <= 2);
#endif
                                                var finCellIndexBitSet = keyRowCellIndexBitSet3F & finCellsBitMask;
                                                if (finCellIndexBitSet != 0)
                                                {
#if DEBUG
                                                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is >= 1 and <= 2);
                                                    System.Diagnostics.Debug.Assert(finCellIndexBitSet.ExistAllCellsInOneBlock(out _));
#endif
                                                    var keyRowCellIndexBitSet3 = keyRowCellIndexBitSet3F & ~finCellsBitMask;
                                                    if ((int)UInt128.PopCount(keyRowCellIndexBitSet3) is >= 1 and <= 3)
                                                    {
                                                        var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3).GetUnionByColumn();
                                                        if (keyColumnBitSet.GetPopCount() == 3)
                                                        {
                                                            // 1) フィンが keyRow3F に 存在し、かつ
                                                            // 2) フィンの数は 1 個以上 2 個以下であり、かつ
                                                            // 3) フィンは単一ブロックに収まっており、かつ
                                                            // 4) フィンを除く keyRow1, keyRow2, keyRow3F に存在する数字 digit を候補に持つセルはちょうど 3 個の列に収まっている。
                                                            // => 広義の意味でのソードフィッシュが成立している。
                                                            // ※ フィンを除く keyRow3F のセル数あるいは何れかの列のセル数が 1 かもしれないので、一般的にソードフィッシュとは見做されないパターンである可能性がある。
                                                            //   ※ finCellIndexBitSet のブロックが keyColumn1, keyColumn2, keyColumn3 の何れかの列と交差するかについては
                                                            //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                                            var (keyColumn1, keyColumn2, keyColumn3) = keyColumnBitSet.SplitAsTripleCellColulmns();
                                                            var allExceptFinKeyCellIndexBitSet = keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3;
                                                            var result =
                                                                MatchPatternByHouse(
                                                                    digit,
                                                                    SudokuBoardHouseType.Row,
                                                                    SudokuBoardHouseType.Column,
                                                                    allCellIndexBitSet,
                                                                    keyRowHouse1,
                                                                    keyRowHouse2,
                                                                    keyRowHouse3F,
                                                                    keyRowCellIndexBitSet1,
                                                                    keyRowCellIndexBitSet2,
                                                                    keyRowCellIndexBitSet3,
                                                                    finCellIndexBitSet,
                                                                    finCellsBlock,
                                                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1),
                                                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2),
                                                                    SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3),
                                                                    allExceptFinKeyCellIndexBitSet,
                                                                    keyRow1.ToHouseName(),
                                                                    keyRow2.ToHouseName(),
                                                                    keyRow3F.ToHouseName(),
                                                                    keyColumn1.ToHouseName(),
                                                                    keyColumn2.ToHouseName(),
                                                                    keyColumn3.ToHouseName(),
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
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> keyColumnsBuffer)
            {
                // digit を候補に持つセルが 2 個以上 5 個以下ある列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 5);

                // digit を候補に持つセルが 2 個以上 5 個以下ある列が 3 個未満である場合はフィン付きソードフィッシュが成立しないので、検索を中断する。
                if (keyColumns.Length < 3)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 1; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
                    if (keyColumns.Span[keyIndex1].cellCount is >= 2 and <= 3)
                    {
                        var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                        var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= 2 and <= 3);
#endif
                        for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length; ++keyIndex2)
                        {
                            var keyColumn2 = keyColumns.Span[keyIndex2].column;
                            if (keyColumns.Span[keyIndex2].cellCount is >= 2 and <= 3)
                            {
                                var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                                var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= 2 and <= 3);
#endif
                                for (var keyIndex3F = 0; keyIndex3F < keyColumns.Length; ++keyIndex3F)
                                {
                                    if (keyIndex3F != keyIndex1 && keyIndex3F != keyIndex2)
                                    {
                                        var keyColumn3F = keyColumns.Span[keyIndex3F].column;
                                        if (keyColumns.Span[keyIndex3F].cellCount is >= 2 and <= 5)
                                        {
                                            var keyColumnHouse3F = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3F);
                                            var keyColumnCellIndexBitSet3F = allCellIndexBitSet & keyColumnHouse3F;
#if DEBUG
                                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet3F) is >= 2 and <= 5);
#endif
                                            // keyColumn3F に存在し得るフィンの位置のパターン (フィンの個数は1～2個でありフィン同士は同じ列・ブロックに存在する) を取得する。(全 18 通り)
                                            var finPatterns = SudokuBoardCellGeometry.GetFinCellPatterns(keyColumn3F);

                                            // keyColumn3F に存在し得るフィンの位置のパターンのすべてに対して、フィン付きソードフィッシュの条件を満たす組み合わせを探す。
                                            for (var finPatternIndex = 0; finPatternIndex < finPatterns.Length; ++finPatternIndex)
                                            {
                                                var (finCellsBitMask, finCellsBlock) = finPatterns.Span[finPatternIndex];
#if DEBUG
                                                System.Diagnostics.Debug.Assert((finCellsBitMask & keyColumnHouse3F) != 0);
                                                System.Diagnostics.Debug.Assert((finCellsBitMask & ~keyColumnHouse3F) == 0);
                                                System.Diagnostics.Debug.Assert(finCellsBitMask.ExistAllCellsInOneBlock(out _));
                                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellsBitMask) is >= 1 and <= 2);
#endif
                                                var finCellIndexBitSet = keyColumnCellIndexBitSet3F & finCellsBitMask;
                                                if (finCellIndexBitSet != 0)
                                                {
#if DEBUG
                                                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is >= 1 and <= 2);
                                                    System.Diagnostics.Debug.Assert(finCellIndexBitSet.ExistAllCellsInOneBlock(out _));
#endif
                                                    var keyColumnCellIndexBitSet3 = keyColumnCellIndexBitSet3F & ~finCellsBitMask;
                                                    if ((int)UInt128.PopCount(keyColumnCellIndexBitSet3) is >= 1 and <= 3)
                                                    {
                                                        var keyColumnBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3).GetUnionByRow();
                                                        if (keyColumnBitSet.GetPopCount() == 3)
                                                        {
                                                            // 1) フィンが keyColumn3F に 存在し、かつ
                                                            // 2) フィンの数は 1 個以上 2 個以下であり、かつ
                                                            // 3) フィンは単一ブロックに収まっており、かつ
                                                            // 4) フィンを除く keyColumn1, keyColumn2, keyColumn3F に存在する数字 digit を候補に持つセルはちょうど 3 個の行に収まっている。
                                                            // => 広義の意味でのソードフィッシュが成立している。
                                                            // ※ フィンを除く keyColumn3F のセル数あるいは何れかの行のセル数が 1 かもしれないので、一般的にソードフィッシュとは見做されないパターンである可能性がある。
                                                            //   ※ finCellIndexBitSet のブロックが keyRow1, keyRow2, keyRow3 の何れかの行と交差するかについては
                                                            //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                                            var (keyRow1, keyRow2, keyRow3) = keyColumnBitSet.SplitAsTripleCellRows();
                                                            var allExceptFinKeyCellIndexBitSet = keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3;
                                                            var result =
                                                                MatchPatternByHouse(
                                                                    digit,
                                                                    SudokuBoardHouseType.Column,
                                                                    SudokuBoardHouseType.Row,
                                                                    allCellIndexBitSet,
                                                                    keyColumnHouse1,
                                                                    keyColumnHouse2,
                                                                    keyColumnHouse3F,
                                                                    keyColumnCellIndexBitSet1,
                                                                    keyColumnCellIndexBitSet2,
                                                                    keyColumnCellIndexBitSet3,
                                                                    finCellIndexBitSet,
                                                                    finCellsBlock,
                                                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1),
                                                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2),
                                                                    SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3),
                                                                    allExceptFinKeyCellIndexBitSet,
                                                                    keyColumn1.ToHouseName(),
                                                                    keyColumn2.ToHouseName(),
                                                                    keyColumn3F.ToHouseName(),
                                                                    keyRow1.ToHouseName(),
                                                                    keyRow2.ToHouseName(),
                                                                    keyRow3.ToHouseName(),
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
                    }
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                SudokuBoardHouseType baseLineHouseType,
                SudokuBoardHouseType coverLineHouseType,
                UInt128 cellIndexBitSet,
                UInt128 baseLineHouseBitMask1,
                UInt128 baseLineHouseBitMask2,
                UInt128 baseLineHouseBitMask3F,
                UInt128 baseLineKeyCellIndexBitSet1,
                UInt128 baseLineKeyCellIndexBitSet2,
                UInt128 baseLineKeyCellIndexBitSet3,
                UInt128 finCellIndexBitSet,
                SudokuBoardCellBlock finCellsBlock,
                UInt128 coverLineHouseBitMask1,
                UInt128 coverLineHouseBitMask2,
                UInt128 coverLineHouseBitMask3,
                UInt128 allExceptFinKeyCellIndexBitSet,
                string baseLineHouseName1,
                string baseLineHouseName2,
                string baseLineHouseName3F,
                string coverLineHouseName1,
                string coverLineHouseName2,
                string coverLineHouseName3,
                SashimiFishDegeneratable sashimi)
            {
                var baseLineHouseBitMask = baseLineHouseBitMask1 | baseLineHouseBitMask2 | baseLineHouseBitMask3F;
                var coverLineHouseBitMask = coverLineHouseBitMask1 | coverLineHouseBitMask2 | coverLineHouseBitMask3;
                var excludableCellsRegionBitMask = coverLineHouseBitMask & ~baseLineHouseBitMask & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(finCellsBlock);
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allExceptFinKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allExceptFinKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = cellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

#if DEBUG
                System.Diagnostics.Debug.Assert(UInt128.PopCount(baseLineKeyCellIndexBitSet1) > 1);
                System.Diagnostics.Debug.Assert(UInt128.PopCount(baseLineKeyCellIndexBitSet2) > 1);
#endif
                var isDegeneratable =
                    UInt128.PopCount(allExceptFinKeyCellIndexBitSet & coverLineHouseBitMask1) <= 1
                    || UInt128.PopCount(allExceptFinKeyCellIndexBitSet & coverLineHouseBitMask2) <= 1
                    || UInt128.PopCount(allExceptFinKeyCellIndexBitSet & coverLineHouseBitMask3) <= 1
                    // || UInt128.PopCount(baseLineKeyCellIndexBitSet1) <= 1 // 自明的に不成立
                    // || UInt128.PopCount(baseLineKeyCellIndexBitSet2) <= 1 // 自明的に不成立
                    || UInt128.PopCount(baseLineKeyCellIndexBitSet3) <= 1;

                return
                    new FinnedSwordfishOperation(
                        isDegeneratable,
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat((baseLineHouseBitMask & ~finCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((coverLineHouseBitMask & ~baseLineHouseBitMask).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat(finCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedBlinkingType1AndType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType1)
                            .Concat(finCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType2)),
                        GetDescriptionBuilder(
                            isDegeneratable,
                            digit,
                            baseLineHouseType,
                            coverLineHouseType,
                            baseLineKeyCellIndexBitSet1,
                            baseLineKeyCellIndexBitSet2,
                            baseLineKeyCellIndexBitSet3,
                            finCellIndexBitSet,
                            finCellsBlock,
                            coverLineHouseBitMask1 & allExceptFinKeyCellIndexBitSet,
                            coverLineHouseBitMask2 & allExceptFinKeyCellIndexBitSet,
                            coverLineHouseBitMask3 & allExceptFinKeyCellIndexBitSet,
                            baseLineHouseName1,
                            baseLineHouseName2,
                            baseLineHouseName3F,
                            coverLineHouseName1,
                            coverLineHouseName2,
                            coverLineHouseName3,
                            sashimi));

                static Func<string> GetDescriptionBuilder(
                    bool isDegeneratable,
                    SudokuBoardCellDigit digit,
                    SudokuBoardHouseType baseLineHouseType,
                    SudokuBoardHouseType coverLineHouseType,
                    UInt128 baseLineKeyCellIndexBitSet1,
                    UInt128 baseLineKeyCellIndexBitSet2,
                    UInt128 baseLineKeyCellIndexBitSet3,
                    UInt128 finCellIndexBitSet,
                    SudokuBoardCellBlock finCellsBlock,
                    UInt128 coverLineKeyCellIndexBitSet1,
                    UInt128 coverLineKeyCellIndexBitSet2,
                    UInt128 coverLineKeyCellIndexBitSet3,
                    string baseLineHouseName1,
                    string baseLineHouseName2,
                    string baseLineHouseName3F,
                    string coverLineHouseName1,
                    string coverLineHouseName2,
                    string coverLineHouseName3,
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
                            var baseLineHouseNamesText = $"{baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3F}";
                            var coverLineHouseNamesText = $"{coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}";
                            var finCellPositionsText = string.Join(", ", finCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var finBlockHouseName = finCellsBlock.ToHouseName();
                            if (isDegeneratable)
                            {
                                // フィッシュが退化可能である場合 (サシミソードフィッシュ)

                                var (degenerationMessages, undegeneratedBaseLineHouseNames, undegeneratedCoverLineHouseNames, determinedCellIndexBitSet) =
                                    sashimi.Degenerate(
                                        (baseHouseName, effectedHouseName, cellPositionText)
                                            => $"{baseHouseName}で {digitText} が入り得るマスは{cellPositionText}だけなので{cellPositionText}に {digitText} が入ります。" +
                                                $"したがって、{effectedHouseName}の{cellPositionText}以外のマスに {digitText} は入り得ません。",
                                        (coverHouseName, effectedHouseName, cellPositionText)
                                            => $"{cellPositionText}に {digitText} が入らないと仮定すると{coverHouseName}以外の{coverLineHouseTypeName}で {digitText} が入り得るマスが足りなくなるので{cellPositionText}には {digitText} が入ります。" +
                                                $"したがって、{effectedHouseName}の{cellPositionText}以外のマスに {digitText} は入り得ません。");
#if DEBUG
                                System.Diagnostics.Debug.Assert(undegeneratedBaseLineHouseNames.Length == undegeneratedCoverLineHouseNames.Length);
#pragma warning disable IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)
                                System.Diagnostics.Debug.Assert((determinedCellIndexBitSet != UInt128.Zero) == (degenerationMessages.Length > 0));
#pragma warning restore IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)

                                // サシミフィッシュは退化可能であることが前提なので、以下も成立する。
                                System.Diagnostics.Debug.Assert(determinedCellIndexBitSet != UInt128.Zero);
                                System.Diagnostics.Debug.Assert(degenerationMessages.Length > 0);
#endif
                                var undegeneratedBaseLineHouseNamesText = string.Join(", ", undegeneratedBaseLineHouseNames.AsEnumerable());
                                var undegeneratedCoverLineHouseNamesText = string.Join(", ", undegeneratedCoverLineHouseNames.AsEnumerable());
                                var degenerationTextLines = degenerationMessages.AsEnumerable();
                                if (undegeneratedBaseLineHouseNames.Length > 0)
                                {
                                    var lastFishName =
                                        undegeneratedBaseLineHouseNames.Length switch
                                        {
                                            2 => "X-ウィング",
                                            _ => throw new ApplicationException(),

                                        };
                                    degenerationTextLines =
                                        degenerationTextLines
                                        .Append(
                                            $"残った {undegeneratedBaseLineHouseNamesText} と {undegeneratedCoverLineHouseNamesText} で{lastFishName}が成立するので、{undegeneratedCoverLineHouseNamesText} の各{coverLineHouseTypeName}の {undegeneratedBaseLineHouseNamesText} を除くマスには {digitText} は入り得ません。");
                                }

                                var degenerationText =
                                    string.Join(
                                        "\n",
                                        degenerationTextLines
                                        .Zip(Enumerable.Range(1, int.MaxValue - 1), (message, n) => $"  {n}) {message}"));
                                if (!UInt128.IsPow2(finCellIndexBitSet))
                                {
                                    // フィンのセルが複数存在する場合
                                    return
                                        $"数字 {digitText} に着目します。\n" +
                                        $"以下の条件が全て成立するため、サシミソードフィッシュが成立しています。\n" +
                                        $"1) {baseLineHouseName3F}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                                        $"2) {(
                                            UInt128.PopCount(coverLineKeyCellIndexBitSet1) <= 1
                                            ? $"{coverLineHouseName1}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(coverLineKeyCellIndexBitSet2) <= 1
                                            ? $"{coverLineHouseName2}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(coverLineKeyCellIndexBitSet3) <= 1
                                            ? $"{coverLineHouseName3}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            // : UInt128.PopCount(baseLineKeyCellIndexBitSet1) <= 1 // 自明的に不成立
                                            // ? $"{baseLineHouseName1}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            // : UInt128.PopCount(baseLineKeyCellIndexBitSet2) <= 1 // 自明的に不成立
                                            // ? $"{baseLineHouseName2}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(baseLineKeyCellIndexBitSet3) <= 1
                                            ? $"{baseLineHouseName3F}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : throw new ApplicationException()
                                        )}\n" +
                                        $"3) 全てのフィンが{finBlockHouseName}に収まっている。\n" +
                                        $"\n" +
                                        $"ここで、何れかのフィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                        $"\n" +
                                        $"a) もし、何れかのフィンのマスの数字が {digitText} である場合、" +
                                        $"全てのフィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"b) もし (a) ではない場合、つまり何れのフィンのマスの数字も {digitText} ではない場合、以下のことが言えます。\n" +
                                        $"{degenerationText}\n" +
                                        $"結局 {coverLineHouseNamesText} の各{coverLineHouseTypeName}で {baseLineHouseNamesText} 以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                        $"1) {coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} 以外のマスであり、かつ\n" +
                                        $"2) 全てのフィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                                }
                                else
                                {
                                    // フィンのセルが一つだけ存在する場合
                                    return
                                        $"数字 {digitText} に着目します。\n" + $"以下の条件が全て成立するため、サシミソードフィッシュが成立しています。\n" +
                                        $"1) {baseLineHouseName3F}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                                        $"2) {(
                                            UInt128.PopCount(baseLineKeyCellIndexBitSet1) <= 1
                                            ? $"{baseLineHouseName1}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(baseLineKeyCellIndexBitSet2) <= 1
                                            ? $"{baseLineHouseName2}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(baseLineKeyCellIndexBitSet3) <= 1
                                            ? $"{baseLineHouseName3F}の {coverLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(coverLineKeyCellIndexBitSet1) <= 1
                                            ? $"{coverLineHouseName1}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(coverLineKeyCellIndexBitSet2) <= 1
                                            ? $"{coverLineHouseName2}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : UInt128.PopCount(coverLineKeyCellIndexBitSet3) <= 1
                                            ? $"{coverLineHouseName3}の {baseLineHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。"
                                            : throw new ApplicationException()
                                        )}\n" +
                                        $"\n" +
                                        $"ここで、フィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                        $"\n" +
                                        $"a) もし、フィンのマスの数字が {digitText} である場合、" +
                                        $"フィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"b) もし (a) ではない場合、つまりフィンのマスの数字が {digitText} ではない場合、以下のことが言えます。\n" +
                                        $"{degenerationText}\n" +
                                        $"結局 {coverLineHouseNamesText} の各{coverLineHouseTypeName}で {baseLineHouseNamesText} 以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                        $"1) {coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} 以外のマスであり、かつ\n" +
                                        $"2) フィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                                }
                            }
                            else
                            {
                                // フィッシュが退化可能ではない場合 (フィン付きソードフィッシュ)

                                if (!UInt128.IsPow2(finCellIndexBitSet))
                                {
                                    // フィンのセルが複数存在する場合
                                    return
                                        $"数字 {digitText} に着目します。\n" +
                                        $"以下の条件が全て成立するため、フィン付きソードフィッシュが成立しています。\n" +
                                        $"1) {baseLineHouseName3F}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                                        $"2) 全てのフィンが{finBlockHouseName}に収まっている。\n" +
                                        $"\n" +
                                        $"a) もし、何れかのフィンのマスの数字が {digitText} である場合、" +
                                        $"全てのフィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"b) もし (a) ではない場合、つまり何れのフィンのマスの数字も {digitText} ではない場合、" +
                                        $"ソードフィッシュが成立するので {baseLineHouseNamesText} を除く {coverLineHouseNamesText} のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                        $"1) {baseLineHouseNamesText} を除く {coverLineHouseNamesText} の何れかに存在し、かつ\n" +
                                        $"2) 全てのフィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                                }
                                else
                                {
                                    // フィンのセルが一つだけ存在する場合
                                    return
                                        $"数字 {digitText} に着目します。\n" +
                                        $"{baseLineHouseName3F}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在するため、" +
                                        $"フィン付きソードフィッシュが成立しています。\n" +
                                        $"\n" +
                                        $"ここで、フィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                        $"\n" +
                                        $"a) もし、フィンのマスの数字が {digitText} である場合、" +
                                        $"フィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"b) もし (a) ではない場合、つまりフィンのマスの数字が {digitText} ではない場合、" +
                                        $"ソードフィッシュが成立するので、" +
                                        $"{baseLineHouseNamesText} を除く {coverLineHouseNamesText} のマスには {digitText} は入り得ません。\n" +
                                        $"\n" +
                                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                        $"1) {baseLineHouseNamesText} を除く {coverLineHouseNamesText} の何れかに存在し、かつ\n" +
                                        $"2) フィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                                }
                            }
                        };
                }
            }
        }
    }
}
