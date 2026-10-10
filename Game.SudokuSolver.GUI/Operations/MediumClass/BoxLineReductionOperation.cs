using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Game.SudokuSolver.GUI.Operations.MediumClass
{
    /// <summary>
    /// ボックスラインリダクションのオペレーションのクラスです。
    /// </summary>
    internal class BoxLineReductionOperation
        : SudokuBoardOperation
    {
        private BoxLineReductionOperation(ReadOnlyMemory<BoardCell> hilightedCellNotes, ReadOnlyMemory<BoardCell> removedCellNotes, ReadOnlyMemory<BoardCellPosition> relatedCellPositions, string description)
            : base("ボックスラインリダクション", DifficultyLevel.Easy, null, hilightedCellNotes, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static PointingOperation? MatchPattern(BoardWorkspace ws)
        {
            // TODO: ボックスラインリダクションの実装
#error

            for (var cellDigit = BoardCellDigit.MinValue; cellDigit <= BoardCellDigit.MaxValue; cellDigit++)
            {
                var cellNoteBits = ws.GetNoteBits(cellDigit);
                for (var block = BoardCellBlock.MinValue; block <= BoardCellBlock.MaxValue; ++block)
                {
                    var blockHouseBitMask = BoardCellGeometry.GetBlockCellsBitMask(block);

                    // 行ハウスの検索
                    for (var blockRowOffset = BoardCellBlockRow.MinValue; blockRowOffset <= BoardCellBlockRow.MaxValue; ++blockRowOffset)
                    {
                        var row = BoardCellGeometry.GetRow(block, blockRowOffset);
                        var operation =
                            MatchPatternByHouse(
                                block,
                                blockHouseBitMask,
                                BoardCellGeometry.GetRowCellsBitMask(row),
                                row.ToFriendlyString(),
                                cellDigit,
                                cellNoteBits);
                        if (operation is not null)
                            return operation;
                    }

                    // 列ハウスの検索
                    for (var blockColumnOffset = BoardCellBlockColumn.MinValue; blockColumnOffset <= BoardCellBlockColumn.MaxValue; ++blockColumnOffset)
                    {
                        var column = BoardCellGeometry.GetColumn(block, blockColumnOffset);
                        var operation =
                            MatchPatternByHouse(
                                block,
                                blockHouseBitMask,
                                BoardCellGeometry.GetColumnCellsBitMask(column),
                                column.ToFriendlyString(),
                                cellDigit,
                                cellNoteBits);
                        if (operation is not null)
                            return operation;
                    }
                }
            }

            return null;

            static PointingOperation? MatchPatternByHouse(BoardCellBlock block, UInt128 blockHouseBitMask, UInt128 rowBitMask, string columnOrRowHouseText, BoardCellDigit cellDigit, UInt128 cellNoteBits)
            {
                var hilighedCellNoteBits = cellNoteBits & blockHouseBitMask & rowBitMask;
                var removedCellNoteBits = cellNoteBits & ~blockHouseBitMask & rowBitMask;
                if (hilighedCellNoteBits != 0 && (cellNoteBits & blockHouseBitMask & ~rowBitMask) == 0 && removedCellNoteBits != 0)
                {
                    // あるブロック B とある行または列 CR とある数字 D に対して、
                    // B 内の CR の何れかのセルの候補に D が存在し、かつ B 内の CR 以外のセルの候補に D が存在せず、かつ B 以外の CR のセルの候補に D の候補が存在する場合
                    // => B 以外の CR セルは D にはなり得ないので、それらのセルの候補から D を削除する。
                    var cellDigitChar = cellDigit.ToCellDigitChar();
                    var removedCellNotePositionTexts = $"({string.Join(", ", removedCellNoteBits.EnumerateForCellPositions().Select(position => position.ToFriendlyString()))})";
                    return
                        new PointingOperation(
                            hilighedCellNoteBits.EnumerateForCells(cellDigit).ToArray(),
                            removedCellNoteBits.EnumerateForCells(cellDigit).ToArray(),
                            (blockHouseBitMask | rowBitMask).EnumerateForCellPositions().ToArray(),
                            $"{block.ToFriendlyString()}のブロックには{columnOrRowHouseText}のセルのみに数字の候補 '{cellDigitChar}' が含まれており、これらのセルの何れかが '{cellDigitChar}' のはずです。そのため、{columnOrRowHouseText}にある他のセル {removedCellNotePositionTexts} は '{cellDigitChar}' になり得ないので候補から削除します。");
                }

                return null;
            }
        }

        public override BoardCellsSet Execute(BoardCellsSet cells)
        {
            if (DeterminedCell is not null)
                throw new InvalidOperationException();

            var newCellSet = cells.Clone();

            var removedCellNotes = RemovedCellNotes.Span;
            for (var index = 0; index < RemovedCellNotes.Length; ++index)
            {
                var removedCellNote = removedCellNotes[index];
                if (!newCellSet.RemoveNote(removedCellNote.Column, removedCellNote.Row, removedCellNote.Digit))
                    throw new ApplicationException();
            }

            return newCellSet;
        }
    }
}
