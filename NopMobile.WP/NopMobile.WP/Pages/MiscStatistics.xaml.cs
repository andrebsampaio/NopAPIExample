using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using NopMobile.AppCore.NopAPI;
using System.Windows.Media;

namespace NopMobile.WP
{
    public partial class MiscStatistics : PhoneApplicationPage
    {
        NopCore api = new NopCore();

        private const int SIZE1 = 33;
        private const int SIZE2 = 36;
        private const int SIZE3 = 39;
        private const int SIZE4 = 43;
        private const int SIZE5 = 46;
        private const int SIZE6 = 49;
        private const string KeywordsInStats = "KeywordsStatistics";
        Settings settings = new Settings();
        
        public MiscStatistics()
        {
            InitializeComponent();
            InitializeStats();
        }

        private async Task InitializeStats(){

            var PopularKeywords = await api.GetPopularKeywords(20);
            var PopularByCount = new List<KeywordDTO>();
            PopularByCount.AddRange(PopularKeywords);
            var PopularByAlphabet = new List<KeywordDTO>();
            PopularByAlphabet.AddRange(PopularKeywords);
            PopularByAlphabet.Sort(delegate(KeywordDTO x, KeywordDTO y) { return x.Keyword.CompareTo(y.Keyword); });

            var MaxCount = PopularByCount.First().Count;
            var MinCount = PopularByCount.Last().Count;

            var Counter = 0;
            var NPopWords = settings.GetValueOrDefault<int>(KeywordsInStats,10);
            foreach (KeywordDTO K in PopularByAlphabet)
            {
                if (Counter == NPopWords) break;
                var Word = new TextBlock();
                var Delta =  1 + ((double)(MaxCount - MinCount) / 6);
                if (K.Count <= Delta)
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 176, 229, 124));
                    Word.FontSize = SIZE1;
                }
                else if (K.Count > Delta && K.Count < Delta * 2)
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 216, 231));
                    Word.FontSize = SIZE2;
                }
                else if (K.Count > Delta * 2 && K.Count < Delta * 3)
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 86, 186, 236));
                    Word.FontSize = SIZE3;
                }
                else if (K.Count > Delta * 3 && K.Count < Delta * 4)
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 240, 170));
                    Word.FontSize = SIZE4;
                }
                else if (K.Count > Delta * 4 && K.Count < Delta * 5)
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 174, 174));
                    Word.FontSize = SIZE5;
                }
                else
                {
                    Word.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 236, 148));
                    Word.FontSize = SIZE6;
                }

                Word.Text = K.Keyword;
                Word.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                Word.Margin = new Thickness(0, 0, 5, 0);
                
                KeywordsHolder.Children.Add(Word);
            }

            var WeekCustomers = await api.GetCustomerCountByTime(7);
            var TwoWeeksCustomers = await api.GetCustomerCountByTime(14);
            var MonthCustomers = await api.GetCustomerCountByTime(30);
            var YearCustomers = await api.GetCustomerCountByTime(365);

            UsersSeven.Text = WeekCustomers.ToString();
            UsersFour.Text = TwoWeeksCustomers.ToString();
            UsersMonth.Text = MonthCustomers.ToString();
            UsersYear.Text = YearCustomers.ToString();

            var WeekSales = await api.GetTotalSalesByTime(7);
            var TwoWeeksSales = await api.GetTotalSalesByTime(14);
            var MonthSales = await api.GetTotalSalesByTime(30);
            var YearSales = await api.GetTotalSalesByTime(365);
            var Currency = await api.GetCurrency();

            TotalSeven.Text = WeekSales.ToString("0.0#") + " " + Currency;
            TotalFour.Text = TwoWeeksSales.ToString("0.0#")  + " " + Currency;
            TotalMonth.Text = MonthSales.ToString("0.0#")  + " " + Currency;
            TotalYear.Text = YearSales.ToString("0.0#") + " " + Currency;
        }
    }
}