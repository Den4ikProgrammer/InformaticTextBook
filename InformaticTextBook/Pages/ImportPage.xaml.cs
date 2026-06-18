using Microsoft.Win32;
using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InformaticTextBook.Pages
{
    public partial class ImportPage : Page, INotifyPropertyChanged
    {
        private readonly ImportService _importService;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public ImportPage()
        {
            InitializeComponent();
            DataContext = this;

            // Инициализация сервиса
            var context = new InformaticTextBookContext();
            _importService = new ImportService(context);

            UserLoginText = $"Преподаватель: {CurrentUser.UserLogin}";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Проверка прав доступа
            if (CurrentUser.Role.RoleId != 1) // Не преподаватель
            {
                MessageBox.Show("Только преподаватели могут импортировать данные",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.CurrentFrame.Navigate(new LectionsNavigatorPage());
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Выберите DOCX файл"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = dialog.FileName;
                UpdateFileInfo(dialog.FileName);
                ImportButton.IsEnabled = true;
            }
        }

        private void UpdateFileInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                FileInfoText.Text = $"Файл: {fileInfo.Name}\n" +
                                   $"Размер: {fileInfo.Length / 1024} KB\n" +
                                   $"Изменен: {fileInfo.LastWriteTime:dd.MM.yyyy HH:mm}";
            }
            catch
            {
                FileInfoText.Text = "Не удалось получить информацию о файле";
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportButton.IsEnabled = false;
            ImportButton.Content = "Импорт...";
            try
            {
                var result = await _importService.ImportFromDocxAsync(FilePathTextBox.Text);
                ShowResults(result);

                if (result.Success)
                {
                    var dialogResult = MessageBox.Show(
                        "Импорт успешно завершен. Хотите импортировать еще данные?",
                        "Импорт завершен",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (dialogResult == MessageBoxResult.No)
                    {
                        App.CurrentFrame.Navigate(new ProfilePage(CurrentUser.UserID));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ImportButton.IsEnabled = true;
                ImportButton.Content = "Начать импорт";
            }
        }

        private void ShowResults(ImportResult result)
        {
            ResultBorder.Visibility = Visibility.Visible;

            if (result.Success)
            {
                ResultTitle.Text = "✅ Импорт успешно завершен";
                ResultTitle.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ResultTitle.Text = "⚠ Импорт завершен с ошибками";
                ResultTitle.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }

            ResultText.Text = result.GetSummary();
            ErrorList.ItemsSource = result.Errors;

            // Прокрутить к результатам
            ResultBorder.BringIntoView();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilePathTextBox.Text = string.Empty;
            FileInfoText.Text = string.Empty;
            ResultBorder.Visibility = Visibility.Collapsed;
            ImportButton.IsEnabled = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ProfilePage(CurrentUser.UserID));
        }

        private void DownloadTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                FileName = "Шаблон_лекции.docx",
                Title = "Сохранить шаблон"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _importService.CreateTemplateDocx(dialog.FileName);
                    MessageBox.Show("Шаблон сохранён!", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания шаблона: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}