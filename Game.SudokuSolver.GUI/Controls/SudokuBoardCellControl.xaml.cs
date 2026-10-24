using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI.Controls
{
    /// <summary>
    /// SudokuBoardCellControl.xaml の相互作用ロジック
    /// </summary>
    internal sealed partial class SudokuBoardCellControl
        : UserControl
    {
        public SudokuBoardCellControl()
        {
            InitializeComponent();

            InitializeUserControl(this, DataContext);
        }

        private static void InitializeUserControl(SudokuBoardCellControl c, object oldDataContext)
        {
            c.DataContextChanged -= OnDataContextChanged;
            c.DataContextChanged += OnDataContextChanged;
            if (oldDataContext is SudokuBoardCellModel oldModel)
                oldModel.PropertyChanged -= OnModelPropertyChanged;
            if (c.DataContext is SudokuBoardCellModel model)
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

        private static void InitializeUserControlByModel(SudokuBoardCellControl c, SudokuBoardCellModel model)
        {
            c.CellContainer.Background =
                model.BackgroundEffect switch
                {
                    SudokuBoardCellBackgroundEffect.Normal => Brushes.Transparent,
                    SudokuBoardCellBackgroundEffect.StrongPotisivelyHighlighted => Brushes.LightGreen,
                    SudokuBoardCellBackgroundEffect.WeakPositivelyHighlighted => Brushes.LightCyan,
                    SudokuBoardCellBackgroundEffect.NegativelyHighlighted => Brushes.LightPink,
                    _ => throw new ApplicationException(),
                };

            switch (model.ForegroundEffect)
            {
                case SudokuBoardCellForegroundEffect.NotDetermined:
                    c.CellNotesContainer.Visibility = Visibility.Visible;
                    c.CellDeterminedDigitContainer.Visibility = Visibility.Hidden;
                    break;
                case SudokuBoardCellForegroundEffect.Normal:
                    c.CellNotesContainer.Visibility = Visibility.Hidden;
                    c.CellDeterminedDigitContainer.Visibility = Visibility.Visible;
                    c.CellDeterminedDigitTextBlock.Foreground = Brushes.Black;
                    c.CellDeterminedDigitBackgroundRectangle.Fill = Brushes.Transparent;
                    break;
                case SudokuBoardCellForegroundEffect.Updated:
                    c.CellNotesContainer.Visibility = Visibility.Hidden;
                    c.CellDeterminedDigitContainer.Visibility = Visibility.Visible;
                    c.CellDeterminedDigitTextBlock.Foreground = Brushes.Blue;
                    c.CellDeterminedDigitBackgroundRectangle.Fill = Brushes.Transparent;
                    break;
                case SudokuBoardCellForegroundEffect.PositivelyHighlighted:
                    c.CellNotesContainer.Visibility = Visibility.Hidden;
                    c.CellDeterminedDigitContainer.Visibility = Visibility.Visible;
                    c.CellDeterminedDigitTextBlock.Foreground = Brushes.White;
                    c.CellDeterminedDigitBackgroundRectangle.Fill = Brushes.Green;
                    break;
                case SudokuBoardCellForegroundEffect.NegativelyHighlighted:
                    c.CellNotesContainer.Visibility = Visibility.Hidden;
                    c.CellDeterminedDigitContainer.Visibility = Visibility.Visible;
                    c.CellDeterminedDigitTextBlock.Foreground = Brushes.White;
                    c.CellDeterminedDigitBackgroundRectangle.Fill = Brushes.Red;
                    break;
                default:
                    throw new ApplicationException();
            }
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardCellControl c)
                InitializeUserControl(c, e.OldValue);
        }

        private static void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardCellControl c && c.DataContext is SudokuBoardCellModel model)
            {
                switch (e.PropertyName)
                {
                    case nameof(model.ForegroundEffect):
                        InitializeUserControl(c, model);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
