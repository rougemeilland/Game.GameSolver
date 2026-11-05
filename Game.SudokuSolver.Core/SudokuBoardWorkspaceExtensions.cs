using System;
using System.Collections.Generic;

namespace Game.SudokuSolver
{
    internal static class SudokuBoardWorkspaceExtensions
    {
        /// <summary>
        /// 指定されたセルの位置の配列から n 国同盟のメンバーになり得るセルの位置を抽出します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="countOfAllianceMembers">
        /// 同盟のメンバー数です。
        /// </param>
        /// <param name="sourceCellsIndexes">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値の配列です。
        /// </param>
        /// <param name="allianceMemberCellsBuffer">
        /// 同盟の候補になり得るセルの位置を <see cref="SudokuBoardCellIndex"/> 値を格納するためのバッファです。
        /// </param>
        /// <returns>
        /// <paramref name="allianceMemberCellsBuffer"/> に格納された実際の要素の範囲を示す <see cref="ReadOnlyMemory{T}"/> 値です。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="allianceMemberCellsBuffer"/> に格納される要素は以下の条件を満たしていますが、確実に n 国同盟のメンバーであるとは限らないことに注意してください。
        /// </para>
        /// <para>
        /// <list type="number">
        /// <item>未確定のセルであり、かつ</item>
        /// <item>数字の候補の数が <paramref name="countOfAllianceMembers"/> で指定された値以下であるセル。</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static ReadOnlyMemory<SudokuBoardCellIndex> ExtractMembersOfAlliance(this SudokuBoardWorkspace ws, int countOfAllianceMembers, ReadOnlyMemory<SudokuBoardCellIndex> sourceCellsIndexes, Memory<SudokuBoardCellIndex> allianceMemberCellsBuffer)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(allianceMemberCellsBuffer.Length >= sourceCellsIndexes.Length);
#endif
            var sourceBuffer = sourceCellsIndexes.Span;
            var destinationBuffer = allianceMemberCellsBuffer.Span;
            var length = 0;
            for (var index = 0; index < sourceCellsIndexes.Length; ++index)
            {
                if (!ws.IsDetermined(sourceBuffer[index]) && ws.GetCellNoteDigitBitSet(sourceBuffer[index]).GetPopCount() <= countOfAllianceMembers)
                    destinationBuffer[length++] = sourceBuffer[index];
            }

            return allianceMemberCellsBuffer[..length];
        }

        /// <summary>
        /// 指定されたセル位置のビット集合と数字のマスクからセルの文字の数字の候補を抽出して、セルの数字の候補の表示効果とのペアのシーケンスを取得します。
        /// </summary>
        /// <param name="ws">
        /// 盤面の状態を示す <see cref="SudokuBoardWorkspace"/> オブジェクトです。
        /// </param>
        /// <param name="cellIndexBitSet">
        /// セルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="cellNoteBitMask">
        /// セルの数字の候補を抽出するためのビットマスクを示す <see cref="SudokuBoardCellDigitBitSet"/> 値です。
        /// </param>
        /// <param name="effect"></param>
        /// セルの数字の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値です。
        /// <returns>
        /// セルの数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値とセルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> のペアのシーケンスを返します。
        /// </returns>
        internal static IEnumerable<(SudokuBoardCellPositionDigit cellNote, SudokuBoardCellNoteEffect effect)> EnumerateAsCellNoteEffects(this SudokuBoardWorkspace ws, UInt128 cellIndexBitSet, SudokuBoardCellDigitBitSet cellNoteBitMask, SudokuBoardCellNoteEffect effect)
        {
            while (cellIndexBitSet != 0)
            {
                var cellIndex = cellIndexBitSet.GetOneCellIndex();
#if DEBUG
                System.Diagnostics.Debug.Assert(!ws.IsDetermined(cellIndex));
#endif
                var cellNoteDigitBitSet = ws.GetCellNoteDigitBitSet(cellIndex) & cellNoteBitMask;
                while (cellNoteDigitBitSet != 0)
                {
                    var cellNoteDigit = cellNoteDigitBitSet.GetOneCellDigit();
                    yield return (new SudokuBoardCellPositionDigit(cellIndex, cellNoteDigit), effect);

                    cellNoteDigitBitSet &= ~cellNoteDigit.ToCellDigitBitSet();
                }

                cellIndexBitSet &= ~cellIndex.ToCellIndexBitSet();
            }
        }
    }
}
