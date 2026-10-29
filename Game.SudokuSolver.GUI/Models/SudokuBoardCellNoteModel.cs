namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardCellNoteModel
        : Model
    {
        private SudokuBoardCellNoteEffect _foregroundEffect;
        private string _cellNoteDigit;

        public SudokuBoardCellNoteModel()
        {
            ForegroundEffect = SudokuBoardCellNoteEffect.Hidden;
            _cellNoteDigit = string.Empty;
        }

        public SudokuBoardCellNoteEffect ForegroundEffect
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

        public string CellNoteDigit
        {
            get => _cellNoteDigit;
            set
            {
                if (value == _cellNoteDigit)
                    return;
                _cellNoteDigit = value;
                RaisePropertyChangedEvent(_cellNoteDigit);
            }
        }
    }
}
