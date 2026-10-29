using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの数字の候補の表示効果のコレクションを示すインターフェースです。
    /// </summary>
    public interface ISudokuBoardCellNoteEffectCollection
        : IEnumerable<(SudokuBoardCellPositionDigit positionDigit, SudokuBoardCellNoteEffect effect)>, IReadOnlyVectorIndexer<SudokuBoardCellNoteEffect, SudokuBoardCellPositionDigit>
    {
        /// <summary>
        /// 指定したセルの数字の候補の表示効果の取得を試みます。
        /// </summary>
        /// <param name="positionDigit">
        /// セルの位置と数字の候補を示す <see cref="SudokuBoardCellPositionDigit"/> 値です。
        /// </param>
        /// <param name="effect">
        /// 復帰値が <see langword="true"/> の場合は、セルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値を出力します。そうではない場合の出力値は規定されていません。
        /// </param>
        /// <returns>
        /// セルの数字の候補の表示効果の取得に成功した場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// </returns>
        bool TryGetValue(SudokuBoardCellPositionDigit positionDigit, [MaybeNullWhen(false)] out SudokuBoardCellNoteEffect effect);

        /// <summary>
        /// 指定されたセルと指定された数字に対応する数字の候補の表示効果を取得します。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="cellNoteDigit">
        /// セルの数字の候補を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <returns>
        /// セルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値です。
        /// </returns>
        SudokuBoardCellNoteEffect this[SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellNoteDigit] { get; }

        /// <summary>
        /// 指定したセルの指定した数字の候補の表示効果の取得を試みます。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="cellNoteDigit">
        /// セルの数字の候補を示す <see cref="SudokuBoardCellDigit"/> 値です。
        /// </param>
        /// <param name="effect">
        /// 復帰値が <see langword="true"/> の場合は、セルの数字の候補の表示効果を示す <see cref="SudokuBoardCellNoteEffect"/> 値を出力します。そうではない場合の出力値は規定されていません。
        /// </param>
        /// <returns>
        /// セルの数字の候補の表示効果の取得に成功した場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// </returns>
        bool TryGetValue(SudokuBoardCellIndex cellIndex, SudokuBoardCellDigit cellNoteDigit, [MaybeNullWhen(false)] out SudokuBoardCellNoteEffect effect);
    }
}
