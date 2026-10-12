using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// 隠れ二国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenPairOperation
        : SudokuBoardOperation
    {
        private HiddenPairOperation(ReadOnlyMemory<SudokuBoardCell> hilightedCellNotes, ReadOnlyMemory<SudokuBoardCell> removedCellNotes, ReadOnlyMemory<SudokuBoardCellPosition> relatedCellPositions, string description)
            : base("隠れ二国同盟", SudokuBoardDifficultyLevel.Hard, null, hilightedCellNotes, removedCellNotes, relatedCellPositions, description)
        {
        }

        public static HiddenPairOperation? MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // あるハウス H と ある二つの数字 D1, D2 とある二つのセル C1, C2 に対して、
            // H 内に数字の候補として D1, D2 の何れかを持つセルが C1, C2 のみの場合
            // => C1, C2 の数字はそれぞれ D1, D2 の何れかであるので、C1, C2 の D1, S2 以外の数字の候補は否定される。

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

            static HiddenPairOperation? MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var cellDigitA = SudokuBoardCellDigit.MinValue; cellDigitA < SudokuBoardCellDigit.MaxValue - 1; ++cellDigitA)
                {
                    var cellNoteBitSetA = ws.GetNoteBitSet(cellDigitA);
                    for (var cellDigitB = cellDigitA + 1; cellDigitB < SudokuBoardCellDigit.MaxValue; ++cellDigitB)
                    {
                        var cellNoteBitSetB = ws.GetNoteBitSet(cellDigitB);

                        var cellNoteBitSet = houseMask & (cellNoteBitSetA | cellNoteBitSetB);
                        if (UInt128.PopCount(cellNoteBitSet) == 2)
                        {
                            // あるハウスにおいて、ある二つの数字が二つのセルのみの候補に存在する
                            // => 隠れ二国同盟が成立している
                            // ただし、隠れ二国同盟によって否定可能な数字の候補を持つセルが存在するかどうかはまだわからない。

                            // 隠れ二国同盟のセルの位置を求める
                            var cellIndexes = cellNoteBitSet.EnumerateAsCellIndexes().ToArray();
#if DEBUG
                            System.Diagnostics.Debug.Assert(cellIndexes.Length == 2);
#endif
                            // 隠れ二国同盟に含まれるセル 1
                            var cellIndex1 = cellIndexes[0];
                            // 隠れ二国同盟に含まれるセル 2
                            var cellIndex2 = cellIndexes[1];

                            var keyCellDigitBitSet = cellDigitA.ToCellNoteBitSet() | cellDigitB.ToCellNoteBitSet();
                            var cellNoteBitSet1 = ws.GetNoteBitSet(cellIndex1);
                            var cellNoteBitSet2 = ws.GetNoteBitSet(cellIndex2);
                            var removedCellDigitBitSet1 = cellNoteBitSet1 & ~keyCellDigitBitSet;
                            var removedCellDigitBitSet2 = cellNoteBitSet2 & ~keyCellDigitBitSet;
                            var removedCellDigitBitSet = removedCellDigitBitSet1 | removedCellDigitBitSet2;
                            if (removedCellDigitBitSet != 0)
                            {
                                // この時点で、隠れ二国同盟によって否定される数字の数字の候補が存在する

                                // 隠れ二国同盟によって否定されるセルの位置と数字の候補を求める
                                var hilightedCellNote =
                                    (cellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex1)
                                    .Concat((cellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellDigits().ToCells(cellIndex2))
                                    .ToArray();
                                var removedCellNotes =
                                    removedCellDigitBitSet1.EnumerateAsCellDigits().ToCells(cellIndex1)
                                    .Concat(removedCellDigitBitSet2.EnumerateAsCellDigits().ToCells(cellIndex2))
                                    .ToArray();

                                var houseName = houseNameGetter();
                                var cellPositionText1 = cellIndex1.ToFriendlyString();
                                var cellPositionText2 = cellIndex2.ToFriendlyString();
                                var cellDigitCharA = cellDigitA.ToCellDigitChar();
                                var cellDigitCharB = cellDigitB.ToCellDigitChar();
                                var removedDigitChars =
                                    string.Join(
                                        ", ",
                                        removedCellDigitBitSet
                                        .EnumerateAsCellDigits()
                                        .Select(cellDigit => $"'{cellDigit.ToCellDigitChar()}'"));
                                var keyCellDigitCharsText = $"'{cellDigitA}', '{cellDigitB}'";
                                var keyCellPositionsText = $"{cellPositionText1}, {cellPositionText2}";
                                var description =
                                    $"{houseName}において、二つの数字 {keyCellDigitCharsText} を候補として含むセルは {keyCellPositionsText} の二つしかないので、" +
                                    $"これらのセルの数字は必ず {keyCellDigitCharsText} の何れかです。" +
                                    $"したがって、二つのセル {keyCellPositionsText} の数字の候補からそれ以外の数字 {removedDigitChars} を削除します。";
                                return
                                    new HiddenPairOperation(
                                        hilightedCellNote,
                                        removedCellNotes,
                                        houseMask.EnumerateAsCellIndexes().ToCellPositions().ToArray(),
                                        description);
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
