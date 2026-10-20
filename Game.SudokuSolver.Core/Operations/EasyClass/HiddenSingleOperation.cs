using System;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.EasyClass
{
    /// <summary>
    /// 隠れシングルパターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenSingleOperation
        : SudokuBoardOperation
    {
        private HiddenSingleOperation(SudokuBoardCellPositionDigit? determinedCell, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base(GameBoardStatus.Solving, "隠れシングル", SudokuBoardDifficultyLevel.Easy, determinedCell, ReadOnlyMemory<SudokuBoardCellPosition>.Empty, ReadOnlyMemory<SudokuBoardCellPositionDigit>.Empty, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static HiddenSingleOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と H 内のセル C とある数字 D に対して、
            // H 内の C の数字の候補に D が含まれており、かつ C 以外のすべてのセルの数字の候補に D が含まれていない場合
            // => C の数字は D で確定する。

            for (var cellDigit = SudokuBoardCellDigit.MinValue; cellDigit <= SudokuBoardCellDigit.MaxValue; ++cellDigit)
            {
                var cellNoteBitSet = ws.GetNoteBitSet(cellDigit);

                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Block,
                            SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                            cellDigit,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }

                for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Column,
                            SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                            cellDigit + 1,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }

                for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
                {
                    var operation =
                        MatchPatternByHouse(
                            ws,
                            SudokuBoardHouseType.Row,
                            SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                            cellDigit + 1,
                            cellNoteBitSet);
                    if (operation is not null)
                        return operation;
                }
            }

            return null;

            static HiddenSingleOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, SudokuBoardHouseType house, UInt128 houseBitMask, SudokuBoardCellDigit cellDigit, UInt128 cellNoteBitSet)
            {
                var foundBitSet = cellNoteBitSet & houseBitMask;
                if (UInt128.PopCount(foundBitSet) != 1)
                    return null;
                var determinedCellIndex = foundBitSet.GetOneCellIndex();

                // 未確定であり、指定された数字を候補に含み、かつ確定されたセルから見え、かつ確定されたセル自身を除く、セルの集合
                var removedCellsNoteBitSet =
                    ~ws.DeterminedCellBitSet
                    & cellNoteBitSet
                    & SudokuBoardCellGeometry.GetVisibleCellsBitSetByCellIndex(determinedCellIndex)
                    & ~foundBitSet;

                var removedCellNotes = removedCellsNoteBitSet.EnumerateAsCellIndexes().ToCells(cellDigit).ToArray();
                var relatedCellPositions = houseBitMask.EnumerateAsCellIndexes().ToCellPositions().ToArray();
                var (determinedCellColumn, determinedCellRow, determinedCellBlock) = SudokuBoardCellGeometry.GetColumnRowBlock(determinedCellIndex);
                var determinedCellPosiionText = determinedCellIndex.ToFriendlyString();
                var determinedCellDigitText = cellDigit.ToCellDigitChar();

                var description =
                    house switch
                    {
                        SudokuBoardHouseType.Row =>
                            $"{determinedCellRow.ToFriendlyString()}のセルのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        SudokuBoardHouseType.Column =>
                            $"{determinedCellColumn.ToFriendlyString()}のセルのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        SudokuBoardHouseType.Block =>
                            $"{determinedCellBlock.ToFriendlyString()}のセルが属しているブロックのうち '{determinedCellDigitText}' であり得るセルが{determinedCellPosiionText}のみであるため、" +
                            $"{determinedCellPosiionText}のセルの数字は '{determinedCellDigitText}' に確定します。" +
                            $"{(removedCellNotes.Length > 0 ? $"それとともに、確定したセルと同じハウスに属する別のセルの数字の候補から '{determinedCellDigitText}' を削除します。" : "")}",
                        _ => throw new ArgumentOutOfRangeException(nameof(house)),
                    };

                return
                    new HiddenSingleOperation(
                        new SudokuBoardCellPositionDigit(determinedCellIndex, cellDigit),
                        removedCellNotes,
                        relatedCellPositions,
                        description);
            }
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
        {
            if (DeterminedCell is null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            if (!newCellSet.DetermineCell(DeterminedCell.Value.CellIndex, DeterminedCell.Value.Digit))
                throw new ApplicationException();

            return newCellSet;
        }
    }
}
