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
            // 1) ある行にセル C1, C2 の何れの候補にも D が含まれており、かつ
            // 2) C1, C2 と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 3) 別の行のセル  C3, C4, CF の何れの候補にも D が含まれており、かつ
            // 4) C3, C4, CF と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 5) C1, C3 は同じ列に存在し、かつ
            // 6) C2, C4 は同じ列に存在し、かつ
            // 7) C1 とC4 は同じブロックには存在せず、かつ
            // 8) C2 とC3 は同じブロックには存在せず、かつ
            // 9) C3 または C4 と CF が同じブロックに存在する場合。
            // => X-Wing に似ているが、CF (フィン) の存在があるためにこのままでは X-Wing は成立しない。
            //   仮に、CF に入る数字が D ではない場合、X-Wing が成立するので、C1-C3 の列の C1, C3 を除くセルおよび C2-C4 の列の C2, C4 を除くセルの数字の候補からは D を排除できる。
            //   また仮に、、CF に入る数字が D である場合、CF が存在するブロックの他のセルの数字の候補から D を削除できる。
            //   つまり、以下の条件を満たすセルに数字の候補 D があれば、どちらにしろそのセルの数字の候補から D を排除できる。
            //   a) C1-C3 の列あるいは C2-C4 の列の C1, C2, C3, C4 を除くセルであり、かつ
            //   b) CF と同一ブロックにあるセルである。
            // 
            // ※上記の論拠は、行と列を言い換えても成立する。

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

                // 数字 digit を候補に持つセルの数が 2 以上 3 以下の行が 2 個未満である場合はフィン付き X-Wing が成立しないため、検索を中断する。
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

                        for (var keyIndex34 = 0; keyIndex34 < keyRows.Length; ++keyIndex34)
                        {
                            if (keyRows.Span[keyIndex34].cellCount == 3)
                            {
                                var row34 = keyRows.Span[keyIndex34].row;
                                var rowHouseMask34 = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row34);
                                var keyCellIndexBitSet34F = allCellIndexBitSet & rowHouseMask34;
#if DEBUG
                                System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet34F) == 3);
#endif

                                // row12 とば別の行 row34 に数字の候補 digit を持つセルは 3 個存在する。

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
                                    System.Diagnostics.Debug.Assert(UInt128.IsPow2(finCellIndexBitSet), $"{nameof(finCellIndexBitSet)} は 1 のビットを一つだけ持つ");
