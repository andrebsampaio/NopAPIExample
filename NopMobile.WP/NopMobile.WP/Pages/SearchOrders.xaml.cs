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
using System.Windows.Input;
using System.Windows.Media;

namespace NopMobile.WP
{
    public partial class SearchOrders : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        OrderDTO[] OrdersList;
        string Filter = "Email";
        string Search;
        string[] FilterOpts = { "Email", "Shipping Status", "Order Status", "Pay Status", "Store ID", "Vendor ID", "Customer ID", "Product ID", "Affiliate ID" };
        string[] OrderStatus = { "Complete", "Pending", "Cancelled", "Processing" };
        string[] ShippingStatus = { "Delivered", "Not Yet Shipped", "Partially Shipped", "Shipped", "Shipping Not Required" };
        string[] PayStatus = { "Paid", "Authorized", "Partially Refunded","Pending" ,"Refunded", "Voided" };
        public SearchOrders()
        {
            InitializeComponent();
        }

        private async Task SearchForOrders(string Search, string Filter)
        {
            ListBox ResultList = null;
            switch (Filter)
            {
                case "Email":
                    OrdersList = await api.UserOrders(Search);
                    ResultList = EmailSearch;
                    break;
                case "Store ID":
                    OrdersList = await api.StoreIdOrders(Int32.Parse(Search));
                    ResultList = StoreIdSearch;
                    break;
                case "Vendor ID":
                    OrdersList = await api.VendorIdOrders(Int32.Parse(Search));
                    ResultList = VendorIdSearch;
                    break;
                case "Customer ID":
                    OrdersList = await api.CustomerIdOrders(Int32.Parse(Search));
                    ResultList = CustomerIdSearch;
                    break;
                case "Product ID":
                    OrdersList = await api.ProductIdOrders(Int32.Parse(Search));
                    ResultList = ProductIdSearch;
                    break;
                case "Affiliate ID":
                    OrdersList = await api.AffiliateIdOrders(Int32.Parse(Search));
                    ResultList = AffiliateIdSearch;
                    break;
            }
            this.PopulateResults(ResultList);
            
        }

        private void Search_ActionIconTapped(object sender, EventArgs e)
        {
            this.Focus();
            FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
            Search = SearchBox.Text;
            var Task = SearchForOrders(Search, Filter);
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
                LoadingHolder.Visibility = System.Windows.Visibility.Visible;
                Search = SearchBox.Text;
                var SearchTask = SearchForOrders(Search, Filter);
                SearchBox.Text = Search;
            }
        }

        private void FilterHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Focus();
            NoResults.Visibility = System.Windows.Visibility.Collapsed;
            SearchBox.Text = "";
            var Item = (PivotItem)this.FilterHolder.SelectedItem;
            Filter = Item.Header.ToString();
            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();

            name.NameValue = InputScopeNameValue.Search;
            switch (Filter)
            {
                case "Email":
                    SearchBox.Visibility = System.Windows.Visibility.Visible;
                    StatusFilter.Visibility = System.Windows.Visibility.Collapsed;
                    name.NameValue = InputScopeNameValue.Search;
                    break;
                case "Store ID": 
                case "Vendor ID":
                case "Customer ID":
                case "Product ID":
                    name.NameValue = InputScopeNameValue.Number;
                    StatusFilter.Visibility = Visibility.Collapsed;
                    SearchBox.Hint = "Insert your search here";
                    SearchBox.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Shipping Status":
                    SearchBox.Visibility = System.Windows.Visibility.Collapsed;
                    StatusFilter.Visibility = System.Windows.Visibility.Visible;
                    StatusFilter.ItemsSource = ShippingStatus;
                    break;
                case "Order Status":
                    SearchBox.Visibility = System.Windows.Visibility.Collapsed;
                    StatusFilter.Visibility = System.Windows.Visibility.Visible;
                    StatusFilter.ItemsSource = OrderStatus;
                    break;
                case "Payment Status":
                    SearchBox.Visibility = System.Windows.Visibility.Collapsed;
                    StatusFilter.Visibility = System.Windows.Visibility.Visible;
                    StatusFilter.ItemsSource = PayStatus;
                    break;
            }
            scope.Names.Add(name);
            SearchBox.InputScope = scope;
            
        }

        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Status = (sender as ListPicker).SelectedItem.ToString();
                ListBox ResultList = null;
                switch (Filter)
                {
                    case "Shipping Status":
                        OrdersList = await api.ShippingStatusOrders(Status);
                        ResultList = ShippingSearch;
                        break;
                    case "Order Status":
                        OrdersList = await api.OrderStatusOrders(Status);
                        ResultList = OrderStatusSearch;
                        break;
                    case "Payment Status":
                        OrdersList = await api.PayStatusOrders(Status);
                        ResultList = PayStatusSearch;
                        break;
                }
                PopulateResults(ResultList);
        }

        private void PopulateResults(ListBox ResultList){
            var Result = new List<OrderData>();
            foreach (OrderDTO c in OrdersList)
            {
                SolidColorBrush color = null;
                switch (c.OrderStatus){
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
                var Order = new OrderData { Email = c.OrderEmail, ProductName = c.ProductsList.First().Product.Name, Id = c.OrderID, BackgroundColor = color };
                Result.Add(Order);
            }
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
            if (Result.Count == 0)
            {
                NoResults.Visibility = System.Windows.Visibility.Visible;
            }
            else 
            {
                NoResults.Visibility = System.Windows.Visibility.Collapsed;
            }
            
            ResultList.ItemsSource = Result;
            FilterHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void OrderSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                var Delete = Int32.Parse(NavigationContext.QueryString["delete"]);
                if (Delete == 1)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {

            }
           
            
        }
    }

    

    public class OrderData{
        public string Email { get; set; }
        public int Id { get; set; }
        public string ProductName { get; set; }
        public SolidColorBrush BackgroundColor {get; set;}
    }
}