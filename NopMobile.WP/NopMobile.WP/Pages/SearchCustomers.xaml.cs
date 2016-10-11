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
    public partial class SearchCustomers : PhoneApplicationPage
    {
        CustomerDTO[] CustomersList;
        NopCore api = new NopCore();
        string Filter = "Email";
        public SearchCustomers()
        {
            InitializeComponent();
        }

        private void UserSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Results = sender as ListBox;
            // If selected index is -1 (no selection) do nothing
            if (Results.SelectedIndex == -1)
                return;
            var CustomerEmail = "";
            
            if (e.AddedItems[0] != null)
            {
                CustomerEmail = (e.AddedItems[0] as CustomerData).Email;
            }


            // Navigate to the new page
            NavigationService.Navigate(new Uri("/Pages/CustomerDetails.xaml?Email=" + CustomerEmail, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            Results.SelectedIndex = -1;
        }

        private void Search_ActionIconTapped(object sender, EventArgs e)
        {
            this.Focus();
            FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
            var Search = SearchBox.Text;
            var Task = SearchForCustomers(Search, Filter);
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                FilterHolder.Visibility = System.Windows.Visibility.Collapsed;
                LoadingHolder.Visibility = System.Windows.Visibility.Visible;
                var Search = SearchBox.Text;
                var SearchTask = SearchForCustomers(Search, Filter);
                SearchBox.Text = Search;
            }
        }

        private async Task SearchForCustomers(string Search, string Filter)
        {
            ListBox ResultList = null;
            switch (Filter)
            {
                case "Email":
                    CustomersList = await api.GetCustomersByEmail(Search);
                    ResultList = EmailSearch;
                    break;
                case "Username":
                    CustomersList = await api.GetCustomersByUsername(Search);
                    ResultList = UsernameSearch;
                    break;
                case "Firstname":
                    CustomersList = await api.GetCustomersByFirstname(Search);
                    ResultList = FirstnameSearch;
                    break;
                case "Lastname":
                    CustomersList = await api.GetCustomersByLastname(Search);
                    break;
                case "Fullname":
                    CustomersList = await api.GetCustomersByFullname(Search);
                    ResultList = FullnameSearch;
                    break;
                case "Company":
                    CustomersList = await api.GetCustomersByCompany(Search);
                    ResultList = CompanySearch;
                    break;
                case "Phone":
                    CustomersList = await api.GetCustomersByPhone(Search);
                    ResultList = PhoneSearch;
                    break;
                case "Postal Code":
                    CustomersList = await api.GetCustomersByPostalCode(Search);
                    ResultList = PostalSearch;
                    break;
            }
            PopulateResults(ResultList);
        }

        private void FilterHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = (PivotItem) FilterHolder.SelectedItem;
            Filter = Item.Header.ToString();
        }

        private void PopulateResults(ListBox ResultList)
        {
            var Result = new List<CustomerData>();
            foreach (CustomerDTO c in CustomersList)
            {
                var Customer = new CustomerData { Email = c.Email, Fullname = c.FullName, Id = c.Id};
                Result.Add(Customer);
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
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;

        }

        public class CustomerData{
            public string Email {get;set;}
            public string Fullname {get;set;}
            public int Id {get;set;}
        }
    }
}