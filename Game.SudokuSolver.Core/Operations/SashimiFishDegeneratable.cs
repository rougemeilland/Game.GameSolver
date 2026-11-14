//#define __DO_SELF_TEST
using System;
#if __DO_SELF_TEST
using System.Linq;
#endif

namespace Game.SudokuSolver.Operations
{
    /// <summary>
    /// 退化可能なフィッシュの抽象クラスです。
    /// </summary>
    internal abstract class SashimiFishDegeneratable
    {
        /// <summary>
        /// フィッシュの退化によりハウス内のセルの位置が確定したことを示すよう書式化された文字列を返すデリゲートです。
        /// </summary>
        /// <param name="houseName">
        /// 確定の根拠となったハウスの名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="effectedHouseName">
        /// 確定の結果影響を及ぼすハウスの名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="cellPositionText">
        /// 確定したセルの位置の文字列を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// フィッシュの退化によりハウス内のセルの位置が確定したことを示すよう書式化された文字列である <see cref="string"/> オブジェクトを返します。
        /// </returns>
        public delegate string CellDeterminedMessageFormatter(string houseName, string effectedHouseName, string cellPositionText);

        static SashimiFishDegeneratable()
        {
#if DEBUG && __DO_SELF_TEST
            SelfTest();
#endif
        }

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        protected SashimiFishDegeneratable()
        {
        }

        /// <summary>
        /// フィッシュの退化過程の分析を試みます。
        /// </summary>
        /// <param name="baseLineCellDeterminedMessageFormatter">
        /// ベースライン上で退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <param name="coverLineLineCellDeterminedMessageFormatter">
        /// カバーライン上で退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <returns>
        /// 以下の値のタプルを返します。
        /// <list type="bullet">
        /// <item>
        /// <term>degenerationMessages</term>
        /// <description>フィッシュの退化過程を示す文字列である <see cref="string"/> オブジェクトの配列です。フィッシュが退化していない場合はこの配列の長さは 0 です。</description>
        /// </item>
        /// <item>
        /// <term>undegeneratedBaseLineHouseNames</term>
        /// <description>退化せずに残ったベースラインのハウス名を示す <see cref="string"/> オブジェクトの配列です。</description>
        /// </item>
        /// <item>
        /// <term>undegeneratedCoverLineHouseNames</term>
        /// <description>退化せずに残ったカバーラインのハウス名を示す <see cref="string"/> オブジェクトの配列です。</description>
        /// </item>
        /// <item>
        /// <term>determinedCellIndexBitSet</term>
        /// <description>退化の過程で確定したセルの位置のビット集合を示す <see cref="UInt128"/> 値です。フィッシュが退化していない場合はこの値は <see cref="UInt128.Zero"/> です。</description>
        /// </item>
        /// </list>
        /// </returns>

        public (ReadOnlyMemory<string> degenerationMessages, ReadOnlyMemory<string> undegeneratedBaseLineHouseNames, ReadOnlyMemory<string> undegeneratedCoverLineHouseNames, UInt128 determinedCellIndexBitSet) Degenerate(
            CellDeterminedMessageFormatter baseLineCellDeterminedMessageFormatter,
            CellDeterminedMessageFormatter coverLineLineCellDeterminedMessageFormatter)
        {
            while (DegenerateOneStep(baseLineCellDeterminedMessageFormatter, coverLineLineCellDeterminedMessageFormatter))
            {
            }

            return Finalize();
        }

        /// <summary>
        /// 列ハウスで検出された退化の内容を書式化するデリゲートです。
        /// </summary>
        /// <param name="column">
        /// 退化を検出した列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <param name="effectedRow">
        /// 退化の結果影響が及ぶ行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <param name="cellIndex">
        /// 退化の原因となったセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 列ハウスで検出された退化の内容が書式化された文字列である <see cref="string"/> を返します。
        /// </returns>
        protected delegate string ColumnDeterminedCellMessageFormatter(SudokuBoardCellColumn column, SudokuBoardCellRow effectedRow, SudokuBoardCellIndex cellIndex);