#endif
                                    var finCellIndex = finCellIndexBitSet.AsSingleCellIndex();
                                    var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet34F;

                                    // row34 に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndex のみであり、
                                    // そのうち keyCellIndex3 と keyCellIndex1 は同じ列であり、keyCellIndex4 と keyCellIndex2 は同じ列である。

                                    var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                    var (_, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                                    if (block1 != block4 && block2 != block3)
                                    {
                                        // 1) keyCellIndex1 のセルの列と keyCellIndex3 のセルの列は等しく、かつ
                                        // 2) keyCellIndex2 のセルの列と keyCellIndex4 のセルの列は等しく、かつ
                                        // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                                        // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                        // 5) finCellIndex のセルの行は keyCellIndex3 および keyCellIndex4 のセルの行と等しい。
                                        // => フィン付き X-Wing が成立している)。
                                        //   ※ finCellIndex のセルのブロックが keyCellIndex3 あるいは keyCellIndex4 のセルのブロックと等しいかどうかについては
                                        //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。
                                        var operation =
                                            MatchPatternByHouse(
                                                digit,
                                                allCellIndexBitSet,
                                                allKeyCellIndexBitSet,
                                                rowHouseMask12,
                                                columnHouseMask13,
                                                columnHouseMask24,
                                                rowHouseMask34,
                                                keyCellIndex1,
                                                keyCellIndex2,
                                                keyCellIndex3,
                                                keyCellIndex4,
                                                finCellIndex,
                                                row12.ToHouseName(),
                                                column1.ToHouseName(),
                                                column2.ToHouseName(),
                                                row34.ToHouseName());
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
                // 数字 digit を候補に持つセルの数が 2 以上 3 以下の列を抽出する。
                var keyColumns = allCellIndexBitSet.ExtractColumnsByCellCount(keyColumnsBuffer, 2, 3);

                // 数字 digit を候補に持つセルの数が 2 以上 3 以下の列が 2 個未満である場合はフィン付き X-Wing が成立しないため、検索を中断する。
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

                        for (var keyIndex34 = 0; keyIndex34 < keyColumns.Length; ++keyIndex34)
                        {
                            if (keyColumns.Span[keyIndex34].cellCount == 3)
                            {
                                var column34 = keyColumns.Span[keyIndex34].column;
                                var _columnHouseMask34 = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column34);
                                var keyCellIndexBitSet34F = allCellIndexBitSet & _columnHouseMask34;
#if DEBUG
                                System.Diagnostics.Debug.Assert(UInt128.PopCount(keyCellIndexBitSet34F) == 3);
#endif

                                // column12 とば別の列 column34 に数字の候補 digit を持つセルは 3 個存在する。
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
                                    System.Diagnostics.Debug.Assert(UInt128.IsPow2(finCellIndexBitSet), $"{nameof(finCellIndexBitSet)} は 1 のビットを一つだけ持つ");
#endif
                                    var finCellIndex = finCellIndexBitSet.AsSingleCellIndex();
                                    var allKeyCellIndexBitSet = keyCellIndexBitSet12 | keyCellIndexBitSet34F;

                                    // column34 に数字の候補 digit を持つセルは keyCellIndex3, keyCellIndex4, finCellIndex のみであり、
                                    // そのうち keyCellIndex3 と keyCellIndex1 は同じ行であり、keyCellIndex4 と keyCellIndex2 は同じ行である。

                                    var (_, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex3);
                                    var (_, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(keyCellIndex4);
                                    if (block1 != block4 && block2 != block3)
                                    {
                                        // 1) keyCellIndex1 のセルの行と keyCellIndex3 のセルの行は等しく、かつ
                                        // 2) keyCellIndex2 のセルの行と keyCellIndex4 のセルの行は等しく、かつ
                                        // 3) keyCellIndex1 のセルのブロックと keyCellIndex4 のセルのブロックは等しくなく、かつ
                                        // 4) keyCellIndex2 のセルのブロックと keyCellIndex3 のセルのブロックは等しくなく、かつ
                                        // 5) finCellIndex のセルの列は keyCellIndex3 および keyCellIndex4 のセルの列と等しい。
                                        // => フィン付き X-Wing が成立している)。
                                        //   ※ finCellIndex のセルのブロックが keyCellIndex3 あるいは keyCellIndex4 のセルのブロックと等しいかどうかについては
                                        //   以降の排除対象のセルの数字の候補が存在するかどうかのチェックで同時に行うので、ここでは省略している。
                                        var operation =
                                            MatchPatternByHouse(
                                                digit,
                                                allCellIndexBitSet,
                                                allKeyCellIndexBitSet,
                                                columnHouseMask12,
                                                rowHouseMask13,
                                                rowHouseMask24,
                                                _columnHouseMask34,
                                                keyCellIndex1,
                                                keyCellIndex2,
                                                keyCellIndex3,
                                                keyCellIndex4,
                                                finCellIndex,
                                                column12.ToHouseName(),
                                                row1.ToHouseName(),
                                                row2.ToHouseName(),
                                                column34.ToHouseName());
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
                UInt128 houseMask34,
                SudokuBoardCellIndex keyCellIndex1,
                SudokuBoardCellIndex keyCellIndex2,
                SudokuBoardCellIndex keyCellIndex3,
                SudokuBoardCellIndex keyCellIndex4,
                SudokuBoardCellIndex finCellIndex,
                string houseName12,
                string houseName13,
                string houseName24,
                string houseName34)
            {

                // 排除対象のセル:
                // 1) 数字 digit を候補として持ち、かつ
                // 2) house13 または house24 に属しており、かつ
                // 3) finCellIndex と何れかのハウスを共有しており、かつ
                // 4) keyCellIndex1, keyCellIndex2. keyCellIndex3, keyCellIndex4, finCellIndex の何れでもないセル。
                var excludedCellRegionBitSet =
                    (houseMask13 | houseMask24)
                    & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(finCellIndex)
                    & ~allKeyCellIndexBitSet;
                var excludedCellIndexBitSet = allCellIndexBitSet & excludedCellRegionBitSet;
                if (excludedCellIndexBitSet == 0)
                    return null;

                // 排除対象の数字の候補が存在する

                return
                    new FinnedXWingOperation(
                        excludedCellIndexBitSet.EnumerateAsCellNotes(digit),
                        allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat((houseMask12 | houseMask34).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                            .Concat((houseMask13 | houseMask24).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                            .Concat(excludedCellRegionBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        GetDescriptionBuilder(
                            digit,
                            keyCellIndex1,
                            keyCellIndex2,
                            keyCellIndex3,
                            keyCellIndex4,
                            finCellIndex,
                            houseName12,
                            houseName13,
                            houseName24,
                            houseName34));

                static Func<string> GetDescriptionBuilder(
                    SudokuBoardCellDigit digit,
                    SudokuBoardCellIndex keyCellIndex1,
                    SudokuBoardCellIndex keyCellIndex2,
                    SudokuBoardCellIndex keyCellIndex3,
                    SudokuBoardCellIndex keyCellIndex4,
                    SudokuBoardCellIndex finCellIndex,
                    string houseName12,
                    string houseName13,
                    string houseName24,
                    string houseName34)
                {
                    return
                        () =>
                        {
                            var digitText = digit.ToCellDigitText();
                            var cell1PositionText = keyCellIndex1.ToFriendlyString();
                            var cell2PositionText = keyCellIndex2.ToFriendlyString();
                            var cell3PositionText = keyCellIndex3.ToFriendlyString();
                            var cell4PositionText = keyCellIndex4.ToFriendlyString();
                            var cellFinPositionText = finCellIndex.ToFriendlyString();
                            return
                                $"{houseName12}では数字 {digitText} の候補は {cell1PositionText} と {cell2PositionText} のマスにしか存在しないので、数字 {digitText} は必ずどちらかのマスだけに入ります。\n" +
                                $"{houseName34}では数字 {digitText} の候補は {cell3PositionText} と {cell4PositionText} と {cellFinPositionText} のマスにしか存在しないので、数字 {digitText} は必ず何れかのマスだけに入ります。\n" +
                                $"そして、" +
                                $"{cell1PositionText} と {cell3PositionText} は共に {houseName13} に存在し、少なくとも両方に {digitText} が入ることはあり得ません。\n" +
                                $"同様に、" +
                                $"{cell2PositionText} と {cell4PositionText} は共に {houseName24} に存在し、少なくとも両方に {digitText} が入ることはあり得ません。\n" +
                                $"ここで、{cellFinPositionText} のマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                                $"a) もし、{cellFinPositionText} のマスの数字が {digitText}である場合、" +
                                $"{cellFinPositionText} のマスとハウスを共有する全てのマスの数字の数字の候補から {digitText} を排除できます。\n" +
                                $"b) もし、{cellFinPositionText} のマスの数字が {digitText}でない場合、" +
                                $"X-Wing ({houseName12}, {houseName34}, {houseName13}, {houseName24}) が成立するので、" +
                                $"{houseName13}または{houseName24}にある {cell1PositionText},  {cell2PositionText},  {cell3PositionText},  {cell4PositionText} を除くすべてのマスの数字の候補から {digitText} を排除できます。\n" +
                                $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスは数字 {digitText} ではあり得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                                $"1) {houseName13}または{houseName24}のどちらかに存在し、かつ\n" +
                                $"2) {cellFinPositionText} のマスと同じハウスを共有しており、かつ\n" +
                                $"3) {cell1PositionText},  {cell2PositionText},  {cell3PositionText},  {cell4PositionText}, {cellFinPositionText} の何れでもないマス。";
                        };
                }
            }
        }
    }
}
