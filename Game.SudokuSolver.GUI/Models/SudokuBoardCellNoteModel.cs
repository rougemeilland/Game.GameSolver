namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardCellNoteModel
        : Model
    {
        private SudokuBoardCellNoteEffect _foregroundEffect;
        private string _cellNoteDidit;

        public SudokuBoardCellNoteModel()
        {
            ForegroundEffect = SudokuBoardCellNoteEffect.Hidden;
            _cellNoteDidit = string.Empty;
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

        public string CellNoteDidit
        {
            get => _cellNoteDidit;
            set
            {
                if (value == _cellNoteDidit)
                    return;
                _cellNoteDidit = value;
                RaisePropertyChangedEvent(_cellNoteDidit);
            }
        }
    }
}