        /// <summary>
        /// 行ハウスで検出された退化の内容を書式化するデリゲートです。
        /// </summary>
        /// <param name="row">
        /// 退化を検出した行を示す <see cref="SudokuBoardCellRow"/> 値です。
        /// </param>
        /// <param name="effectedColumn">
        /// 退化の結果影響が及ぶ列を示す <see cref="SudokuBoardCellColumn"/> 値です。
        /// </param>
        /// <param name="cellIndex">
        /// 退化の原因となったセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <returns>
        /// 列ハウスで検出された退化の内容が書式化された文字列である <see cref="string"/> を返します。
        /// </returns>
        protected delegate string RowDeterminedCellMessageFormatter(SudokuBoardCellRow row, SudokuBoardCellColumn effectedColumn, SudokuBoardCellIndex cellIndex);

        /// <summary>
        /// フィッシュの退化を一段階だけ進めようと試みます。
        /// </summary>
        /// <param name="baseLineCellDeterminedMessageFormatter">
        /// ベースライン上で退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <param name="coverLineLineCellDeterminedMessageFormatter">
        /// カバーライン上で退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <returns>
        /// 退化が行われた場合は <see langword="true"/>、そうでない場合は <see langword="false"/> を返します。
        /// </returns>
        protected abstract bool DegenerateOneStep(
            CellDeterminedMessageFormatter baseLineCellDeterminedMessageFormatter,
            CellDeterminedMessageFormatter coverLineLineCellDeterminedMessageFormatter);

        /// <summary>
        /// フィッシュの退化処理を終了します。
        /// </summary>
        /// <returns>
        /// 以下の値のタプルを返します。
        /// <list type="bullet">
        /// <item>
        /// <term>degenerationMessages</term>
        /// <description>フィッシュの退化過程を示す文字列である <see cref="string"/> オブジェクトの配列です。フィッシュが退化していない場合はこの配列の長さは 0 です。</description>
        /// </item>
        /// <item>
        /// <term>undegeneratedBaseLineHouseNames</term>
        /// <description>退化せずに残ったベースラインのハウス名を示す <see cref="string"/> オブジェクトの配列です。</description>
        /// </item>
        /// <item>
        /// <term>undegeneratedCoverLineHouseNames</term>
        /// <description>退化せずに残ったカバーラインのハウス名を示す <see cref="string"/> オブジェクトの配列です。</description>
        /// </item>
        /// <item>
        /// <term>determinedCellIndexBitSet</term>
        /// <description>退化の過程で確定したセルの位置のビット集合を示す <see cref="UInt128"/> 値です。フィッシュが退化していない場合はこの値は <see cref="UInt128.Zero"/> です。</description>
        /// </item>
        /// </list>
        /// </returns>
        protected abstract (ReadOnlyMemory<string> degenerationMessages, ReadOnlyMemory<string> undegeneratedBaseLineHouseNames, ReadOnlyMemory<string> undegeneratedCoverLineHouseNames, UInt128 determinedCellIndexBitSet) Finalize();

        /// <summary>
        /// 列ハウスでの退化の検出を試みます。
        /// </summary>
        /// <param name="keyCellIndexBitSet">
        /// フィッシュの対象の数字が存在するすべての交差点のセルの位置の集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="columnBitSet">
        /// フィッシュの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <param name="rowBitSet">
        /// フィッシュの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <param name="determinedCellIndexBitSet">
        /// 退化の過程で確定したセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="columnDeterminedCellMessageFormatter">
        /// 退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <returns>
        /// <param>
        /// 退化が行われた場合は、<paramref name="keyCellIndexBitSet"/>, <paramref name="columnBitSet"/>, <paramref name="rowBitSet"/>, <paramref name="determinedCellIndexBitSet"/> の更新された値と退化の過程を示す文字列のタプルを返します。
        /// </param>
        /// <para>
        /// そうではない場合は <see langword="null"/> を返します。
        /// </para>
        /// </returns>
        protected static (UInt128 keyCellIndexBitSet, SudokuBoardCellColumnBitSet undeterminedColumnBitSet, SudokuBoardCellRowBitSet undeterminedRowBitSet, UInt128 determinedCellIndexBitSet, string message)? DegenerateOneStepByColumn(
            UInt128 keyCellIndexBitSet,
            SudokuBoardCellColumnBitSet columnBitSet,
            SudokuBoardCellRowBitSet rowBitSet,
            UInt128 determinedCellIndexBitSet,
            ColumnDeterminedCellMessageFormatter columnDeterminedCellMessageFormatter)
        {
            if (!ExistSingleBitColumn(keyCellIndexBitSet, columnBitSet, out var singleBitColumn, out var singleBitColumnCellIndex))
                return null;
            var singleBitRow = SudokuBoardCellGeometry.GetRow(singleBitColumnCellIndex);
            columnBitSet &= ~singleBitColumn.ToCellColumnBitSet();
            rowBitSet &= ~singleBitRow.ToCellRowBitSet();
            keyCellIndexBitSet &= ~SudokuBoardCellGeometry.GetColumnCellIndexBitSet(singleBitColumn);
            keyCellIndexBitSet &= ~SudokuBoardCellGeometry.GetRowCellIndexBitSet(singleBitRow);
            determinedCellIndexBitSet |= singleBitColumnCellIndex.ToCellIndexBitSet();
            return (keyCellIndexBitSet, columnBitSet, rowBitSet, determinedCellIndexBitSet, columnDeterminedCellMessageFormatter(singleBitColumn, singleBitRow, singleBitColumnCellIndex));
        }

