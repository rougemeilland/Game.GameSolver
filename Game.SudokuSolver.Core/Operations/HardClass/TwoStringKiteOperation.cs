using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// 2ストリング・カイトのオペレーションのクラスです。
    /// </summary>
    internal sealed class TwoStringKiteOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private TwoStringKiteOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            Func<string> descriptionBuilder)
            : base(
                  "2ストリング・カイト (2 String Kite)",
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
            // ある数字 D とある行 Row とある列 Col とあるブロック Blk について
            // 1) Row にある数字 D を候補に持つセルは Cra と Crb のみであり、かつ
            // 2) Col にある数字 D を候補に持つセルは Cca と Ccb のみであり、かつ
            // 3) Crb と Ccb は Blk にある場合。
            // => 仮に、「Cra と Cca のどちらにも数字 D は入り得ない」と仮定する。
            //    Row において Cra の他に D が入り得るセルは Crb のみなので、Crb に D が入る。
            //    同様に、Col において Cca の他に D が入り得るセルは Ccb のみなので、Ccb に D が入る。
            //    その結果、Blk の二つのセル Crb, Ccb の両方に D が入ることになってしまうので、
            //    「Cra と Cca のどちらにも数字 D は入り得ない」という仮定が誤っている、つまり Cra と Cca の少なくともどちらかには 数字 D が入る。
            //    したがって、Cra と Cca の両方とハウスを共有するセルの数字は D ではあり得ないので、そのセルの数字の候補からは D を排除できる。

            for (var digit = SudokuBoardCellDigit.MinValue; digit <= SudokuBoardCellDigit.MaxValue; ++digit)
            {
                var allCellIndexBitSet = ws.GetCellIndexBitSet(digit);
                for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
                {
                    var rowHouse = SudokuBoardCellGeometry.GetRowCellIndexBitSet(row);
                    var keyRowCellIndexBitSet = allCellIndexBitSet & rowHouse;
                    if (keyRowCellIndexBitSet.GetPopCount() == 2u)
                    {
                        for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
                        {
                            var columnHouse = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column);
                            var keyColumnCellIndexBitSet = allCellIndexBitSet & columnHouse;
                            if (keyColumnCellIndexBitSet.GetPopCount() == 2u)
                            {
                                // row と column との交差点にあるブロックとその領域を求める。
                                var block = SudokuBoardCellGeometry.GetBlock(SudokuBoardCellGeometry.GetCellIndex(column, row));
                                var blockHouse = SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block);

                                if ((keyRowCellIndexBitSet & keyColumnCellIndexBitSet) == 0 && (keyRowCellIndexBitSet & blockHouse) != 0 && (keyColumnCellIndexBitSet & blockHouse) != 0)
                                {
                                    // keyRowCellIndexBitSet にはブロック blockHouse に含まれているセルが存在し、かつ keyColumnCellIndexBitSet にも同一ブロック blockHouse に含まれているセルが存在する。
                                    // => 2ストリング・カイトが成立している。
                                    // ※ keyRowCellIndexBitSet または keyColumnCellIndexBitSet の全ての要素がブロック内に含まれているケースではより発見が容易なボックスライン・リダクションの手法が適用可能であるが、
                                    //    チェックの簡略化のためそのようなケースを排除していない。

                                    // 2ストリング・カイトを構成するセルの位置を列挙する。(列挙しているのは複数のパターンが存在するかもしれないから)
                                    // ※ 複数のパターンが存在するケースは主 keyRowCellIndexBitSet あるいは keyColumnCellIndexBitSet が同一ブロック内で完結してしまっている場合が想定できる。
                                    //    そのようなケースではより発見が容易なボックスライン・リダクションの手法が適用可能であるため、事実上2ストリング・カイトの手法が適用されることはない。
                                    //    しかし、他に複数のパターンが存在するケースを否定できていないので、列挙を行っている。
                                    foreach (var (endOfRowStringInsideBlock, endOfRowStringAwayFromBlock, endOfColumnStringInsideBlock, endOfColumnStringAwayFromBlock) in EnumerateTwoStringKiteForm(block, keyRowCellIndexBitSet, keyColumnCellIndexBitSet))
                                    {
                                        var allKeyCellIndexBitSet = keyRowCellIndexBitSet | keyColumnCellIndexBitSet;
                                        var excludableCellsRegionBitMask = SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(endOfRowStringAwayFromBlock) & SudokuBoardCellGeometry.GetVisibleCellIndexBitSet(endOfColumnStringAwayFromBlock) & ~blockHouse;
#if DEBUG
                                        System.Diagnostics.Debug.Assert((excludableCellsRegionBitMask & ~allKeyCellIndexBitSet) == excludableCellsRegionBitMask, $"{nameof(excludableCellsRegionBitMask)} から {nameof(allKeyCellIndexBitSet)} を除去する必要はない。");
#endif
                                        var excludableCellIndexBitSet = allCellIndexBitSet & excludableCellsRegionBitMask;
                                        if (excludableCellIndexBitSet != 0)
                                        {
                                            // 排除対象の数字の候補が存在する

                                            var stringRegion = (rowHouse | columnHouse) & ~allKeyCellIndexBitSet;
                                            var effectedRegion =
                                                SudokuBoardCellGeometry.GetColumnCellIndexBitSet(SudokuBoardCellGeometry.GetColumn(endOfRowStringAwayFromBlock))
                                                | SudokuBoardCellGeometry.GetRowCellIndexBitSet(SudokuBoardCellGeometry.GetRow(endOfColumnStringAwayFromBlock));
                                            yield return
                                                new TwoStringKiteOperation(
                                                    excludableCellIndexBitSet.EnumerateAsCellNotes(digit),
                                                    allKeyCellIndexBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType1)
                                                        .Concat(stringRegion.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                        .Concat(blockHouse.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                        .Concat(effectedRegion.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4))
                                                        .Concat(excludableCellsRegionBitMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                    allKeyCellIndexBitSet.EnumerateAsCellNotes(digit).ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                                    GetDescriptionBuilder(
                                                        digit,
                                                        endOfRowStringInsideBlock,
                                                        endOfColumnStringInsideBlock,
                                                        endOfRowStringAwayFromBlock,
                                                        endOfColumnStringAwayFromBlock,
                                                        row.ToHouseName(),
                                                        column.ToHouseName(),
                                                        block.ToHouseName()));
                                        }

                                        static Func<string> GetDescriptionBuilder(
                                            SudokuBoardCellDigit digit,
                                            SudokuBoardCellIndex rowHouseInsideBlockCellIndex,
                                            SudokuBoardCellIndex columnHouseInsideBlockCellIndex,
                                            SudokuBoardCellIndex rowHouseAwayFromBlockCellIndex,
                                            SudokuBoardCellIndex columnHouseAwayFromBlockCellIndex,
                                            string rowHouseName,
                                            string columnHouseName,
                                            string blockHouseName)
                                        {
                                            return
                                                () =>
                                                {
                                                    var digitText = digit.ToCellDigitText();
                                                    var rowHouseInsideBlockCellPositionText = rowHouseInsideBlockCellIndex.ToFriendlyString();
                                                    var columnHouseInsideBlockCellPositionText = columnHouseInsideBlockCellIndex.ToFriendlyString();
                                                    var rowHouseAwayFromBlockCellPositionText = rowHouseAwayFromBlockCellIndex.ToFriendlyString();
                                                    var columnHouseAwayFromBlockCellPositionText = columnHouseAwayFromBlockCellIndex.ToFriendlyString();
                                                    var rowHouseCellPositionsText = JoinToCellPositionsText(rowHouseInsideBlockCellIndex, rowHouseAwayFromBlockCellIndex);
                                                    var columnHouseCellPositionsText = JoinToCellPositionsText(columnHouseInsideBlockCellIndex, columnHouseAwayFromBlockCellIndex);
                                                    var insideBlockCellPositionsText = JoinToCellPositionsText(rowHouseInsideBlockCellIndex, columnHouseInsideBlockCellIndex);
                                                    var awayFromBlockCellPositionsText = JoinToCellPositionsText(rowHouseAwayFromBlockCellIndex, columnHouseAwayFromBlockCellIndex);
                                                    return
                                                        $"数字 {digitText} に着目します。\n" +
                                                        $"{rowHouseName}で数字の候補 {digitText} があるマスは {rowHouseCellPositionsText} の二つだけで、" +
                                                        $"{columnHouseName}で数字の候補 {digitText} があるマスは {columnHouseCellPositionsText} の二つだけです。\n" +
                                                        $"そして、" +
                                                        $"{insideBlockCellPositionsText} のマスは共に{blockHouseName}にあるので、" +
                                                        $"2ストリング・カイトが成立しています。\n" +
                                                        $"\n" +
                                                        $"仮に、「{awayFromBlockCellPositionsText} のどちらのマスにも {digitText} が入らない」と仮定します。\n" +
                                                        $"{rowHouseName}で{rowHouseAwayFromBlockCellPositionText}の他に {digitText} が入り得るマスは{rowHouseInsideBlockCellPositionText}だけなので、{rowHouseInsideBlockCellPositionText}のマスに {digitText} が入ります。\n" +
                                                        $"同様に、" +
                                                        $"{columnHouseName}で{columnHouseAwayFromBlockCellPositionText}の他に {digitText} が入り得るマスは{columnHouseInsideBlockCellPositionText}だけなので、{columnHouseInsideBlockCellPositionText}のマスに {digitText} が入ります。\n" +
                                                        $"その結果、{blockHouseName}の二つのマス {insideBlockCellPositionsText} の数字が共に {digitText} ということになってしまうので、" +
                                                        $"「{awayFromBlockCellPositionsText} のどちらのマスにも {digitText} が入らない」という仮定が誤っていることになります。\n" +
                                                        $"つまり {awayFromBlockCellPositionsText} の少なくともどちらかのマスには必ず {digitText} が入ります。\n" +
                                                        $"\n" +
                                                        $"したがって、二つのマス {awayFromBlockCellPositionsText} とハウスを共有する他のマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。";

                                                    static string JoinToCellPositionsText(SudokuBoardCellIndex rowHouseInsideBlockCellIndex, SudokuBoardCellIndex rowHouseAwayFromBlockCellIndex)
                                                    {
                                                        return
                                                            string.Join(
                                                                ", ",
                                                                Enumerable.Empty<SudokuBoardCellIndex>()
                                                                .Append(rowHouseInsideBlockCellIndex)
                                                                .Append(rowHouseAwayFromBlockCellIndex)
                                                                .OrderBy(cellIndex => cellIndex)
                                                                .Select(cellIndex => cellIndex.ToFriendlyString()));
                                                    }
                                                };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            static IEnumerable<(SudokuBoardCellIndex endOfRowStringInsideBlock, SudokuBoardCellIndex endOfRowStringAwayFromBlock, SudokuBoardCellIndex endOfColumnStringInsideBlock, SudokuBoardCellIndex endOfColumnStringAwayFromBlock)> EnumerateTwoStringKiteForm(SudokuBoardCellBlock block, UInt128 keyRowCellIndexBitSet, UInt128 keyColumnCellIndexBitSet)
            {
                var (rowStringCellIndex1, rowStringCellIndex2) = keyRowCellIndexBitSet.SplitAsPairCellIndexes();
                var (columnStringCellIndex1, columnStringCellIndex2) = keyColumnCellIndexBitSet.SplitAsPairCellIndexes();
                var rowStringCellBlock1 = SudokuBoardCellGeometry.GetBlock(rowStringCellIndex1);
                var rowStringCellBlock2 = SudokuBoardCellGeometry.GetBlock(rowStringCellIndex2);
                var columnStringCellBlock1 = SudokuBoardCellGeometry.GetBlock(columnStringCellIndex1);
                var columnStringCellBlock2 = SudokuBoardCellGeometry.GetBlock(columnStringCellIndex2);

                // rowStringCellBlock1 と rowStringCellBlock2 が共に block と等しいケースは、より発見が容易なボックスライン・リダクションの手法が利用可能であるため事実上はあり得ない。
                // columnStringCellBlock1 と columnStringCellBlock2 が共に block と等しいケースについても同様である。

                if (rowStringCellBlock1 == block)
                {
                    if (columnStringCellBlock1 == block)
                        yield return (rowStringCellIndex1, rowStringCellIndex2, columnStringCellIndex1, columnStringCellIndex2);
                    if (columnStringCellBlock2 == block)
                        yield return (rowStringCellIndex1, rowStringCellIndex2, columnStringCellIndex2, columnStringCellIndex1);
                }

                if (rowStringCellBlock2 == block)
                {
                    if (columnStringCellBlock1 == block)
                        yield return (rowStringCellIndex2, rowStringCellIndex1, columnStringCellIndex1, columnStringCellIndex2);
                    if (columnStringCellBlock2 == block)
                        yield return (rowStringCellIndex2, rowStringCellIndex1, columnStringCellIndex2, columnStringCellIndex1);
                }
            }
        }
    }
}
