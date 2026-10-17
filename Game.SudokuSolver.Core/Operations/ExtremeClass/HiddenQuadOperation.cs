using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.ExtremeClass
{
    /// <summary>
    /// 隠れ四国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenQuadOperation
        : SudokuBoardOperation
    {
        private HiddenQuadOperation(ReadOnlyMemory<SudokuBoardCellPositionDigit> highlightedCellNotes, ReadOnlyMemory<SudokuBoardCellPositionDigit> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base("隠れ四国同盟", SudokuBoardDifficultyLevel.Extreme, null, highlightedCellNotes, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static HiddenQuadOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある四つの数字 D1, D2, D3, D4 とある四つのセル C1, C2, C3, C4 に対して、
            // H 内に数字の候補として D1, D2, D3, D4 の何れかを持つセルが C1, C2, C3, C4 のみの場合
            // => C1, C2, C3, C4 の数字はそれぞれ D1, D2, D3, D4 の何れかであるので、C1, C2, C3, C4 の D1, S2, D3 以外の数字の候補は否定される。

            // ブロックハウスの検索
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                        () => $"{block.ToFriendlyString()}のブロック",
                        SudokuBoardCellGeometry.GetCellIndexes(block).Span);
                if (operation is not null)
                    return null;
            }

            // 行ハウスの検索
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                        () => $"{row.ToFriendlyString()}の行",
                        SudokuBoardCellGeometry.GetCellIndexes(row).Span);
                if (operation is not null)
                    return null;
            }

            // 列ハウスの検索
            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operation =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                        () => $"{column.ToFriendlyString()}の列",
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                if (operation is not null)
                    return null;
            }

            return null;

            static HiddenQuadOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var cellDigitA = SudokuBoardCellDigit.MinValue; cellDigitA < SudokuBoardCellDigit.MaxValue - 3; ++cellDigitA)
                {
                    var cellNoteBitSetA = ws.GetNoteBitSet(cellDigitA);
                    for (var cellDigitB = cellDigitA + 1; cellDigitB < SudokuBoardCellDigit.MaxValue - 2; ++cellDigitB)
                    {
                        var cellNoteBitSetB = ws.GetNoteBitSet(cellDigitB);
                        for (var cellDigitC = cellDigitB + 1; cellDigitC < SudokuBoardCellDigit.MaxValue - 1; ++cellDigitC)
                        {
                            var cellNoteBitSetC = ws.GetNoteBitSet(cellDigitC);
                            for (var cellDigitD = cellDigitC + 1; cellDigitD < SudokuBoardCellDigit.MaxValue; ++cellDigitD)
                            {
                                var cellNoteBitSetD = ws.GetNoteBitSet(cellDigitD);

                                var cellNoteBitSet = houseMask & (cellNoteBitSetA | cellNoteBitSetB | cellNoteBitSetC | cellNoteBitSetD);
                                if (UInt128.PopCount(cellNoteBitSet) == 4)
                                {
                                    // あるハウスにおいて、ある四つの数字が四つのセルのみの候補に存在する
                                    // => 隠れ四国同盟が成立している
                                    // ただし、隠れ四国同盟によって否定可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                                    // 隠れ四国同盟のセルの位置を求める
                                    var cellIndexes = cellNoteBitSet.EnumerateAsCellIndexes().ToArray();
#if DEBUG
                                    System.Diagnostics.Debug.Assert(cellIndexes.Length == 4);
#endif
                                    // 隠れ四国同盟に含まれるセル 1
                                    var cellIndex1 = cellIndexes[0];
                                    // 隠れ四国同盟に含まれるセル 2
                                    var cellIndex2 = cellIndexes[1];
                                    // 隠れ四国同盟に含まれるセル 3
                                    var cellIndex3 = cellIndexes[2];
                                    // 隠れ四国同盟に含まれるセル 4
                                    var cellIndex4 = cellIndexes[3];

                                    var keyCellDigitBitSet = cellDigitA.ToCellNoteBitSet() | cellDigitB.ToCellNoteBitSet() | cellDigitC.ToCellNoteBitSet() | cellDigitD.ToCellNoteBitSet();
                                    var cellNoteBitSet1 = ws.GetNoteBitSet(cellIndex1);
                                    var cellNoteBitSet2 = ws.GetNoteBitSet(cellIndex2);
                                    var cellNoteBitSet3 = ws.GetNoteBitSet(cellIndex3);
                                    var cellNoteBitSet4 = ws.GetNoteBitSet(cellIndex4);
                                    var removedCellDigitBitSet1 = cellNoteBitSet1 & ~keyCellDigitBitSet;
                                    var removedCellDigitBitSet2 = cellNoteBitSet2 & ~keyCellDigitBitSet;
                                    var removedCellDigitBitSet3 = cellNoteBitSet3 & ~keyCellDigitBitSet;
                                    var removedCellDigitBitSet4 = cellNoteBitSet4 & ~keyCellDigitBitSet;
                                    var removedCellDigitBitSet = removedCellDigitBitSet1 | removedCellDigitBitSet2 | removedCellDigitBitSet3 | removedCellDigitBitSet4;
                                    if (removedCellDigitBitSet != 0)
                                    {
                                        // この時点で、隠れ四国同盟によって否定される数字の候補が存在する

                                        var highlightedCellNote =
                                            (cellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex1)
                                            .Concat((cellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex2))
                                            .Concat((cellNoteBitSet3 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex3))
                                            .Concat((cellNoteBitSet4 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex4))
                                            .ToArray();
                                        var removedCellNotes =
                                            removedCellDigitBitSet1.EnumerateAsCellDigits().ToCells(cellIndex1)
                                            .Concat(removedCellDigitBitSet2.EnumerateAsCellDigits().ToCells(cellIndex2))
                                            .Concat(removedCellDigitBitSet3.EnumerateAsCellDigits().ToCells(cellIndex3))
                                            .Concat(removedCellDigitBitSet4.EnumerateAsCellDigits().ToCells(cellIndex4))
                                            .ToArray();
                                        var houseName = houseNameGetter();
                                        var cellPositionText1 = cellIndex1.ToFriendlyString();
                                        var cellPositionText2 = cellIndex2.ToFriendlyString();
                                        var cellPositionText3 = cellIndex3.ToFriendlyString();
                                        var cellPositionText4 = cellIndex4.ToFriendlyString();
                                        var cellDigitCharA = cellDigitA.ToCellDigitChar();
                                        var cellDigitCharB = cellDigitB.ToCellDigitChar();
                                        var cellDigitCharC = cellDigitC.ToCellDigitChar();
                                        var cellDigitCharD = cellDigitD.ToCellDigitChar();
                                        var removedDigitChars =
                                            string.Join(
                                                ", ",
                                                removedCellDigitBitSet
                                                .EnumerateAsCellDigits()
                                                .Select(cellDigit => $"'{cellDigit.ToCellDigitChar()}'"));
                                        var keyCellDigitCharsText = $"'{cellDigitA}', '{cellDigitB}', '{cellDigitC}', '{cellDigitD}'";
                                        var keyCellPositionsText = $"{cellPositionText1}, {cellPositionText2}, {cellPositionText3}, {cellPositionText4}";
                                        var description =
                                            $"{houseName}において、四つの数字 {keyCellDigitCharsText} を候補として含むセルは {keyCellPositionsText} の四つしかないので、" +
                                            $"これらのセルの数字は必ず {keyCellDigitCharsText} の何れかです。" +
                                            $"したがって、四つのセル {keyCellPositionsText} の数字の候補からそれ以外の数字 {removedDigitChars} を削除します。";
                                        return
                                            new HiddenQuadOperation(
                                                highlightedCellNote,
                                                removedCellNotes,
                                                houseMask.EnumerateAsCellIndexes().ToCellPositions().ToArray(),
                                                description);
                                    }
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }

        internal override SudokuBoardCellsSet Execute(SudokuBoardCellsSet cells)
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