        /// <summary>
        /// 行ハウスでの退化の検出を試みます。
        /// </summary>
        /// <param name="keyCellIndexBitSet">
        /// フィッシュの対象の数字が存在するすべての交差点のセルの位置の集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="rowBitSet">
        /// フィッシュの行のビット集合を示す <see cref="SudokuBoardCellRowBitSet"/> 値です。
        /// </param>
        /// <param name="columnBitSet">
        /// フィッシュの列のビット集合を示す <see cref="SudokuBoardCellColumnBitSet"/> 値です。
        /// </param>
        /// <param name="determinedCellIndexBitSet">
        /// 退化の過程で確定したセルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="rowDeterminedCellMessageFormatter">
        /// 退化を検出した際のメッセージを書式化するデリゲートです。
        /// </param>
        /// <returns>
        /// <param>
        /// 退化が行われた場合は、<paramref name="keyCellIndexBitSet"/>, <paramref name="columnBitSet"/>, <paramref name="rowBitSet"/>, <paramref name="determinedCellIndexBitSet"/> の更新された値と退化の過程を示す文字列のタプルを返します。
        /// </param>
        /// <para>
        /// そうではない場合は <see langword="null"/> を返します。
        /// </para>
        /// </returns>
        protected static (UInt128 keyCellIndexBitSet, SudokuBoardCellRowBitSet undeterminedRowBitSet, SudokuBoardCellColumnBitSet undeterminedColumnBitSet, UInt128 determinedCellIndexBitSet, string message)? DegenerateOneStepByRow(
            UInt128 keyCellIndexBitSet,
            SudokuBoardCellRowBitSet rowBitSet,
            SudokuBoardCellColumnBitSet columnBitSet,
            UInt128 determinedCellIndexBitSet,
            RowDeterminedCellMessageFormatter rowDeterminedCellMessageFormatter)
        {
            if (!ExistSingleBitRow(keyCellIndexBitSet, rowBitSet, out var singleBitRow, out var singleBitRowCellIndex))
                return null;
            var singleBitColumn = SudokuBoardCellGeometry.GetColumn(singleBitRowCellIndex);
            columnBitSet &= ~singleBitColumn.ToCellColumnBitSet();
            rowBitSet &= ~singleBitRow.ToCellRowBitSet();
            keyCellIndexBitSet &= ~SudokuBoardCellGeometry.GetColumnCellIndexBitSet(singleBitColumn);
            keyCellIndexBitSet &= ~SudokuBoardCellGeometry.GetRowCellIndexBitSet(singleBitRow);
            determinedCellIndexBitSet |= singleBitRowCellIndex.ToCellIndexBitSet();
            return (keyCellIndexBitSet, rowBitSet, columnBitSet, determinedCellIndexBitSet, rowDeterminedCellMessageFormatter(singleBitRow, singleBitColumn, singleBitRowCellIndex));
        }

        private static bool ExistSingleBitColumn(UInt128 keyCellIndexBitSet, SudokuBoardCellColumnBitSet columnBitSet, out SudokuBoardCellColumn singleBitColumn, out SudokuBoardCellIndex singleBitColumnCellIndex)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(columnBitSet != SudokuBoardCellColumnBitSet.None);
#endif
            do
            {
                singleBitColumn = columnBitSet.GetOneCellColumn();
                var cellBitSet = SudokuBoardCellGeometry.GetColumnCellIndexBitSet(singleBitColumn) & keyCellIndexBitSet;
#if DEBUG
                System.Diagnostics.Debug.Assert(cellBitSet != 0);
#endif
                if (UInt128.IsPow2(cellBitSet))
                {
                    singleBitColumnCellIndex = cellBitSet.AsSingleCellIndex();
                    return true;
                }

                columnBitSet &= ~singleBitColumn.ToCellColumnBitSet();
            }
            while (columnBitSet != SudokuBoardCellColumnBitSet.None);
            singleBitColumnCellIndex = SudokuBoardCellIndex.Undefined;
            return false;
        }

