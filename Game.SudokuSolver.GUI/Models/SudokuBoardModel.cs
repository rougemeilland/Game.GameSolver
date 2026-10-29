using System;
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
#if DEBUG
                System.Diagnostics.Debug.Assert(source is not null && source.Length == SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1);
#endif
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

            public int Length => _source.Length;
        }

        private readonly SudokuBoardCellModel[] _cells;
        private readonly bool _latestOperationIsNotNull;
        private bool _showOPeration;
        private bool _showOPerationDetail;
        private string _operationName;
        private Visibility _operationNameVisibility;
        private string _description;
        private Visibility _operationVisibility;

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
            _showOPerationDetail = false;
            _operationName = string.Empty;
            _operationNameVisibility = (Visibility)255;
            _description = string.Empty;
            _operationVisibility = (Visibility)255;
            StepCount = boardState.StepCount;
            var latestOperation = boardState.LatestOperation;
            if (latestOperation is null || latestOperation.Status == GameBoardStatus.Success)
            {
                _latestOperationIsNotNull = false;
                OperationNameVisibility = Visibility.Visible;
                ShowOperation = false;
                ShowOperationDetail = false;
            }
            else if (latestOperation.Status == GameBoardStatus.Failed)
            {
                _latestOperationIsNotNull = true;
                ShowOperation = true;
                ShowOperationDetail = true;
                OperationName = string.Empty;
                OperationNameVisibility = Visibility.Collapsed;
                Description = latestOperation.Description;
            }
            else
            {
                _latestOperationIsNotNull = true;
                ShowOperation = true;
                ShowOperationDetail = _latestOperationIsNotNull;
                OperationName = $"{latestOperation.Title} ({latestOperation.Difficulty.ToFriendlyString()})";
                OperationNameVisibility = Visibility.Visible;
                Description = latestOperation.Description;
            }

            SetOperationVisibility();

            static SudokuBoardCellModel CreateCellModel(SudokuBoardCell sourceCell)
            {
                if (sourceCell is SudokuBoardDeterminedCell determinedCell)
                {
                    return
                        new SudokuBoardCellModel
                        {
                            BackgroundEffect = determinedCell.BackgroundEffect,
                            DeterminedDigit = determinedCell.DeterminedDigit.ToCellDigitChar().ToString(),
                            ForegroundEffect =
                                determinedCell.ForegroundEffect switch
                                {
                                    SudokuBoardCellForegroundEffect.Normal => SudokuBoardCellForegroundEffect.Normal,
                                    SudokuBoardCellForegroundEffect.PositivelyHighlighted => SudokuBoardCellForegroundEffect.PositivelyHighlighted,
                                    SudokuBoardCellForegroundEffect.NegativelyHighlighted => SudokuBoardCellForegroundEffect.NegativelyHighlighted,
                                    SudokuBoardCellForegroundEffect.Updated => SudokuBoardCellForegroundEffect.Updated,
                                    _ => throw new ApplicationException(),
                                },
                        };
                }
                else if (sourceCell is SudokuBoardNotDeterminedCell notDeterminedCell)
                {
                    var model = new SudokuBoardCellModel
                    {
                        BackgroundEffect = notDeterminedCell.BackgroundEffect,
                        DeterminedDigit = "",
                        ForegroundEffect = SudokuBoardCellForegroundEffect.NotDetermined
                    };
                    for (var cellNoteDigit = SudokuBoardCellDigit.MinValue; cellNoteDigit <= SudokuBoardCellDigit.MaxValue; ++cellNoteDigit)
                        model.Notes[cellNoteDigit].ForegroundEffect = notDeterminedCell.Notes[cellNoteDigit].Effect;
                    return model;
                }
                else
                {
                    throw new ApplicationException();
                }
            }
        }

        public IReadOnlyCellsIndexer Cells { get; }

        public bool ShowOperation
        {
            get => _showOPeration;
            set
            {
                if (value == _showOPeration)
                    return;
                _showOPeration = value;
                RaisePropertyChangedEvent(nameof(ShowOperation));
                SetOperationVisibility();
            }
        }

        public bool ShowOperationDetail
        {
            get => _showOPerationDetail;
            private set
            {
                if (value == _showOPerationDetail)
                    return;
                _showOPerationDetail = value;
                RaisePropertyChangedEvent(nameof(ShowOperationDetail));
                SetOperationVisibility();
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

        private void SetOperationVisibility()
            => OperationVisibility =
                !ShowOperation
                ? Visibility.Hidden
                : ShowOperationDetail
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
