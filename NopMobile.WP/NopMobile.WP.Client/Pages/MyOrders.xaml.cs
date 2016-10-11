using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NopMobile.AppCore.NopAPI;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace NopMobile.WP.Client.Pages
{
    public partial class MyOrders : PhoneApplicationPage
    {
        OrderDTO [] Orders;
        CustomerDTO Customer;
        NopCore api = new NopCore();
        public MyOrders()
        {
            InitializeComponent();
            InitializeOrders();
        }

        private async void InitializeOrders(){
            var CustomerTmp = await api.GetCustomersByEmail(Helpers.Helper.CurrentCustomer());
            var Currency = await api.GetCurrency();
            Customer = CustomerTmp.First();
            Orders = await api.CustomerIdOrders(Customer.Id);
            var Result = new List<OrderItemData>();
            foreach (OrderDTO o in Orders)
            {
                SolidColorBrush Status = null;
                switch (o.OrderStatus){
                    case OrderStatus.Complete:
                        Status = new SolidColorBrush(Colors.Green);
                        break;
                    case OrderStatus.Cancelled:
                        Status = new SolidColorBrush(Colors.Red);
                        break;
                    case OrderStatus.Pending:
                        Status = new SolidColorBrush(Colors.Yellow);
                        break;
                    case OrderStatus.Processing:
                        Status = new SolidColorBrush(Colors.Orange);
                        break;
                }
                Result.Add(new OrderItemData { Id = o.OrderID, Date = o.CreateDate.ToString(), Status = o.OrderStatus.ToString(), Image = Helpers.Helper.ConvertToBitmapImage(o.ProductsList.First().Product.Image.First()), StatusColor = Status, Total = o.Total.ToString("0.0#") + " " + Currency });
            }
            OrdersList.ItemsSource = Result;
            HideLoading();
        }

        private class OrderItemData {
            public int Id {get;set;}
            public BitmapImage Image {get;set;}
            public SolidColorBrush StatusColor {get;set;}
            public string Date {get;set;}
            public string Status { get; set; }
            public string Total { get; set; }
        }

        private void OrdersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersList.SelectedIndex != -1)
            {
                var Item = OrdersList.SelectedItem as OrderItemData;
                NavigationService.Navigate(new Uri("/Pages/OrderDetails.xaml?OrderId=" + Item.Id , UriKind.Relative));
            }
            OrdersList.SelectedIndex = -1;
        }

        private void ShowLoading()
        {
            OrdersList.Visibility = System.Windows.Visibility.Collapsed;
            SearchHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideLoading()
        {
            OrdersList.Visibility = System.Windows.Visibility.Visible;
            SearchHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}