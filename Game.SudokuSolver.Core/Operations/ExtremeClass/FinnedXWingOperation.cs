using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// フィン付きX-ウィングパターンのオペレーションのクラスです。
    /// </summary>
    /// <remarks>
    /// フィン付きX-ウィングの論拠では <see cref="FinnedXWingOperation">X-Wing</see> の論拠を前提にしています。
    /// </remarks>
    internal sealed class FinnedXWingOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private FinnedXWingOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "フィン付きX-ウィング (Finned X-Wing)",
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
            // 3) ROW12 とは別の行 ROW34F にセル C3, C4 が存在し、かつ
            // 4) ROW34F において C3, C4 を除く数字の候補 D を持つ全てのセルの集合 (フィン) を CF とし、かつ,
            // 5) CF の大きさは 1 以上 2 であり、かつ
            // 6) C1, C3 は同じ列 COL13 に存在し、かつ
            // 7) C2, C4 は同じ列 COL24 に存在し、かつ
            // 8) C1 とC4 は同じハウスを共有しておらず (== 同じブロックに存在しない)、かつ
            // 9) C2 とC3 は同じハウスを共有しておらず (== 同じブロックに存在しない)、かつ
            // 10) CF の要素の全てのセルと C3 が同一ブロックに存在するか、あるいは CF の要素の全てのセルと C4 が同一ブロックに存在する場合。
            // => X-Wing に似ているが、CF の存在があるためにこのままでは X-Wing は成立しない。
            //   仮に、CF の何れの要素のセルにも数字 D が 入らないと仮定した場合、X-Wing が成立するので、COL13 の C1, C3 を除くセルおよび COL24 の列の C2, C4 を除くセルの数字の候補からは D を排除できる。
            //   また仮に、CF の何れかの要素のセルに数字 D が入る仮定した場合、CF の全ての要素のセルが共有するハウスの他のセルの数字の候補から D を排除できる。
            //   つまり、以下の条件を満たすセルに数字の候補 D があれば、どちらにしろそのセルの数字の候補から D を排除できる。
            //   a) COL13 あるいは COL24 上にある C1, C2, C3, C4 を除くセルであり、かつ
            //   b) CF の全ての要素のセルが共有するハウス (==ブロック) 上の CF の要素の何れでもないセルである。
            // 
            // ※上記の論拠は、行と列を言い換えても成立する。
            // ※フィンのセルは複数存在し得ることに注意すること。
            //   ただしフィン付き X-Wing として有効なフィンの数は 1 個以上 2 個以下である。
            //   その理由は、排除可能な数字の候補が存在するためには、X-Wing を構成するセルの一つと全てのフィンが同一ブロック内に存在しなければならず、
            //   もしフィンの数が 3 個以上あった場合はその条件を満たせないからである。

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
                // 数字 digit を候補に持つセルの数が 2 以上 4 以下の行を抽出する。
                var keyRows = allCellIndexBitSet.ExtractRowsByCellCount(keyRowsBuffer, 2, 4);

                // 数字 digit を候補に持つセルの数が 2 以上 4 以下の行が 2 個未満である場合はフィン付き X-Wing が成立しないため、検索を中断する。
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

                        var (column1, _, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                        var (column2, _, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                        var columnHouseMask13 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column1);
                        var columnHouseMask24 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column2);

                        for (var keyIndex34F = 0; keyIndex34F < keyRows.Length; ++keyIndex34F)
                        {
                            if (keyRows.Span[keyIndex34F].cellCount is 3 or 4)
                            {
                                var row34F = keyRows.Span[keyIndex34F].row;
                                var rowHouseMask34F = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row34F);
                                var keyCellIndexBitSet34F = allCellIndexBitSet & rowHouseMask34F;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyCellIndexBitSet34F) is 3 or 4);
