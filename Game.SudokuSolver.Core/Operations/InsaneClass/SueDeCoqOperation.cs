using System;
using System.Collections.Generic;
using Game.GameSolver;

namespace Game.SudokuSolver.Operations.InsaneClass
{
    /// <summary>
    /// Sue de Coq (スー・デ・コック) パターンのオペレーションのクラスです。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <term>参考文献</term>
    /// <description>
    /// <see href="https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates">【コラム】Sue De Coq の一般化 - 3. 追加候補が重複している！</see>/></description>
    /// </item>
    /// </list>
    /// </remarks>
    internal sealed partial class SueDeCoqOperation
        : SudokuBoardRemoveCellNoteOperation
    {
        private SueDeCoqOperation(
            IEnumerable<SudokuBoardCellPositionDigit> excludedCellNotes,
            IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)> cellBackgroundEffects,
            IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> cellNoteEffects,
            string description)
            : base(
                  GameBoardStatus.Solving,
                  "Su de Coq",
                  SudokuBoardDifficultyLevel.Insane,
                  excludedCellNotes,
                  cellBackgroundEffects,
                  cellNoteEffects,
                  description)
        {
        }

        public static IEnumerable<SudokuBoardOperation> MatchPattern(SudokuBoardWorkspace ws)
        {
            // 【論拠】
            // 基本的な考え方は、ブロックハウスと、それと交差するライン (行または列) ハウスの上で N 国同盟を作るというもの。
            // ブロックハウスとラインハウスが交差するセルの集合を交差部、ラインハウス上を除くブロックハウス上のセルの集合をブロックウィング、ブロックハウス上を除くラインハウス上のセルの集合をラインウィングと呼称する。
            // 以下の条件で Su de Coq が成立する。
            // 1) 交差部から選んだ数字の候補の数が N 個以下のセルの集合を C とし、かつ
            // 2) ブロックウィングから選んだ数字の候補の数が N 個以下のセルの集合を Cb とし、かつ
            // 3) ラインウィングから選んだ数字の候補の数が N 個以下のセルの集合を Cr とし、かつ
            // 4) C に含まれるセルの数字の候補の和集合を D とし、かつ
            // 5) Cb に含まれるセルの数字の候補の和集合を Db とし、かつ
            // 6) Cr に含まれるセルの数字の候補の和集合を Dr とし、かつ
            // 7) |D| + |Db ＼ D| + |Dr ＼ D| = N であり、かつ
            // 8) D ∩ Db ∩ Dr = Ø (空集合) である場合。
            // 
            // 【この条件で Su de Coq が成立する理由】
            // 同一ハウス内のセルの数字は排他的であるので、C ∪ Cb のセルの数字は互いに排他的であり、同じく C ∪ Cr のセルの数字も互いに排他的である。
            // あとは、Cb ∪ Cb のセルの数字も互いに排他的であると示せれば、選ばれた N 個のセルが全て互いに排他的であると言える。
            //
            // そこで、次に、Cb と Cb のセルの数字も互いに排他的、つまり重複することはないことを示す。
            // 任意の数字 d について、以下に場合分けをする。
            // a) d ∉ C, d ∉ Db, d ∉ Dr である場合
            //   => そもそもどのセルにも含まれていないので重複した数字であるはずがない。
            // b) d ∉ C, d ∉ Db, d ∈ Dr である場合
            //   => 自明に重複することはない。
            // c) d ∉ C, d ∈ Db, d ∉ Dr である場合
            //   => 自明に重複することはない。
            // d) d ∉ C, d ∈ Db, d ∈ Dr である場合
            //   => Db と Dr に同じ数字 d が含まれているが、C には含まれていない。
            //      ブロックウィングとラインウィングは共通の領域がないので、例え Cb のどのセルの数字が d であっても Cr のどのセルの数字が d かには全く影響がない。その逆もまた同じである。
            //      例えば d が 5 で、Cb の何れかの数字に 5 が入るのなら、Cr の何れかには 5'(ダッシュ) という名称が似ているだけの別のモノが入ると考えれば、重複していないと考えることが出来る。
            //      それに、実際、条件 7 により、5 と 5' も含めた上で合計 N 個の数字になっている。
            // e) d ∈ C, d ∉ Db, d ∉ Dr である場合
            //   => 自明に重複することはない。
            // f) d ∈ C, d ∉ Db, d ∈ Dr である場合
            //   => 自明に重複することはない。
            // g) d ∈ C, d ∈ Db, d ∉ Dr である場合
            //   => 自明に重複することはない。
            // h) d ∈ C, d ∈ Db, d ∈ Dr である場合
            //   => 条件 8 によりこれはあり得ない。
            // 
            // 前述により、全ての数字に対して Cb と Cr の何れのセルに入る数字も排他的であることを示すことが出来たので、
            // C ∪ Cb ∪ Cr 内の何れのセルも同じ数字が入ることはない。(前述のケース d のような 5 と 5' のような数字は別なモノとして扱う)
            //
            // 【排除される数字の候補】
            // ブロックハウスから C ∪ Cb を除くセルの数字の候補からは、d ∈ Db ∪ (D ＼ Dr) である全ての数字 d を排除出来る。
            // ラインハウスからから C ∪ Cr を除くセルの数字の候補からは、d ∈ Dr ∪ (D ＼ Db) である全ての数字 d を排除出来る。

            for (var numberOfAlliance = 4; numberOfAlliance <= 9; ++numberOfAlliance)
            {
                for (var block = SudokuBoardCellBlock.MinValue; block <= SudokuBoardCellBlock.MaxValue; ++block)
                {
                    // 行ハウスの検索
                    for (var rowOffset = SudokuBoardCellBlockRow.MinValue; rowOffset <= SudokuBoardCellBlockRow.MaxValue; ++rowOffset)
                    {
                        var row = SudokuBoardCellGeometry.GetRow(block, rowOffset);
                        var operations =
                            MatchPatternByHouse(
                                ws,
                                numberOfAlliance,
                                block,
                                SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                                SudokuBoardCellGeometry.GetRowCellsBitSet(row),
                                () => $"{block.ToFriendlyString()}のブロック",
                                () => $"{row.ToFriendlyString()}の行");
                        foreach (var operation in operations)
                            yield return operation;
                    }

                    // 列ハウスの検索
                    for (var columnOffset = SudokuBoardCellBlockColumn.MinValue; columnOffset <= SudokuBoardCellBlockColumn.MaxValue; ++columnOffset)
                    {
                        var column = SudokuBoardCellGeometry.GetColumn(block, columnOffset);
                        var operations =
                            MatchPatternByHouse(
                                ws,
                                numberOfAlliance,
                                block,
                                SudokuBoardCellGeometry.GetBlockCellsBitSet(block),
                                SudokuBoardCellGeometry.GetColumnCellsBitSet(column),
                                () => $"{block.ToFriendlyString()}のブロック",
                                () => $"{column.ToFriendlyString()}の列");
                        foreach (var operation in operations)
                            yield return operation;
                    }
                }
            }

            static IEnumerable<SudokuBoardOperation> MatchPatternByHouse(SudokuBoardWorkspace ws, int numberOfAlliance, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 columnOrRowHouseMask, Func<string> blockHouseNameGetter, Func<string> columnOrRowHouseNameGetter)
            {
                switch (numberOfAlliance)
                {
                    case 4:
                    {
                        var operation = MatchPatternByHouse4(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }

                    case 5:
                    {
                        var operation = MatchPatternByHouse5(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }
                    case 6:
                    {
                        var operation = MatchPatternByHouse6(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }
                    case 7:
                    {
                        var operation = MatchPatternByHouse7(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }
                    case 8:
                    {
                        var operation = MatchPatternByHouse8(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }
                    case 9:
                    {
                        var operation = MatchPatternByHouse9(ws, block, blockHouseMask, columnOrRowHouseMask, blockHouseNameGetter, columnOrRowHouseNameGetter);
                        if (operation is not null)
                            yield return operation;
                        break;
                    }
                    default:
                        throw new ApplicationException();
                }
            }
        }

        /// <summary>
        /// 指定されたセルの位置のビット集合から、未確定かつ数字の候補が指定された数以下のセルの位置を配列に格納します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="maxCellNoteCount">
        /// セルの数字の候補の数の最大値を示す <see cref="int"/> 値です。
        /// </param>
        /// <param name="cellBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="keyCellIIndexesBuffer">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値の格納先の配列です。
        /// </param>
        /// <returns>
        /// セルの位置を格納した配列とその実際の長さを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        private static Span<SudokuBoardCellIndex> SearchKeyCells(SudokuBoardWorkspace ws, int maxCellNoteCount, UInt128 cellBitSet, Span<SudokuBoardCellIndex> keyCellIIndexesBuffer)
        {
            var length = 0;
            while (cellBitSet != 0)
            {
                var cellIndex = cellBitSet.GetOneCellIndex();
                if (!ws.IsDetermined(cellIndex) && ws.GetNoteBitSet(cellIndex).GetPopCount() <= maxCellNoteCount)
                    keyCellIIndexesBuffer[length++] = cellIndex;
                cellBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return keyCellIIndexesBuffer[..length];
        }

        /// <summary>
        /// 指定されたセルの位置の集合から、各セルの候補の数字の和集合を取得します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <returns>
        /// 指定されたセルの何れかの候補である数字のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> です。
        /// </returns>
        private static SudokuBoardCellNoteBitSet GetUnionDigitBitSet(SudokuBoardWorkspace ws, UInt128 cellIndexBitSet)
        {
            var digitBitSet = SudokuBoardCellNoteBitSet.None;
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(!ws.IsDetermined(cellIndex));
#endif
                digitBitSet |= ws.GetNoteBitSet(cellIndex);
                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return digitBitSet;
        }

        /// <summary>
        /// 指定されたセルの位置のビット集合と候補の数字のビット集合から、セルの位置と候補の数字の組み合わせを求めて配列に格納します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellNoteDigitBitSet">
        /// セルの数字の候補のビット集合を示す <see cref="SudokuBoardCellNoteBitSet"/> 値です。
        /// </param>
        /// <param name="cellNoteDigitsBuffer">
        /// セルの位置と数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値の格納先の配列です。
        /// </param>
        /// <returns>
        /// セルの位置を格納した配列とその実際の長さを示す <see cref="Span{T}"/> 値です。
        /// </returns>
        private static Span<SudokuBoardCellPositionDigit> SearchCellNotes(SudokuBoardWorkspace ws, UInt128 cellIndexBitSet, SudokuBoardCellNoteBitSet cellNoteDigitBitSet, Span<SudokuBoardCellPositionDigit> cellNoteDigitsBuffer)
        {
            var length = 0;
            cellIndexBitSet &= ~ws.DeterminedCellBitSet;
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
                var digitBitSet = cellNoteDigitBitSet & ws.GetNoteBitSet(cellIndex);
                while (digitBitSet != 0)
                {
                    var digit = digitBitSet.GetOneCellNoteDigit();
                    cellNoteDigitsBuffer[length++] = new SudokuBoardCellPositionDigit(cellIndex, digit);
                    digitBitSet &= ~digit.ToCellNoteBitSet();
                }

                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }

            return cellNoteDigitsBuffer[..length];
        }
    }
}
