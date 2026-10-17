using System;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:自動プロパティを使用する", Justification = "<保留中>")]
        private readonly SudokuBoardCellModel[] _cells;

        public SudokuBoardModel(IReadOnlySudokuBoardCellCollection sourceCells)
        {
            const int TOTAL_CELL_NUMBER = SudokuBoardCellIndex.MaxValue - SudokuBoardCellIndex.MinValue + 1;
#if DEBUG
            System.Diagnostics.Debug.Assert(sourceCells is not null && sourceCells.Length == TOTAL_CELL_NUMBER);
#endif
            _cells = new SudokuBoardCellModel[TOTAL_CELL_NUMBER];
            Cells = new CellCollection(_cells);
            for (var cellIndex = SudokuBoardCellIndex.MinValue; cellIndex <= SudokuBoardCellIndex.MaxValue; ++cellIndex)
                _cells[cellIndex - SudokuBoardCellIndex.MinValue] = CreateCellModel(sourceCells[cellIndex]);

            static SudokuBoardCellModel CreateCellModel(SudokuBoardCell sourceCell)
            {
                if (sourceCell is SudokuBoardDeterminedCell determinedCell)
                {
                    return
                        new SudokuBoardCellModel
                        {
                            IsHighlightedBackground = determinedCell.IsHighlightedBackground,
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
                        IsHighlightedBackground = notDeterminedCell.IsHighlightedBackground,
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
        public SudokuBoardCellModel[] Cells2 => _cells;
    }
}
