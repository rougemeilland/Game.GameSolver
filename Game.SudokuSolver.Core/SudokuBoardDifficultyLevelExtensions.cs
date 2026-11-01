using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// <see cref="SudokuBoardDifficultyLevel"/> 型の拡張メソッドのクラスです。
    /// </summary>
    public static class SudokuBoardDifficultyLevelExtensions
    {
        /// <summary>
        /// 数独のオペレーションの難易度を読みやすい文字列に変換します。
        /// </summary>
        /// <param name="difficulty">
        /// 数独のオペレーションの難易度を示す <see cref="SudokuBoardDifficultyLevel"/> 値です。
        /// </param>
        /// <returns>
        /// 数独のオペレーションの難易度を示す <see cref="string"/> オブジェクトです。
        /// </returns>
        public static string ToFriendlyString(this SudokuBoardDifficultyLevel difficulty)
            => difficulty switch
            {
                SudokuBoardDifficultyLevel.Beginner => "★ 入門",
                SudokuBoardDifficultyLevel.Easy => "★★ 初級",
                SudokuBoardDifficultyLevel.Medium => "★★★ 中級",
                SudokuBoardDifficultyLevel.Hard => "★★★★ 上級",
                SudokuBoardDifficultyLevel.Extreme => "★★★★★ 超上級",
                SudokuBoardDifficultyLevel.Insane => "★★★★★★ 究極",
                SudokuBoardDifficultyLevel.Nonstandard => "(規格外)",
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty)),
            };
    }
}
