using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// フィン付きジェリーフィッシュパターンのオペレーションのクラスです。
    /// </summary>
    /// <remarks>
    /// フィン付きジェリーフィッシュの論拠では <see cref="JellyfishOperation">ジェリーフィッシュ</see> の論拠を前提にしています。
    /// </remarks>
    internal sealed class FinnedJellyfishOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private FinnedJellyfishOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "フィン付きジェリーフィッシュ (Finned Jellyfish)",
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
            // 1) Row1 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注1) 4 個以下であり、かつ
            // 2) Row2 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注1) 4 個以下であり、かつ
            // 3) Row3 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所のセルにのみ数字の候補 D が存在し、かつそれらのセルの数は 2 個以上 (注1) 4 個以下であり、かつ
            // 4) Row4 のハウスのセルには Col1, Col2, Col3, Col4 の何れかと交わる場所の数字の候補 D を持つセルが 2 個以上 4 個以下存在し、かつ
            // 5) Col1 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも二つに数字の候補 D が存在し (注2)、かつ
            // 6) Col2 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも二つに数字の候補 D が存在し (注2)、かつ
            // 7) Col3 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも二つに数字の候補 D が存在し (注2)、かつ
            // 8) Col4 のハウスのセルには Row1, Row2, Row3, Row4 の何れかと交わる場所のセルのうち少なくとも二つに数字の候補 D が存在し (注2)、かつ
            // 9) Row4 のハウスのうち Col1, Col2, Col3, Col4 の何れとも交差しないセル (フィン) が 1 個以上 2 個以下存在し、かつ
            // 10) 全てのフィンのセルが同一ブロックに存在している場合。
            // => フィンの何れかのセルに数字 D が入るかどうかの仮定を行ってみる。
            //    a) フィンの何れかのセルに数字 D が入る場合
            //      => 全てのフィンとハウスを共有する他のマスの数字は D ではあり得ない。
            //    b) (a)ではない、つまりフィンの何れのセルも D ではない場合
            //      => Row1, Row2, Row3, Row4, Col1, Col2, Col3, Col4 においてジェリーフィッシュが成立するため、Col1, Col2, Col3, Col4 上の Row1, Row2, Row3, Row4 上にない全てのマスの数字は D ではあり得ない。
            //    (a) と (b) のどちらが成立したとしても、以下の条件を満たすマスの数字は D ではあり得ないので、それらのマスの数字の候補から D を排除できる。
            //      排除条件1) 全てのフィンとハウスを共有し (== 全てのフィンと同一ブロックにある)、かつ
            //      排除条件2) Col1, Col2, Col3, Col4 の何れかに属し、かつ
            //      排除条件3) Row1, Row2, Row3, Row4 の何れにも属さないマス。
            //
            // 注1) 同一行にある数字 D を候補に持つセルの数が 1 の場合、フィン付きジェリーフィッシュより発見が容易な Hidden Single が適用可能であるため、本実装では 2 個以上の場合のみ考える。
            // 注2) この条件がないと、サシミジェリーフィッシュと成立条件が重複してしまう。
            //
            // ※ Row1, Row2, Row3, Row4 と Col1, Col2, Col3, Col4 の合計 16 個の交差点にあるセルには数字の候補に必ず D があるとは限らない。
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
                // digit を候補に持つセルが 2 個以上 6 個以下ある行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 6);

                // digit を候補に持つセルが 2 個以上 6 個以下ある行が 4 個未満である場合はフィン付きジェリーフィッシュが成立しないので、検索を中断する。
                if (keyRows.Length < 4)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyRows.Length - 2; ++keyIndex1)
                {
                    var keyRow1 = keyRows.Span[keyIndex1].row;
                    if (keyRows.Span[keyIndex1].cellCount is >= 2 and <= 4)
                    {
                        var keyRowHouse1 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1);
                        var keyRowCellIndexBitSet1 = allCellIndexBitSet & keyRowHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet1) is >= 2 and <= 4);
#endif
                        for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyRows.Length - 1; ++keyIndex2)
                        {
                            var keyRow2 = keyRows.Span[keyIndex2].row;
                            if (keyRows.Span[keyIndex2].cellCount is >= 2 and <= 4)
                            {
                                var keyRowHouse2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2);
                                var keyRowCellIndexBitSet2 = allCellIndexBitSet & keyRowHouse2;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet2) is >= 2 and <= 4);
