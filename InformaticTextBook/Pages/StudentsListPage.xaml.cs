using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ServiceLayer.Services;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для StudentsListPage.xaml
    /// </summary>
    public partial class StudentsListPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly UserService _userService;
        private readonly VisitService _visitService;
        private readonly QuestionsResultsService _resultsService;

        private ObservableCollection<StudentDTO> _students;
        public ObservableCollection<StudentDTO> Students
        {
            get => _students;
            set
            {
                _students = value;
                OnPropertyChanged();
                UpdateStatusText();
            }
        }

        private StudentDTO _selectedStudent;
        public StudentDTO SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
                UpdateStudentDetail();
            }
        }

        private string _userLoginText;
        public string UserLoginText
        {
            get => _userLoginText;
            set
            {
                _userLoginText = value;
                OnPropertyChanged();
            }
        }

        public StudentsListPage()
        {
            InitializeComponent();
            DataContext = this;

            // Инициализация сервисов
            var context = new InformaticTextBookContext();
            _userService = new UserService(context);
            _visitService = new VisitService(context);
            _resultsService = new QuestionsResultsService(context);

            UserLoginText = $"Преподаватель: {CurrentUser.UserLogin}";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStudents();
        }

        private async Task LoadStudents()
        {
            try
            {
                var users = await _userService.GetAllStudentsAsync();
                var studentDTOs = new List<StudentDTO>();

                foreach (var user in users)
                {
                    var studentDTO = await CreateStudentDTO(user);
                    studentDTOs.Add(studentDTO);
                }

                // Сортировка по логину
                Students = new ObservableCollection<StudentDTO>(
                    studentDTOs.OrderBy(s => s.UserLogin)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<StudentDTO> CreateStudentDTO(ServiceLayer.Models.User user)
        {
            var dto = new StudentDTO
            {
                UserId = user.UserId,
                UserLogin = user.UserLogin,
                RoleName = user.Role?.RoleName ?? "Студент"
            };

            // Загрузка статистики
            await LoadStudentStatistics(dto);

            return dto;
        }

        private async Task LoadStudentStatistics(StudentDTO studentDTO)
        {
            try
            {
                // Получить посещения лекций
                var visits = await _visitService.GetUserVisits(studentDTO.UserId);
                studentDTO.LectionsCount = visits?.Count ?? 0;

                // Получить результаты тестов
                var results = await _resultsService.GetUserResults(studentDTO.UserId);
                studentDTO.TestsCount = results?
                    .GroupBy(r => r.Question.TestId)
                    .Count() ?? 0;

                // Рассчитать средний балл
                if (results != null && results.Any())
                {
                    var testGroups = results
                        .GroupBy(r => r.Question.TestId)
                        .Select(g => new
                        {
                            TestId = g.Key,
                            CorrectAnswers = g.Count(r => r.IsRightAnswer),
                            TotalQuestions = g.Count() / 2
                        });

                    var average = testGroups.Any()
                        ? testGroups.Average(g => (g.CorrectAnswers * 5.0) / g.TotalQuestions)
                        : 0;

                    studentDTO.AverageGrade = Math.Round(average, 1);
                }

                // Общее время
                studentDTO.TotalTimeSeconds = visits?.Sum(v => v.VisitTime ?? 0) ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void UpdateStudentDetail()
        {
            if (SelectedStudent != null)
            {
                NoSelectionBorder.Visibility = Visibility.Collapsed;
                StudentDetailBorder.Visibility = Visibility.Visible;
            }
            else
            {
                NoSelectionBorder.Visibility = Visibility.Visible;
                StudentDetailBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateStatusText()
        {
            StatusText.Text = $"Всего студентов: {Students?.Count ?? 0}";
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadStudents();
            SearchTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Students == null) return;

            var searchText = SearchTextBox.Text.ToLower();
            var allStudents = Students.ToList();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                Students = new ObservableCollection<StudentDTO>(allStudents);
            }
            else
            {
                var filtered = allStudents
                    .Where(s => s.UserLogin.ToLower().Contains(searchText))
                    .ToList();

                Students = new ObservableCollection<StudentDTO>(filtered);
            }
        }

        private void OpenProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStudent != null)
            {
                App.CurrentFrame.Navigate(new ProfilePage(SelectedStudent.UserId));
            }
        }

        private void ViewProgressButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStudent != null)
            {
                MessageBox.Show($"Функция просмотра прогресса для {SelectedStudent.UserLogin}",
                    "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ProfilePage(CurrentUser.UserID));
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = "Успеваемость_студентов.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var exportService = new ExportService(new InformaticTextBookContext());
                    await exportService.ExportStudentGradesAsync(dialog.FileName);
                    MessageBox.Show("Экспорт успешно завершён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

