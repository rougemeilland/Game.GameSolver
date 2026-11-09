using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// サシミX-ウィングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class SashimiXWingOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private SashimiXWingOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "サシミX-ウィング (Sashimi X-Wing)",
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
            // ある数字 D について、
            // 1) ある行 ROW12 にセル C1, C2 が存在し、かつ
            // 2) ROW12 では C1, C2 以外には数字の候補 D を持つセルは存在せず、かつ
            // 3) ROW12 とは別の行 ROW3F にセル C3 が存在し、かつ
            // 4) ROW3F において C3 を除く数字の候補 D を持つ全てのセルの集合 (フィン) を CF とし、かつ,
            // 5) CF の大きさは 1 以上 2 であり、かつ
            // 6) C1, C3 は同じ列 COL13 に存在し、かつ
            // 7) 何れの CF の要素のセルも C2 と 同じ列には存在せず、かつ
            // 8) CF の何れの要素のセルも C1 と同じハウスを共有しておらず (== 同じブロックに存在しない)、かつ
            // 9) C2 とC3 は同じハウスを共有しておらず (== 同じブロックに存在しない)、かつ
            // 10) CF の要素の全てのセルが同一ブロックに存在する場合。
            // => CF の要素のセルの何れかが数字 D であるかどうかの仮定をする。
            //    a) CF の要素のセルの何れかが数字 D である場合
            //       CFの要素のセルは全て同一のブロックに存在するので、同じブロック内の他のセルの数字は D ではあり得ない。
            //    b) CF の要素のセルの何れも 数字 D ではない場合
            //       この場合、ROW3F において数字 D が入れるセルは C3 のみなので、C3 に D が入る。
            //       次に、COL13 において、C3 の数字が D なので、C1 の数字は D ではあり得ない。
            //       更に、ROW12 において、C1 の数字が D ではないことから D が入れるセルは C2 しかないので、C2 には D が入る。
            //       故に、C2 あるいは C3 を含むどのハウスのセルも C2 および C3 を除いて数字 D が入ることはあり得ない。
            //   (a)と(b)のどちらが成立したとしても、以下の条件を満たすセルの数字は D ではあり得ないので、それらのセルの数字の候補から D を排除できる。
            //   排除条件1) CF の全ての要素のセルと同じブロックに存在し、かつ
            //   排除条件2) C2 と同じ列に存在するセル。
            // 
            // ※上記の論拠は、行と列を言い換えても成立する。
            // ※フィンのセルは複数存在し得ることに注意すること。
            //   ただしサシミ X-Wing として有効なフィンの数は 1 個以上 2 個以下である。
            //   その理由は、排除可能な数字の候補が存在するためには、
            //   a) フィンは同一ブロック内の列または行に存在しなければならないので、少なくとも 3 個以下でなければならない。
            //   b) フィンの数が 3 個だとしても、全てのフィンの要素のセルと C2 あるいは C3 との間には (数字の候補を排除可能な他のセルを含む) 共有のハウスが存在しなければならず、
            //      そのためには、C3 と全てのフィンが同一行/列に存在するか、あるいはフィンが属するブロックと C2 の行/列の交差部分が存在しかつその交差部分にフィンの一部が含まれていてはならない。
            //      (もし交差部分にフィンの一部が含まれていると (7) の条件に反してしまい、サシミX-Wingではなくフィン付きX-WIngとなってしまう)
            //      何れにしても、フィンの数が 3 だとサシミX-Wingとして排除可能な数字の候補を持つことが出来ない。
            // ※フィンの数が 1 個である場合、それはスカイクレイパーと等価である。

            // 行ハウスにフィンが含まれている場合
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

            // 列ハウスにフィンが含まれている場合
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
                // 数字 digit を候補に持つセルの数が 2 以上 3 以下の行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 3);

                // 数字 digit を候補に持つセルの数が 2 以上 3 以下の行が 2 個未満である場合はサシミ X-Wing が成立しないため、検索を中断する。
                if (keyRows.Length < 2)
                    yield break;

                for (var keyIndex12 = 0; keyIndex12 < keyRows.Length; ++keyIndex12)
                {
                    if (keyRows.Span[keyIndex12].cellCount == 2)
                    {
                        var row12 = keyRows.Span[keyIndex12].row;
                        var rowHouseMask12 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row12);
                        var keyCellIndexBitSet12 = allCellIndexBitSet & rowHouseMask12;
#if DEBUG
                        System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet12) == 2);