#endif
                                for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyRows.Length; ++keyIndex3)
                                {
                                    var keyRow3 = keyRows.Span[keyIndex3].row;
                                    if (keyRows.Span[keyIndex3].cellCount is >= 2 and <= 4)
                                    {
                                        var keyRowHouse3 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3);
                                        var keyRowCellIndexBitSet3 = allCellIndexBitSet & keyRowHouse3;
#if DEBUG
                                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet3) is >= 2 and <= 4);
#endif
                                        var keyColumnBitSet = (keyRowCellIndexBitSet1 | keyRowCellIndexBitSet2 | keyRowCellIndexBitSet3).GetUnionByColumn();
                                        if (keyColumnBitSet.GetPopCount() == 4)
                                        {
                                            // keyRow1, keyRow2, keyRow3 に存在する数字 digit を候補に持つセルはちょうど 4 個の列に収まっている。
                                            // ※ この条件はフィン付きジェリーフィッシュの成立条件に含まれていないが、以下の理由から本実装ではこの条件付けをしている。
                                            // a) この条件を満たしていない場合はより発見が容易な別の定石が適用可能であること。
                                            // b) この条件付けを行った方がフィンの発見が容易であること。

                                            var (keyColumn1, keyColumn2, keyColumn3, keyColumn4) = keyColumnBitSet.SplitAsQuadCellColulmns();
                                            for (var keyIndex4F = 0; keyIndex4F < keyRows.Length; ++keyIndex4F)
                                            {
                                                if (keyIndex4F != keyIndex1 && keyIndex4F != keyIndex2 && keyIndex4F != keyIndex3)
                                                {
                                                    var keyRow4F = keyRows.Span[keyIndex4F].row;
                                                    if (keyRows.Span[keyIndex4F].cellCount is >= 3 and <= 6)
                                                    {
                                                        var keyRowHouse4F = SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4F);
                                                        var keyRowCellIndexBitSet4F = allCellIndexBitSet & keyRowHouse4F;
#if DEBUG
                                                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyRowCellIndexBitSet4F) is >= 3 and <= 6);
#endif
                                                        var operation =
                                                            MatchPatternByHouse(
                                                                digit,
                                                                allCellIndexBitSet,
                                                                keyRowHouse1,
                                                                keyRowHouse2,
                                                                keyRowHouse3,
                                                                keyRowHouse4F,
                                                                keyRowCellIndexBitSet1,
                                                                keyRowCellIndexBitSet2,
                                                                keyRowCellIndexBitSet3,
                                                                keyRowCellIndexBitSet4F,
                                                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1),
                                                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2),
                                                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3),
                                                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4),
                                                                keyRow1.ToHouseName(),
                                                                keyRow2.ToHouseName(),
                                                                keyRow3.ToHouseName(),
                                                                keyRow4F.ToHouseName(),
                                                                keyColumn1.ToHouseName(),
                                                                keyColumn2.ToHouseName(),
                                                                keyColumn3.ToHouseName(),
                                                                keyColumn4.ToHouseName());
                                                        if (operation is not null)
                                                            yield return operation;
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
                // digit を候補に持つセルが 2 個以上 6 個以下ある列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 6);

                // digit を候補に持つセルが 2 個以上 6 個以下ある列が 4 個未満である場合はフィン付きジェリーフィッシュが成立しないので、検索を中断する。
                if (keyColumns.Length < 4)
                    yield break;

                for (var keyIndex1 = 0; keyIndex1 < keyColumns.Length - 2; ++keyIndex1)
                {
                    var keyColumn1 = keyColumns.Span[keyIndex1].column;
                    if (keyColumns.Span[keyIndex1].cellCount is >= 2 and <= 4)
                    {
                        var keyColumnHouse1 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn1);
                        var keyColumnCellIndexBitSet1 = allCellIndexBitSet & keyColumnHouse1;
#if DEBUG
                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet1) is >= 2 and <= 4);
