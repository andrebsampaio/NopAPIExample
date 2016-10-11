using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NopMobile.WP.Resources;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Windows.Controls.Primitives;
using NopMobile.AppCore.Exceptions;
namespace NopMobile.WP
{
    public partial class Login : PhoneApplicationPage
    {
        private NopCore api = new NopCore();
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        private IList<string> Urls = new List<string>();
        private bool isNewStore = true;
        private bool Associate = false;
        public Login()
        {
            InitializeComponent();
            CheckIfAssociated();
            var Task = CheckIfStayConnected();
            CheckStoreLinks();
            URL.Text = "absampaio-001-site1.myasp.net";
            User.Text = "demo@demo.com";
            Pass.Password = "password";
        }

        private void CheckIfAssociated()
        {
            if (UserSettings.TryGetValue("associate", out Associate))
            {
                if (Associate)
                {
                    LinkSelector.Visibility = System.Windows.Visibility.Collapsed;
                    URL.Visibility = System.Windows.Visibility.Visible;
                    SaveStore.Visibility = System.Windows.Visibility.Visible;
                    StayConnect.Visibility = System.Windows.Visibility.Collapsed;
                    SaveStore.Visibility = System.Windows.Visibility.Collapsed;
                    SaveStore.IsChecked = true;
                    StayConnect.IsChecked = true;
                    LoginWithNewButton.Visibility = System.Windows.Visibility.Collapsed;
                    UserSettings.Remove("associate");
                    LoginTitle.Text = "Associate New Store";
                }
            }
           
        }        

        private async Task CheckIfStayConnected(){
            bool Stay = false;
            if (UserSettings.TryGetValue("stay_connected", out Stay)){
                if (Stay && !Associate)
                {
                    var Current = "";
                    var Password = "";
                    var URL = "";
                    UserSettings.TryGetValue("current_user",out Current);
                    UserSettings.TryGetValue(Current,out Password);
                    UserSettings.TryGetValue("current_url",out URL);
                    Byte[] pwBytes = Convert.FromBase64String(Password);
                    Byte[] decryptedPw = ProtectedData.Unprotect(pwBytes, null);
                    string pw = System.Text.Encoding.Unicode.GetString(decryptedPw,0,decryptedPw.Length);
                    await RunLogin(Current, pw, URL, true);
                }
            }
        }

        private void CheckStoreLinks()
        {
            if (!Associate)
            {
                foreach (KeyValuePair<string, object> k in UserSettings)
                {
                    if (k.Key.StartsWith("store_url_"))
                    {
                        Urls.Add((string)k.Value);
                    }
                }

                if (Urls.Count > 0)
                {
                    ExistingStore();
                }
                else
                {
                    NewStore();
                }
            }
            
        }

        private void NewStore()
        {
            isNewStore = true;
            LinkSelector.Visibility = System.Windows.Visibility.Collapsed;
            URL.Visibility = System.Windows.Visibility.Visible;
            SaveStore.Visibility = System.Windows.Visibility.Visible;
            LoginWithNewButton.Content = "Login With Existing";
        }

        private void ExistingStore()
        {
            isNewStore = false;
            LinkSelector.ItemsSource = Urls;
            LinkSelector.Visibility = System.Windows.Visibility.Visible;
            URL.Visibility = System.Windows.Visibility.Collapsed;
            LoginWithNewButton.Visibility = System.Windows.Visibility.Visible;
            SaveStore.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowLoading(){
            FormHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideLoading()
        {
            FormHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var URLString = URL.Text;
            ShowLoading();
            if (!isNewStore)
            {
                URLString = LinkSelector.SelectedItem.ToString();
            }
            var LoginTask = RunLogin(User.Text, Pass.Password, URLString, false);
        }

        private void NewStore_Click(object sender, RoutedEventArgs e)
        {
            if (!isNewStore)
                NewStore();
            else
                ExistingStore();
        }

        private async Task RunLogin(string User, string Password, string URL, bool StayConnected)
        {
            try
            {
                api.SetStoreUrl(URL);
                var LoginResult = await api.CheckLogin(User, Password);
                if (!LoginResult)
                {
                    throw new LoginException("The credentials you entered are incorrect");
                }
                if (LoginResult && StayConnected)
                {
                    NavigationService.Navigate(new Uri("/Pages/Dashboard.xaml", UriKind.Relative));
                }
                else if (LoginResult && !StayConnected)
                {
                    if (Convert.ToBoolean(SaveStore.IsChecked))
                    {
                        var StoreName = await api.GetStoreName();
                        if (!UserSettings.Contains("store_url_" + StoreName))
                        {
                            UserSettings.Add("store_url_" + StoreName, URL);
                        }
                    }
                    if (Convert.ToBoolean(StayConnect.IsChecked))
                    {
                        var EncryptedPass = ProtectedData.Protect(System.Text.Encoding.Unicode.GetBytes(Password), null);
                        var EncryptedString = Convert.ToBase64String(EncryptedPass);
                        UserSettings.Add(User, EncryptedString);
                        if (UserSettings.Contains("stay_connected")) UserSettings.Remove("stay_connected");
                        UserSettings.Add("stay_connected", true);
                    }
                    if (UserSettings.Contains("current_user")) UserSettings.Remove("current_user");
                    UserSettings.Add("current_user", User);
                    if (UserSettings.Contains("current_url")) UserSettings.Remove("current_url");
                    UserSettings.Add("current_url", URL);
                    NavigationService.Navigate(new Uri("/Pages/Dashboard.xaml", UriKind.Relative));
                }
            }catch (LoginException le)
            {
                HideLoading();
                MessageBox.Show(le.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (!Associate)
            {
                UserSettings.Save();
                Application.Current.Terminate();
            }
            
        }

        private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermark.Visibility = Visibility.Collapsed;
        }

        private void PasswordWatermark_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PasswordWatermark.Visibility = Visibility.Collapsed;
            Pass.Focus();
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermark.Visibility = Pass.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}