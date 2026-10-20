using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardSlideShowModel
        : Model
    {
        private sealed class SudokuBoardModelCollection
            : IReadOnlySudokuBoardModelsIndexer
        {
            private readonly SudokuBoardModel[] _source;

            public SudokuBoardModelCollection(ReadOnlyMemory<SudokuBoardModel> source)
            {
                _source = source.ToArray();
            }

            public SudokuBoardModel this[int index]
            {
                get
                {
                    ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _source.Length);

                    return _source[index];
                }
            }

            public int Length => _source.Length;
        }

        private int _currentStepCount = -1;
        private string _currentStepTitle = "";
        private bool _isFirstStep;
        private bool _isLastStep;
        private Visibility _previousStepButtonVisibility;
        private Visibility _nextStepButtonVisibility;

        public SudokuBoardSlideShowModel(IEnumerable<SudokuBoardModel> models)
            : this(models.ToArray().AsMemory())
        {
        }

        public SudokuBoardSlideShowModel(ReadOnlyMemory<SudokuBoardModel> boardModels)
        {
            BoardModels = new SudokuBoardModelCollection(boardModels);
            CurrentStepCount = 0;
        }

        public IReadOnlySudokuBoardModelsIndexer BoardModels { get; }

        public int CurrentStepCount
        {
            get => _currentStepCount;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, BoardModels.Length);

                if (value == _currentStepCount)
                    return;
                _currentStepCount = value;
                RaisePropertyChangedEvent(nameof(CurrentStepCount));
                if (_currentStepCount <= 0)
                {
                    CurrentStepTitle = "初期配置";
                    IsFirstStep = true;
                    IsLastStep = false;
                    PreviousStepButtonVisibility = Visibility.Hidden;
                    NextStepButtonVisibility = Visibility.Visible;
                }
                else if (_currentStepCount >= BoardModels.Length - 1)
                {
                    CurrentStepTitle = "完了";
                    IsFirstStep = false;
                    IsLastStep = true;
                    PreviousStepButtonVisibility = Visibility.Visible;
                    NextStepButtonVisibility = Visibility.Hidden;
                }
                else
                {
                    CurrentStepTitle = $"{_currentStepCount} 手順目";
                    IsFirstStep = false;
                    IsLastStep = false;
                    PreviousStepButtonVisibility = Visibility.Visible;
                    NextStepButtonVisibility = Visibility.Visible;
                }
            }
        }

        public string CurrentStepTitle
        {
            get => _currentStepTitle;
            private set
            {
                if (value == _currentStepTitle)
                    return;
                _currentStepTitle = value;
                RaisePropertyChangedEvent(nameof(CurrentStepTitle));
            }
        }

        public bool IsFirstStep
        {
            get => _isFirstStep;
            private set
            {
                if (value == _isFirstStep)
                    return;
                _isFirstStep = value;
                RaisePropertyChangedEvent(nameof(IsFirstStep));
            }
        }

        public bool IsLastStep
        {
            get => _isLastStep;
            private set
            {
                if (value == _isLastStep)
                    return;
                _isLastStep = value;
                RaisePropertyChangedEvent(nameof(IsLastStep));
            }
        }

        public Visibility PreviousStepButtonVisibility
        {
            get => _previousStepButtonVisibility;
            private set
            {
                if (value == _previousStepButtonVisibility)
                    return;
                _previousStepButtonVisibility = value;
                RaisePropertyChangedEvent(nameof(PreviousStepButtonVisibility));
            }
        }

        public Visibility NextStepButtonVisibility
        {
            get => _nextStepButtonVisibility;
            private set
            {
                if (value == _nextStepButtonVisibility)
                    return;
                _nextStepButtonVisibility = value;
                RaisePropertyChangedEvent(nameof(NextStepButtonVisibility));
            }
        }
    }
}
