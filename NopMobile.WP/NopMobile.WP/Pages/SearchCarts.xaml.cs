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

namespace NopMobile.WP
{
    public partial class SearchCarts : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        CustomerDTO[] CartsList;
        string Filter = "Email";
        string Search;
        public SearchCarts()
        {
            InitializeComponent();
        }

        private async Task SearchForCarts(string Search, string Filter)
        {
            ListBox ResultList = null;
            switch (Filter)
            {
                case "Email":
                    CartsList = await api.GetCurrentCarts("email", Search, 0, 0, 0, 0, false);
                    ResultList = EmailSearch;
                    break;
                case ">Items":
                    CartsList = await api.GetCurrentCarts("higher items", null, 0, Int32.Parse(Search), 0, 0, false);
                    ResultList = GItemsSearch;
                    break;
                case "<Items":
                    CartsList = await api.GetCurrentCarts("lower items", null, Int32.Parse(Search), 0, 0, 0, false);
                    ResultList = LItemsSearch;
                    break;
                case ">Total":
                    CartsList = await api.GetCurrentCarts("higher total", null, 0, 0, 0, Int32.Parse(Search), false);
                    ResultList = GTotalSearch;
                    break;
                case "<Total":
                    CartsList = await api.GetCurrentCarts("lower total", null, 0, 0, Int32.Parse(Search), 0, false);
                    ResultList = LTotalSearch;
                    break;
                case "Abandoned":
                    CartsList = await api.GetCurrentCarts("abandoned", null, 0, 0, 0, 0, true);
                    ResultList = AbandonedSearch;
                    break;
                case "Active":
                    CartsList = await api.GetCurrentCarts("active", null, 0, 0, 0, 0, false);
                    ResultList = ActiveSearch;
                    break;
            }
            var Result = new List<CartData>();
            var Currency = await api.GetCurrency();
            foreach (CustomerDTO c in CartsList)
            {
                var Quantity = 0; var Amount = 0;
                foreach (CartItemDTO item in c.ShoppingCart){
                    Quantity += item.Quantity;
                    Amount +=  item.Quantity * (int) item.Product.Price;
                }
                var Cart = new CartData { Email = c.Email, FullName = c.FullName, Quantity = Quantity, Amount = Amount.ToString() + " " + Currency };
                Result.Add(Cart);
            }
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
            ResultList.ItemsSource = Result;
            FilterHolder.Visibility = System.Windows.Visibility.Visible;
            
        }

        private void Search_ActionIconTapped(object sender, EventArgs e)
        {
            this.Focus();
            FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
            Search = SearchBox.Text;
            var SearchTask = SearchForCarts(Search, Filter);
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter)
            {
                this.Focus();
                FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
                LoadingHolder.Visibility = System.Windows.Visibility.Visible;
                Search = SearchBox.Text;
                var SearchTask = SearchForCarts(Search, Filter);
            }
        }

        private void FilterHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Focus();
            SearchBox.Text = "";
            var Item = (PivotItem)this.FilterHolder.SelectedItem;
            Filter = Item.Header.ToString();
            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();

            name.NameValue = InputScopeNameValue.Search;
            Task SearchTask = null;
            switch (Filter)
            {
                case "Email":
                    SearchBox.IsReadOnly = false;
                    SearchBox.Hint = "Insert your search here";
                    name.NameValue = InputScopeNameValue.Search;
                    break;
                case ">Items": 
                case "<Items":
                case ">Total":
                case "<Total":
                    name.NameValue = InputScopeNameValue.Number;
                    SearchBox.Hint = "Insert your search here";
                    SearchBox.IsReadOnly = false;
                    break;
                case "Abandoned":
                    SearchBox.IsReadOnly = true;
                    SearchBox.Hint = "Disabled";
                    SearchTask = SearchForCarts(Search, Filter);
                    break;
                case "Active":
                    SearchBox.IsReadOnly = true;
                    SearchBox.Hint = "Disabled";
                    SearchTask = SearchForCarts(Search, Filter);
                    break;
            }
            scope.Names.Add(name);
            SearchBox.InputScope = scope;
            
        }

        private void CartSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Results = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (Results.SelectedIndex == -1)
                return;
            var CustomerEmail = "";

            if (e.AddedItems[0] != null)
            {
                CustomerEmail = (e.AddedItems[0] as CartData).Email;
            }

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/Pages/Cart.xaml?Email=" + CustomerEmail, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            Results.SelectedIndex = -1;
        }
    }

    public class CartData{
        public string Email { get; set; }
        public string FullName { get; set; }
        public int Quantity { get; set; }
        public string Amount { get; set; }
    }
}