#endif

                        // 行 row12 に 数字の候補 digit を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex2 である。
                        var (keyCellIndex1, keyCellIndex2) = keyCellIndexBitSet12.SplitAsPairCellIndexes();

                        // サシミX-Wingの条件を満たすフィンのある行を探す。
                        // サシミX-Wingの論法上、行方向に非対称なので、keyCellIndex1, keyCellIndex2 の順番を変えて 2 回試行する。
                        var operations =
                            MatchPatternByFinnedRowHouse(digit, allCellIndexBitSet, keyRows, row12, rowHouseMask12, keyCellIndexBitSet12, keyCellIndex1, keyCellIndex2)
                            .Concat(MatchPatternByFinnedRowHouse(digit, allCellIndexBitSet, keyRows, row12, rowHouseMask12, keyCellIndexBitSet12, keyCellIndex2, keyCellIndex1));
                        foreach (var operarion in operations)
                            yield return operarion;
                    }
                }

                static IEnumerable<SudokuBoardOperation> MatchPatternByFinnedRowHouse(
                    SudokuBoardCellDigit digit,
                    UInt128 allCellIndexBitSet,
                    ReadOnlyMemory<(SudokuBoardCellRow row, int cellCount)> keyRows,
                    SudokuBoardCellRow row12,
                    UInt128 rowHouseMask12,
                    UInt128 keyCellIndexBitSet12,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2)
                {
                    var (column1, _, _) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                    var (column2, _, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                    var columnHouseMask13 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column1);
                    var columnHouseMask2 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column2);

                    for (var keyIndex3F = 0; keyIndex3F < keyRows.Length; ++keyIndex3F)
                    {
                        if (keyRows.Span[keyIndex3F].cellCount is 2 or 3)
                        {
                            var row3F = keyRows.Span[keyIndex3F].row;
                            var rowHouseMask3F = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row3F);
                            var keyCellIndexBitSet3F = allCellIndexBitSet & rowHouseMask3F;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyCellIndexBitSet3F) is 2 or 3);
#endif

                            // row12 とば別の行 row3F に数字の候補 digit を持つセルは 2 個または 3 個存在する。

                            var keyCellIndex3BitSet = keyCellIndexBitSet3F & columnHouseMask13;
                            var keyCellIndex4BitSet = keyCellIndexBitSet3F & columnHouseMask2;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyCellIndex3BitSet == 0 || UInt128.IsPow2(keyCellIndex3BitSet), $"{nameof(keyCellIndex3BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
                            System.Diagnostics.Debug.Assert(keyCellIndex4BitSet == 0 || UInt128.IsPow2(keyCellIndex4BitSet), $"{nameof(keyCellIndex4BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
#endif
                            if (keyCellIndex3BitSet != 0 && keyCellIndex4BitSet == 0)
                            {
                                var keyCellIndex3 = keyCellIndex3BitSet.AsSingleCellIndex();
                                var finCellIndexBitSet = keyCellIndexBitSet3F & ~keyCellIndex3BitSet;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is 1 or 2);
#endif
                                var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet3F;

                                // row3F に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndexBitSet のみであり、
                                // そのうち keyCellIndex3 と keyCellIndex1 は同じ列であり、keyCellIndex4 と keyCellIndex2 は同じ列である。

                                var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                if (block2 != block3 && finCellIndexBitSet.ExistAllCellsInOneBlock())
                                {
                                    // 1) keyCellIndex1 のセルの行と keyCellIndex2 のセルの行は等しく、かつ
                                    // 1) keyCellIndex1 のセルの列と keyCellIndex3 のセルの列は等しく、かつ
                                    // 2) keyCellIndex2 のセルの列と keyCellIndex3 のセルの行の交点のセルは D を数字の候補として持たず、かつ
                                    // 3) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                    // 5) finCellIndexBitSet の要素の全てのセルの行は keyCellIndex3 のセルの行と等しく、かつ
                                    // 6) finCellIndexBitSet の要素の全てのセルはただ一つのブロックのみに存在している。
                                    // => サシミ X-Wing が成立している)。
                                    //   ※ finCellIndexBitSet の各セルのブロックと keyCellIndex2 のマスの列が交点を持つかどうかについては
                                    //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                    // finCellsBlock が全て同一ブロックに属していることが判明しているので、finCellsBlock から適当な要素を一つ取り出してブロックを求める。
                                    var finCellsBlock = SudokuBoardCellGeometry.GetBlock(finCellIndexBitSet.GetOneCellIndex());
                                    var operation =
                                        MatchPatternByHouse(
                                            digit,
                                            allCellIndexBitSet,
                                            allKeyCellIndexBitSet,
                                            rowHouseMask12,
                                            columnHouseMask13,
                                            columnHouseMask2,
                                            rowHouseMask3F,
                                            keyCellIndex1,
                                            keyCellIndex2,
                                            keyCellIndex3,
                                            finCellIndexBitSet,
                                            finCellsBlock,
                                            row12.ToHouseName(),
                                            row3F.ToHouseName(),
                                            column1.ToHouseName(),
                                            column2.ToHouseName());
                                    if (operation is not null)
                                        yield return operation;
                                }
                            }
                        }
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByColumnHouse(SudokuBoardCellDigit digit, UInt128 allCellIndexBitSet, Memory<(SudokuBoardCellColumn column, int cellCount)> keyColumnsBuffer)
            {
                // 数字 digit を候補に持つセルの数が 2 以上 4 以下の列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 4);

                // 数字 digit を候補に持つセルの数が 2 以上 4 以下の列が 2 個未満である場合はサシミX-Wingが成立しないため、検索を中断する。
                if (keyColumns.Length < 2)
                    yield break;

                for (var keyIndex12 = 0; keyIndex12 < keyColumns.Length; ++keyIndex12)
                {
                    if (keyColumns.Span[keyIndex12].cellCount == 2)
                    {
                        var column12 = keyColumns.Span[keyIndex12].column;
                        var columnHouseMask12 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column12);
                        var keyCellIndexBitSet12 = allCellIndexBitSet & columnHouseMask12;
#if DEBUG
                        System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet12) == 2);
