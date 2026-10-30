using System;
using System.Linq;

namespace Game.SudokuSolver.Operations.InsaneClass
{
    internal partial class SueDeCoqOperation
    {
        #region MatchPatternByHouse4

        /// <summary>
        /// キーとなるセルの数が 4 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse4(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 4, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 4 個以下のセルが 4 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 4)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 3; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 2; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 1; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);

                            // キーとなるすべてのセルの位置の和集合
                            var allKeyCellBitSet =
                                keyCellIndex1.ToCellIndexBitSet()
                                | keyCellIndex2.ToCellIndexBitSet()
                                | keyCellIndex3.ToCellIndexBitSet()
                                | keyCellIndex4.ToCellIndexBitSet();

                            // ブロックに属するキーセルの位置の集合
                            var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                            // ラインに属するキーセルの位置の集合
                            var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                            // 交差部に属するキーセルの位置の集合
                            var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                            // ブロックウィングに属するキーセルの位置の集合
                            var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                            // ラインウィングに属するキーセルの位置の集合
                            var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                            // 交差部の各セルの数字の候補の和集合
                            var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                            // ブロックウィングの各キーセルの数字の候補の和集合
                            var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                            // ラインウィングの各キーセルの数字の候補の和集合
                            var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                            // 以下の条件で、スー・デ・コック法が成立する。
                            // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                            // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                            // 3) 以下の数の合計が 4 に等しい場合。
                            //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                            //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                            //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                            // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                            if (intersectionKeyCellBitSet != 0
                                && blockWingKeyCellBitSet != 0
                                && lineWingKeyCellBitSet != 0
                                && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 4)
                            {
                                // この時点でスー・デ・コック法の条件が成立している。

                                // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                var excludedCellNotesInBlock =
                                    SearchCellNotes(
                                        ws,
                                        blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                        blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                        excludedCellNotesBuffer);

                                // ライン内で排除可能な数字の候補とそのセル位置を求める
                                var excludedCellNotesInLine =
                                    SearchCellNotes(
                                        ws,
                                        lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                        lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                        excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                // 排除可能な数字の候補の存在を確認する
                                if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                {
                                    // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                    // => Su de Coq 法を適用する。

                                    var excludedCellNotes =
                                        excludedCellNotesInBlock.ToArray()
                                        .Concat(excludedCellNotesInLine.ToArray());

                                    var blockHouseName = blockHouseNameGetter();
                                    var lineHouseName = lineHouseNameGetter();
                                    var keyCellPositionsText =
                                        string.Join(
                                            ", ",
                                            allKeyCellBitSet
                                            .EnumerateAsCellPositionTexts());
                                    var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                    var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                    var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                    var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                    var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                    var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                    var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                    var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                    var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                    var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                    var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                    var allKeyDigitsText =
                                        string.Join(
                                            ", ",
                                            intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                            .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                            .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                            .OrderBy(digit => digit)
                                            .Select(digit => digit.ToCellDigitText()));
                                    var description =
                                        $"{blockHouseName}と{lineHouseName}において、四つのマス ({keyCellPositionsText}) を選びます。\n" +
                                        $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                        $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                        $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                        $"{(
                                            bothWingKeyigitsText.Length > 0
                                            ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、四つの数字 ({allKeyDigitsText}) は重複せず四つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                            : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、四つの数字 ({allKeyDigitsText}) は重複せず四つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                        )}" +
                                        $"したがって、" +
                                        $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                        $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                    return
                                        new SueDeCoqOperation(
                                            excludedCellNotes,
                                            intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                            ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
                                            description);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region MatchPatternByHouse5

        /// <summary>
        /// キーとなるセルの数が 5 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse5(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 5, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 5 個以下のセルが 5 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 5)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 4; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 3; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 2; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length - 1; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                            for (var keyIndex5 = keyIndex4 + 1; keyIndex5 < keyCellIIndexes.Length; ++keyIndex5)
                            {
                                var keyCellIndex5 = keyCellIIndexes[keyIndex5];
                                var keyDigitBitSet5 = ws.GetNoteBitSet(keyCellIndex5);

                                // キーとなるすべてのセルの位置の和集合
                                var allKeyCellBitSet =
                                    keyCellIndex1.ToCellIndexBitSet()
                                    | keyCellIndex2.ToCellIndexBitSet()
                                    | keyCellIndex3.ToCellIndexBitSet()
                                    | keyCellIndex4.ToCellIndexBitSet()
                                    | keyCellIndex5.ToCellIndexBitSet();

                                // ブロックに属するキーセルの位置の集合
                                var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                                // ラインに属するキーセルの位置の集合
                                var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                                // 交差部に属するキーセルの位置の集合
                                var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                                // ブロックウィングに属するキーセルの位置の集合
                                var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                                // ラインウィングに属するキーセルの位置の集合
                                var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                                // 交差部の各セルの数字の候補の和集合
                                var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                                // ブロックウィングの各キーセルの数字の候補の和集合
                                var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                                // ラインウィングの各キーセルの数字の候補の和集合
                                var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                                // 以下の条件で、スー・デ・コック法が成立する。
                                // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                                // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                                // 3) 以下の数の合計が 5 に等しい場合。
                                //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                                //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                                if (intersectionKeyCellBitSet != 0
                                    && blockWingKeyCellBitSet != 0
                                    && lineWingKeyCellBitSet != 0
                                    && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                    && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 5)
                                {
                                    // この時点でスー・デ・コック法の条件が成立している。

                                    // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                    var excludedCellNotesInBlock =
                                        SearchCellNotes(
                                            ws,
                                            blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                            blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                            excludedCellNotesBuffer);

                                    // ライン内で排除可能な数字の候補とそのセル位置を求める
                                    var excludedCellNotesInLine =
                                        SearchCellNotes(
                                            ws,
                                            lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                            lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                            excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                    // 排除可能な数字の候補の存在を確認する
                                    if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                    {
                                        // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                        // => Su de Coq 法を適用する。

                                        var excludedCellNotes =
                                            excludedCellNotesInBlock.ToArray()
                                            .Concat(excludedCellNotesInLine.ToArray());

                                        var blockHouseName = blockHouseNameGetter();
                                        var lineHouseName = lineHouseNameGetter();
                                        var keyCellPositionsText =
                                            string.Join(
                                                ", ",
                                                allKeyCellBitSet
                                                .EnumerateAsCellPositionTexts());
                                        var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                        var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                        var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                        var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                        var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                        var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                        var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                        var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                        var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                        var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                        var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                        var allKeyDigitsText =
                                            string.Join(
                                                ", ",
                                                intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                                .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                .OrderBy(digit => digit)
                                                .Select(digit => digit.ToCellDigitText()));
                                        var description =
                                            $"{blockHouseName}と{lineHouseName}において、五つのマス ({keyCellPositionsText}) を選びます。\n" +
                                            $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                            $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                            $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                            $"{(
                                                bothWingKeyigitsText.Length > 0
                                                ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、五つの数字 ({allKeyDigitsText}) は重複せず五つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                                : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、五つの数字 ({allKeyDigitsText}) は重複せず五つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                            )}" +
                                            $"したがって、" +
                                            $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                            $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                        return
                                            new SueDeCoqOperation(
                                                excludedCellNotes,
                                                intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                    .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                    .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                    .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                    .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                    .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
                                                description);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region MatchPatternByHouse6

        /// <summary>
        /// キーとなるセルの数が 6 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse6(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 6, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 6 個以下のセルが 6 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 6)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 5; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 4; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 3; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length - 2; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                            for (var keyIndex5 = keyIndex4 + 1; keyIndex5 < keyCellIIndexes.Length - 1; ++keyIndex5)
                            {
                                var keyCellIndex5 = keyCellIIndexes[keyIndex5];
                                var keyDigitBitSet5 = ws.GetNoteBitSet(keyCellIndex5);
                                for (var keyIndex6 = keyIndex5 + 1; keyIndex6 < keyCellIIndexes.Length; ++keyIndex6)
                                {
                                    var keyCellIndex6 = keyCellIIndexes[keyIndex6];
                                    var keyDigitBitSet6 = ws.GetNoteBitSet(keyCellIndex6);

                                    // キーとなるすべてのセルの位置の和集合
                                    var allKeyCellBitSet =
                                        keyCellIndex1.ToCellIndexBitSet()
                                        | keyCellIndex2.ToCellIndexBitSet()
                                        | keyCellIndex3.ToCellIndexBitSet()
                                        | keyCellIndex4.ToCellIndexBitSet()
                                        | keyCellIndex5.ToCellIndexBitSet()
                                        | keyCellIndex6.ToCellIndexBitSet();

                                    // ブロックに属するキーセルの位置の集合
                                    var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                                    // ラインに属するキーセルの位置の集合
                                    var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                                    // 交差部に属するキーセルの位置の集合
                                    var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                                    // ブロックウィングに属するキーセルの位置の集合
                                    var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                                    // ラインウィングに属するキーセルの位置の集合
                                    var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                                    // 交差部の各セルの数字の候補の和集合
                                    var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                                    // ブロックウィングの各キーセルの数字の候補の和集合
                                    var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                                    // ラインウィングの各キーセルの数字の候補の和集合
                                    var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                                    // 以下の条件で、スー・デ・コック法が成立する。
                                    // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                                    // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                                    // 3) 以下の数の合計が 6 に等しい場合。
                                    //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                                    //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                    //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                    // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                                    if (intersectionKeyCellBitSet != 0
                                        && blockWingKeyCellBitSet != 0
                                        && lineWingKeyCellBitSet != 0
                                        && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                        && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 6)
                                    {
                                        // この時点でスー・デ・コック法の条件が成立している。

                                        // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                        var excludedCellNotesInBlock =
                                            SearchCellNotes(
                                                ws,
                                                blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                                blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                                excludedCellNotesBuffer);

                                        // ライン内で排除可能な数字の候補とそのセル位置を求める
                                        var excludedCellNotesInLine =
                                            SearchCellNotes(
                                                ws,
                                                lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                                lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                                excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                        // 排除可能な数字の候補の存在を確認する
                                        if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                        {
                                            // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                            // => Su de Coq 法を適用する。

                                            var excludedCellNotes =
                                                excludedCellNotesInBlock.ToArray()
                                                .Concat(excludedCellNotesInLine.ToArray());

                                            var blockHouseName = blockHouseNameGetter();
                                            var lineHouseName = lineHouseNameGetter();
                                            var keyCellPositionsText =
                                                string.Join(
                                                    ", ",
                                                    allKeyCellBitSet
                                                    .EnumerateAsCellPositionTexts());
                                            var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                            var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                            var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                            var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                            var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                            var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                            var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                            var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                            var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                            var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                            var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                            var allKeyDigitsText =
                                                string.Join(
                                                    ", ",
                                                    intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                                    .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                    .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                    .OrderBy(digit => digit)
                                                    .Select(digit => digit.ToCellDigitText()));
                                            var description =
                                                $"{blockHouseName}と{lineHouseName}において、六つのマス ({keyCellPositionsText}) を選びます。\n" +
                                                $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                                $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                                $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                                $"{(
                                                    bothWingKeyigitsText.Length > 0
                                                    ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、六つの数字 ({allKeyDigitsText}) は重複せず六つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                                    : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、六つの数字 ({allKeyDigitsText}) は重複せず六つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                                )}" +
                                                $"したがって、" +
                                                $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                                $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                            return
                                                new SueDeCoqOperation(
                                                    excludedCellNotes,
                                                    intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                        .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                        .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                        .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                    ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                        .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                        .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
                                                    description);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region MatchPatternByHouse7

        /// <summary>
        /// キーとなるセルの数が 7 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse7(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 7, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 7 個以下のセルが 7 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 7)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 6; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 5; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 4; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length - 3; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                            for (var keyIndex5 = keyIndex4 + 1; keyIndex5 < keyCellIIndexes.Length - 2; ++keyIndex5)
                            {
                                var keyCellIndex5 = keyCellIIndexes[keyIndex5];
                                var keyDigitBitSet5 = ws.GetNoteBitSet(keyCellIndex5);
                                for (var keyIndex6 = keyIndex5 + 1; keyIndex6 < keyCellIIndexes.Length - 1; ++keyIndex6)
                                {
                                    var keyCellIndex6 = keyCellIIndexes[keyIndex6];
                                    var keyDigitBitSet6 = ws.GetNoteBitSet(keyCellIndex6);
                                    for (var keyIndex7 = keyIndex6 + 1; keyIndex7 < keyCellIIndexes.Length; ++keyIndex7)
                                    {
                                        var keyCellIndex7 = keyCellIIndexes[keyIndex7];
                                        var keyDigitBitSet7 = ws.GetNoteBitSet(keyCellIndex7);

                                        // キーとなるすべてのセルの位置の和集合
                                        var allKeyCellBitSet =
                                            keyCellIndex1.ToCellIndexBitSet()
                                            | keyCellIndex2.ToCellIndexBitSet()
                                            | keyCellIndex3.ToCellIndexBitSet()
                                            | keyCellIndex4.ToCellIndexBitSet()
                                            | keyCellIndex5.ToCellIndexBitSet()
                                            | keyCellIndex6.ToCellIndexBitSet()
                                            | keyCellIndex7.ToCellIndexBitSet();

                                        // ブロックに属するキーセルの位置の集合
                                        var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                                        // ラインに属するキーセルの位置の集合
                                        var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                                        // 交差部に属するキーセルの位置の集合
                                        var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                                        // ブロックウィングに属するキーセルの位置の集合
                                        var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                                        // ラインウィングに属するキーセルの位置の集合
                                        var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                                        // 交差部の各セルの数字の候補の和集合
                                        var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                                        // ブロックウィングの各キーセルの数字の候補の和集合
                                        var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                                        // ラインウィングの各キーセルの数字の候補の和集合
                                        var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                                        // 以下の条件で、スー・デ・コック法が成立する。
                                        // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                                        // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                                        // 3) 以下の数の合計が 7 に等しい場合。
                                        //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                                        //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                        //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                        // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                                        if (intersectionKeyCellBitSet != 0
                                            && blockWingKeyCellBitSet != 0
                                            && lineWingKeyCellBitSet != 0
                                            && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                            && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 7)
                                        {
                                            // この時点でスー・デ・コック法の条件が成立している。

                                            // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                            var excludedCellNotesInBlock =
                                                SearchCellNotes(
                                                    ws,
                                                    blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                                    blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                                    excludedCellNotesBuffer);

                                            // ライン内で排除可能な数字の候補とそのセル位置を求める
                                            var excludedCellNotesInLine =
                                                SearchCellNotes(
                                                    ws,
                                                    lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                                    lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                                    excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                            // 排除可能な数字の候補の存在を確認する
                                            if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                            {
                                                // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                                // => Su de Coq 法を適用する。

                                                var excludedCellNotes =
                                                    excludedCellNotesInBlock.ToArray()
                                                    .Concat(excludedCellNotesInLine.ToArray());

                                                var blockHouseName = blockHouseNameGetter();
                                                var lineHouseName = lineHouseNameGetter();
                                                var keyCellPositionsText =
                                                    string.Join(
                                                        ", ",
                                                        allKeyCellBitSet
                                                        .EnumerateAsCellPositionTexts());
                                                var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                                var allKeyDigitsText =
                                                    string.Join(
                                                        ", ",
                                                        intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                                        .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                        .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                        .OrderBy(digit => digit)
                                                        .Select(digit => digit.ToCellDigitText()));
                                                var description =
                                                    $"{blockHouseName}と{lineHouseName}において、七つのマス ({keyCellPositionsText}) を選びます。\n" +
                                                    $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                                    $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                                    $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                                    $"{(
                                                        bothWingKeyigitsText.Length > 0
                                                        ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、七つの数字 ({allKeyDigitsText}) は重複せず七つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                                        : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、七つの数字 ({allKeyDigitsText}) は重複せず七つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                                    )}" +
                                                    $"したがって、" +
                                                    $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                                    $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                                return
                                                    new SueDeCoqOperation(
                                                        excludedCellNotes,
                                                        intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                            .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                            .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                            .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                        ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                            .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                            .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
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

            return null;
        }

        #endregion

        #region MatchPatternByHouse8

        /// <summary>
        /// キーとなるセルの数が 8 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse8(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 8, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 8 個以下のセルが 8 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 8)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 7; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 6; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 5; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length - 4; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                            for (var keyIndex5 = keyIndex4 + 1; keyIndex5 < keyCellIIndexes.Length - 3; ++keyIndex5)
                            {
                                var keyCellIndex5 = keyCellIIndexes[keyIndex5];
                                var keyDigitBitSet5 = ws.GetNoteBitSet(keyCellIndex5);
                                for (var keyIndex6 = keyIndex5 + 1; keyIndex6 < keyCellIIndexes.Length - 2; ++keyIndex6)
                                {
                                    var keyCellIndex6 = keyCellIIndexes[keyIndex6];
                                    var keyDigitBitSet6 = ws.GetNoteBitSet(keyCellIndex6);
                                    for (var keyIndex7 = keyIndex6 + 1; keyIndex7 < keyCellIIndexes.Length - 1; ++keyIndex7)
                                    {
                                        var keyCellIndex7 = keyCellIIndexes[keyIndex7];
                                        var keyDigitBitSet7 = ws.GetNoteBitSet(keyCellIndex7);
                                        for (var keyIndex8 = keyIndex7 + 1; keyIndex8 < keyCellIIndexes.Length; ++keyIndex8)
                                        {
                                            var keyCellIndex8 = keyCellIIndexes[keyIndex8];
                                            var keyDigitBitSet8 = ws.GetNoteBitSet(keyCellIndex8);

                                            // キーとなるすべてのセルの位置の和集合
                                            var allKeyCellBitSet =
                                                keyCellIndex1.ToCellIndexBitSet()
                                                | keyCellIndex2.ToCellIndexBitSet()
                                                | keyCellIndex3.ToCellIndexBitSet()
                                                | keyCellIndex4.ToCellIndexBitSet()
                                                | keyCellIndex5.ToCellIndexBitSet()
                                                | keyCellIndex6.ToCellIndexBitSet()
                                                | keyCellIndex7.ToCellIndexBitSet()
                                                | keyCellIndex8.ToCellIndexBitSet();

                                            // ブロックに属するキーセルの位置の集合
                                            var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                                            // ラインに属するキーセルの位置の集合
                                            var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                                            // 交差部に属するキーセルの位置の集合
                                            var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                                            // ブロックウィングに属するキーセルの位置の集合
                                            var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                                            // ラインウィングに属するキーセルの位置の集合
                                            var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                                            // 交差部の各セルの数字の候補の和集合
                                            var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                                            // ブロックウィングの各キーセルの数字の候補の和集合
                                            var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                                            // ラインウィングの各キーセルの数字の候補の和集合
                                            var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                                            // 以下の条件で、スー・デ・コック法が成立する。
                                            // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                                            // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                                            // 3) 以下の数の合計が 8 に等しい場合。
                                            //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                                            //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                            //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                            // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                                            if (intersectionKeyCellBitSet != 0
                                                && blockWingKeyCellBitSet != 0
                                                && lineWingKeyCellBitSet != 0
                                                && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                                && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 8)
                                            {
                                                // この時点でスー・デ・コック法の条件が成立している。

                                                // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                                var excludedCellNotesInBlock =
                                                    SearchCellNotes(
                                                        ws,
                                                        blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                                        blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                                        excludedCellNotesBuffer);

                                                // ライン内で排除可能な数字の候補とそのセル位置を求める
                                                var excludedCellNotesInLine =
                                                    SearchCellNotes(
                                                        ws,
                                                        lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                                        lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                                        excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                                // 排除可能な数字の候補の存在を確認する
                                                if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                                {
                                                    // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                                    // => Su de Coq 法を適用する。

                                                    var excludedCellNotes =
                                                        excludedCellNotesInBlock.ToArray()
                                                        .Concat(excludedCellNotesInLine.ToArray());

                                                    var blockHouseName = blockHouseNameGetter();
                                                    var lineHouseName = lineHouseNameGetter();
                                                    var keyCellPositionsText =
                                                        string.Join(
                                                            ", ",
                                                            allKeyCellBitSet
                                                            .EnumerateAsCellPositionTexts());
                                                    var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                    var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                    var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                    var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                    var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                    var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                    var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                    var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                    var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                    var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                    var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                                    var allKeyDigitsText =
                                                        string.Join(
                                                            ", ",
                                                            intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                                            .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                            .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                            .OrderBy(digit => digit)
                                                            .Select(digit => digit.ToCellDigitText()));
                                                    var description =
                                                        $"{blockHouseName}と{lineHouseName}において、八つのマス ({keyCellPositionsText}) を選びます。\n" +
                                                        $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                                        $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                                        $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                                        $"{(
                                                            bothWingKeyigitsText.Length > 0
                                                            ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、八つの数字 ({allKeyDigitsText}) は重複せず八つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                                            : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、八つの数字 ({allKeyDigitsText}) は重複せず八つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                                        )}" +
                                                        $"したがって、" +
                                                        $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                                        $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                                    return
                                                        new SueDeCoqOperation(
                                                            excludedCellNotes,
                                                            intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                                .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                                .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                                .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                            ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                                .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                                .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
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

            return null;
        }

        #endregion

        #region MatchPatternByHouse9

        /// <summary>
        /// キーとなるセルの数が 9 のスー・デ・コックの適用を試みる。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="block">
        /// 指定されたブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="blockHouseMask">
        /// ブロックに属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="lineHouseMask">
        /// ライン (行または列) に属するセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="blockHouseNameGetter">
        /// ブロックハウスの名称を取得するデリゲートです。
        /// </param>
        /// <param name="lineHouseNameGetter">
        /// ラインハウスの名称を取得するデリゲートです。
        /// </param>
        /// <returns>
        /// スー・デ・コックの適用が可能である場合は適用結果を示す <see cref="SueDeCoqOperation"/> オブジェクトを返します。そうではない場合は <see langword="null"/> を返します。 
        /// </returns>
        /// <remarks>
        /// <para>
        /// 指定されたブロックとそのブロックと交差する指定されたラインに対して、キーとなるセルと数字の数が 4 のスー・デ・コック法の適用を試みます。
        /// </para>
        /// <para>
        /// 以下にメソッド中で使用されている用語の説明を示します。
        /// <list type="bullet">
        /// <item>
        /// <term>ライン</term>
        /// <description>行または列のことです。</description>
        /// </item>
        /// <item>
        /// <term>ブロックウィング</term>
        /// <description>指定されたライン上以外の指定されたブロック上の領域です。</description>
        /// </item>
        /// <item>
        /// <term>ラインウィング</term>
        /// <description>指定されたブロック上以外の指定されたライン上の領域です。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static SueDeCoqOperation? MatchPatternByHouse9(SudokuBoardWorkspace ws, SudokuBoardCellBlock block, UInt128 blockHouseMask, UInt128 lineHouseMask, Func<string> blockHouseNameGetter, Func<string> lineHouseNameGetter)
        {
            var bothHouseMask = blockHouseMask | lineHouseMask;
            var intersectionCellBitMask = blockHouseMask & lineHouseMask;
            var blockWingCellBitMask = blockHouseMask & ~lineHouseMask;
            var lineWingCellBitMask = lineHouseMask & ~blockHouseMask;

            // 数字が排他関係である可能性があるセルを抽出する
            Span<SudokuBoardCellIndex> keyCellIIndexesBuffer = stackalloc SudokuBoardCellIndex[15];
            var keyCellIIndexes = SearchKeyCells(ws, 9, bothHouseMask, keyCellIIndexesBuffer);

            // ハウス内の未確定かつ数字の候補が 9 個以下のセルが 9 個未満の場合、セルの数字が排他関係ではあり得ないので、検索を中断する
            if (keyCellIIndexes.Length < 9)
                return null;

            // 15 はブロックハウスとラインハウスのどちらかに含まれるセルの数
            Span<SudokuBoardCellPositionDigit> excludedCellNotesBuffer = stackalloc SudokuBoardCellPositionDigit[15];
            for (var keyIndex1 = 0; keyIndex1 < keyCellIIndexes.Length - 8; ++keyIndex1)
            {
                var keyCellIndex1 = keyCellIIndexes[keyIndex1];
                var keyDigitBitSet1 = ws.GetNoteBitSet(keyCellIndex1);
                for (var keyIndex2 = keyIndex1 + 1; keyIndex2 < keyCellIIndexes.Length - 7; ++keyIndex2)
                {
                    var keyCellIndex2 = keyCellIIndexes[keyIndex2];
                    var keyDigitBitSet2 = ws.GetNoteBitSet(keyCellIndex2);
                    for (var keyIndex3 = keyIndex2 + 1; keyIndex3 < keyCellIIndexes.Length - 6; ++keyIndex3)
                    {
                        var keyCellIndex3 = keyCellIIndexes[keyIndex3];
                        var keyDigitBitSet3 = ws.GetNoteBitSet(keyCellIndex3);
                        for (var keyIndex4 = keyIndex3 + 1; keyIndex4 < keyCellIIndexes.Length - 5; ++keyIndex4)
                        {
                            var keyCellIndex4 = keyCellIIndexes[keyIndex4];
                            var keyDigitBitSet4 = ws.GetNoteBitSet(keyCellIndex4);
                            for (var keyIndex5 = keyIndex4 + 1; keyIndex5 < keyCellIIndexes.Length - 4; ++keyIndex5)
                            {
                                var keyCellIndex5 = keyCellIIndexes[keyIndex5];
                                var keyDigitBitSet5 = ws.GetNoteBitSet(keyCellIndex5);
                                for (var keyIndex6 = keyIndex5 + 1; keyIndex6 < keyCellIIndexes.Length - 3; ++keyIndex6)
                                {
                                    var keyCellIndex6 = keyCellIIndexes[keyIndex6];
                                    var keyDigitBitSet6 = ws.GetNoteBitSet(keyCellIndex6);
                                    for (var keyIndex7 = keyIndex6 + 1; keyIndex7 < keyCellIIndexes.Length - 2; ++keyIndex7)
                                    {
                                        var keyCellIndex7 = keyCellIIndexes[keyIndex7];
                                        var keyDigitBitSet7 = ws.GetNoteBitSet(keyCellIndex7);
                                        for (var keyIndex8 = keyIndex7 + 1; keyIndex8 < keyCellIIndexes.Length - 1; ++keyIndex8)
                                        {
                                            var keyCellIndex8 = keyCellIIndexes[keyIndex8];
                                            var keyDigitBitSet8 = ws.GetNoteBitSet(keyCellIndex8);
                                            for (var keyIndex9 = keyIndex8 + 1; keyIndex9 < keyCellIIndexes.Length; ++keyIndex9)
                                            {
                                                var keyCellIndex9 = keyCellIIndexes[keyIndex9];
                                                var keyDigitBitSet9 = ws.GetNoteBitSet(keyCellIndex9);

                                                // キーとなるすべてのセルの位置の和集合
                                                var allKeyCellBitSet =
                                                    keyCellIndex1.ToCellIndexBitSet()
                                                    | keyCellIndex2.ToCellIndexBitSet()
                                                    | keyCellIndex3.ToCellIndexBitSet()
                                                    | keyCellIndex4.ToCellIndexBitSet()
                                                    | keyCellIndex5.ToCellIndexBitSet()
                                                    | keyCellIndex6.ToCellIndexBitSet()
                                                    | keyCellIndex7.ToCellIndexBitSet()
                                                    | keyCellIndex8.ToCellIndexBitSet()
                                                    | keyCellIndex9.ToCellIndexBitSet();

                                                // ブロックに属するキーセルの位置の集合
                                                var blockKeyCellBitSet = allKeyCellBitSet & blockHouseMask;
                                                // ラインに属するキーセルの位置の集合
                                                var lineKeyCellBitSet = allKeyCellBitSet & lineHouseMask;
                                                // 交差部に属するキーセルの位置の集合
                                                var intersectionKeyCellBitSet = allKeyCellBitSet & intersectionCellBitMask;
                                                // ブロックウィングに属するキーセルの位置の集合
                                                var blockWingKeyCellBitSet = allKeyCellBitSet & blockWingCellBitMask;
                                                // ラインウィングに属するキーセルの位置の集合
                                                var lineWingKeyCellBitSet = allKeyCellBitSet & lineWingCellBitMask;
                                                // 交差部の各セルの数字の候補の和集合
                                                var intersectionKeyDigitBitSet = GetUnionDigitBitSet(ws, intersectionKeyCellBitSet);
                                                // ブロックウィングの各キーセルの数字の候補の和集合
                                                var blockWingKeyDigitBitSet = GetUnionDigitBitSet(ws, blockWingKeyCellBitSet);
                                                // ラインウィングの各キーセルの数字の候補の和集合
                                                var lineWingKeyDigitBitSet = GetUnionDigitBitSet(ws, lineWingKeyCellBitSet);

                                                // 以下の条件で、スー・デ・コック法が成立する。
                                                // 1) 交差部、ブロックウィング、ラインウィングの何れにも一つ以上のキーセルが存在し、かつ
                                                // 2) 交差部上の任意のキーセル C、ブロックウィング上の任意のキーセルCb、ラインウィング上の任意のキーセル Cr において、全ての C, Cb, Cr の組み合わせにおいて C, Cb, Cr の全てに含まれる数字の候補が存在せず、かつ
                                                // 3) 以下の数の合計が 9 に等しい場合。
                                                //   a) 交差部の各キーセルの数字の候補の和集合の要素の数
                                                //   b) ブロックウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                                //   b) ラインウィングの各キーセルの数字の候補の和集合から交差部の各キーセルの数字の候補の和集合を除いた集合の要素の数
                                                // 参考文献: https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/sdc_generalization/#same_extra_candidates
                                                if (intersectionKeyCellBitSet != 0
                                                    && blockWingKeyCellBitSet != 0
                                                    && lineWingKeyCellBitSet != 0
                                                    && (intersectionKeyDigitBitSet & blockWingKeyDigitBitSet & lineWingKeyDigitBitSet) == SudokuBoardCellNoteBitSet.None
                                                    && intersectionKeyDigitBitSet.GetPopCount() + (blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() + (lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).GetPopCount() == 9)
                                                {
                                                    // この時点でスー・デ・コック法の条件が成立している。

                                                    // ブロック内で排除可能な数字の候補とそのセル位置を求める
                                                    var excludedCellNotesInBlock =
                                                        SearchCellNotes(
                                                            ws,
                                                            blockHouseMask & ~(intersectionKeyCellBitSet | blockWingKeyCellBitSet),
                                                            blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet),
                                                            excludedCellNotesBuffer);

                                                    // ライン内で排除可能な数字の候補とそのセル位置を求める
                                                    var excludedCellNotesInLine =
                                                        SearchCellNotes(
                                                            ws,
                                                            lineHouseMask & ~(intersectionKeyCellBitSet | lineWingKeyCellBitSet),
                                                            lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet),
                                                            excludedCellNotesBuffer[excludedCellNotesInBlock.Length..]);

                                                    // 排除可能な数字の候補の存在を確認する
                                                    if (excludedCellNotesInBlock.Length > 0 || excludedCellNotesInLine.Length > 0)
                                                    {
                                                        // ブロックあるいはラインで排除可能な数字の候補が存在する場合
                                                        // => Su de Coq 法を適用する。

                                                        var excludedCellNotes =
                                                            excludedCellNotesInBlock.ToArray()
                                                            .Concat(excludedCellNotesInLine.ToArray());

                                                        var blockHouseName = blockHouseNameGetter();
                                                        var lineHouseName = lineHouseNameGetter();
                                                        var keyCellPositionsText =
                                                            string.Join(
                                                                ", ",
                                                                allKeyCellBitSet
                                                                .EnumerateAsCellPositionTexts());
                                                        var blockKeyCellPositionsText = string.Join(", ", blockKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                        var lineKeyCellPositionsText = string.Join(", ", lineKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                        var intersectionKeyCellPositionsText = string.Join(", ", intersectionKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                        var blockWingKeyCellPositionsText = string.Join(", ", blockWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                        var lineWingKeyCellPositionsText = string.Join(", ", lineWingKeyCellBitSet.EnumerateAsCellPositionTexts());
                                                        var blockOnlyKeyDigitsText = string.Join(", ", (blockWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~lineWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                        var lineOnlyKeyDigitsText = string.Join(", ", (lineWingKeyDigitBitSet | (intersectionKeyDigitBitSet & ~blockWingKeyDigitBitSet)).EnumerateAsCellDigitTexts());
                                                        var intersectionKeyDigitsText = string.Join(", ", intersectionKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                        var blockWingKeyDigitsText = string.Join(", ", blockWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                        var lineWingKeyDigitsText = string.Join(", ", lineWingKeyDigitBitSet.EnumerateAsCellDigitTexts());
                                                        var bothWingKeyigitsText = string.Join(", ", (blockWingKeyDigitBitSet & lineWingKeyDigitBitSet).EnumerateAsCellDigitTexts());
                                                        var allKeyDigitsText =
                                                            string.Join(
                                                                ", ",
                                                                intersectionKeyDigitBitSet.EnumerateAsCellDigits()
                                                                .Concat((blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                                .Concat((lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet).EnumerateAsCellDigits())
                                                                .OrderBy(digit => digit)
                                                                .Select(digit => digit.ToCellDigitText()));
                                                        var description =
                                                            $"{blockHouseName}と{lineHouseName}において、九つのマス ({keyCellPositionsText}) を選びます。\n" +
                                                            $"このうち、交差部 ({blockHouseName}と{lineHouseName}が重なっている部分) のマス ({intersectionKeyCellPositionsText}) の候補の数字は {intersectionKeyDigitsText} で、" +
                                                            $"ブロックウィング ({blockHouseName}のうち{lineHouseName}ではない部分) のマス ({blockWingKeyCellPositionsText}) の候補の数字は {blockWingKeyDigitsText}、" +
                                                            $"ラインウィング ({lineHouseName}のうち{blockHouseName}のマス ({lineWingKeyCellPositionsText}) の候補の数字は {lineWingKeyDigitsText} です。\n" +
                                                            $"{(
                                                                bothWingKeyigitsText.Length > 0
                                                                ? $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補 {bothWingKeyigitsText} が含まれていますが、これらの数字は交差部のマス {intersectionKeyCellPositionsText} の候補には含まれておらず、数字 {bothWingKeyigitsText} がどこに入るかはブロックウィングのマスとライトウィングのマスの間で互いに干渉することはないので便宜上は別々の数字として扱うことが出来、九つの数字 ({allKeyDigitsText}) は重複せず九つのマス ({keyCellPositionsText}) の何れかにしか入り得ないと言えます。\n"
                                                                : $"ブロックウィングのマス {blockWingKeyCellPositionsText} とラインウィングのマス {lineWingKeyCellPositionsText} には同一の数字の候補が存在しないため、九つの数字 ({allKeyDigitsText}) は重複せず九つのマス ({keyCellPositionsText}) の何れかにしか入り得ません。\n"
                                                            )}" +
                                                            $"したがって、" +
                                                            $"ラインウィング内のマス {lineWingKeyCellPositionsText} にしか出現しない数字の候補を除いた数字 ({blockOnlyKeyDigitsText}) を {blockKeyCellPositionsText} を除くブロック内のマスの数字の候補から排除し、" +
                                                            $"ラインウィング内のマス {blockWingKeyCellPositionsText} のマスにしか出現しない数字の候補を除いた数字 ({lineOnlyKeyDigitsText}) をライン内の {lineKeyCellPositionsText} 以外のマスの数字の候補から排除します。";
                                                        return
                                                            new SueDeCoqOperation(
                                                                excludedCellNotes,
                                                                intersectionKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType4)
                                                                    .Concat(blockWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType2))
                                                                    .Concat(lineWingKeyCellBitSet.EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.PositivelyHighlightedType3))
                                                                    .Concat((bothHouseMask & ~allKeyCellBitSet).EnumerateAsCellBackgroundEffects(SudokuBoardCellBackgroundEffect.NegativelyHighlighted)),
                                                                ws.EnumerateAsCellNoteEffects(blockKeyCellBitSet, blockWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType2)
                                                                    .Concat(ws.EnumerateAsCellNoteEffects(lineKeyCellBitSet, lineWingKeyDigitBitSet & ~intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType3))
                                                                    .Concat(ws.EnumerateAsCellNoteEffects(allKeyCellBitSet, ~blockWingKeyDigitBitSet & ~lineWingKeyDigitBitSet | intersectionKeyDigitBitSet, SudokuBoardCellNoteEffect.PositivelyHilightedType1)),
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
            }

            return null;
        }

        #endregion
    }
}
