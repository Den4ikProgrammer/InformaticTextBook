using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;

namespace InformaticTextBook.Pages
{
    public partial class ProfilePage : Page
    {
        public static readonly VisitService _visitService = new(new InformaticTextBookContext());
        public static readonly QuestionsResultsService _resultsService = new(new InformaticTextBookContext());
        public static readonly UserService _userService = new(new InformaticTextBookContext());
        public static readonly TestService _testService = new(new InformaticTextBookContext());
        public static readonly StatisticsService _statisticsService = new(new InformaticTextBookContext());

        private static int ProfileOwnerId { get; set; }
        private static User ProfileOwner { get; set; }

        public ProfilePage(int profileOwnerId)
        {
            InitializeComponent();
            ProfileOwnerId = profileOwnerId;
        }

        private static int CalculateGrade(int correct, int total)
        {
            if (total == 0) return 0;
            double percent = (double)correct / total * 100;
            if (percent >= 90) return 5;
            if (percent >= 75) return 4;
            if (percent >= 60) return 3;
            return 2;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            ProfileOwner = await _userService.GetUserByIdAsync(ProfileOwnerId);

            var visits = await _visitService.GetUserVisits(ProfileOwner.UserId);
            var testResults = await _resultsService.GetUserResults(ProfileOwner.UserId);

            // группировка лекций по темам
            var lectionsByTheme = visits
                .GroupBy(v => v.Lection?.Theme?.ThemeName ?? "Без темы")
                .Select(g => new
                {
                    ThemeName = g.Key,
                    Lections = g.Select(v => new VisitDTO
                    {
                        LectionName = v.Lection?.LectionName ?? "Неизвестно",
                        VisitTimeSeconds = v.VisitTime ?? 0
                    }).ToList()
                }).ToList();

            // пройденные тесты
            var passedTests = testResults
                .GroupBy(r => r.Question?.Test)
                .Where(g => g.Key != null)
                .Select(g => new
                {
                    TestId = g.Key!.TestId,
                    DisplayText = $"{g.Key.Lection?.LectionName ?? "Тест"} — оценка: {CalculateGrade(g.Count(r => r.IsRightAnswer), g.Count())}"
                }).ToList();

            var totalSeconds = TimeSpan.FromSeconds(visits.Sum(v => v.VisitTime ?? 0));
            string totalTime;
            if (totalSeconds.TotalMinutes >= 1)
                totalTime = $"{totalSeconds.Minutes} мин {totalSeconds.Seconds} сек";
            else
                totalTime = $"{totalSeconds.Seconds} сек";

            DataContext = new
            {
                CurrentUserLogin = CurrentUser.UserLogin,
                IsTeacherMode = CurrentUser.Role.RoleId == 1 ? Visibility.Visible : Visibility.Collapsed,
                ProfileText = CurrentUser.UserID == ProfileOwner.UserId ? "Личный кабинет" : "Кабинет студента",
                ProfileOwnerLogin = ProfileOwner.UserLogin ?? "Unknown",
                RoleName = ProfileOwner.Role?.RoleName ?? "User",
                TotalLections = visits.Count,
                TotalTime = totalTime,
                LectionsByTheme = lectionsByTheme
            };

            PassedTestsList.ItemsSource = passedTests;
        }

        // Загрузка графиков при выборе вкладки "Статистика"
        private async void StatisticsTab_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var stats = await _statisticsService.GetUserStatisticsAsync(ProfileOwnerId);

                // Заполняем карточки статистики
                TestsPassedText.Text = stats.TestsPassed.ToString();
                LectionsReadText.Text = stats.LectionsRead.ToString();
                AverageGradeText.Text = stats.AverageGrade.ToString("F1");
                BestSpeedText.Text = $"{stats.TypingBestSpeed:F0} зн/мин";

                // График по темам
                var themeGrades = await _statisticsService.GetThemeGradesAsync(ProfileOwnerId);
                ThemeChart.Model = CreateThemeChart(themeGrades);

                // График по тестам
                var testGrades = await _statisticsService.GetTestGradesAsync(ProfileOwnerId);
                TestChart.Model = CreateTestChart(testGrades);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private PlotModel CreateThemeChart(List<ThemeGrade> grades)
        {
            var model = new PlotModel
            {
                Title = "Оценки по темам",
                TitleFontSize = 13,
                IsLegendVisible = false
            };

            if (!grades.Any()) return model;

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = null,
                FontSize = 10
            };
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Оценка",
                Minimum = 0,
                Maximum = 5,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                FontSize = 10
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries
            {
                FillColor = OxyColor.FromRgb(205, 133, 63),
                StrokeColor = OxyColor.FromRgb(139, 90, 43),
                StrokeThickness = 1
            };

            foreach (var g in grades)
            {
                series.Items.Add(new BarItem { Value = g.Grade });
                categoryAxis.Labels.Add(g.ThemeName.Length > 20 ? g.ThemeName[..20] + "..." : g.ThemeName);
            }

            model.Series.Add(series);
            return model;
        }

        private PlotModel CreateTestChart(List<TestGrade> grades)
        {
            var model = new PlotModel
            {
                Title = "Результаты тестов (%)",
                TitleFontSize = 13,
                IsLegendVisible = false
            };

            if (!grades.Any()) return model;

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = null,
                FontSize = 10
            };
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "%",
                Minimum = 0,
                Maximum = 100,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                FontSize = 10
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries
            {
                FillColor = OxyColor.FromRgb(85, 107, 47),
                StrokeColor = OxyColor.FromRgb(60, 80, 35),
                StrokeThickness = 1
            };

            foreach (var g in grades)
            {
                series.Items.Add(new BarItem { Value = g.Percentage });
                categoryAxis.Labels.Add(g.TestName.Length > 25 ? g.TestName[..25] + "..." : g.TestName);
            }

            model.Series.Add(series);
            return model;
        }

        private async void PassedTests_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PassedTestsList.SelectedItem is not null)
            {
                var selected = (dynamic)PassedTestsList.SelectedItem;
                int testId = selected.TestId;
                var test = await _testService.GetTestById(testId);
                if (test != null)
                {
                    App.CurrentFrame.Navigate(new TestResultPage(test, ProfileOwner));
                }
            }
        }

        private void ToStudentsButton_Click(object sender, RoutedEventArgs e) =>
            App.CurrentFrame.Navigate(new StudentsListPage());

        private void ToNavigator_Click(object sender, RoutedEventArgs e) =>
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());

        private void ImportButton_Click(object sender, RoutedEventArgs e) =>
            App.CurrentFrame.Navigate(new ImportPage());
    }
}
