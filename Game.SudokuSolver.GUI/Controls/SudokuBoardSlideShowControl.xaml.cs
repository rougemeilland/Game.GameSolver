using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Game.SudokuSolver.GUI.Models;

namespace Game.SudokuSolver.GUI.Controls
{
    /// <summary>
    /// SudokuBoardSlideShowControl1.xaml の相互作用ロジック
    /// </summary>
    internal sealed partial class SudokuBoardSlideShowControl
        : UserControl
    {
        public SudokuBoardSlideShowControl()
        {
            InitializeComponent();

            InitializeUserControl(this, DataContext);
        }

        private static void InitializeUserControl(SudokuBoardSlideShowControl c, object oldDataContext)
        {
            c.DataContextChanged -= DataContextChangedEventHandler;
            c.DataContextChanged += DataContextChangedEventHandler;
            if (oldDataContext is SudokuBoardSlideShowModel oldModel)
                oldModel.PropertyChanged -= c.PropertyChangedEventHandler;
            if (c.DataContext is SudokuBoardSlideShowModel model)
            {
                InitializeUserControlByModel(c, model);
                model.PropertyChanged += c.PropertyChangedEventHandler;
            }
            else if (c.DataContext is null)
            {
            }
            else
            {
                throw new ApplicationException();
            }
        }

        private static void RefreshBoardList(SudokuBoardSlideShowControl c, SudokuBoardSlideShowModel model)
        {
            while (c.BoardListPanel.Children.Count > 0)
            {
                var child = c.BoardListPanel.Children[0];
                if (child is not SudokuBoardControl childControl)
                    throw new ApplicationException();
                if ((int)childControl.Tag >= model.CurrentStepCount - 5)
                    break;
                if (childControl.DataContext is not SudokuBoardModel childControlModel)
                    throw new ApplicationException();
                childControlModel.ClearPropertyChangedEventRegistration();
                childControl.DataContext = null;
                c.BoardListPanel.Children.RemoveAt(0);
            }

            while (c.BoardListPanel.Children.Count > 0)
            {
                var child = c.BoardListPanel.Children[^1];
                if (child is not SudokuBoardControl childControl)
                    throw new ApplicationException();
                if ((int)childControl.Tag <= model.CurrentStepCount + 5)
                    break;
                if (childControl.DataContext is not SudokuBoardModel childControlModel)
                    throw new ApplicationException();
                childControlModel.ClearPropertyChangedEventRegistration();
                childControl.DataContext = null;
                c.BoardListPanel.Children.RemoveAt(c.BoardListPanel.Children.Count - 1);
            }

            if (c.BoardListPanel.Children.Count <= 0)
            {
                var newChildControl = new SudokuBoardControl
                {
                    Tag = model.CurrentStepCount,
                    DataContext = model.BoardModels[model.CurrentStepCount],
                };
                _ = c.BoardListPanel.Children.Add(newChildControl);
            }

            while (true)
            {
                var child = c.BoardListPanel.Children[0];
                if (child is not SudokuBoardControl childControl)
                    throw new ApplicationException();
                var currentMinimumStepCount = (int)childControl.Tag;
                if (currentMinimumStepCount <= 0 || currentMinimumStepCount <= model.CurrentStepCount - 2)
                    break;
                var newChildControl = new SudokuBoardControl
                {
                    Tag = currentMinimumStepCount - 1,
                    DataContext = model.BoardModels[currentMinimumStepCount - 1],
                };
                c.BoardListPanel.Children.Insert(0, newChildControl);
            }

            while (true)
            {
                var child = c.BoardListPanel.Children[^1];
                if (child is not SudokuBoardControl childControl)
                    throw new ApplicationException();
                var currentMaximumStepCount = (int)childControl.Tag;
                if (currentMaximumStepCount >= model.BoardModels.Length - 1 || currentMaximumStepCount >= model.CurrentStepCount + 2)
                    break;
                var newChildControl = new SudokuBoardControl
                {
                    Tag = currentMaximumStepCount + 1,
                    DataContext = model.BoardModels[currentMaximumStepCount + 1],
                };
                _ = c.BoardListPanel.Children.Add(newChildControl);
            }

            foreach (var child in c.BoardListPanel.Children)
            {
                if (child is not SudokuBoardControl childControl)
                    throw new ApplicationException();
                childControl.Visibility =
                    (int)childControl.Tag == model.CurrentStepCount
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private static void InitializeUserControlByModel(SudokuBoardSlideShowControl c, SudokuBoardSlideShowModel model)
        {
            c.BoardListPanel.Children.Clear();
            RefreshBoardList(c, model);
        }

        private static void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SudokuBoardSlideShowControl c)
                InitializeUserControl(c, e.OldValue);
        }

        private void PropertyChangedEventHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is SudokuBoardSlideShowModel model)
            {
                switch (e.PropertyName)
                {
                    case nameof(model.CurrentStepCount):
                        RefreshBoardList(this, model);
                        break;
                    default:
                        break;
                }
            }
        }

        private void KeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (DataContext is SudokuBoardSlideShowModel model)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        if (!model.IsFirstStep)
                            --model.CurrentStepCount;
                        break;
                    case Key.Right:
                        if (!model.IsLastStep)
                            ++model.CurrentStepCount;
                        break;
                    default:
                        break;
                }
            }
        }

        private void NextButtonClickedEventHandler(object sender, RoutedEventArgs e)
        {
            if (DataContext is SudokuBoardSlideShowModel model)
            {
                if (!model.IsLastStep)
                    ++model.CurrentStepCount;
            }
        }

        private void PreviousButtonEventHandler(object sender, RoutedEventArgs e)
        {
            if (DataContext is SudokuBoardSlideShowModel model)
            {
                if (!model.IsFirstStep)
                    --model.CurrentStepCount;
            }
        }
    }
}
