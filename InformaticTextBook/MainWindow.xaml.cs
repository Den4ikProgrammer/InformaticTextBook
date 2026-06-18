using InformaticTextBook.Pages;
using System.Windows;

namespace InformaticTextBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.CurrentFrame = MainFrame;
            MainFrame.Navigate(new AuthorizationPage());

            using (var context = new ServiceLayer.Data.InformaticTextBookContext())
            {
                // Словарь известных паролей
                var passwordDictionary = new Dictionary<string, string>
                {
                    { "teacher1", "pass123" },
                    { "student1", "stud123" },
                    { "student2", "stud456" },
                    { "teacher2", "teach789" },
                    { "q", "q" }
                };

                var users = context.Users.ToList();
                foreach (var user in users)
                {
                    if (passwordDictionary.TryGetValue(user.UserLogin, out var plainPassword))
                    {
                        user.UserPassword = ServiceLayer.Services.UserService.HashPassword(plainPassword);
                    }
                }
                context.SaveChanges();
            }

        }
    }
}