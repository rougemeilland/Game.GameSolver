namespace Game.SudokuSolver.GUI.Models
{
    internal sealed class SudokuBoardCellNoteModel
        : Model
    {
        private SudokuBoardCellNoteShowMode _showMode;
        private string _cellNoteDidit;

        public SudokuBoardCellNoteModel()
        {
            ShowMode = SudokuBoardCellNoteShowMode.Hidden;
            _cellNoteDidit = string.Empty;
        }

        public SudokuBoardCellNoteShowMode ShowMode
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
