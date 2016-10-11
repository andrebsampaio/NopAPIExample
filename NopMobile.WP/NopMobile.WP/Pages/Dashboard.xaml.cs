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
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using NopMobile.AppCore;
using NopMobile.AppCore.NopAPI;

namespace NopMobile.WP
{
    public partial class Dashboard : PhoneApplicationPage
    {
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        NopCore api = new NopCore();
        IDictionary<String, String> UrlsMap = new Dictionary<String, String>();
        IList<String> urls = new List<String>();
        Settings settings = new Settings();
        bool ChangeStore = false;
        string UserName = "";
        string StoreUrl = "";
        const string LOADMESSAGE = "Loading..";

        private const string KeywordsInDash = "KeywordsDashboard";
        private const string SaleValuesDash = "SaleValuesDashboard";

        public Dashboard()
        {
            InitializeComponent();
            PanoramaControl.DefaultItem = PanoramaControl.Items[1];
            var LoadStore = InitializeStore();
        }

        private void ResetValues()
        {
            SideMenu.Header = LOADMESSAGE;
            PendingSales.Text = LOADMESSAGE;
            CompleteSales.Text = LOADMESSAGE;
            CancelledSales.Text = LOADMESSAGE;
            AdminName.Text = LOADMESSAGE;
            PendingOrders.Text = LOADMESSAGE;
            Carts.Text = LOADMESSAGE;
            Wishlists.Text = LOADMESSAGE;
            Visitors.Text = LOADMESSAGE;
            Registered.Text = LOADMESSAGE;
            Vendors.Text = LOADMESSAGE;
            WordsHolder.Children.Clear();
            var Sp = new StackPanel();
            var WordBlock = new TextBlock();
            WordBlock.Text = LOADMESSAGE;
            WordBlock.FontSize = 20;
            Sp.Background = new SolidColorBrush(Colors.Gray);
            WordBlock.Margin = new System.Windows.Thickness { Left = 5, Right = 5 };
            Sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Sp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Sp.Margin = new System.Windows.Thickness { Left = 5 };
            Sp.Children.Add(WordBlock);
            WordsHolder.Children.Add(Sp);
            BestsellerAmount.Text = LOADMESSAGE;
            BestsellerQuantity.Text = LOADMESSAGE;


        }

        private async Task InitializeStore(){
            
            var Current = "";
            if (ChangeStore)
            {
                ResetValues();
                UserSettings.Remove("current_url");
                UserSettings.Add("current_url", StoreUrl);
                UserSettings.Remove("current_user");
                UserSettings.Add("current_user", UserName);  
            }
            UserSettings.TryGetValue("current_user", out Current);
            SideMenu.Header = await api.GetStoreName();
            SideMenu.Header = SideMenu.Header.ToString().ToLower();
            AdminName.Text = await api.GetFullName(Current);
            if (!ChangeStore)
            {
                foreach (KeyValuePair<string, object> url in UserSettings)
                {
                    if (url.Key.StartsWith("store_url_"))
                    {
                        var Name = url.Key.Substring(10);
                        UrlsMap.Add(new KeyValuePair<String, String>(Name, (string)url.Value));
                        urls.Add(Name);
                    }
                }
                MyStoresList.ItemsSource = urls;
            }
            var Currency = await api.GetCurrency();

            var SaleValuesIn = settings.GetValueOrDefault<int>(SaleValuesDash, 0);

            var PendingV = await api.GetStats(3);
            var CompleteV = await api.GetStats(2);
            var CancelledV = await api.GetStats(1);

            if (SaleValuesIn == 0)
            {
                PendingSales.Text = PendingV + " " + Currency; ;
                CompleteSales.Text = CompleteV + " " + Currency; ;
                CancelledSales.Text = CancelledV  + " " + Currency; ;
            }
            else
            {
                PendingSales.Text = PendingV.ToString("0.0#") + " " + Currency; ;
                CompleteSales.Text = CompleteV.ToString("0.0#") + " " + Currency; ;
                CancelledSales.Text = CancelledV.ToString("0.0#") + " " + Currency; ;
            }

            var PendingOrdersCount = await api.GetPendingOrdersCount();
            var CartsCount = await api.GetCartsCount();
            var WishlistCount = await api.GetWishlistCount();
            PendingOrders.Text = PendingOrdersCount.ToString();
            Carts.Text = CartsCount.ToString();
            Wishlists.Text = WishlistCount.ToString();

            var VisitorsCount = await api.GetOnlineCount();
            var RegisteredCount = await api.GetRegisteredCustomersCount();
            var VendorsCount = await api.GetVendorsCount();
            Visitors.Text = VisitorsCount.ToString();
            Registered.Text = RegisteredCount.ToString();
            Vendors.Text = VendorsCount.ToString();

            var NPopWords = settings.GetValueOrDefault<int>(KeywordsInDash, 3);

            var PopularWords = await api.GetPopularKeywords(NPopWords);
            WordsHolder.Children.Clear();
            foreach (KeywordDTO word in PopularWords)
            {
                var Sp = new StackPanel();
                var WordBlock = new TextBlock();
                WordBlock.Text = word.Keyword;
                WordBlock.FontSize = 20;
                Sp.Background = new SolidColorBrush(Colors.Gray);
                WordBlock.Margin = new System.Windows.Thickness { Left = 5, Right = 5 };
                Sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                Sp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                Sp.Margin = new System.Windows.Thickness { Left = 5 };
                Sp.Children.Add(WordBlock);
                WordsHolder.Children.Add(Sp);
            }
            var BestAmount = await api.GetBestsellerByAmount();
            BestsellerAmount.Text = BestAmount.First().Product.Name;
            var BestQuantity = await api.GetBestsellerByQuantity();
            BestsellerQuantity.Text = BestQuantity.First().Product.Name;

            var WeekCustomers = await api.GetCustomerCountByTime(7);
            var TwoWeeksCustomers = await api.GetCustomerCountByTime(14);
            var MonthCustomers = await api.GetCustomerCountByTime(30);
            var YearCustomers = await api.GetCustomerCountByTime(365);

            var registered = new int[4] { WeekCustomers, TwoWeeksCustomers, MonthCustomers, YearCustomers };

            RegisteredUsersGraph Graph = new RegisteredUsersGraph(registered,true);
            RegisteredPlot.Model = Graph.MyModel;

            ChangeStore = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) 
        {
            UserSettings.Save();
            Application.Current.Terminate();
        }

