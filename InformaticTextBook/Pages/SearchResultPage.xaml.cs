using Microsoft.EntityFrameworkCore;
using ServiceLayer;
using ServiceLayer.Data;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace InformaticTextBook.Pages
{
    /// <summary>
    /// Логика взаимодействия для SearchResultPage.xaml
    /// </summary>
    public partial class SearchResultPage : Page
    {
        private readonly SearchService _searchService = new(new InformaticTextBookContext());
        private readonly string _query;

        public string UserLoginText => $"Пользователь: {CurrentUser.UserLogin}";
        public string TitleText => $"Результаты поиска: «{_query}»";

        public SearchResultPage(string query)
        {
            InitializeComponent();
            DataContext = this;
            _query = query;

            // Загружаем результаты сразу в конструкторе
            LoadResults();
        }

        private async void LoadResults()
        {
            try
            {
                var results = await _searchService.SearchAsync(_query);

                MessageBox.Show($"Поиск: '{_query}', найдено: {results.Count}", "Отладка");
                foreach (var r in results)
                    System.Diagnostics.Debug.WriteLine($"  - {r.ThemeName} / {r.LectionName}");

                if (!results.Any())
                {
                    NoResultsText.Visibility = Visibility.Visible;
                    ResultsItemsControl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoResultsText.Visibility = Visibility.Collapsed;
                    ResultsItemsControl.Visibility = Visibility.Visible;
                    ResultsItemsControl.ItemsSource = results;
                }
            }
            catch (Exception ex)
            {
                NoResultsText.Text = $"Ошибка поиска: {ex.Message}";
                NoResultsText.Visibility = Visibility.Visible;
                ResultsItemsControl.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex}");
            }
        }

        private async void Result_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SearchResult result)
            {
                try
                {
                    using var context = new InformaticTextBookContext();
                    var lection = await context.Lections
                        .Include(l => l.Theme)
                        .FirstOrDefaultAsync(l => l.LectionId == result.LectionId);

                    if (lection != null)
                    {
                        App.CurrentFrame.Navigate(new LectionPage(lection));
                    }
                    else
                    {
                        MessageBox.Show("Лекция не найдена в базе данных", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки лекции: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new LectionsNavigatorPage());
        }
    }
}
