using System;

namespace Game.SudokuSolver
{
    /// <summary>
    /// 数独の盤面の数字が確定していないセルのクラスです。
    /// </summary>
    public class SudokuBoardNotDeterminedCell
        : SudokuBoardCell
    {
        private sealed class CellNoteCollection
            : IReadOnlySudokuBoardCellNoteCollection
        {
            private readonly SudokuBoardCellNote[] _notes;

            public CellNoteCollection(SudokuBoardCellNote[] notes)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(notes is not null && notes.Length == SudokuBoardCellDigit.MaxValue - SudokuBoardCellDigit.MinValue + 1);
#endif
                _notes = notes;
            }

            public SudokuBoardCellNote this[SudokuBoardCellDigit cellNoteDigit]
            {
                get
                {
#if DEBUG
                    if (cellNoteDigit is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(cellNoteDigit));
#endif

                    return _notes[cellNoteDigit - SudokuBoardCellDigit.MinValue];
                }
            }

            public int Length => _notes.Length;
        }

        internal SudokuBoardNotDeterminedCell(
            SudokuBoardCellBackgroundEffect backgroundEffect,
            Func<SudokuBoardCellDigit, SudokuBoardCellNoteEffect> cellNoteEffectGetter)
            : base(backgroundEffect)
        {
            var notes = new SudokuBoardCellNote[SudokuBoardCellDigit.MaxValue - SudokuBoardCellDigit.MinValue + 1];
            for (var cellNoteDigit = SudokuBoardCellDigit.MinValue; cellNoteDigit <= SudokuBoardCellDigit.MaxValue; ++cellNoteDigit)
            {
                var effect = cellNoteEffectGetter(cellNoteDigit);
                notes[cellNoteDigit - SudokuBoardCellDigit.MinValue] = new SudokuBoardCellNote(effect);
            }

            Notes = new CellNoteCollection(notes);
        }

        /// <summary>
        /// セルの数字の候補のコレクションを取得します。
        /// </summary>
        /// <value>
        /// セルの数字の候補のコレクションにアクセスするための <see cref="IReadOnlySudokuBoardCellNoteCollection"/> インターフェースです。
        /// </value>
        public IReadOnlySudokuBoardCellNoteCollection Notes { get; }
    }
}
