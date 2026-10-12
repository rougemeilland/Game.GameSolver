using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.EasyClass
{
    /// <summary>
    /// 隠れシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal class HiddenSingleOperation
        : SudokuBoardOperation
    {
        private HiddenSingleOperation(BoardCell? determinedCell, ReadOnlyMemory<BoardCell> removedCellNotes, ReadOnlyMemory<BoardCellPosition> relatedCellPositions, string description)
            : base("隠れシングル", DifficultyLevel.Easy, determinedCell, ReadOnlyMemory<BoardCell>.Empty, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static HiddenSingleOperation? MatchPattern(BoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と H 内のセル C とある数字 D に対して、
            // H 内の C の数字の候補に D が含まれており、かつ C 以外のすべてのセルの数字の候補に D が含まれていない場合
            // => C の数字は D で確定する。

            for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; ++cellDigit)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);

                for (var block = BoardCellBlock.MinValue; block <= BoardCellBlock.MaxValue; ++block)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Block,
                            BoardCellGeometry.GetBlockCellsBitSet(block),
                            cellDigit,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }

                for (var column = BoardCellColumn.MinValue; column <= BoardCellColumn.MaxValue; ++column)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Column,
                            BoardCellGeometry.GetColumnCellsBitSet(column),
                            cellDigit + 1,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }

                for (var row = BoardCellRow.MinValue; row <= BoardCellRow.MaxValue; ++row)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            HouseType.Row,
                            BoardCellGeometry.GetRowCellsBitSet(row),
                            cellDigit + 1,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }
            }

            return null;

            static HiddenSingleOperation? MatchPatternByHouse(BoardWorkspace ws, HouseType house, UInt128 houseBitMask, BoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var foundBitSet = cellNoteBitSet & houseBitMask;
                if (UInt128.PopCount(foundBitSet) != 1)
                    return null;
                var determinedCellIndex = foundBitSet.GetOneCellIndex();

                // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellsNoteBitSet =
                    ~ws.DeterminedCellBitSet
                    & cellNoteBitSet
                    & BoardCellGeometry.GetVisibleCellsBitSetByCellIndex(determinedCellIndex)
                    & ~foundBitSet;

                var removedCellNotes = removedCellsNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                var relatedCellPositions = houseBitMask.EnumerateAsCellIndexes().ToCellPositions().ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = BoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                var determinedCellPosiionText = determinedCellIndex.ToFriendlyString();
                var determinedCellDigitText = cellDigit.ToCellDigitChar();

                var description =
                    house switch
                    {
                        HouseType.Row =>
                            $"{determinedCellRow.ToFriendlyString()}のセルのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        HouseType.Column =>
                            $"{determinedCellColumn.ToFriendlyString()}のセルのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        HouseType.Block =>
                            $"{determinedCellBlock.ToFriendlyString()}のセルが属しているブロックのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        _ => throw new ArgumentOutOfRangeException(nameof(house)),
                    };

                return
                    new HiddenSingleOperation(
                        new BoardCell(determinedCellIndex, cellDigit),
                        removedCellNotes,
                        relatedCellPositions,
                        description);
            }
        }

        public override BoardCellsSet Execute(BoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.Column, DeterminedCell.Value.Row, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
