using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.HardClass
{
    /// <summary>
    /// 隠れ二国同盟パターンのオペレーションのクラスです。
    /// </summary>
    internal sealed class HiddenPairOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private HiddenPairOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "隠れ二国同盟",
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
            // あるハウス H と ある二つの数字 D1, D2 とある二つのセル C1, C2 に対して、
            // H 内に数字の候補として D1, D2 の何れかを持つセルが C1, C2 のみの場合
            // => C1, C2 の数字はそれぞれ D1, D2 の何れかであるので、C1, C2 の D1, S2 以外の数字の候補は排除することが出来る。

            // ブロックハウスの検索
            for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                        () => $"{block.ToFriendlyString()}のブロック",
                        SudokuBoardCellGeometry.GetCellIndexes(block).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            // 行ハウスの検索
            for (var row = SudokuBoardCellRow.MinValue; row <= SudokuBoardCellRow.MaxValue; ++row)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                        () => $"{row.ToFriendlyString()}の行",
                        SudokuBoardCellGeometry.GetCellIndexes(row).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            // 列ハウスの検索
            for (var column = SudokuBoardCellColumn.MinValue; column <= SudokuBoardCellColumn.MaxValue; ++column)
            {
                var operations =
                    MatchPatternByHouse(
                        ws,
                        SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                        () => $"{column.ToFriendlyString()}の列",
                        SudokuBoardCellGeometry.GetCellIndexes(column).Span);
                foreach (var operation in operations)
                    yield return operation;
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByHouse(SudokuBoardWorkspace ws, UInt128 houseMask, Func<string> houseNameGetter, ReadOnlySpan<SudokuBoardCellIndex> houseCellIndexes)
            {
                for (var cellDigitA = SudokuBoardCellDigit.MinValue; cellDigitA < SudokuBoardCellDigit.MaxValue - 1; ++cellDigitA)
                {
                    var cellNoteBitSetA = ws.GetNoteBitSet(cellDigitA) & houseMask;
                    if (cellNoteBitSetA != 0)
                    {
                        // 指定されたハウスにおいて、cellDigitA が何れかのセルの数字の候補に存在する
                        for (var cellDigitB = cellDigitA + 1; cellDigitB < SudokuBoardCellDigit.MaxValue; ++cellDigitB)
                        {
                            var cellNoteBitSetB = ws.GetNoteBitSet(cellDigitB) & houseMask;
                            if (cellNoteBitSetB != 0)
                            {
                                // 指定されたハウスにおいて、cellDigitB が何れかのセルの数字の候補に存在する
                                var cellNoteBitSet = cellNoteBitSetA | cellNoteBitSetB;
                                if (UInt128.PopCount(cellNoteBitSet) == 2)
                                {
                                    // あるハウスにおいて、ある二つの数字が二つのセルのみの候補に存在する
                                    // => 隠れ二国同盟が成立している
                                    // ただし、隠れ二国同盟によって排除可能な数字の候補を持つセルが存在するかどうかはまだわからない。

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
                                    var excludedCellDigitBitSet1 = cellNoteBitSet1 & ~keyCellDigitBitSet;
                                    var excludedCellDigitBitSet2 = cellNoteBitSet2 & ~keyCellDigitBitSet;
                                    var excludedCellDigitBitSet = excludedCellDigitBitSet1 | excludedCellDigitBitSet2;
                                    if (excludedCellDigitBitSet != 0)
                                    {
                                        // この時点で、隠れ二国同盟によって排除される数字の候補が存在する

                                        var highlightedCellNote =
                                            (cellNoteBitSet1 & keyCellDigitBitSet).EnumerateAsCellNotes(cellIndex1)
                                            .Concat((cellNoteBitSet2 & keyCellDigitBitSet).EnumerateAsCellNotes(cellIndex2));
                                        var excludedCellNotes =
                                            excludedCellDigitBitSet1.EnumerateAsCellNotes(cellIndex1)
                                            .Concat(excludedCellDigitBitSet2.EnumerateAsCellNotes(cellIndex2));
                                        var houseName = houseNameGetter();
                                        var excludedDigitChars =
                                            string.Join(
                                                ", ",
                                                excludedCellDigitBitSet
                                                .EnumerateAsCellDigits()
                                                .Select(cellDigit => $"'{cellDigit.ToCellDigitChar()}'"));
                                        var keyCellDigitCharsText = $"'{cellDigitA.ToCellDigitChar()}', '{cellDigitB.ToCellDigitChar()}'";
                                        var keyCellPositionsText = $"{cellIndex1.ToFriendlyString()}, {cellIndex2.ToFriendlyString()}";
                                        var description =
                                            $"{houseName}において、二つの数字 {keyCellDigitCharsText} を候補として含むセルは {keyCellPositionsText} の二つしかないので、" +
                                            $"これらのセルの数字は必ず {keyCellDigitCharsText} の何れかです。\n" +
                                            $"したがって、二つのセル {keyCellPositionsText} の数字の候補からそれ以外の数字 {excludedDigitChars} を排除します。";
                                        yield return
                                            new HiddenPairOperation(
                                                excludedCellNotes,
                                                houseMask.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PrimaryPositivelyHighlighted),
                                                highlightedCellNote.ToCellNoteEffects(SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted),
                                                description);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
