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

namespace NopMobile.WP
{
    public partial class Settings : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        private const string SaleVQuantity = "SaleValuesQuantity";
        private const string SaleVAmount = "SaleValuesAmount";
        private const string KeywordsInDash = "KeywordsDashboard";
        private const string KeywordsInStats = "KeywordsStatistics";
        private const string SaleValuesDash = "SaleValuesDashboard";

        public Settings()
        {
            InitializeComponent();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            QuantitySale.Value = GetValueOrDefault<int>(SaleVQuantity,5);
            BestQuantity.Text = QuantitySale.Value.ToString();
            AmountSale.Value = GetValueOrDefault<int>(SaleVAmount, 5);
            BestAmount.Text = AmountSale.Value.ToString();
            ValuesInDash.SelectedIndex = GetValueOrDefault<int>(SaleValuesDash, 0 );
            KeywordsDash.Value = GetValueOrDefault<int>(KeywordsInDash, 3);
            PopDash.Text = KeywordsDash.Value.ToString();
            KeywordsStats.Value = GetValueOrDefault<int>(KeywordsInStats, 10);
            PopStats.Text= KeywordsStats.Value.ToString();
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        private void QuantitySale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderSetInteger(QuantitySale,BestQuantity, e);

        }

        private void AmountSale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderSetInteger(AmountSale,BestAmount ,e);
        }

        private void KeywordsDash_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderSetInteger(KeywordsDash,PopDash, e);
        }

        private void ValuesInDash_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ValuesInDash.SelectedItem = e.AddedItems[0];
            } catch(Exception ex){

            }
            
        }

        private void KeywordsStats_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderSetInteger(KeywordsStats,PopStats, e);
        }

        private void SliderSetInteger(Slider Slider, TextBlock Text , RoutedPropertyChangedEventArgs<double> e)
        {
            if (Slider != null)
            {
                Slider.Value = (Math.Round(e.NewValue));
                Text.Text = Slider.Value.ToString("0");
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.AddOrUpdateValue(SaleVQuantity,(int)QuantitySale.Value);
            this.AddOrUpdateValue(SaleVAmount,(int)AmountSale.Value);
            this.AddOrUpdateValue(SaleValuesDash, ValuesInDash.SelectedIndex);
            this.AddOrUpdateValue(KeywordsInDash, (int)KeywordsDash.Value);
            this.AddOrUpdateValue(KeywordsInStats, (int)KeywordsStats.Value);
            MessageBox.Show("Changes stored successfully. \nApp needs to be restarted to apply all changes");
            NavigationService.GoBack();
        }
    }
}