#endif
                        // 列 column12 に 数字の候補 digit を持つセルは 2 個だけで、それらの位置は keyCellIndex1 と keyCellIndex2 である。
                        var (keyCellIndex1, keyCellIndex2) = keyCellIndexBitSet12.SplitAsPairCellIndexes();

                        // サシミX-Wingの条件を満たすフィンのある列を探す。
                        // サシミX-Wingの論法上、列方向に非対称なので、keyCellIndex1, keyCellIndex2 の順番を変えて 2 回試行する。
                        var operations =
                            MatchPatternByFinnedColumnHouse(digit, allCellIndexBitSet, keyColumns, column12, columnHouseMask12, keyCellIndexBitSet12, keyCellIndex1, keyCellIndex2)
                            .Concat(MatchPatternByFinnedColumnHouse(digit, allCellIndexBitSet, keyColumns, column12, columnHouseMask12, keyCellIndexBitSet12, keyCellIndex2, keyCellIndex1));
                        foreach (var operarion in operations)
                            yield return operarion;
                    }
                }

                static IEnumerable<SudokuBoardOperation> MatchPatternByFinnedColumnHouse(
                    SudokuBoardCellDigit digit,
                    UInt128 allCellIndexBitSet,
                    ReadOnlyMemory<(SudokuBoardCellColumn column, int cellCount)> keyColumns,
                    SudokuBoardCellColumn column12,
                    UInt128 columnHouseMask12,
                    UInt128 keyCellIndexBitSet12,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2)
                {
                    var (_, row1, _) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                    var (_, row2, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                    var rowHouseMask13 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row1);
                    var rowHouseMask2 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row2);

                    for (var keyIndex3F = 0; keyIndex3F < keyColumns.Length; ++keyIndex3F)
                    {
                        if (keyColumns.Span[keyIndex3F].cellCount is 2 or 3)
                        {
                            var column3F = keyColumns.Span[keyIndex3F].column;
                            var columnHouseMask3F = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column3F);
                            var keyCellIndexBitSet3F = allCellIndexBitSet & columnHouseMask3F;
#if DEBUG
                            System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyCellIndexBitSet3F) is 2 or 3);
#endif

                            // column12 とば別の列 column3F に数字の候補 digit を持つセルは 3 個存在する。
                            var keyCellIndex3BitSet = keyCellIndexBitSet3F & rowHouseMask13;
                            var keyCellIndex4BitSet = keyCellIndexBitSet3F & rowHouseMask2;
