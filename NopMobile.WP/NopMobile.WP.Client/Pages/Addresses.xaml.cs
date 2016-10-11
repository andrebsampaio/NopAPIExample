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
using NopMobile.WP.Client.Helpers;

namespace NopMobile.WP.Client.Pages
{
    public partial class Addresses : PhoneApplicationPage
    {

        CustomerDTO Customer;
        NopCore api = new NopCore();
        CountryDTO[] Countries;
        public Addresses()
        {
            InitializeComponent();
            InitializeAddresses();
        }

        private async void InitializeAddresses()
        {
            var CustomerTmp = await api.GetCustomersByEmail(Helper.CurrentCustomer());
            Customer = CustomerTmp.First();
            var ResultList = new List<AddressData>();
            foreach (AddressDTO a in Customer.Addresses)
            {
                ResultList.Add(new AddressData { Address = a.Street, City = a.City + ", ", Country = a.Country, Email = Customer.Email, 
                    Name = a.Firstname + " " + a.Lastname, Phone = a.Phone, PostalCode = a.PostalCode});
            }
            AddressesList.ItemsSource = ResultList;
            Countries = await api.GetAllCountries();
            var CountryString = new List<string>();
            foreach (CountryDTO c in Countries)
            {
                CountryString.Add(c.Name);
            }
            Country.ItemsSource = CountryString.Distinct();
            
            HideLoading();
        }

        private void ShowLoading()
        {
            AddressesList.Visibility = System.Windows.Visibility.Collapsed;
            SearchHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
            ApplicationBar.IsVisible = false;
        }

        private void HideLoading()
        {
            AddressesList.Visibility = System.Windows.Visibility.Visible;
            SearchHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
            ApplicationBar.IsVisible = true;
        }

        private class AddressData
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        private void AddAddress_Click(object sender, EventArgs e)
        {
            AddressForm.Visibility = System.Windows.Visibility.Visible;
            AddressesList.Visibility = System.Windows.Visibility.Collapsed;
            Title.Text = "new address";
            ChangeButtons(false);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (AddressForm.Visibility.Equals(System.Windows.Visibility.Visible))
            {
                AddressForm.Visibility = System.Windows.Visibility.Collapsed;
                AddressesList.Visibility = System.Windows.Visibility.Visible;
                Title.Text = "add address";
                ChangeButtons(true);
                e.Cancel = true;
            }
        }

        private void ChangeButtons(bool FormOpen)
        {
            if (FormOpen)
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBarIconButton b = new ApplicationBarIconButton();
                b.Text = "add address";
                b.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative);
                b.Click += AddAddress_Click;
                ApplicationBar.Buttons.Add(b);
            }
            else
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBarIconButton b = new ApplicationBarIconButton();
                b.Text = "submit";
                b.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative);
                b.Click += async (e2, s2) =>
                {
                    try
                    {
                        var Province = "";
                        if (State.Items.Count == 0) Province = null;
                        else Province = State.SelectedItem.ToString();
                        var Result = await api.AddAddress(Customer.Email, Firstname.Text, Lastname.Text, Country.SelectedItem.ToString(), City.Text, Street.Text, PostalCode.Text, Phone.Text, Province);
                        if (Result)
                        {
                            CustomMessageBox messageBox = new CustomMessageBox
                            {
                                Message = "Address added successfully",
                                Title = "Add Success",
                                LeftButtonContent = "OK"
                            };
                            messageBox.Show();
                            messageBox.Dismissed += (s3, e3) => {
                                if (e3.Result == CustomMessageBoxResult.LeftButton)
                                {
                                    AddressForm.Visibility = System.Windows.Visibility.Collapsed;
                                }
                            };
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };
                ApplicationBar.Buttons.Add(b);
            }
        }

        private void State_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Country.SelectedItem.ToString().Equals("United States"))
            {
                foreach (CountryDTO c in Countries){
                    if (c.Name.Equals("United States")){
                        State.ItemsSource = c.Provinces;
                        break;
                    }
                }
                
            }
            else
            {
                State.ItemsSource = new List<string>();
            }
        }

    }
}