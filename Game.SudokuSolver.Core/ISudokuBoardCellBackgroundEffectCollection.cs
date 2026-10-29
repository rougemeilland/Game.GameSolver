using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Game.GameSolver;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面のセルの背景表示効果のコレクションのインターフェースです。
    /// </summary>
    public interface ISudokuBoardCellBackgroundEffectCollection
        : IEnumerable<(SudokuBoardCellIndex cellIndex, SudokuBoardCellBackgroundEffect effect)>, IReadOnlyVectorIndexer<SudokuBoardCellBackgroundEffect, SudokuBoardCellIndex>
    {
        /// <summary>
        /// 指定した位置のセルの背景表示効果の取得を試みます。
        /// </summary>
        /// <param name="cellIndex">
        /// セルの位置を示す <see cref="SudokuBoardCellIndex"/> 値です。
        /// </param>
        /// <param name="effect">
        /// 復帰値が <see langword="true"/> の場合は、セルの背景表示効果を示す <see cref="SudokuBoardCellBackgroundEffect"/> 値を出力します。そうではない場合の出力値は規定されていません。
        /// </param>
        /// <returns>
        /// セルの背景表示効果に成功した場合は <see langword="true"/>、そうではない場合は <see langword="false"/> を返します。
        /// </returns>
        bool TryGetValue(SudokuBoardCellIndex cellIndex, [MaybeNullWhen(false)] out SudokuBoardCellBackgroundEffect effect);
    }
}
