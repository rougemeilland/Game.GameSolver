using System;
using Game.GameSolver;

namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardCellModel
        : Model
    {
        private sealed class CellNoteCollection
            : IReadOnlyCellNotesIndexer
        {
            private const int _SOURCE_LENGTH = SudokuBoardCellDigit.MaxValue - SudokuBoardCellDigit.MinValue + 1;
            private readonly SudokuBoardCellNoteModel[] _source;

            public CellNoteCollection(SudokuBoardCellNoteModel[] source)
            {
                if (source.Length != _SOURCE_LENGTH)
                    throw new ArgumentException($"The argument \"{nameof(source)}\" must be an array with {_SOURCE_LENGTH} elements.", nameof(source));

                _source = source;
            }

            public SudokuBoardCellNoteModel this[int index]
            {
                get
                {
                    ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _source.Length);
                    return _source[index];
                }
            }

            public SudokuBoardCellNoteModel this[SudokuBoardCellDigit index]
            {
                get
                {
                    if (index is < SudokuBoardCellDigit.MinValue or > SudokuBoardCellDigit.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _source[index - SudokuBoardCellDigit.MinValue];
                }
            }

            public int Length => _SOURCE_LENGTH;
        }

        private readonly CellNoteCollection _notes;
        private string _determinedDigit;
        private bool _isHighlightedBackground;
        private SudokuBoardCellShowMode _showMode;

        public SudokuBoardCellModel()
        {
            _determinedDigit = "";
            ShowMode = SudokuBoardCellShowMode.NotDetermined;
            _notes =
                new CellNoteCollection(
                [
                    new(){ CellNoteDidit = "1" },
                    new(){ CellNoteDidit = "2" },
                    new(){ CellNoteDidit = "3" },
                    new(){ CellNoteDidit = "4" },
                    new(){ CellNoteDidit = "5" },
                    new(){ CellNoteDidit = "6" },
                    new(){ CellNoteDidit = "7" },
                    new(){ CellNoteDidit = "8" },
                    new(){ CellNoteDidit = "9" },
                ]);
        }

        public string DeterminedDigit
        {
            get => ShowMode == SudokuBoardCellShowMode.NotDetermined ? "" : _determinedDigit;
            set
            {
                if (value is null || value.Length != 1 || value[0] is < '1' or > '9')
                {
                    RaisePropertyChangedEvent(nameof(DeterminedDigit));
                    return;
                }

                if (value == _determinedDigit)
                    return;

                _determinedDigit = value;
                RaisePropertyChangedEvent(nameof(DeterminedDigit));
            }
        }

        public bool IsHighlightedBackground
        {
            get => _isHighlightedBackground;
            set
            {
                if (value == _isHighlightedBackground)
                    return;
                _isHighlightedBackground = value;
                RaisePropertyChangedEvent(nameof(IsHighlightedBackground));
            }
        }

        public SudokuBoardCellShowMode ShowMode
        {
            get => _showMode;
            set
            {
                if (value == _showMode)
                    return;
                _showMode = value;
                RaisePropertyChangedEvent(nameof(ShowMode));
            }
        }

        public IReadOnlyCellNotesIndexer Notes => _notes;
    }
}
