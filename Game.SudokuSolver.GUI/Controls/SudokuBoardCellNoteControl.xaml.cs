using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI.Controls
{
    /// <summary>
    /// SudokuBoardCellNoteControl.xaml の相互作用ロジック
    /// </summary>
    internal sealed partial class SudokuBoardCellNoteControl
        : UserControl
    {
        public SudokuBoardCellNoteControl()
        {
            InitializeComponent();

            InitializeUserControl(this, DataContext);
        }

        private static void InitializeUserControl(SudokuBoardCellNoteControl c, object oldDataContext)
        {
            c.DataContextChanged -= OnDataContextChanged;
            c.DataContextChanged += OnDataContextChanged;
            if (oldDataContext is SudokuBoardCellNoteModel oldModel)
                oldModel.PropertyChanged -= OnModelPropertyChanged;
            if (c.DataContext is SudokuBoardCellNoteModel model)
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

        private static void InitializeUserControlByModel(SudokuBoardCellNoteControl c, SudokuBoardCellNoteModel model)
        {
            switch (model.ForegroundEffect)
            {
                case SudokuBoardCellNoteEffect.Hidden:
                    c.CellNoteContainer.Visibility = Visibility.Hidden;
                    break;
                case SudokuBoardCellNoteEffect.Normal:
                    c.CellNoteContainer.Visibility = Visibility.Visible;
                    c.CellNoteDigitTextBlock.Foreground = Brushes.Black;
                    c.CellNoteBackgroundRectangle.Fill = Brushes.Transparent;
                    break;
                case SudokuBoardCellNoteEffect.PrimaryPositivelyHighlighted:
                    c.CellNoteContainer.Visibility = Visibility.Visible;
                    c.CellNoteDigitTextBlock.Foreground = Brushes.White;
                    c.CellNoteBackgroundRectangle.Fill = Brushes.Green;
                    break;
                case SudokuBoardCellNoteEffect.SecondaryPositivelyHilighted:
                    c.CellNoteContainer.Visibility = Visibility.Visible;
                    c.CellNoteDigitTextBlock.Foreground = Brushes.Black;
                    c.CellNoteBackgroundRectangle.Fill = Brushes.Pink;
                    break;
                case SudokuBoardCellNoteEffect.NegativelyHighlighted:
                    c.CellNoteContainer.Visibility = Visibility.Visible;
                    c.CellNoteDigitTextBlock.Foreground = Brushes.White;
                    c.CellNoteBackgroundRectangle.Fill = Brushes.Red;
                    break;
                default:
                    throw new ApplicationException();
            }
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardCellNoteControl c)
                InitializeUserControl(c, e.OldValue);
        }

        private static void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardCellNoteControl c && c.DataContext is SudokuBoardCellNoteModel model)
            {
                switch (e.PropertyName)
                {
                    case nameof(model.ForegroundEffect):
                        InitializeUserControlByModel(c, model);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
