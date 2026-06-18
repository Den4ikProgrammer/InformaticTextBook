using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Логика взаимодействия для TypingTrainerPage.xaml
    /// </summary>
    public partial class TypingTrainerPage : Page, INotifyPropertyChanged
    {
        private readonly TypingService _typingService = new(new InformaticTextBookContext());
        private DispatcherTimer? _timer;
        private Stopwatch? _stopwatch;
        private string _targetText = "";
        private bool _isTyping;
        private double _resultSpeed;
        private double _resultAccuracy;
        private string _resultTime = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        public string UserLoginText => $"Пользователь: {CurrentUser.UserLogin}";

        public string TargetText { get => _targetText; set { _targetText = value; OnPropertyChanged(); } }

        private double _speed;
        public double Speed { get => _speed; set { _speed = value; OnPropertyChanged(); } }

        private double _accuracy;
        public double Accuracy { get => _accuracy; set { _accuracy = value; OnPropertyChanged(); } }

        private string _elapsedTimeText = "0 сек";
        public string ElapsedTimeText { get => _elapsedTimeText; set { _elapsedTimeText = value; OnPropertyChanged(); } }

        public bool IsTyping { get => _isTyping; set { _isTyping = value; OnPropertyChanged(); } }

        public TypingTrainerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNewText();
            await LoadBestResult();
            await LoadHistory();
        }

        private async Task LoadNewText()
        {
            string difficulty = (DifficultyComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Normal";
            TargetText = await _typingService.GetRandomTrainingTextAsync(difficulty);
            InputTextBox.Text = "";
            HighlightedTextBlock.Inlines.Clear();
            HighlightedTextBlock.Inlines.Add(new Run(TargetText) { Foreground = Brushes.Gray });
        }

        private async Task LoadBestResult()
        {
            try
            {
                var best = await _typingService.GetBestResultAsync(CurrentUser.UserID);
                if (best != null)
                {
                    BestResultText.Text = $"Скорость: {best.Speed:F0} зн/мин\nТочность: {best.Accuracy:F1}%\nСложность: {best.Difficulty ?? "Normal"}";
                }
            }
            catch
            {
                BestResultText.Text = "Нет данных";
            }
        }

        private async Task LoadHistory()
        {
            try
            {
                var history = await _typingService.GetUserHistoryAsync(CurrentUser.UserID);
                HistoryItemsControl.ItemsSource = history;
            }
            catch (Exception ex)
            {
                // Если ошибка — показываем пустой список
                HistoryItemsControl.ItemsSource = null;
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private async void NewTextButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadNewText();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            IsTyping = true;
            StartButton.Visibility = Visibility.Collapsed;
            FinishButton.Visibility = Visibility.Visible;
            DifficultyComboBox.IsEnabled = false;
            InputTextBox.Focus();
            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.2) };
            _timer.Tick += UpdateStats;
            _timer.Start();
        }

        private void UpdateStats(object? sender, EventArgs e)
        {
            var elapsed = _stopwatch!.Elapsed;
            ElapsedTimeText = elapsed.TotalMinutes >= 1
                ? $"{(int)elapsed.TotalMinutes} мин {elapsed.Seconds} сек"
                : $"{elapsed.Seconds} сек";

            int typed = InputTextBox.Text.Length;
            if (typed > 0 && elapsed.TotalMinutes > 0.01)
                Speed = typed / elapsed.TotalMinutes;

            UpdateHighlighting();
        }

        private void UpdateHighlighting()
        {
            string input = InputTextBox.Text, target = TargetText;
            HighlightedTextBlock.Inlines.Clear();
            for (int i = 0; i < target.Length; i++)
            {
                var run = new Run(target[i].ToString());
                if (i < input.Length)
                    run.Foreground = input[i] == target[i] ? Brushes.Green : Brushes.Red;
                else
                    run.Foreground = Brushes.Gray;
                HighlightedTextBlock.Inlines.Add(run);
            }
            if (input.Length > 0)
            {
                int correct = 0;
                for (int i = 0; i < input.Length && i < target.Length; i++)
                    if (input[i] == target[i]) correct++;
                Accuracy = (double)correct / input.Length * 100;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsTyping && InputTextBox.Text.Length >= TargetText.Length)
                FinishTraining();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e) => FinishTraining();

        private void FinishTraining()
        {
            if (!IsTyping) return;
            _timer?.Stop();
            _stopwatch?.Stop();
            IsTyping = false;
            FinishButton.Visibility = Visibility.Collapsed;
            InputTextBox.IsEnabled = false;
            DifficultyComboBox.IsEnabled = true;

            ResultSpeed = Speed;
            ResultAccuracy = Accuracy;
            ResultTime = ElapsedTimeText;

            SaveResult();
        }

        private async void SaveResult()
        {
            string difficulty = (DifficultyComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Normal";
            await _typingService.SaveResultAsync(
                CurrentUser.UserID,
                (decimal)ResultSpeed,
                (decimal)ResultAccuracy,
                (int)(_stopwatch?.Elapsed.TotalSeconds ?? 0),
                difficulty
            );

            await LoadBestResult();
            await LoadHistory();
            await LoadNewText();
            InputTextBox.IsEnabled = true;
            StartButton.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) =>
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());

        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public double ResultSpeed
        {
            get => _resultSpeed;
            set { _resultSpeed = value; OnPropertyChanged(); }
        }

        public double ResultAccuracy
        {
            get => _resultAccuracy;
            set { _resultAccuracy = value; OnPropertyChanged(); }
        }

        public string ResultTime
        {
            get => _resultTime;
            set { _resultTime = value; OnPropertyChanged(); }
        }
    }


}
