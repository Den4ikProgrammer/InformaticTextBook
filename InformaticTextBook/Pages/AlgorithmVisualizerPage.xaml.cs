using ServiceLayer;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для AlgorithmVisualizerPage.xaml
    /// </summary>
    public partial class AlgorithmVisualizerPage : Page
    {
        private readonly AlgorithmService _algoService = new AlgorithmService();
        private List<AlgorithmState> _steps = new List<AlgorithmState>();
        private int _currentStep = 0;
        private DispatcherTimer? _playTimer;

        public string UserLoginText => $"Пользователь: {CurrentUser.UserLogin}";

        private int _currentLine;
        public int CurrentLine
        {
            get => _currentLine;
            set
            {
                if (_currentLine != value)
                {
                    _currentLine = value;
                    OnPropertyChanged();
                }
            }
        }

        // Модель для строки псевдокода
        public class PseudoCodeLine
        {
            public int Number { get; set; }
            public string Text { get; set; } = "";
        }

        private ObservableCollection<PseudoCodeLine>? _pseudoCodeLines;
        public ObservableCollection<PseudoCodeLine> PseudoCodeLines
        {
            get => _pseudoCodeLines ?? new ObservableCollection<PseudoCodeLine>();
            set { _pseudoCodeLines = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public AlgorithmVisualizerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Установим начальные значения после полной загрузки страницы
            AlgoComboBox.SelectedIndex = 0;
            LoadPseudoCode("BubbleSort");
            ResetVisualization();
        }

        private void AlgoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return; // игнорируем до загрузки
            if (AlgoComboBox.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag.ToString()!;
                bool isSearch = tag == "LinearSearch" || tag == "BinarySearch";
                TargetLabel.Visibility = isSearch ? Visibility.Visible : Visibility.Collapsed;
                TargetTextBox.Visibility = isSearch ? Visibility.Visible : Visibility.Collapsed;
                LoadPseudoCode(tag);
                ResetVisualization();
            }
        }

        private void LoadPseudoCode(string tag)
        {
            List<PseudoCodeLine> lines = new();
            string[] code = tag switch
            {
                "BubbleSort" => new[]
                {
                    "for i = 0 to n-2",
                    "  for j = 0 to n-i-2",
                    "    if arr[j] > arr[j+1]",
                    "      swap(arr[j], arr[j+1])",
                    "end"
                },
                "InsertionSort" => new[]
                {
                    "for i = 1 to n-1",
                    "  key = arr[i]",
                    "  j = i - 1",
                    "  while j >= 0 and arr[j] > key",
                    "    arr[j+1] = arr[j]",
                    "    j = j - 1",
                    "  arr[j+1] = key",
                    "end"
                },
                "LinearSearch" => new[]
                {
                    "for i = 0 to n-1",
                    "  if arr[i] == target",
                    "    return i",
                    "return -1"
                },
                "BinarySearch" => new[]
                {
                    "left = 0, right = n-1",
                    "while left <= right",
                    "  mid = (left + right) / 2",
                    "  if arr[mid] == target",
                    "    return mid",
                    "  else if arr[mid] < target",
                    "    left = mid + 1",
                    "  else",
                    "    right = mid - 1",
                    "return -1"
                },
                _ => new[] { "Выберите алгоритм" }
            };

            for (int i = 0; i < code.Length; i++)
            {
                lines.Add(new PseudoCodeLine { Number = i + 1, Text = code[i] });
            }

            PseudoCodeLines = new ObservableCollection<PseudoCodeLine>(lines);
            PseudoCodeItemsControl.ItemsSource = PseudoCodeLines;
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] array = DataTextBox.Text.Split(',')
                    .Select(s => int.Parse(s.Trim()))
                    .ToArray();

                string tag = (AlgoComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
                _steps = tag switch
                {
                    "BubbleSort" => _algoService.GenerateBubbleSortSteps(array),
                    "InsertionSort" => _algoService.GenerateInsertionSortSteps(array),
                    "LinearSearch" => _algoService.GenerateLinearSearchSteps(array, int.Parse(TargetTextBox.Text)), // можно заменить на ввод target
                    "BinarySearch" => _algoService.GenerateBinarySearchSteps(array.OrderBy(x => x).ToArray(), 5),
                    _ => new List<AlgorithmState>()
                };
                _currentStep = 0;
                ShowStep(_currentStep);
                StatusText.Text = "Данные загружены, готов к запуску";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ShowStep(int stepIndex)
        {
            //if (VisualCanvas == null) return;
            //if (stepIndex < 0 || stepIndex >= _steps.Count) return;

            //var state = _steps[stepIndex];
            //VisualCanvas.Children.Clear();

            //if (VisualCanvas.ActualWidth <= 0 || state.Array.Length == 0) return;

            //int barWidth = Math.Min(40, (int)(VisualCanvas.ActualWidth / state.Array.Length) - 2);
            //for (int i = 0; i < state.Array.Length; i++)
            //{
            //    int value = state.Array[i];
            //    double height = value * 20;
            //    Brush brush = state.HighlightedIndices.Contains(i) ? Brushes.Red : Brushes.SteelBlue;
            //    Rectangle rect = new Rectangle
            //    {
            //        Width = barWidth,
            //        Height = height,
            //        Fill = brush,
            //        Stroke = Brushes.Black,
            //        StrokeThickness = 1
            //    };
            //    Canvas.SetLeft(rect, i * (barWidth + 2) + 10);
            //    Canvas.SetTop(rect, VisualCanvas.ActualHeight - height - 10);
            //    VisualCanvas.Children.Add(rect);

            //    TextBlock label = new TextBlock
            //    {
            //        Text = value.ToString(),
            //        FontSize = 10,
            //        Foreground = Brushes.Black,
            //        TextAlignment = TextAlignment.Center
            //    };
            //    Canvas.SetLeft(label, i * (barWidth + 2) + 10 + barWidth / 2 - 10);
            //    Canvas.SetTop(label, VisualCanvas.ActualHeight - 25);
            //    VisualCanvas.Children.Add(label);
            //}

            //CurrentLine = state.CurrentLine;
            //HighlightPseudoCodeLine(state.CurrentLine);

            //StatusText.Text = state.Comment;

            if (VisualCanvas == null) return;
            if (stepIndex < 0 || stepIndex >= _steps.Count) return;

            var state = _steps[stepIndex];
            VisualCanvas.Children.Clear();

            if (VisualCanvas.ActualWidth <= 0 || state.Array.Length == 0) return;

            int maxValue = state.Array.Max();
            if (maxValue == 0) maxValue = 1;

            int barWidth = Math.Min(50, (int)(VisualCanvas.ActualWidth / state.Array.Length) - 4);
            double scale = (VisualCanvas.ActualHeight - 30) / maxValue;

            for (int i = 0; i < state.Array.Length; i++)
            {
                int value = state.Array[i];
                double height = value > 0 ? value * scale : 0;

                Brush brush = state.HighlightedIndices.Contains(i) ? Brushes.Red : Brushes.SteelBlue;

                Rectangle rect = new Rectangle
                {
                    Width = barWidth,
                    Height = height,
                    Fill = brush,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rect, i * (barWidth + 4) + 10);
                Canvas.SetTop(rect, VisualCanvas.ActualHeight - height - 20);
                VisualCanvas.Children.Add(rect);

                TextBlock label = new TextBlock
                {
                    Text = value == -1 ? "" : value.ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    Width = barWidth
                };

                Canvas.SetLeft(label, i * (barWidth + 4) + 10);
                Canvas.SetTop(label, VisualCanvas.ActualHeight - 20);
                VisualCanvas.Children.Add(label);
            }

            CurrentLine = state.CurrentLine;
            HighlightPseudoCodeLine(state.CurrentLine);
            StatusText.Text = state.Comment;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _playTimer?.Stop();
            _playTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.4) };
            _playTimer.Tick += (s, args) =>
            {
                if (_currentStep < _steps.Count - 1)
                {
                    _currentStep++;
                    ShowStep(_currentStep);
                }
                else
                {
                    _playTimer.Stop();
                    StatusText.Text = "Выполнение завершено";
                }
            };
            _playTimer.Start();
            StatusText.Text = "Выполнение...";
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e) => _playTimer?.Stop();

        private void StepForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < _steps.Count - 1)
            {
                _currentStep++;
                ShowStep(_currentStep);
            }
        }

        private void StepBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                ShowStep(_currentStep);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _playTimer?.Stop();
            if (_steps.Count > 0)
            {
                _currentStep = 0;
                ShowStep(0);
                StatusText.Text = "Сброшено";
            }
        }

        private void ResetVisualization()
        {
            if (VisualCanvas != null)
                VisualCanvas.Children.Clear();
            _steps.Clear();
            _currentStep = 0;
            StatusText.Text = "Готово";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());
        }

        private void HighlightPseudoCodeLine(int lineNumber)
        {
            for (int i = 0; i < PseudoCodeItemsControl.Items.Count; i++)
            {
                var container = PseudoCodeItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container != null)
                {
                    var border = FindVisualChild<Border>(container, "PseudoLineBorder");
                    if (border != null)
                    {
                        border.Background = (i + 1 == lineNumber)
                            ? new SolidColorBrush(Colors.Yellow)
                            : new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }
        private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (child != null && child.Name == name)
                    return child as T;
                else if (child != null)
                {
                    var found = FindVisualChild<T>(child, name);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}
