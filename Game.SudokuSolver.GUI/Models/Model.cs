using System.ComponentModel;

namespace Game.SudokuSolver.GUI.Models
{
    internal abstract class Model
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void ClearPropertyChangedEventRegistration() => PropertyChanged = null;

        protected void RaisePropertyChangedEvent(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