#endif
                        for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyColumns.Length - 1; ++keyIndex2)
                        {
                            var keyColumn2 = keyColumns.Span[keyIndex2].column;
                            if (keyColumns.Span[keyIndex2].cellCount is >= 2 and <= 4)
                            {
                                var keyColumnHouse2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn2);
                                var keyColumnCellIndexBitSet2 = allCellIndexBitSet & keyColumnHouse2;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet2) is >= 2 and <= 4);
#endif
                                for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyColumns.Length; ++keyIndex3)
                                {
                                    var keyColumn3 = keyColumns.Span[keyIndex3].column;
                                    if (keyColumns.Span[keyIndex3].cellCount is >= 2 and <= 4)
                                    {
                                        var keyColumnHouse3 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn3);
                                        var keyColumnCellIndexBitSet3 = allCellIndexBitSet & keyColumnHouse3;
#if DEBUG
                                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet3) is >= 2 and <= 4);
#endif
                                        var keyRowBitSet = (keyColumnCellIndexBitSet1 | keyColumnCellIndexBitSet2 | keyColumnCellIndexBitSet3).GetUnionByRow();
                                        if (keyRowBitSet.GetPopCount() == 4)
                                        {
                                            // keyColumn1, keyColumn2, keyColumn3 に存在する数字 digit を候補に持つセルはちょうど 4 個の行に収まっている。
                                            // ※ この条件はフィン付きジェリーフィッシュの成立条件に含まれていないが、以下の理由から本実装ではこの条件付けをしている。
                                            // a) この条件を満たしていない場合はより発見が容易な別の定石が適用可能であること。
                                            // b) この条件付けを行った方がフィンの発見が容易であること。

                                            var (keyRow1, keyRow2, keyRow3, keyRow4) = keyRowBitSet.SplitAsQuadCellRows();
                                            for (var keyIndex4F = 0; keyIndex4F < keyColumns.Length; ++keyIndex4F)
                                            {
                                                if (keyIndex4F != keyIndex1 && keyIndex4F != keyIndex2 && keyIndex4F != keyIndex3)
                                                {
                                                    var keyColumn4F = keyColumns.Span[keyIndex4F].column;
                                                    if (keyColumns.Span[keyIndex4F].cellCount is >= 3 and <= 6)
                                                    {
                                                        var keyColumnHouse4F = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(keyColumn4F);
                                                        var keyColumnCellIndexBitSet4F = allCellIndexBitSet & keyColumnHouse4F;
#if DEBUG
                                                        System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyColumnCellIndexBitSet4F) is >= 3 and <= 6);
#endif
                                                        var operation =
                                                            MatchPatternByHouse(
                                                                digit,
                                                                allCellIndexBitSet,
                                                                keyColumnHouse1,
                                                                keyColumnHouse2,
                                                                keyColumnHouse3,
                                                                keyColumnHouse4F,
                                                                keyColumnCellIndexBitSet1,
                                                                keyColumnCellIndexBitSet2,
                                                                keyColumnCellIndexBitSet3,
                                                                keyColumnCellIndexBitSet4F,
                                                                SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow1),
                                                                SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow2),
                                                                SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow3),
                                                                SudokuBoardCellGeometry.GetRowCellIndexBitSet(keyRow4),
                                                                keyColumn1.ToHouseName(),
                                                                keyColumn2.ToHouseName(),
                                                                keyColumn3.ToHouseName(),
                                                                keyColumn4F.ToHouseName(),
                                                                keyRow1.ToHouseName(),
                                                                keyRow2.ToHouseName(),
                                                                keyRow3.ToHouseName(),
                                                                keyRow4.ToHouseName());
                                                        if (operation is not null)
                                                            yield return operation;
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
                UInt128 allCellIndexBitSet,
                UInt128 baseLineHouseBitMask1,
                UInt128 baseLineHouseBitMask2,
                UInt128 baseLineHouseBitMask3,
                UInt128 baseLineHouseBitMask4F,
                UInt128 baseLineKeyCellIndexBitSet1,
                UInt128 baseLineKeyCellIndexBitSet2,
                UInt128 baseLineKeyCellIndexBitSet3,
                UInt128 baseLineKeyCellIndexBitSet4F,
                UInt128 coverLineHouseBitMask1,
                UInt128 coverLineHouseBitMask2,
                UInt128 coverLineHouseBitMask3,
                UInt128 coverLineHouseBitMask4,
                string baseLineHouseName1,
                string baseLineHouseName2,
                string baseLineHouseName3,
                string baseLineHouseName4F,
                string coverLineHouseName1,
                string coverLineHouseName2,
                string coverLineHouseName3,
                string coverLineHouseName4)
            {
                var coverLineHouseBitMask = coverLineHouseBitMask1 | coverLineHouseBitMask2 | coverLineHouseBitMask3 | coverLineHouseBitMask4;
                var baseLineKeyCellIndexBitSet4 = baseLineKeyCellIndexBitSet4F & coverLineHouseBitMask;
                var finCellIndexBitSet = baseLineKeyCellIndexBitSet4F & ~baseLineKeyCellIndexBitSet4;
                if ((int)UInt128.PopCount(baseLineKeyCellIndexBitSet4) is < 2 or > 4 || (int)UInt128.PopCount(finCellIndexBitSet) is < 1 or > 2 || !finCellIndexBitSet.ExistAllCellsInOneBlock(out var finCellsBlock))
                    return null;

                // 1) フィンを除いて各ベースラインに 2 個以上 4 個以下の数字の候補 D を持つセルがあり、かつ
                // 2) フィンを除くそれらの全てのセルはちょうど 4 個のカバーラインに収まっており、かつ
                // 3) フィンの数は 1 以上 2 以下であり、かつ
                // 4) フィンは同一ブロックに収まっている。
                // => フィン付きジェリーフィッシュの成立
                // ※ 各カバーラインにおいて交差点上にある数字 digit を候補に持つセルの数が 2 以上であるかどうかのチェックは以降に行っている。
                // ※ finCellIndexBitSet のブロックが何れかのカバーラインと交差するかについては
                //    以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                var allExceptFinKeyCellIndexBitSet = baseLineKeyCellIndexBitSet1 | baseLineKeyCellIndexBitSet2 | baseLineKeyCellIndexBitSet3 | baseLineKeyCellIndexBitSet4;
                var coverLineKeyCellIndexBitSet1 = coverLineHouseBitMask1 & allExceptFinKeyCellIndexBitSet;
                var coverLineKeyCellIndexBitSet2 = coverLineHouseBitMask2 & allExceptFinKeyCellIndexBitSet;
                var coverLineKeyCellIndexBitSet3 = coverLineHouseBitMask3 & allExceptFinKeyCellIndexBitSet;
                var coverLineKeyCellIndexBitSet4 = coverLineHouseBitMask4 & allExceptFinKeyCellIndexBitSet;

                // 以下の条件が成立しない場合、フィン付きジェリーフィッシュではなくサシミジェリーフィッシュの手法を利用して解かれるべきであるので、探索を中止する。
                // 1) 全てのカバーラインにおいて、カバーライン上の交差点上にある数字の候補が digit であるセルの数が 2 以上であること。
                if ((int)UInt128.PopCount(coverLineKeyCellIndexBitSet1) < 2
                    || (int)UInt128.PopCount(coverLineKeyCellIndexBitSet2) < 2
                    || (int)UInt128.PopCount(coverLineKeyCellIndexBitSet3) < 2
                    || (int)UInt128.PopCount(coverLineKeyCellIndexBitSet4) < 2)
                {
                    return null;
                }

                var baseLineHouseBitMask = baseLineHouseBitMask1 | baseLineHouseBitMask2 | baseLineHouseBitMask3 | baseLineHouseBitMask4F;
                var excludableCellsRegionBitMask = coverLineHouseBitMask & ~baseLineHouseBitMask & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(finCellsBlock);
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allExceptFinKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allExceptFinKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new FinnedJellyfishOperation(
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
                            baseLineKeyCellIndexBitSet1,
                            baseLineKeyCellIndexBitSet2,
                            baseLineKeyCellIndexBitSet3,
                            baseLineKeyCellIndexBitSet4,
                            finCellIndexBitSet,
                            finCellsBlock,
                            coverLineKeyCellIndexBitSet1,
                            coverLineKeyCellIndexBitSet2,
                            coverLineKeyCellIndexBitSet3,
                            coverLineKeyCellIndexBitSet4,
                            baseLineHouseName1,
                            baseLineHouseName2,
                            baseLineHouseName3,
                            baseLineHouseName4F,
                            coverLineHouseName1,
                            coverLineHouseName2,
                            coverLineHouseName3,
                            coverLineHouseName4));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    UInt128 baseLineKeyCellIndexBitSet1,
                    UInt128 baseLineKeyCellIndexBitSet2,
                    UInt128 baseLineKeyCellIndexBitSet3,
                    UInt128 baseLineKeyCellIndexBitSet4,
                    UInt128 finCellIndexBitSet,
                    SudokuBoardCellBlock finCellsBlock,
                    UInt128 coverLineKeyCellIndexBitSet1,
                    UInt128 coverLineKeyCellIndexBitSet2,
                    UInt128 coverLineKeyCellIndexBitSet3,
                    UInt128 coverLineKeyCellIndexBitSet4,
                    string baseLineHouseName1,
                    string baseLineHouseName2,
                    string baseLineHouseName3,
                    string baseLineHouseName4F,
                    string coverLineHouseName1,
                    string coverLineHouseName2,
                    string coverLineHouseName3,
                    string coverLineHouseName4)
                {
                    return
                        () =>
                        {
                            var digitText = digit.ToCellDigitText();
                            var finBlockHouseName = finCellsBlock.ToHouseName();
                            var coverLineKeyCellPositionsTextF = string.Join(", ", finCellIndexBitSet.EnumerateAsCellPositionTexts());
                            if (!UInt128.IsPow2(finCellIndexBitSet))
                            {
                                // フィンのセルが複数存在する場合
                                return
                                    $"数字 {digitText} に着目します。\n" +
                                    $"{baseLineHouseName4F}にあるフィンと呼ばれる一部のマス ({coverLineKeyCellPositionsTextF}) を除けば {baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} の数字の候補 {digitText} を含むマスは {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} と交差するマスの何れかにのみに存在し、" +
                                    $"かつフィンは単一のブロック ({finBlockHouseName}) に収まっているので、" +
                                    $"フィン付きジェリーフィッシュが成立しています。\n" +
                                    $"\n" +
                                    $"ここで、何れかのフィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                    $"\n" +
                                    $"a) もし、何れかのフィンのマスの数字が {digitText} である場合、" +
                                    $"全てのフィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                    $"\n" +
                                    $"b) もし (a) ではない場合、つまり何れのフィンのマスの数字も {digitText} ではない場合、" +
                                    $"ジェリーフィッシュが成立するので、" +
                                    $"{baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} を除く {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} のマスには {digitText} は入り得ません。\n" +
                                    $"\n" +
                                    $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                    $"1) {baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} を除く {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} の何れかに存在し、かつ\n" +
                                    $"2) 全てのフィンのマス ({coverLineKeyCellPositionsTextF}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                            }
                            else
                            {
                                // フィンのセルが一つだけ存在する場合
                                return
                                    $"数字 {digitText} に着目します。\n" +
                                    $"{baseLineHouseName4F}にあるフィンと呼ばれる一部のマス ({coverLineKeyCellPositionsTextF}) を除けば {baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} の数字の候補 {digitText} を含むマスは {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} と交差するマスの何れかにのみに存在するので、" +
                                    $"フィン付きジェリーフィッシュが成立しています。\n" +
                                    $"\n" +
                                    $"ここで、フィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                    $"\n" +
                                    $"a) もし、フィンのマスの数字が {digitText} である場合、" +
                                    $"フィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                    $"\n" +
                                    $"b) もし (a) ではない場合、つまりフィンのマスの数字が {digitText} ではない場合、" +
                                    $"ジェリーフィッシュが成立するので、" +
                                    $"{baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} を除く {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} のマスには {digitText} は入り得ません。\n" +
                                    $"\n" +
                                    $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                    $"1) {baseLineHouseName1}, {baseLineHouseName2}, {baseLineHouseName3}, {baseLineHouseName4F} を除く {coverLineHouseName1}, {coverLineHouseName2}, {coverLineHouseName3}, {coverLineHouseName4} の何れかに存在し、かつ\n" +
                                    $"2) フィンのマス ({coverLineKeyCellPositionsTextF}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                            }
                        };
                }
            }
        }
    }
}
