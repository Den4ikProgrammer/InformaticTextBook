using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для TestPage.xaml
    /// </summary>
    public partial class TestPage : Page, INotifyPropertyChanged
    {
        public static readonly TestService _testService = new TestService(new InformaticTextBookContext());
        public static readonly UserService _userService = new UserService(new InformaticTextBookContext());
        public static readonly QuestionsResultsService _resultsService = new QuestionsResultsService(new InformaticTextBookContext());
        public event PropertyChangedEventHandler? PropertyChanged;

        private Stopwatch _stopwatch;
        private DispatcherTimer _timer;
        private TimeSpan _timeLimit = TimeSpan.FromMinutes(5);
        private TimeSpan _remainingTime;

        private ObservableCollection<QuestionDTO> _testQuestions;
        public ObservableCollection<QuestionDTO> TestQuestions
        {
            get => _testQuestions;
            set
            {
                if (_testQuestions != value)
                {
                    _testQuestions = value;
                    OnPropertyChanged();
                }
            }
        }
        public string _userLoginText { get; set; } = "Пользователь: ";
        public string UserLoginText
        {
            get => _userLoginText;
            set
            {
                if (_userLoginText != value)
                {
                    _userLoginText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string _testName { get; set; }
        public string TestName
        {
            get => _testName;
            set
            {
                if (_testName != value)
                {
                    _testName = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Lection Lection { get; set; }
        public Test Test { get; set; }

        public TestPage(Lection lection)
        {
            InitializeComponent();
            DataContext = this;
            UserLoginText = $"Пользователь: {CurrentUser.UserLogin}";
            Lection = lection;
        }

        private string _remainingTimeText = "05:00";
        public string RemainingTimeText
        {
            get => _remainingTimeText;
            set
            {
                if (_remainingTimeText != value)
                {
                    _remainingTimeText = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTestAsync();
        }

        private async Task LoadTestAsync()
        {
            int number = 1;
            Test = await _testService.GetTestByLectionIdAsync(Lection.LectionId);
            TestName = await _testService.GetTestNameAsync(Test);
            TestQuestions = new ObservableCollection<QuestionDTO>();
            foreach (Question question in Test.Questions)
            {
                string text = question.QuestionText.Trim();
                bool alreadyNumbered = text.Length > 2 && char.IsDigit(text[0]) &&
                (text[1] == '.' || (char.IsDigit(text[1]) && text.Length > 3 && text[2] == '.'));

                QuestionDTO questionDTO = new QuestionDTO()
                {
                    QuestionId = question.QuestionId,
                    QuestionText = alreadyNumbered ? text : $"{number}. {text}",
                    Answers = question.Answers,
                    TestId = question.TestId
                };
                TestQuestions.Add(questionDTO);
                number++;
            }
        }

        private void ToLectionButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new LectionPage(Lection));
        }

        private void ToNavigatorButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());
        }

        private async Task FinishTest()
        {
            List<QuestionsResult> results = new List<QuestionsResult>();

            foreach (QuestionDTO question in TestQuestions)
            {
                results.Add(new QuestionsResult
                {
                    QuestionId = question.QuestionId,
                    UserId = CurrentUser.UserID,
                    IsRightAnswer = question.SelectedAnswer?.IsCorrect ?? false
                });
                question.IsTestFinished = true;
            }

            await _resultsService.AddTestResults(results);
            // Отладка
            System.Diagnostics.Debug.WriteLine($"Сохранено результатов: {results.Count}");
            foreach (var r in results)
            {
                System.Diagnostics.Debug.WriteLine($"  Вопрос {r.QuestionId}: IsRightAnswer = {r.IsRightAnswer}");
            }
            FinishTestButton.Visibility = Visibility.Collapsed;
            ToResultButton.Visibility = Visibility.Visible;

            // Отключаем возможность выбора ответов
            foreach (var question in TestQuestions)
            {
                question.IsTestFinished = true;
            }

            //List<QuestionsResult> results = new List<QuestionsResult>();
            //var allQuestions = await _testService.GetTestQuestionsAsync(Test.TestId);

            //foreach (var question in allQuestions)
            //{
            //    results.Add(new QuestionsResult() { QuestionId = question.QuestionId, UserId = CurrentUser.UserID, IsRightAnswer = false });
            //}

            //foreach (QuestionDTO question in TestQuestions)
            //{
            //    results.FirstOrDefault(q => q.QuestionId == question.QuestionId)!.IsRightAnswer = question.SelectedAnswer != null ? question.SelectedAnswer.IsCorrect : false;
            //    question.IsTestFinished = true;
            //}

            //await _resultsService.AddTestResults(results);
            //FinishTestButton.Visibility = Visibility.Collapsed;
            //ToResultButton.Visibility = Visibility.Visible;

            //// Отключаем возможность выбора ответов
            //foreach (var question in TestQuestions)
            //{
            //    question.IsTestFinished = true;
            //}
        }

        private async void FinishTestButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _stopwatch.Stop();
            await FinishTest();
        }

        private async void ToResultButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new TestResultPage(Test, await _userService.GetUserByIdAsync(CurrentUser.UserID)));
        }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            StartTestButton.Visibility = Visibility.Collapsed;
            ToLectionButton.Visibility = Visibility.Collapsed;
            ToNavigatorButton.Visibility = Visibility.Collapsed;
            BottomPanel.Visibility = Visibility.Visible;
            QuestionsPanel.Visibility = Visibility.Visible;

            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _remainingTime = _timeLimit - _stopwatch.Elapsed;

            if (_remainingTime.TotalSeconds <= 0)
            {
                _remainingTime = TimeSpan.Zero;
                TimeExpired();
            }

            RemainingTimeText = _remainingTime.ToString(@"mm\:ss");
        }

        private async void TimeExpired()
        {
            _timer.Stop();
            _stopwatch.Stop();

            // Показываем сообщение о завершении времени
            MessageBox.Show("Время вышло! Тест будет автоматически завершен.", "Время истекло",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Автоматически завершаем тест
            await FinishTest();
        }
    }
}