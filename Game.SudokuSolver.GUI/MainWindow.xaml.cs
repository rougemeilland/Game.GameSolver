using System.Windows;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal sealed partial class MainWindow
        : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowModel();
        }
    }
}