#if DEBUG
                            System.Diagnostics.Debug.Assert(keyCellIndex3BitSet == 0 || UInt128.IsPow2(keyCellIndex3BitSet), $"{nameof(keyCellIndex3BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
                            System.Diagnostics.Debug.Assert(keyCellIndex4BitSet == 0 || UInt128.IsPow2(keyCellIndex4BitSet), $"{nameof(keyCellIndex4BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
#endif
                            if (keyCellIndex3BitSet != 0 && keyCellIndex4BitSet == 0)
                            {
                                var keyCellIndex3 = keyCellIndex3BitSet.AsSingleCellIndex();
                                var finCellIndexBitSet = keyCellIndexBitSet3F & ~keyCellIndex3BitSet;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is 1 or 2);
#endif
                                var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet3F;

                                // column3F に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndex のみであり、
                                // そのうち keyCellIndex3 と keyCellIndex1 は同じ行である。

                                var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                if (block2 != block3 && finCellIndexBitSet.ExistAllCellsInOneBlock())
                                {
                                    // 1) keyCellIndex1 のセルの列と keyCellIndex2 のセルの列は等しく、かつ
                                    // 2) keyCellIndex1 のセルの行と keyCellIndex3 のセルの行は等しく、かつ
                                    // 3) keyCellIndex2 のセルの行と keyCellIndex3 のセルの列の交点のセルは D を数字の候補として持たず、かつ
                                    // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                    // 5) finCellIndexBitSet の要素の全てのセルの列は keyCellIndex3 のセルの列と等しく、かつ
                                    // 6) finCellIndexBitSet の要素の全てのセルはただ一つのブロックのみに存在している。
                                    // => サシミ X-Wing が成立している)。
                                    //   ※ finCellIndexBitSet の各セルのブロックと keyCellIndex2 のマスの行が交点を持つかどうかについては
                                    //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。

                                    // finCellsBlock が全て同一ブロックに属していることが判明しているので、finCellsBlock から適当な要素を一つ取り出してブロックを求める。
                                    var finCellsBlock = SudokuBoardCellGeometry.GetBlock(finCellIndexBitSet.GetOneCellIndex());
                                    var operation =
                                        MatchPatternByHouse(
                                            digit,
                                            allCellIndexBitSet,
                                            allKeyCellIndexBitSet,
                                            columnHouseMask12,
                                            rowHouseMask13,
                                            rowHouseMask2,
                                            columnHouseMask3F,
                                            keyCellIndex1,
                                            keyCellIndex2,
                                            keyCellIndex3,
                                            finCellIndexBitSet,
                                            finCellsBlock,
                                            column12.ToHouseName(),
                                            column3F.ToHouseName(),
                                            row1.ToHouseName(),
                                            row2.ToHouseName());
                                    if (operation is not null)
                                        yield return operation;
                                }
                            }
                        }
                    }
                }
            }

            static SudokuBoardOperation? MatchPatternByHouse(
                SudokuBoardCellDigit digit,
                UInt128 allCellIndexBitSet,
                UInt128 allKeyCellIndexBitSet,
                UInt128 baseLineHouseMask12,
                UInt128 baseLineHouseMask13,
                UInt128 coverLineHouseMask2,
                UInt128 coverLineHouseMask3F,
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                UInt128 finCellIndexBitSet,
                SudokuBoardCellBlock finCellsBlock,
                string baseLineHouseName12,
                string baseLineHouseName3F,
                string coverLineHouseName13,
                string coverLineHouseName2)
            {

                // 排除対象のセル:
                // 1) 数字 digit を候補として持ち、かつ
                // 2) keyCellIndex2 と 何れかのハウスを共有しており、かつ
                // 3) finCellIndexBitSet の全てのセルと何れかのハウスを共有しており、かつ
                // 4) keyCellIndex1, keyCellIndex2. keyCellIndex3, keyCellIndex4, finCellIndexBitSet の何れでもないセル。
                var excludableCellRegionBitMask = coverLineHouseMask2 & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(finCellsBlock) & ~allKeyCellIndexBitSet;
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellRegionBitMask & ~allKeyCellIndexBitSet) == excludableCellRegionBitMask, $"{nameof(excludableCellRegionBitMask)} から {nameof(allKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new SashimiXWingOperation(
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        (allKeyCellIndexBitSet & ~finCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(((baseLineHouseMask12 | coverLineHouseMask3F) & ~finCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((baseLineHouseMask13 | coverLineHouseMask2).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat(finCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedBlinkingType1AndType2))
                            .Concat(excludableCellRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        (allKeyCellIndexBitSet & ~finCellIndexBitSet).EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType1)
                            .Concat(finCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType2)),
                        GetDescriptionBuilder(
                            digit,
                            keyCellIndex1,
                            keyCellIndex2,
                            keyCellIndex3,
                            finCellIndexBitSet,
                            baseLineHouseName12,
                            baseLineHouseName3F,
                            coverLineHouseName13,
                            coverLineHouseName2));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    UInt128 finCellIndexBitSet,
                    string baseLineHouseName12,
                    string baseLineHouseName3F,
                    string coverLineHouseName13,
                    string coverLineHouseName2)
                {
                    return
                        () =>
                        {
                            var existMultipleFinCell = !UInt128.IsPow2(finCellIndexBitSet);
                            var digitText = digit.ToCellDigitText();
                            var cellPositionText1 = keyCellIndex1.ToFriendlyString();
                            var cellPositionText2 = keyCellIndex2.ToFriendlyString();
                            var cellPositionText3 = keyCellIndex3.ToFriendlyString();
                            var cellPositionsText12 = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet()).EnumerateAsCellPositionTexts());
                            var cellPositionsText3F = string.Join(", ", (keyCellIndex3.ToCellIndexBitSet() | finCellIndexBitSet).EnumerateAsCellPositionTexts());
                            var cellPositionsTextF = string.Join(", ", finCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var cellPositionsText123 = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet()).EnumerateAsCellPositionTexts());
                            var cellPositionsText123F = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | finCellIndexBitSet).EnumerateAsCellPositionTexts());
                            return
                                $"数字 {digitText} に着目します。\n" +
                                $"{baseLineHouseName12}では {digitText} を数字の候補に持つマスは {cellPositionsText12} のみなので、 {digitText} は必ず {cellPositionsText12} の何れかのマスに入ります。\n" +
                                $"{baseLineHouseName3F}では {digitText} を数字の候補に持つマスは {cellPositionsText3F} のみなので、 {digitText} は必ず {cellPositionsText3F} の何れかのマスに入ります。\n" +
                                $"{cellPositionText1} と {cellPositionText3} は共に {coverLineHouseName13} に存在し、少なくとも両方に {digitText} が入ることはあり得ません。\n" +
                                $"{baseLineHouseName3F} と {coverLineHouseName2} の交点のマスの数字の候補には {digitText} はありません。\n" +
                                $"\n" +
                                $"ここで、{baseLineHouseName3F}の、{coverLineHouseName13}と{coverLineHouseName2}のどちらでもないマス ({cellPositionsTextF}) をフィンと呼びます。\n" +
                                $"そして、{(existMultipleFinCell ? "何れかのフィン" : "フィン")}のマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                $"\n" +
                                $"a) もし、{(existMultipleFinCell ? "何れかのフィン" : "フィン")}のマスの数字が {digitText} である場合、" +
                                $"{(existMultipleFinCell ? "全てのフィン" : "フィン")}のマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                $"\n" +
                                $"b) もし (a) 以外の場合、つまり{(existMultipleFinCell ? "何れのフィン" : "フィン")}のマスの数字も {digitText} ではない場合、" +
                                $"{baseLineHouseName3F}で、フィンの他に {digitText} が入り得るマスは {cellPositionText3} しかないので、{cellPositionText3}のマスには {digitText} が入ります。\n" +
                                $"{coverLineHouseName13}で、{cellPositionText3}のマスの数字が {digitText} なので、{cellPositionText1}のマスの数字は {digitText} ではあり得ません。\n" +
                                $"{baseLineHouseName12}で、{cellPositionText1}の他に {digitText} が入ることが出来るマスが {cellPositionText2} しかないので、{cellPositionText2}のマスには {digitText} が入ります。\n" +
                                $"\n" +
                                $"(a) と (b) のどちらが成立するにしろ、{(existMultipleFinCell ? "全てのフィン" : "フィン")} ({cellPositionsTextF}) のマスおよび{cellPositionText2}のマスとハウスを共有するマスの数字は {digitText} ではあり得ません。\n" +
                                $"したがって、それらのマスの数字の候補から {digitText} を排除します。";
                        };
                }
            }
        }
    }
}
