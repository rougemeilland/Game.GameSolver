using System;

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
        private SudokuBoardCellBackgroundEffect _backgroundEffect;
        private SudokuBoardCellForegroundEffect _foregroundEffect;

        public SudokuBoardCellModel()
        {
            _determinedDigit = "";
            ForegroundEffect = SudokuBoardCellForegroundEffect.NotDetermined;
            _notes =
                new CellNoteCollection(
                [
                    new(){ CellNoteDigit = "1" },
                    new(){ CellNoteDigit = "2" },
                    new(){ CellNoteDigit = "3" },
                    new(){ CellNoteDigit = "4" },
                    new(){ CellNoteDigit = "5" },
                    new(){ CellNoteDigit = "6" },
                    new(){ CellNoteDigit = "7" },
                    new(){ CellNoteDigit = "8" },
                    new(){ CellNoteDigit = "9" },
                ]);
        }

        public string DeterminedDigit
        {
            get => ForegroundEffect == SudokuBoardCellForegroundEffect.NotDetermined ? "" : _determinedDigit;
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

        public IReadOnlyCellNotesIndexer Notes => _notes;

        public SudokuBoardCellBackgroundEffect BackgroundEffect
        {
            get => _backgroundEffect;
            set
            {
                if (_backgroundEffect == value)
                    return;
                _backgroundEffect = value;
                RaisePropertyChangedEvent(nameof(BackgroundEffect));
            }
        }

        public SudokuBoardCellForegroundEffect ForegroundEffect
        {
            get => _foregroundEffect;
            set
            {
                if (value == _foregroundEffect)
                    return;
                _foregroundEffect = value;
                RaisePropertyChangedEvent(nameof(ForegroundEffect));
            }
        }
    }
}
