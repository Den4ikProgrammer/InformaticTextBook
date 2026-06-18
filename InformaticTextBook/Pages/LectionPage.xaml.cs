using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для LectionPage.xaml
    /// </summary>
    public partial class LectionPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly VisitService _visitService = new VisitService(new InformaticTextBookContext());
        private Stopwatch _stopwatch;
        private DispatcherTimer _timer;
        private Visit _currentVisit;
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
        private string _elapsedTimeText = "0 сек";
        public string ElapsedTimeText
        {
            get => _elapsedTimeText;
            set
            {
                if (_elapsedTimeText != value)
                {
                    _elapsedTimeText = value;
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
        private BitmapImage _currentImage;
        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public LectionPage(Lection selectedLection)
        {
            InitializeComponent();
            DataContext = this;
            SelectedLection = selectedLection;
            UserLoginText = $"Пользователь: {CurrentUser.UserLogin}";
            SetImage(selectedLection.LectionId.ToString());
            Loaded += LectionPage_Loaded;
            Unloaded += LectionPage_Unloaded;
        }
        private async void LectionPage_Loaded(object sender, RoutedEventArgs e)
        {
            // создаём визит
            _currentVisit = await _visitService.CreateOrGetVisitAsync(CurrentUser.UserID, SelectedLection.LectionId);
            TimeSpan lectionTime = TimeSpan.FromSeconds(Convert.ToDouble(_currentVisit.VisitTime));
            ElapsedTimeText = lectionTime.TotalMinutes >= 1
                   ? $"{lectionTime.Minutes} мин {lectionTime.Seconds} сек"
                   : $"{lectionTime.Seconds} сек";

            // запускаем таймер
            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, args) =>
            {
                var elapsed = _stopwatch.Elapsed + TimeSpan.FromSeconds(Convert.ToDouble(_currentVisit.VisitTime));
                ElapsedTimeText = elapsed.TotalMinutes >= 1
                    ? $"{elapsed.Minutes} мин {elapsed.Seconds} сек"
                    : $"{elapsed.Seconds} сек";
            };
            _timer.Start();
        }

        private async void LectionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_stopwatch != null)
            {
                _stopwatch.Stop();
                _timer?.Stop();

                // сохраняем в БД время (в секундах)
                int seconds = (int)_stopwatch.Elapsed.TotalSeconds;
                await _visitService.UpdateVisitTimeAsync(CurrentUser.UserID, SelectedLection.LectionId, seconds);
            }
        }

        public void SetImage(string imageName)
        {
            try
            {
                CurrentImage = new BitmapImage(new Uri($"pack://application:,,,/Images/{imageName}.png"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ресурс не найден");
                LectionImage.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());
        }

        private void ToTestButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new TestPage(SelectedLection));
        }

        private async void ToProfileButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ProfilePage(CurrentUser.UserID));
        }
    }
}
