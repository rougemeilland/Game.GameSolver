using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// X-ウィングパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class XWingOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private XWingOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "X-ウィング",
                  SudokuBoardDifficultyLevel.Hard,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // ある数字 D について、
            // 1) ある行にセル C1, C2 の何れの候補にも D が含まれており、かつ
            // 2) C1, C2 と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 3) 別の行のセル  C3, C4 の何れの候補にも D が含まれており、かつ
            // 4) C3, C4 と同じ行の他のセルの数字の候補には D は含まれておらず、かつ
            // 5) C1, C3 は同じ列に存在し、かつ
            // 6) C2, C4 は同じ列に存在し、かつ
            // 7) C1 とC4 は同じブロックには存在せず、かつ
            // 8) C2 とC3 は同じブロックには存在しない場合。
            // => C1, C2 の数字は何れかだけが必ず D であり、C3, C4 の数字も何れかだけが必ず D である。
            //    そして、C1, C3 は同時には D になり得ないので、C2, C4 の何れかだけが必ず D となる。そのため、C1, C3 も何れかだけが D となる。
            //    したがって、数字の候補を排除できる。
            //    a) C1, C2 と同じハウス (ブロック) のセルに数字の候補 D
            //    b) C1, C3 と同じハウス (列またはブロック) のセルに数字の候補 D
            //    c) C2, C4 と同じハウス (列またはブロック) のセルに数字の候補 D
            //    d) C3, C4 と同じハウス (ブロック) のセルに数字の候補 D
            // 
            // ※上記の論拠は、行と列を言い換えても成立する。

            for (var digit = SudokuBoardCellDigit.MinValue; digit <= SudokuBoardCellDigit.MaxValue; ++digit)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(digit);

                // 行ハウスに含まれる数字が 2 個のみの場合
                for (var row12 = SudokuBoardCellRow.MinValue; row12 <= SudokuBoardCellRow.MaxValue - 1; ++row12)
                {
                    var rowHouse12 = SudokuBoardCellGeometry.GetRowCellsBitSet(row12);
                    var keyCells12 = cellNoteBitSet & rowHouse12;
                    if (UInt128.PopCount(keyCells12) == 2)
                    {
                        var cellIndex1 = keyCells12.GetOneCellIndex();
                        keyCells12 &= ~cellIndex1.ToCellIndexBitSet();
                        var cellIndex2 = keyCells12.GetOneCellIndex();
#if DEBUG
                        keyCells12 &= ~cellIndex2.ToCellIndexBitSet();
                        System.Diagnostics.Debug.Assert(keyCells12 == 0);
#endif
                        // 行 row12 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は cellIndex1 と cellIndex2 である。

                        for (var row34 = row12 + 1; row34 <= SudokuBoardCellRow.MaxValue; ++row34)
                        {
                            var rowHouse34 = SudokuBoardCellGeometry.GetRowCellsBitSet(row34);
                            var keyCells34 = cellNoteBitSet & rowHouse34;
                            if (UInt128.PopCount(keyCells34) == 2)
                            {
                                var cellIndex3 = keyCells34.GetOneCellIndex();
                                keyCells34 &= ~cellIndex3.ToCellIndexBitSet();
                                var cellIndex4 = keyCells34.GetOneCellIndex();
#if DEBUG
                                keyCells34 &= ~cellIndex4.ToCellIndexBitSet();
                                System.Diagnostics.Debug.Assert(keyCells34 == 0);
#endif

                                // 行 row34 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は cellIndex3 と cellIndex4 である。

                                var (column1, _, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex1);
                                var (column2, _, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex2);
                                var (column3, _, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex3);
                                var (column4, _, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex4);
                                if (column1 == column3 && column2 == column4 && block1 != block4 && block2 != block3)
                                {
                                    // 1) cellIndex1 のセルの列と cellIndex3 のセルの列は等しく、かつ
                                    // 2) cellIndex2 のセルの列と cellIndex4 のセルの列は等しく、かつ
                                    // 3) cellIndex1 のセルのブロックと cellIndex4 のセルのブロックは等しくなく、かつ
                                    // 4) cellIndex2 のセルのブロックと cellIndex3 のセルのブロックは等しくない。
                                    // => X-Wing が成立している。

                                    var operation = MatchPatternByHouse(
                                        ws,
                                        digit,
                                        cellNoteBitSet,
                                        rowHouse12,
                                        rowHouse34,
                                        () => $"{row12.ToFriendlyString()}の行ハウス",
                                        () => $"{column1.ToFriendlyString()}の列ハウス",
                                        () => $"{column2.ToFriendlyString()}の列ハウス",
                                        () => $"{row34.ToFriendlyString()}の行ハウス",
                                        cellIndex1,
                                        cellIndex2,
                                        cellIndex3,
                                        cellIndex4);
                                    if (operation is not null)
                                        yield return operation;
                                }
                            }
                        }
                    }
                }

                // 列ハウスに含まれる数字が 2 個のみの場合
                for (var column12 = SudokuBoardCellColumn.MinValue; column12 <= SudokuBoardCellColumn.MaxValue - 1; ++column12)
                {
                    var columnHouse12 = SudokuBoardCellGeometry.GetColumnCellsBitSet(column12);
                    var keyCells12 = cellNoteBitSet & columnHouse12;
                    if (UInt128.PopCount(keyCells12) == 2)
                    {
                        var cellIndex1 = keyCells12.GetOneCellIndex();
                        keyCells12 &= ~cellIndex1.ToCellIndexBitSet();
                        var cellIndex2 = keyCells12.GetOneCellIndex();
#if DEBUG
                        keyCells12 &= ~cellIndex2.ToCellIndexBitSet();
                        System.Diagnostics.Debug.Assert(keyCells12 == 0);
#endif
                        // 行 column12 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は cellIndex1 と cellIndex2 である。

                        for (var column34 = column12 + 1; column34 <= SudokuBoardCellColumn.MaxValue; ++column34)
                        {
                            var columnHouse34 = SudokuBoardCellGeometry.GetColumnCellsBitSet(column34);
                            var keyCells34 = cellNoteBitSet & columnHouse34;
                            if (UInt128.PopCount(keyCells34) == 2)
                            {
                                var cellIndex3 = keyCells34.GetOneCellIndex();
                                keyCells34 &= ~cellIndex3.ToCellIndexBitSet();
                                var cellIndex4 = keyCells34.GetOneCellIndex();
#if DEBUG
                                keyCells34 &= ~cellIndex4.ToCellIndexBitSet();
                                System.Diagnostics.Debug.Assert(keyCells34 == 0);
#endif

                                // 列 column34 に 数字の候補 D を持つセルは 2 個だけで、それらの位置は cellIndex3 と cellIndex4 である。

                                var (_, row1, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex1);
                                var (_, row2, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex2);
                                var (_, row3, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex3);
                                var (_, row4, block4) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex4);
                                if (row1 == row3 && row2 == row4 && block1 != block4 && block2 != block3)
                                {
                                    // 1) cellIndex1 のセルの行と cellIndex3 のセルの行は等しく、かつ
                                    // 2) cellIndex2 のセルの行と cellIndex4 のセルの行は等しく、かつ
                                    // 3) cellIndex1 のセルのブロックと cellIndex4 のセルのブロックは等しくなく、かつ
                                    // 4) cellIndex2 のセルのブロックと cellIndex3 のセルのブロックは等しくない。
                                    // => X-Wing が成立している。

                                    var operation = MatchPatternByHouse(
                                        ws,
                                        digit,
                                        cellNoteBitSet,
                                        columnHouse12,
                                        columnHouse34,
                                        () => $"{column12.ToFriendlyString()}の列ハウス",
                                        () => $"{row1.ToFriendlyString()}の行ハウス",
                                        () => $"{row2.ToFriendlyString()}の行ハウス",
                                        () => $"{column34.ToFriendlyString()}の列ハウス",
                                        cellIndex1,
                                        cellIndex2,
                                        cellIndex3,
                                        cellIndex4);
                                    if (operation is not null)
                                        yield return operation;
                                }
                            }
                        }
                    }
                }
            }

            static XWingOperation? MatchPatternByHouse(
                SudokuBoardWorkspace ws,
                SudokuBoardCellDigit digit,
                UInt128 cellNoteBitSet,
                UInt128 houseMask12,
                UInt128 houseMask34,
                Func<string> houseName12Getter,
                Func<string> houseName13Getter,
                Func<string> houseName24Getter,
                Func<string> houseName34Getter,
                SudokuBoardCellIndex cellIndex1,
                SudokuBoardCellIndex cellIndex2,
                SudokuBoardCellIndex cellIndex3,
                SudokuBoardCellIndex cellIndex4)
            {
                var allKeyCells = cellIndex1.ToCellIndexBitSet() | cellIndex2.ToCellIndexBitSet() | cellIndex3.ToCellIndexBitSet() | cellIndex4.ToCellIndexBitSet();
                var visibleCellsFromCellIndex1 = SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex1);
                var visibleCellsFromCellIndex2 = SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex2);
                var visibleCellsFromCellIndex3 = SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex3);
                var visibleCellsFromCellIndex4 = SudokuBoardCellGeometry.GetVisibleCellsBitSet(cellIndex4);

                // cellIndex1, cellIndex2. cellIndex3. cellIndex4 を除き、cellIndex1, cellIndex2 とハウスを共有するセルの候補からは digit を排除する
                var notKeyCells = cellNoteBitSet & ~allKeyCells;
                var excludedCellsBitSetA = notKeyCells & visibleCellsFromCellIndex1 & visibleCellsFromCellIndex2;
                var excludedCellsBitSetB = notKeyCells & visibleCellsFromCellIndex1 & visibleCellsFromCellIndex3;
                var excludedCellsBitSetC = notKeyCells & visibleCellsFromCellIndex2 & visibleCellsFromCellIndex4;
                var excludedCellsBitSetD = notKeyCells & visibleCellsFromCellIndex3 & visibleCellsFromCellIndex4;
                var excludedCellsBitSet = excludedCellsBitSetA | excludedCellsBitSetB | excludedCellsBitSetC | excludedCellsBitSetD;
                if (excludedCellsBitSet == 0)
                    return null;

                // X-Wing により排除されるセルの数字の候補が存在する。

                var houseName12 = houseName12Getter();
                var houseName13 = houseName13Getter();
                var houseName24 = houseName24Getter();
                var houseName34 = houseName34Getter();
                var digitText = digit.ToCellDigitText();
                var cell1PositionText = cellIndex1.ToFriendlyString();
                var cell2PositionText = cellIndex2.ToFriendlyString();
                var cell3PositionText = cellIndex3.ToFriendlyString();
                var cell4PositionText = cellIndex4.ToFriendlyString();
                var description =
                    $"{houseName12}では数字 {digitText} の候補は {cell1PositionText} と {cell2PositionText} のセルにしか存在しないので、どちらかのセルだけが必ず数字 {digitText} になります。\n" +
                    $"同様に、{houseName34}では数字 {digitText} の候補は {cell3PositionText} と {cell4PositionText} のセルにしか存在しないので、どちらかのセルだけが必ず数字 {digitText} になります。\n" +
                    $"一方、{cell1PositionText} と {cell3PositionText} のセルは同じ{houseName13}に存在し、両方が数字 {digit} になることはあり得ません。\n" +
                    $"これらのことから、{cell2PositionText} と {cell4PositionText} のセルは必ずどちらかだけが数字 {digitText} になります。\n" +
                    $"更に、{cell1PositionText} と {cell3PositionText} のセルも必ずどちらかだけが数字 {digitText} になります。\n" +
                    $"したがって、{cell1PositionText}, {cell2PositionText}, {cell3PositionText}, {cell4PositionText} を除く以下のセルの数字の候補からは数字 {digit} を排除できます。\n" +
                    $"・{cell1PositionText} と {cell2PositionText} とハウスを共有するセル\n" +
                    $"・{cell1PositionText} と {cell3PositionText} とハウスを共有するセル\n" +
                    $"・{cell2PositionText} と {cell4PositionText} とハウスを共有するセル\n" +
                    $"・{cell3PositionText} と {cell4PositionText} とハウスを共有するセル";
                return
                    new XWingOperation(
                        excludedCellsBitSet.EnumerateAsCellNotes(digit),
                        (houseMask12 | houseMask34).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                            .Concat((((visibleCellsFromCellIndex1 & visibleCellsFromCellIndex2) | (visibleCellsFromCellIndex1 & visibleCellsFromCellIndex3) | (visibleCellsFromCellIndex2 & visibleCellsFromCellIndex4) | (visibleCellsFromCellIndex3 & visibleCellsFromCellIndex4)) & ~houseMask12 & ~houseMask34).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                        allKeyCells.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                        description);
            }
        }
    }
}