        private void AssociateNewStore_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            UserSettings.Add("associate", true);
            NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
        }

        private void Configurations_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
        }

        private void Help_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();

            webBrowserTask.Uri = new Uri("http://www.nopcommerce.com/documentation.aspx", UriKind.Absolute);

            webBrowserTask.Show();
        }

        private void Logout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
             IAsyncResult result = Microsoft.Xna.Framework.GamerServices.Guide.BeginShowMessageBox("Logout","Are you sure you want to Logout?",
                new string[] { "Yes", "No" },0,Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.None,null,null);
            result.AsyncWaitHandle.WaitOne();
            int? choice = Microsoft.Xna.Framework.GamerServices.Guide.EndShowMessageBox(result);
            if (choice.HasValue)
            {
                if (choice.Value == 0)
                {
                    UserSettings.Remove("current_user");
                    UserSettings.Remove("current_url");
                    UserSettings.Remove("stay_connected");
                    NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
                }
            }
        }

        private void MyStoresList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyStoresList.SelectedIndex == -1)
                return;
            else
            {
                StackPanel Sp = new StackPanel();
                PhoneTextBox User = new PhoneTextBox();
                User.Hint = "Username";
                PasswordBox Pass = new PasswordBox();
                Pass.Password = "password";
                Sp.Children.Add(User);
                Sp.Children.Add(Pass);
                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Caption = "Change Store",
                    Message = "Want to change to " + urls[MyStoresList.SelectedIndex] + "?",
                    LeftButtonContent = "Yes",
                    RightButtonContent = "No"
                };

                messageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            CustomMessageBox LoginBox = new CustomMessageBox()
                            {
                                Caption = "Login to " + urls[MyStoresList.SelectedIndex],
                                Message = "Enter your credentials",
                                Content = Sp,
                                LeftButtonContent = "Login",
                                RightButtonContent = "Cancel"
                            };
                            LoginBox.Dismissed += async (s2, e2) =>
                            {
                                switch (e2.Result)
                                {
                                    case CustomMessageBoxResult.LeftButton:
                                        
                                        string Url = "";
                                        UrlsMap.TryGetValue(urls[MyStoresList.SelectedIndex], out Url);
                                        api.SetStoreUrl(Url);
                                        var loginResult = await api.CheckLogin(User.Text, Pass.Password);
                                        if (loginResult)
                                        {
                                            StoreUrl = Url;
                                            loginResult = false;
                                            ChangeStore = true;
                                            UserName = User.Text;
                                            var TaskReload = InitializeStore();
                                            MyStoresList.SelectedIndex = -1;
                                        }
                                        else
                                        {
                                            MessageBox.Show("The credentials seem to be incorrect");
                                            MyStoresList.SelectedIndex = -1;
                                        }
                                        break;
                                    case CustomMessageBoxResult.RightButton:
                                        MyStoresList.SelectedIndex = -1;
                                        LoginBox.Dismiss();
                                        break;
                                    default:
                                        break;
                                }
                            };

                            LoginBox.Show();
                            break;
                        case CustomMessageBoxResult.RightButton:
                            MyStoresList.SelectedIndex = -1;
                            messageBox.Dismiss();
                            break;
                        default:
                            break;
                    }
                };

                messageBox.Show();
            }
            
        }

        private void SaleValues_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SaleValues.xaml", UriKind.Relative));
        }

        private void carts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SearchCarts.xaml", UriKind.Relative));
        }

        private void SearchOrders_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SearchOrders.xaml", UriKind.Relative));
        }

        private void SearchCustomers_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SearchCustomers.xaml", UriKind.Relative));
        }

        private async void LatestOrders_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var OrdersList = new ListBox();
            var Orders = await api.GetLatestOrders(5);
            var Result = new List<OrderData>();
            OrdersList.ItemTemplate = App.FindResource<DataTemplate>(this, "OrderTemplate");
            OrdersList.SelectionChanged += OrdersList_SelectionChanged;
            foreach (OrderDTO o in Orders)
            {
                 SolidColorBrush color = null;
                 switch (o.OrderStatus)
                 {
                     case AppCore.NopAPI.OrderStatus.Complete:
                         color = new SolidColorBrush(Colors.Green);
                         break;
                     case AppCore.NopAPI.OrderStatus.Pending:
                         color = new SolidColorBrush(Colors.Gray);
                         break;
                     case AppCore.NopAPI.OrderStatus.Cancelled:
                         color = new SolidColorBrush(Colors.Red);
                         break;
                     case AppCore.NopAPI.OrderStatus.Processing:
                         color = new SolidColorBrush(Colors.Yellow);
                         break;
                 }
                 var Order = new OrderData { Email = o.OrderEmail, ProductName = o.ProductsList.First().Product.Name, Id = o.OrderID, BackgroundColor = color };
                 Result.Add(Order);
            }
            OrdersList.ItemsSource = Result;
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Latest Orders",
                Message = "These are the latest orders",
                Content = OrdersList,
                LeftButtonContent = "Dismiss"
            };
            messageBox.Show();
        }

        void OrdersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Results = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (Results.SelectedIndex == -1)
                return;
            var selectedId = 0;
            if (e.AddedItems[0] != null)
            {
                selectedId = (e.AddedItems[0] as OrderData).Id;
            }


            // Navigate to the new page
            NavigationService.Navigate(new Uri("/Pages/OrderDetails.xaml?selectedId=" + selectedId, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            Results.SelectedIndex = -1;
        }

        private async void BestCustomers_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var Customers = await api.GetBestCustomers();
            var CustomersList = new ListBox();
            CustomersList.ItemTemplate = App.FindResource<DataTemplate>(this, "UserTemplate");
            CustomersList.SelectionChanged += CustomersList_SelectionChanged;
            var Result = new List<SearchCustomers.CustomerData>();
            foreach (CustomerDTO c in Customers)
            {
                var Customer = new SearchCustomers.CustomerData { Email = c.Email, Fullname = c.FullName, Id = c.Id };
                Result.Add(Customer);
            }

            CustomersList.ItemsSource = Result;

            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Best Customers",
                Message = "These are the top customers in the store",
                Content = CustomersList,
                LeftButtonContent = "Dismiss"
            };
            messageBox.Show();
        }

        void CustomersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Results = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (Results.SelectedIndex == -1)
                return;
            var CustomerEmail = "";

            if (e.AddedItems[0] != null)
            {
                CustomerEmail = (e.AddedItems[0] as SearchCustomers.CustomerData).Email;
            }


            // Navigate to the new page
            NavigationService.Navigate(new Uri("/Pages/CustomerDetails.xaml?Email=" + CustomerEmail, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            Results.SelectedIndex = -1;
        }

        private void MiscStats_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/MiscStatistics.xaml", UriKind.Relative));
        }
    }
}