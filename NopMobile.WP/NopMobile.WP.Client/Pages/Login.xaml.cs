using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NopMobile.WP.Client.Resources;
using System.Threading.Tasks;
using NopMobile.AppCore.Exceptions;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;

namespace NopMobile.WP.Client
{
    public partial class Login : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        public Login()
        {
            InitializeComponent();
            User.Text = "demo@demo.com";
            PasswordTextBox.Password = "password";
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (UserSettings.Contains("stay_connected"))
            {
                var Stay = false;
                UserSettings.TryGetValue("stay_connected", out Stay);
                if (Stay)
                {
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }
        }

        private async Task LoginStore()
        {
            ShowLoading();
            try 
            {
                //var Username = User.Text;
                //var Password = PasswordTextBox.Password;
                var Username = "demo@demo.com";
                var Password = "password";
                var LoginResult = await api.CheckLoginClient(Username, Password);
                if (!LoginResult)
                {
                    throw new LoginException("The credentials you entered are incorrect");
                }
                else
                {
                    if (Convert.ToBoolean(StayConnected.IsChecked))
                    {
                        if (UserSettings.Contains("stay_connected"))
                        {
                            UserSettings.Remove("stay_connected");
                            UserSettings.Add("stay_connected", true);
                        }
                        else
                        {
                            UserSettings.Add("stay_connected", true);
                        }
                    }
                    StoreCredentials(Username, Password);
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }
            catch (LoginException le)
            {
                HideLoading();
                MessageBox.Show(le.Message);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Save();
            Application.Current.Terminate();
        }

        private void StoreCredentials(string User, string Pass)
        {
            var EncryptedPass = ProtectedData.Protect(System.Text.Encoding.Unicode.GetBytes(Pass), null);
            var EncryptedString = Convert.ToBase64String(EncryptedPass);
            if (UserSettings.Contains("current_user") || UserSettings.Contains(User))
            {
                UserSettings.Remove(User);
                UserSettings.Remove("current_user");
            }
            UserSettings.Add(User, EncryptedString);
            UserSettings.Add("current_user", User);
        }

        private string [] GetCurrentUserCredentials()
        {
            var credentials = new string [2];
            var Current = "";
            var Password = "";
            var URL = "";
            UserSettings.TryGetValue("current_user", out Current);
            UserSettings.TryGetValue(Current, out Password);
            UserSettings.TryGetValue("current_url", out URL);
            Byte[] pwBytes = Convert.FromBase64String(Password);
            Byte[] decryptedPw = ProtectedData.Unprotect(pwBytes, null);
            string pw = System.Text.Encoding.Unicode.GetString(decryptedPw, 0, decryptedPw.Length);
            credentials[0] = Current; credentials[1] = pw;
            return credentials;
        }

        private void ShowLoading()
        {
            SystemTray.IsVisible = false;
            TitlePanel.Visibility = System.Windows.Visibility.Collapsed;
            FormHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideLoading()
        {
            SystemTray.IsVisible = true;
            TitlePanel.Visibility = System.Windows.Visibility.Visible;
            FormHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermark.Visibility = Visibility.Collapsed;
        }

        private void PasswordWatermark_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PasswordWatermark.Visibility = Visibility.Collapsed;
            PasswordTextBox.Focus();
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermark.Visibility = PasswordTextBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Register.xaml", UriKind.Relative));
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            this.LoginStore();
        }
    }
}