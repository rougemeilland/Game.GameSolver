using System;
using System.Security.AccessControl;
using System.Windows;
using Game.GameSolver;

namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardModel
        : Model
    {
        private sealed class CellCollection
            : IReadOnlyCellsIndexer
        {
            private readonly SudokuBoardCellModel[] _source;

            public CellCollection(SudokuBoardCellModel[] source)
            {
                _source = source;
            }

            public SudokuBoardCellModel this[int index]
            {
                get
                {
                    if (index is < 0 or > SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _source[index];
                }
            }

            public SudokuBoardCellModel this[SudokuBoardCellIndex index]
            {
                get
                {
                    if (index is < SudokuBoardCellIndex.MinValue or > SudokuBoardCellIndex.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _source[index - SudokuBoardCellIndex.MinValue];
                }
            }

            public int Length => throw new NotImplementedException();
        }

        private readonly SudokuBoardCellModel[] _cells;
        private bool _showOPeration;
        private string _operationName;
        private Visibility _operationNameVisibility;
        private string _description;
        private Visibility _operationVisibility;
        private readonly bool _latestOperationIsNotNull;

        public SudokuBoardModel(SudokuBoardState boardState)
        {
            const int TOTAL_CELL_NUMBER = SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1;

            var sourceCells = boardState.GetCells();
#if DEBUG
            System.Diagnostics.Debug.Assert(sourceCells is not null && sourceCells.Length == TOTAL_CELL_NUMBER);
#endif
            _cells = new SudokuBoardCellModel[TOTAL_CELL_NUMBER];
            Cells = new CellCollection(_cells);
            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
                _cells[cellIndex - SudokuBoardCellIndex.MinValue] = CreateCellModel(sourceCells[cellIndex]);
            _showOPeration = false;
            _operationName = string.Empty;
            _operationNameVisibility = Visibility.Hidden;
            _description = string.Empty;
            _operationVisibility = Visibility.Collapsed;
            StepCount = boardState.StepCount;
            var latestOperation = boardState.LatestOperation;
            if (latestOperation is null || latestOperation.Status == GameBoardStatus.Success)
            {
                _latestOperationIsNotNull = false;
                OperationNameVisibility = Visibility.Visible;
                ShowOPeration = false;
            }
            else if (latestOperation.Status == GameBoardStatus.Failed)
            {
                _latestOperationIsNotNull = true;
                ShowOPeration = true;
                OperationName = string.Empty;
                OperationNameVisibility = Visibility.Collapsed;
                Description = latestOperation.Description;
            }
            else
            {
                _latestOperationIsNotNull = true;
                ShowOPeration = true;
                OperationName = $"{latestOperation.Title} ({latestOperation.Difficulty.ToFriendlyString()})";
                OperationNameVisibility = Visibility.Visible;
                Description = latestOperation.Description;
            }

            static SudokuBoardCellModel CreateCellModel(SudokuBoardCell sourceCell)
            {
                if (sourceCell is SudokuBoardDeterminedCell determinedCell)
                {
                    return
                        new SudokuBoardCellModel
                        {
                            IsPositivelyHighlightedBackground = determinedCell.IsPositivelyHighlightedBackground,
                            IsNegativelyHighlightedBackground = determinedCell.IsNegativelyHighlightedBackground,
                            DeterminedDigit = determinedCell.DeterminedDigit.ToCellDigitChar().ToString(),
                            ShowMode =
                                determinedCell.IsPositivelyHighlighted
                                ? SudokuBoardCellShowMode.PositivelyHighlighted
                                : determinedCell.IsInitialSet
                                ? SudokuBoardCellShowMode.Normal
                                : SudokuBoardCellShowMode.Updated
                        };
                }
                else if (sourceCell is SudokuBoardNotDeterminedCell notDeterminedCell)
                {
                    var model = new SudokuBoardCellModel
                    {
                        IsPositivelyHighlightedBackground = notDeterminedCell.IsPositivelyHighlightedBackground,
                        IsNegativelyHighlightedBackground = notDeterminedCell.IsNegativelyHighlightedBackground,
                        DeterminedDigit = "",
                        ShowMode = SudokuBoardCellShowMode.NotDetermined
                    };
                    for (var cellNoteDidit = SudokuBoardCellDigit.MinValue; cellNoteDidit <= SudokuBoardCellDigit.MaxValue; ++cellNoteDidit)
                    {
                        var sourceCellNote = notDeterminedCell.Notes[cellNoteDidit];
                        model.Notes[cellNoteDidit].ShowMode =
                            sourceCellNote.IsPositivelyHighlighted
                            ? SudokuBoardCellNoteShowMode.PositivelyHighlighted
                            : sourceCellNote.IsNegativelyHighlighted
                            ? SudokuBoardCellNoteShowMode.NegativelyHighlighted
                            : sourceCellNote.IsEnabled
                            ? SudokuBoardCellNoteShowMode.Normal
                            : SudokuBoardCellNoteShowMode.Hidden;
                    }

                    return model;
                }
                else
                {
                    throw new ApplicationException();
                }
            }
        }

        public IReadOnlyCellsIndexer Cells { get; }

        public bool ShowOPeration
        {
            get => _showOPeration;
            set
            {
                if (value == _showOPeration)
                    return;
                _showOPeration = value;
                RaisePropertyChangedEvent(nameof(ShowOPeration));
                OperationVisibility =
                    !ShowOPeration
                    ? Visibility.Collapsed
                    : _latestOperationIsNotNull
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        public int StepCount { get; }

        public string OperationName
        {
            get => _operationName;
            private set
            {
                if (value == _operationName)
                    return;
                _operationName = value;
                RaisePropertyChangedEvent(nameof(OperationName));
            }
        }

        public Visibility OperationNameVisibility
        {
            get => _operationNameVisibility;
            private set
            {
                if (value == _operationNameVisibility)
                    return;
                _operationNameVisibility = value;
                RaisePropertyChangedEvent(nameof(OperationNameVisibility));
            }
        }

        public string Description
        {
            get => _description;
            private set
            {
                if (value == _description)
                    return;
                _description = value;
                RaisePropertyChangedEvent(nameof(Description));
            }
        }

        public Visibility OperationVisibility
        {
            get => _operationVisibility;
            private set
            {
                if (value == _operationVisibility)
                    return;
                _operationVisibility = value;
                RaisePropertyChangedEvent(nameof(OperationVisibility));
            }
        }
    }
}
