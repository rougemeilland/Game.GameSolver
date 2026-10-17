using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI.Controls
{
    /// <summary>
    /// SudokuBoardControl.xaml の相互作用ロジック
    /// </summary>
    internal sealed partial class SudokuBoardControl
        : UserControl
    {
        public SudokuBoardControl()
        {
            InitializeComponent();

            InitializeUserControl(this, DataContext);
        }

        private static void InitializeUserControl(SudokuBoardControl c, object oldDataContext)
        {
            c.DataContextChanged -= OnDataContextChanged;
            c.DataContextChanged += OnDataContextChanged;
            if (oldDataContext is SudokuBoardModel oldModel)
                oldModel.PropertyChanged -= OnModelPropertyChanged;
            if (c.DataContext is SudokuBoardModel model)
            {
                InitializeUserControlByModel(c, model);
                model.PropertyChanged += OnModelPropertyChanged;
            }
            else if (c.DataContext is null)
            {
            }
            else
            {
                throw new ApplicationException();
            }
        }

        private static void InitializeUserControlByModel(SudokuBoardControl c, SudokuBoardModel model)
        {
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardControl c)
                InitializeUserControl(c, e.OldValue);
        }

        private static void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
#if false
            if (sender is SudokuBoardCellControl c && c.DataContext is SudokuBoardModel model)
            {
                switch (e.PropertyName)
                {
                    default:
                        break;
                }
            }
#endif
        }
    }
}
