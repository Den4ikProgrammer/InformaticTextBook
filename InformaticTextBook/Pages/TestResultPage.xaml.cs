using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для TestResult.xaml
    /// </summary>
    public partial class TestResultPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public static readonly TestService _testService = new TestService(new InformaticTextBookContext());
        private string _userLoginText;
        public string UserLoginText
        {
            get => _userLoginText;
            set { _userLoginText = value; OnPropertyChanged(); }
        }

        private string _testName;
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }

        private string _toProfileButtonText;
        public string ToProfileButtonText
        {
            get => _toProfileButtonText;
            set { _toProfileButtonText = value; OnPropertyChanged(); }
        }

        private int _totalQuestions;
        public int TotalQuestions
        {
            get => _totalQuestions;
            set { _totalQuestions = value; OnPropertyChanged(); }
        }

        private int _correctAnswers;
        public int CorrectAnswers
        {
            get => _correctAnswers;
            set { _correctAnswers = value; OnPropertyChanged(); }
        }

        private double _percentage;
        public double Percentage
        {
            get => _percentage;
            set { _percentage = value; OnPropertyChanged(); }
        }

        private string _grade;
        public string Grade
        {
            get => _grade;
            set { _grade = value; OnPropertyChanged(); }
        }

        public Test Test { get; set; }
        public User User { get; set; }

        public TestResultPage(Test test, User user)
        {
            InitializeComponent();
            DataContext = this;
            Test = test;
            User = user;
            UserLoginText = $"Пользователь: {user.UserLogin}";
            LoadTestResults();
        }

        private async void LoadTestResults()
        {
            ToProfileButtonText = CurrentUser.UserID == User.UserId ? "Мой личный кабинет" : "Кабинет студента";
            TestName = $"Тест по лекции {Test.Lection.LectionName}";

            using (var context = new InformaticTextBookContext())
            {
                List<Question> questionsWithResults = await _testService.GetQuestionsWithResults(Test.TestId, User.UserId);

                TotalQuestions = questionsWithResults.Count / 2;
                CorrectAnswers = questionsWithResults.Sum(q => q.QuestionsResults.Any(qr => qr.IsRightAnswer) ? 1 : 0);

                Percentage = TotalQuestions == 0 ? 0 : Math.Round((double)CorrectAnswers / TotalQuestions * 100, 2);

                if (Percentage >= 90) Grade = "Отлично";
                else if (Percentage >= 75) Grade = "Хорошо";
                else if (Percentage >= 60) Grade = "Удовлетворительно";
                else Grade = "Неудовлетворительно";
            }
        }

        private void ToTestButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new TestPage(Test.Lection));
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void ToProfileButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ProfilePage(User.UserId));
        }
    }
}
