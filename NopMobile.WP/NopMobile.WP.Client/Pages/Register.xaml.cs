using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Text.RegularExpressions;

namespace NopMobile.WP.Client.Pages
{
    public partial class Register : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        bool Male = true;
        public Register()
        {
            InitializeComponent();
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

        private void PasswordTextBoxConfirm_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermarkConfirm.Visibility = Visibility.Collapsed;
        }

        private void PasswordWatermarkConfirm_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PasswordWatermarkConfirm.Visibility = Visibility.Collapsed;
            PassConfirm.Focus();
        }

        private void PasswordTextBoxConfirm_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordWatermarkConfirm.Visibility = PassConfirm.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void Submit_Click(object sender, EventArgs e)
        {
            if (!IsValidEmail(Email.Text))
            {
                MessageBox.Show("The email has an invalid format");
            }

            else if (!(Firstname.Text != null && Lastname.Text != null && !Firstname.Text.Equals("") && !Lastname.Text.Equals("")))
            {
                MessageBox.Show("Firstname and/or Lastname cannot be empty");
            }
            else if (!(Pass.Password.Length >= 6))
            {
                MessageBox.Show("The password should have at least 6 characters");
            }
            else if (!Pass.Password.Equals(PassConfirm.Password))
            {
                MessageBox.Show("The password and confirmation must match");
            }
            else if (Pass.Password.Equals(PassConfirm.Password) && IsValidEmail(Email.Text) && Firstname.Text != null && Lastname.Text != null && !Firstname.Text.Equals("") && !Lastname.Text.Equals("") && Pass.Password.Length >= 6 )
            {
                var RegisterResult = await api.RegisterClient(Male, Firstname.Text, Lastname.Text, Birthday.Value.Value, Email.Text, Company.Text, Pass.Password);
                if (!RegisterResult)
                {
                    MessageBox.Show("That email already exists, select another one");
                    Pass.Password = "";
                    PassConfirm.Password = "";
                }
                else
                {
                    MessageBox.Show("The registration was concluded successfully");
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }
            else
            {
                MessageBox.Show("There's something wrong in the registration, please review");
            }
            
        }

        public bool IsValidEmail(string Email)
        {
            return Regex.IsMatch(Email.Trim(), @"\A(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?)\Z");
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Cancel registration",
                Message = "Are you sure you want to cancel registration?",
                LeftButtonContent = "Yes",
                RightButtonContent = "No"
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
                        break;
                }
            };

            messageBox.Show();
        }

        private void Gender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Gender = (sender as ListPicker).SelectedIndex;
            if (Gender == 0)
            {
                Male = true;
            }
            else
            {
                Male = false;
            }
        }
    }
}