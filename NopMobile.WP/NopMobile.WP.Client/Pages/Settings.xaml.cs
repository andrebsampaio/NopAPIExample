using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace NopMobile.WP.Client.Pages
{
    public partial class Settings : PhoneApplicationPage
    {
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!UserSettings.Contains("featured_count"))
            {
                UserSettings.Add("featured_count", 3);
            }
            if (!UserSettings.Contains("bestsellers_count"))
            {
                UserSettings.Add("bestsellers_count", 4);
            }
            if (UserSettings.Contains("featured_count"))
            {
                FeaturedSlider.Value = Helpers.Helper.GetSetting<int>("featured_count");
            }
            if (UserSettings.Contains("bestsellers_count"))
            {
                BestSlider.Value = Helpers.Helper.GetSetting<int>("bestsellers_count");
            }
        }

        private void Bestsellers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.SliderSetInteger(BestSlider, Bestsellers, e);
        }

        private void FeaturedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.SliderSetInteger(FeaturedSlider, FeaturedProducts, e);

        }

        private void SliderSetInteger(Slider Slider, TextBlock Text, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Slider != null)
            {
                Slider.Value = Math.Round(e.NewValue);
                Text.Text = Slider.Value.ToString("0");
            }
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            Helpers.Helper.AddOrUpdateValue("bestsellers_count", (int)BestSlider.Value);
            Helpers.Helper.AddOrUpdateValue("featured_count", (int)FeaturedSlider.Value);
            MessageBox.Show("Changes stored successfully. \nApp needs to be restarted to apply all changes");
            NavigationService.GoBack();
        }
    }
}