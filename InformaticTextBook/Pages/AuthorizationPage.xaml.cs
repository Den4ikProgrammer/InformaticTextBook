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
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public static readonly UserService _userService = new UserService(new InformaticTextBookContext());

        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)//при нажатии на кнопку идет заполнение данных текущего пользователя, если такой зарегестрирован
        {
            var user = await _userService.GetUserByLoginAndPasswordAsync(authorizationLoginTextBox.Text, authorizationPasswordTextBox.Password);
            if (user != null)
            {
                CurrentUser.UserID = user.UserId;
                CurrentUser.Role = user.Role;
                CurrentUser.UserLogin = user.UserLogin;
                //CurrentUser.UserPassword = user.UserPassword;
                App.CurrentFrame.Navigate(new LectionsNavigatorPage());
            }
            else
                IncorrectDataLabel.Visibility = Visibility.Visible;
        }

        private void AuthorizationPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            IncorrectDataLabel.Visibility = Visibility.Hidden;
        }

        private void AuthorizationLoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IncorrectDataLabel.Visibility = Visibility.Hidden;
        }
    }
}