#endif

                                // row12 とば別の行 row34F に数字の候補 digit を持つセルは 3 個または 4 個存在する。

                                var keyCellIndex3BitSet = keyCellIndexBitSet34F & columnHouseMask13;
                                var keyCellIndex4BitSet = keyCellIndexBitSet34F & columnHouseMask24;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyCellIndex3BitSet == 0 || UInt128.IsPow2(keyCellIndex3BitSet), $"{nameof(keyCellIndex3BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
                                System.Diagnostics.Debug.Assert(keyCellIndex4BitSet == 0 || UInt128.IsPow2(keyCellIndex4BitSet), $"{nameof(keyCellIndex4BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
#endif
                                if (keyCellIndex3BitSet != 0 && keyCellIndex4BitSet != 0)
                                {
                                    var keyCellIndex3 = keyCellIndex3BitSet.AsSingleCellIndex();
                                    var keyCellIndex4 = keyCellIndex4BitSet.AsSingleCellIndex();
                                    var finCellIndexBitSet = keyCellIndexBitSet34F & ~keyCellIndex3BitSet & ~keyCellIndex4BitSet;
#if DEBUG
                                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is 1 or 2);
#endif
                                    var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet34F;

                                    // row34F に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndexBitSet のみであり、
                                    // そのうち keyCellIndex3 と keyCellIndex1 は同じ列であり、keyCellIndex4 と keyCellIndex2 は同じ列である。

                                    var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                    var (_, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                                    if (block1 != block4 && block2 != block3 && finCellIndexBitSet.ExistAllCellsInOneBlock())
                                    {
                                        // 1) keyCellIndex1 のセルの列と keyCellIndex3 のセルの列は等しく、かつ
                                        // 2) keyCellIndex2 のセルの列と keyCellIndex4 のセルの列は等しく、かつ
                                        // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                                        // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                        // 5) finCellIndexBitSet の要素の全てのセルの行は keyCellIndex3 および keyCellIndex4 のセルの行と等しく、かつ
                                        // 6) finCellIndexBitSet の要素の全てのセルはただ一つのブロックのみに存在している。
                                        // => フィン付き X-Wing が成立している)。
                                        //   ※ finCellIndexBitSet の各セルのブロックが keyCellIndex3 あるいは keyCellIndex4 のセルのブロックと等しいかどうかについては
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
                                                columnHouseMask24,
                                                rowHouseMask34F,
                                                keyCellIndex1,
                                                keyCellIndex2,
                                                keyCellIndex3,
                                                keyCellIndex4,
                                                finCellIndexBitSet,
                                                finCellsBlock,
                                                row12.ToHouseName(),
                                                column1.ToHouseName(),
                                                column2.ToHouseName(),
                                                row34F.ToHouseName());
                                        if (operation is not null)
                                            yield return operation;
                                    }
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

                // 数字 digit を候補に持つセルの数が 2 以上 4 以下の列が 2 個未満である場合はフィン付き X-Wing が成立しないため、検索を中断する。
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

                        var (_, row1, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex1);
                        var (_, row2, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex2);
                        var rowHouseMask13 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row1);
                        var rowHouseMask24 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row2);

                        for (var keyIndex34F = 0; keyIndex34F < keyColumns.Length; ++keyIndex34F)
                        {
                            if (keyColumns.Span[keyIndex34F].cellCount is 3 or 4)
                            {
                                var column34F = keyColumns.Span[keyIndex34F].column;
                                var columnHouseMask34F = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column34F);
                                var keyCellIndexBitSet34F = allCellIndexBitSet & columnHouseMask34F;
#if DEBUG
                                System.Diagnostics.Debug.Assert((int)UInt128.PopCount(keyCellIndexBitSet34F) is 3 or 4);
#endif

                                // column12 とば別の列 column34F に数字の候補 digit を持つセルは 3 個存在する。
                                var keyCellIndex3BitSet = keyCellIndexBitSet34F & rowHouseMask13;
                                var keyCellIndex4BitSet = keyCellIndexBitSet34F & rowHouseMask24;
#if DEBUG
                                System.Diagnostics.Debug.Assert(keyCellIndex3BitSet == 0 || UInt128.IsPow2(keyCellIndex3BitSet), $"{nameof(keyCellIndex3BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
                                System.Diagnostics.Debug.Assert(keyCellIndex4BitSet == 0 || UInt128.IsPow2(keyCellIndex4BitSet), $"{nameof(keyCellIndex4BitSet)} は 0 であるか、あるいは 1 のビットを一つだけ持つ");
#endif
                                if (keyCellIndex3BitSet != 0 && keyCellIndex4BitSet != 0)
                                {
                                    var keyCellIndex3 = keyCellIndex3BitSet.AsSingleCellIndex();
                                    var keyCellIndex4 = keyCellIndex4BitSet.AsSingleCellIndex();
                                    var finCellIndexBitSet = keyCellIndexBitSet34F & ~keyCellIndex3BitSet & ~keyCellIndex4BitSet;
#if DEBUG
                                    System.Diagnostics.Debug.Assert((int)UInt128.PopCount(finCellIndexBitSet) is 1 or 2);
#endif
                                    var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet34F;

                                    // column34F に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndexBitSet のみであり、
                                    // そのうち keyCellIndex3 と keyCellIndex1 は同じ行であり、keyCellIndex4 と keyCellIndex2 は同じ行である。

                                    var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                    var (_, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                                    if (block1 != block4 && block2 != block3 && finCellIndexBitSet.ExistAllCellsInOneBlock())
                                    {
                                        // 1) keyCellIndex1 のセルの行と keyCellIndex3 のセルの行は等しく、かつ
                                        // 2) keyCellIndex2 のセルの行と keyCellIndex4 のセルの行は等しく、かつ
                                        // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                                        // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                        // 5) finCellIndexBitSet の要素の全てのセルの列は keyCellIndex3 および keyCellIndex4 のセルの列と等しく、かつ
                                        // 6) finCellIndexBitSet の要素の全てのセルはただ一つのブロックのみに存在している。
                                        // => フィン付き X-Wing が成立している)。
                                        //   ※ finCellIndexBitSet の各セルのブロックが keyCellIndex3 あるいは keyCellIndex4 のセルのブロックと等しいかどうかについては
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
                                                rowHouseMask24,
                                                columnHouseMask34F,
                                                keyCellIndex1,
                                                keyCellIndex2,
                                                keyCellIndex3,
                                                keyCellIndex4,
                                                finCellIndexBitSet,
                                                finCellsBlock,
                                                column12.ToHouseName(),
                                                row1.ToHouseName(),
                                                row2.ToHouseName(),
                                                column34F.ToHouseName());
                                        if (operation is not null)
                                            yield return operation;
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
                UInt128 allKeyCellIndexBitSet,
                UInt128 houseMask12,
                UInt128 houseMask13,
                UInt128 houseMask24,
                UInt128 houseMask34F,
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4,
                UInt128 finCellIndexBitSet,
                SudokuBoardCellBlock finCellsBlock,
                string houseName12,
                string houseName13,
                string houseName24,
                string houseName34F)
            {
                // 排除対象のセル:
                // 1) 数字 digit を候補として持ち、かつ
                // 2) 以下の何れかのハウスに属しており、かつ
                //   a) keyCellIndex1 と keyCellIndex3 が共有するハウス
                //   b) keyCellIndex2 と keyCellIndex4 が共有するハウス
                // 3) finCellIndexBitSet の全てのセルと何れかのハウスを共有しており、かつ
                // 4) keyCellIndex1, keyCellIndex2. keyCellIndex3, keyCellIndex4, finCellIndexBitSet の何れでもないセル。
                var allExceptFinKeyCellIndexBitSet = allKeyCellIndexBitSet & ~finCellIndexBitSet;
                var excludableCellsRegionBitMask = (houseMask13 | houseMask24) & ~(houseMask12 | houseMask34F) & SudokuBoardCellGeometry.GetBlockCellIndexBitSet(finCellsBlock);
#if DEBUG
                System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                if (excludableCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new FinnedXWingOperation(
                        excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat(((houseMask12 | houseMask34F) & ~finCellIndexBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((houseMask13 | houseMask24).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat(finCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedBlinkingType1AndType2))
                            .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allExceptFinKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType1)
                            .Concat(finCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PositivelyHilightedType2)),
                        GetDescriptionBuilder(
                            digit,
                            keyCellIndex1,
                            keyCellIndex2,
                            keyCellIndex3,
                            keyCellIndex4,
                            finCellIndexBitSet,
                            houseName12,
                            houseName34F,
                            houseName13,
                            houseName24));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    SudokuBoardCellIndex keyCellIndex4,
                    UInt128 finCellIndexBitSet,
                    string baseLineHouseName12,
                    string baseLineHouseName34F,
                    string coverLineHouseName13,
                    string coverLineHouseName24)
                {
                    return
                        () =>
                        {
                            var existMultipleFinCell = !UInt128.IsPow2(finCellIndexBitSet);
                            var digitText = digit.ToCellDigitText();
                            var cellPositionText1 = keyCellIndex1.ToFriendlyString();
                            var cellPositionText2 = keyCellIndex2.ToFriendlyString();
                            var cellPositionText3 = keyCellIndex3.ToFriendlyString();
                            var cellPositionText4 = keyCellIndex4.ToFriendlyString();
                            var cellPositionsText12 = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet()).EnumerateAsCellPositionTexts());
                            var cellPositionsText34F = string.Join(", ", (keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet() | finCellIndexBitSet).EnumerateAsCellPositionTexts());
                            var cellPositionsTextF = string.Join(", ", finCellIndexBitSet.EnumerateAsCellPositionTexts());
                            var cellPositionsText13 = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet()).EnumerateAsCellPositionTexts());
                            var cellPositionsText24 = string.Join(", ", (keyCellIndex2.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet()).EnumerateAsCellPositionTexts());
                            var cellPositionsText1234F = string.Join(", ", (keyCellIndex1.ToCellIndexBitSet() | keyCellIndex2.ToCellIndexBitSet() | keyCellIndex3.ToCellIndexBitSet() | keyCellIndex4.ToCellIndexBitSet() | finCellIndexBitSet).EnumerateAsCellPositionTexts());
                            return
                                $"数字 {digitText} に着目します。\n" +
                                $"{baseLineHouseName12}では {digitText} を数字の候補に持つマスは {cellPositionsText12} のみなので、 {digitText} は必ず {cellPositionsText12} の何れかのマスに入ります。\n" +
                                $"{baseLineHouseName34F}では {digitText} を数字の候補に持つマスは {cellPositionsText34F} のみなので、 {digitText} は必ず {cellPositionsText34F} の何れかのマスに入ります。\n" +
                                $"そして、" +
                                $"{cellPositionText1} と {cellPositionText3} は共に {coverLineHouseName13} に存在し、少なくとも両方に {digitText} が入ることはあり得ません。\n" +
                                $"同様に、" +
                                $"{cellPositionText2} と {cellPositionText4} は共に {coverLineHouseName24} に存在し、少なくとも両方に {digitText} が入ることはあり得ません。\n" +
                                $"\n" +
                                $"ここで、{baseLineHouseName34F}の、{coverLineHouseName13}と{coverLineHouseName24}のどちらでもないマス ({cellPositionsTextF}) をフィンと呼びます。\n" +
                                $"そして、{(existMultipleFinCell ? "何れかのフィン" : "フィン")}のマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                $"\n" +
                                $"a) もし、{(existMultipleFinCell ? "何れかのフィン" : "フィン")}のマスの数字が {digitText}である場合、" +
                                $"{(existMultipleFinCell ? "全てのフィン" : "フィン")}のマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                                $"\n" +
                                $"b) もし、{(existMultipleFinCell ? "何れのフィン" : "フィン")}のマスの数字も {digitText}でない場合、" +
                                $"X-Wing ({baseLineHouseName12}, {baseLineHouseName34F}, {coverLineHouseName13}, {coverLineHouseName24}) が成立するので、" +
                                $"以下に示すマスには {digitText} は入り得ません。\n" +
                                $"・{cellPositionsText13}を除く{coverLineHouseName13}の全てのマス\n" +
                                $"・{cellPositionsText24}を除く{coverLineHouseName24}の全てのマス\n" +
                                $"\n" +
                                $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                $"1) {coverLineHouseName13}と{coverLineHouseName24}の何れかに存在し、かつ\n" +
                                $"2) {(existMultipleFinCell ? "全てのフィン" : "フィン")}のマス ({cellPositionsTextF}) と同じブロックに存在し、かつ\n" +
                                $"3) {cellPositionsText1234F} の何れでもないマス。";
                        };
                }
            }
        }
    }
}
