using System.Drawing;
using System;
using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal static class SudokuExtensions
    {
        static SudokuExtensions()
        {
#if DEBUG
            // self test

            for (var column1 = SudokuBoardCellColumn.MinValue; column1 <= SudokuBoardCellColumn.MaxValue; ++column1)
            {
                for (var column2 = column1; column2 <= SudokuBoardCellColumn.MaxValue; ++column2)
                {
                    for (var column3 = column1; column3 <= SudokuBoardCellColumn.MaxValue; ++column3)
                    {
                        var columnBitSet =
                            column1.ToCellColumnBitSet()
                            | column2.ToCellColumnBitSet()
                            | column3.ToCellColumnBitSet();
                        var actual = columnBitSet.ToCellIndexBitSet();
                        var desired =
                            SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column1)
                            | SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column2)
                            | SudokuBoardCellGeometry.GetColumnCellIndexBitSet(column3);
                        if (actual != desired)
                            throw new ApplicationException();
                    }
                }
            }

            for (var row1 = SudokuBoardCellRow.MinValue; row1 <= SudokuBoardCellRow.MaxValue; ++row1)
            {
                for (var row2 = row1; row2 <= SudokuBoardCellRow.MaxValue; ++row2)
                {
                    for (var row3 = row1; row3 <= SudokuBoardCellRow.MaxValue; ++row3)
                    {
                        var columnBitSet =
                            row1.ToCellRowBitSet()
                            | row2.ToCellRowBitSet()
                            | row3.ToCellRowBitSet();
                        var actual = columnBitSet.ToCellIndexBitSet();
                        var desired =
                            SudokuBoardCellGeometry.GetRowCellIndexBitSet(row1)
                            | SudokuBoardCellGeometry.GetRowCellIndexBitSet(row2)
                            | SudokuBoardCellGeometry.GetRowCellIndexBitSet(row3);
                        if (actual != desired)
                            throw new ApplicationException();
                    }
                }
            }

            for (var block1 = SudokuBoardCellBlock.MinValue; block1 <= SudokuBoardCellBlock.MaxValue; ++block1)
            {
                for (var block2 = block1; block2 <= SudokuBoardCellBlock.MaxValue; ++block2)
                {
                    for (var block3 = block1; block3 <= SudokuBoardCellBlock.MaxValue; ++block3)
                    {
                        var blockBitSet =
                            block1.ToCellBlockBitSet()
                            | block2.ToCellBlockBitSet()
                            | block3.ToCellBlockBitSet();
                        var actual = blockBitSet.ToCellIndexBitSet();
                        var desired =
                            SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block1)
                            | SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block2)
                            | SudokuBoardCellGeometry.GetBlockCellIndexBitSet(block3);
                        if (actual != desired)
                            throw new ApplicationException();
                    }
                }
            }

            for (var cellIndex1 = SudokuBoardCellIndex.MinValue; cellIndex1 <= SudokuBoardCellIndex.MaxValue; ++cellIndex1)
            {
                for (var cellIndex2 = cellIndex1; cellIndex2 <= SudokuBoardCellIndex.MaxValue; ++cellIndex2)
                {
                    for (var cellIndex3 = cellIndex2; cellIndex3 <= SudokuBoardCellIndex.MaxValue; ++cellIndex3)
                    {
                        var cellIndexBitSet =
                            cellIndex1.ToCellIndexBitSet()
                            | cellIndex2.ToCellIndexBitSet()
                            | cellIndex3.ToCellIndexBitSet();

                        var (column1, row1, block1) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex1);
                        var (column2, row2, block2) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex2);
                        var (column3, row3, block3) = SudokuBoardCellGeometry.GetColumnRowBlock(cellIndex3);

                        {
                            var actual = cellIndexBitSet.GetUnionByColumn();
                            var desired =
                                SudokuBoardCellGeometry.GetColumn(cellIndex1).ToCellColumnBitSet()
                                | SudokuBoardCellGeometry.GetColumn(cellIndex2).ToCellColumnBitSet()
                                | SudokuBoardCellGeometry.GetColumn(cellIndex3).ToCellColumnBitSet();
                            if (actual != desired)
                                throw new ApplicationException();
                        }

                        {
                            var actual = cellIndexBitSet.GetUnionByRow();
                            var desired =
                                SudokuBoardCellGeometry.GetRow(cellIndex1).ToCellRowBitSet()
                                | SudokuBoardCellGeometry.GetRow(cellIndex2).ToCellRowBitSet()
                                | SudokuBoardCellGeometry.GetRow(cellIndex3).ToCellRowBitSet();
                            if (actual != desired)
                                throw new ApplicationException();
                        }

                        {
                            var actual = cellIndexBitSet.GetUnionByBlock();
                            var desired =
                                SudokuBoardCellGeometry.GetBlock(cellIndex1).ToCellBlockBitSet()
                                | SudokuBoardCellGeometry.GetBlock(cellIndex2).ToCellBlockBitSet()
                                | SudokuBoardCellGeometry.GetBlock(cellIndex3).ToCellBlockBitSet();
                            if (actual != desired)
                                throw new ApplicationException();
                        }
                    }
                }
            }
#endif

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToFriendlyString(this (SudokuBoardCellColumn column, SudokuBoardCellRow row) position)
            => $"{position.row - SudokuBoardCellRow.One + 1}行{position.column - SudokuBoardCellColumn.One + 1}列";
    }
}
