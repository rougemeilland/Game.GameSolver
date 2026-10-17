using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// セルの数字の候補の表示方法を示す構造体です。
    /// </summary>
    public readonly struct SudokuBoardCellNote
    {
        [Flags]
        private enum CellNoteStatus
            : byte
        {
            None = 0,
            Enabled = 1 << 0,
            PositivelyHighlighted = 1 << 1,
            NegativelyHighlighted = 1 << 2,
        }

        private readonly CellNoteStatus _status;

        private SudokuBoardCellNote(CellNoteStatus status)
        {
            _status = status;
        }

        /// <summary>
        /// セルの数字の候補を表示すべきかどうかの値を取得します。
        /// </summary>
        /// <value>
        /// セルの数字の候補を表示するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        /// </value>
        public bool IsEnabled => (_status & CellNoteStatus.Enabled) != CellNoteStatus.None;

        /// <summary>
        /// セルの数字の候補の表示を肯定的に強調すべきかどうかの値を取得します。
        /// </summary>
        /// セルの数字の候補の表示を肯定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        public bool IsPositivelyHighlighted => (_status & CellNoteStatus.PositivelyHighlighted) != CellNoteStatus.None;

        /// <summary>
        /// セルの数字の候補の表示を否定的に強調すべきかどうかの値を取得します。
        /// </summary>
        /// セルの数字の候補の表示を否定的に強調するならば <see langword="true"/>、そうではないならば <see langword="false"/> です。
        public bool IsNegativelyHighlighted => (_status & CellNoteStatus.NegativelyHighlighted) != CellNoteStatus.None;

        internal static SudokuBoardCellNote CreateHiddenNote() => new(CellNoteStatus.None);
        internal static SudokuBoardCellNote CreateVisibleNote() => new(CellNoteStatus.Enabled);
        internal static SudokuBoardCellNote CreatePositivelyHighlightedNote() => new(CellNoteStatus.Enabled | CellNoteStatus.PositivelyHighlighted);
        internal static SudokuBoardCellNote CreateNegativelyHighlightedNote() => new(CellNoteStatus.Enabled | CellNoteStatus.NegativelyHighlighted);

    }
}
