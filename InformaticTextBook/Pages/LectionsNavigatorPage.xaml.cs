using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для LectionsNavigatorPage.xaml
    /// </summary>
    public partial class LectionsNavigatorPage : Page, INotifyPropertyChanged
    {

        public static readonly ThemeService _themeService = new ThemeService(new InformaticTextBookContext());
        public static readonly LectionService _lectionService = new LectionService(new InformaticTextBookContext());
        public static readonly UserService _userService = new UserService(new InformaticTextBookContext());
        public static readonly SearchService _searchService = new SearchService(new InformaticTextBookContext());

        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<Theme> _themes;
        public ObservableCollection<Theme> Themes
        {
            get => _themes;
            set
            {
                if (_themes != value)
                {
                    _themes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Lection> _selectedThemeLections;
        public ObservableCollection<Lection> SelectedThemeLections
        {
            get => _selectedThemeLections;
            set
            {
                if (_selectedThemeLections != value)
                {
                    _selectedThemeLections = value;
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
        public Theme _currentTheme { get; set; }
        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                }
            }
        }

        public Lection _selectedLection { get; set; }
        public Lection SelectedLection
        {
            get => _selectedLection;
            set
            {
                if (_selectedLection != value)
                {
                    _selectedLection = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public LectionsNavigatorPage()
        {
            InitializeComponent();
            if (CurrentUser.Role.RoleId != 0)
            {
                ToProfileButton.Visibility = Visibility.Visible;
                TypingTrainer.Visibility = Visibility.Visible;
                AlgoritmVisualizerButton.Visibility = Visibility.Visible;
            }
            DataContext = this;
            UserLoginText = $"Пользователь: {CurrentUser.UserLogin}";
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadThemes();
        }

        public async Task LoadThemes()
        {
            Themes = new ObservableCollection<Theme>(await _themeService.GetThemesAsync());
        }

        public async Task LoadThemeLections()
        {
            SelectedThemeLections = new ObservableCollection<Lection>(await _lectionService.GetLectionsByThemeIdAsync(CurrentTheme.ThemeId));
        }

        private void ThemesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadThemeLections();
        }

        private void LecturesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            App.CurrentFrame.Navigate(new LectionPage(listBox.SelectedItem as Lection));
        }

        private async void ToProfileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ProfilePage(CurrentUser.UserID));
        }

        private void ExitButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AuthorizationPage());
        }

        private void TypingTrainer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new TypingTrainerPage());
        }

        private void AlgoritmVisualizerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AlgorithmVisualizerPage());
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(query) && query.Length >= 2)
            {
                App.CurrentFrame.Navigate(new SearchResultPage(query));
            }
            else
            {
                MessageBox.Show("Введите не менее 2 символов для поиска",
                    "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Обработчик очистки поиска
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        // Обработчик Enter в поле поиска
        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        // Обработчик изменения текста (активация кнопки)
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchButton.IsEnabled = SearchTextBox.Text.Trim().Length >= 2;
        }
    }
}