        private static bool ExistSingleBitRow(UInt128 keyCellIndexBitSet, SudokuBoardCellRowBitSet rowBitSet, out SudokuBoardCellRow singleBitRow, out SudokuBoardCellIndex singleBitRowCellIndex)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(rowBitSet != SudokuBoardCellRowBitSet.None);
#endif
            do
            {
                singleBitRow = rowBitSet.GetOneCellRow();
                var cellBitSet = SudokuBoardCellGeometry.GetRowCellIndexBitSet(singleBitRow) & keyCellIndexBitSet;
#if DEBUG
                System.Diagnostics.Debug.Assert(cellBitSet != 0);
#endif
                if (UInt128.IsPow2(cellBitSet))
                {
                    singleBitRowCellIndex = cellBitSet.AsSingleCellIndex();
                    return true;
                }

                rowBitSet &= ~singleBitRow.ToCellRowBitSet();
            }
            while (rowBitSet != SudokuBoardCellRowBitSet.None);
            singleBitRowCellIndex = SudokuBoardCellIndex.Undefined;
            return false;
        }

#if DEBUG && __DO_SELF_TEST
        private static void SelfTest()
        {
            {
                // ベースラインが列の場合
                PrintDegenerationByColumn(
                    "通常の X-Wing",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "         " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "通常のソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "通常のジェリーフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *   * *" +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "一部欠けた X-Wing",
                    "         " +
                    "  *      " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "         " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "列方向に退化可能なソードフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "行方向に退化可能なソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *      " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "列方向に二段退化可能なソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    *    " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "行方向に二段退化可能なソードフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *      " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "列方向に退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   * *" +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "行方向に退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    * * *" +
                    "  * *   *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "列方向に二段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "行方向に二段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "列方向に三段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   * *" +
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByColumn(
                    "行方向に三段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  *     *" +
                    "         " +
                    "  * * * *" +
                    "  * *   *" +
                    "         " +
                    "         ");

            }

            {
                // ベースラインが行の場合
                PrintDegenerationByRow(
                    "通常の X-Wing",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "         " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "通常のソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "通常のジェリーフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *   * *" +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "一部欠けた X-Wing",
                    "         " +
                    "  *      " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "         " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "列方向に退化可能なソードフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "行方向に退化可能なソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "  *      " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "列方向に二段退化可能なソードフィッシュ",
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    *    " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "行方向に二段退化可能なソードフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *      " +
                    "         " +
                    "    * *  " +
                    "         " +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "列方向に退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   * *" +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "行方向に退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    * * *" +
                    "  * *   *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "列方向に二段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   *  " +
                    "         " +
                    "  *   *  " +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "行方向に二段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * *  " +
                    "         " +
                    "    * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "列方向に三段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *   * *" +
                    "         " +
                    "  *      " +
                    "         " +
                    "  * * * *" +
                    "  *     *" +
                    "         " +
                    "         ");

                PrintDegenerationByRow(
                    "行方向に三段退化可能なジェリーフィッシュ",
                    "         " +
                    "  *      " +
                    "         " +
                    "  *     *" +
                    "         " +
                    "  * * * *" +
                    "  * *   *" +
                    "         " +
                    "         ");

            }

            static void PrintDegenerationByColumn(string title, string source)
            {

                var keyCellIndexBitSet = source.ToCellIndexBitSet();
                keyCellIndexBitSet.PrintToDebugConsole($"退化 - ベースラインが列の場合 - {title}");
                System.Diagnostics.Debug.WriteLine($"説明:\n{GetDegenerationMessage(new SashimiFishDegeneratableByColumn(keyCellIndexBitSet), "行", "'5'")}");
                System.Diagnostics.Debug.WriteLine("");
            }

            static void PrintDegenerationByRow(string title, string source)
            {
                var keyCellIndexBitSet = source.ToCellIndexBitSet();
                keyCellIndexBitSet.PrintToDebugConsole($"退化 - ベースラインが行の場合 - {title}");
                System.Diagnostics.Debug.WriteLine($"説明:\n{GetDegenerationMessage(new SashimiFishDegeneratableByRow(keyCellIndexBitSet), "列", "'5'")}");
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        private static string GetDegenerationMessage(SashimiFishDegeneratable degeneratablwFish, string coverLineHouseTypeName, string digitText)
        {
            var (degenerationMessages, undegeneratedBaseLineHouseNames, undegeneratedCoverLineHouseNames, determinedCellIndexBitSet) =
                degeneratablwFish.Degenerate(
                    BaseLineFormatter(digitText),
                    CoverLineFormatter(digitText, coverLineHouseTypeName));
#if DEBUG
            System.Diagnostics.Debug.Assert(undegeneratedBaseLineHouseNames.Length == undegeneratedCoverLineHouseNames.Length);
#pragma warning disable IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)
            System.Diagnostics.Debug.Assert((determinedCellIndexBitSet != UInt128.Zero) == (degenerationMessages.Length > 0));
#pragma warning restore IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)

#endif
            if (determinedCellIndexBitSet == UInt128.Zero)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(degenerationMessages.Length <= 0);
#endif
                var undegeneratedBaseLineHouseNamesText = string.Join(", ", undegeneratedBaseLineHouseNames.AsEnumerable());
                var undegeneratedCoverLineHouseNamesText = string.Join(", ", undegeneratedCoverLineHouseNames.AsEnumerable());
                return
                    $"{undegeneratedBaseLineHouseNamesText} と {undegeneratedCoverLineHouseNamesText} で" +
                    $"{undegeneratedBaseLineHouseNames.Length switch
                    {
                        2 => " X-Wing ",
                        3 => "ソードフィッシュ",
                        4 => "ジェリーフィッシュ",
                        _ => throw new ApplicationException(),

                    }}が成立するので、{undegeneratedCoverLineHouseNamesText} の各{coverLineHouseTypeName}の {undegeneratedBaseLineHouseNamesText} を除くマスには {digitText} は入り得ません。";
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(degenerationMessages.Length > 0);
#endif
                var undegeneratedBaseLineHouseNamesText = string.Join(", ", undegeneratedBaseLineHouseNames.AsEnumerable());
                var undegeneratedCoverLineHouseNamesText = string.Join(", ", undegeneratedCoverLineHouseNames.AsEnumerable());
                var messages = degenerationMessages.AsEnumerable();
                if (undegeneratedBaseLineHouseNames.Length > 0)
                {
                    messages =
                        messages.Append(
                            $"残った {undegeneratedBaseLineHouseNamesText} と {undegeneratedCoverLineHouseNamesText} で" +
                            $"{undegeneratedBaseLineHouseNames.Length switch
                            {
                                2 => " X-Wing ",
                                3 => "ソードフィッシュ",
                                4 => "ジェリーフィッシュ",
                                _ => throw new ApplicationException(),

                            }}が成立するので、{undegeneratedCoverLineHouseNamesText} の各{coverLineHouseTypeName}の {undegeneratedBaseLineHouseNamesText} を除くマスには {digitText} は入り得ません。");
                }

                return
                    string.Join(
                        "\n",
                        messages
                        .Zip(Enumerable.Range(1, int.MaxValue - 1), (message, n) => $"  {n}) {message}"));
            }
        }

        private static CellDeterminedMessageFormatter BaseLineFormatter(string digitText)
            => (baseHouseName, effectedHouseName, cellPositionText)
                => $"{baseHouseName}で {digitText} が入り得るマスは{cellPositionText}だけなので{cellPositionText}に {digitText} が入ります。" +
                    $"したがって、{effectedHouseName}の{cellPositionText}以外のマスに {digitText} は入り得ません。";

        private static CellDeterminedMessageFormatter CoverLineFormatter(string digitText, string coverLineHouseTypeName)
            => (coverHouseName, effectedHouseName, cellPositionText)
                => $"{cellPositionText}に {digitText} が入らないと仮定すると{coverHouseName}以外の{coverLineHouseTypeName}で {digitText} が入り得るマスが足りなくなるので{cellPositionText}には {digitText} が入ります。" +
                    $"したがって、{effectedHouseName}の{cellPositionText}以外のマスに {digitText} は入り得ません。";
#endif
    }
}
