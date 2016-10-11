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
using System.IO.IsolatedStorage;

namespace NopMobile.WP.Client.Pages
{
    public partial class Checkout : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        CustomerDTO Customer;
        decimal ShippingFeesValue, OtherFeesValue = 0;
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        AddressDTO BillingAddress;
        bool Finished = false;
        AddressDTO ShippingAddress;
        decimal Total = 0;
        string Currency = "";
        public Checkout()
        {
            InitializeComponent();
            InitializeCheckout();
        }

        private async void InitializeCheckout()
        {
            Currency = await api.GetCurrency();
            var CustomerTmp = await api.GetCustomersByEmail(Helpers.Helper.CurrentCustomer());
            Customer = CustomerTmp.First();
            var AddressesA = new List<AddressData>();

            foreach (AddressDTO a in Customer.Addresses){
                AddressesA.Add(new AddressData { Address = a.Firstname + " " + a.Lastname + "," + a.Phone + "," + a.Street + "," + a.City + "," + a.PostalCode + "," + a.Country, Id = a.Id} );
            }

            var ShippingM = new List<string>(await api.GetShippingMethods()).Distinct();
            ShippingMethodPicker.ItemsSource = ShippingM;
            BillingPicker.ItemsSource = AddressesA;
            ShippingPicker.ItemsSource = AddressesA;
            PaymentPicker.ItemsSource = await api.GetPaymentMethods();

            decimal SubTotalValue = 0;

            foreach (CartItemDTO c in Customer.ShoppingCart){
                SubTotalValue += c.Product.Price * c.Quantity;
            }

            var ShipTmp = await api.GetShippingFees(Customer.Id);
            var TaxTmp = await api.GetTaxFees(Customer.Id);
            ShippingFees.Text = ShipTmp.ToString("0.0");
            OtherFees.Text = TaxTmp.ToString("0.0");
            SubTotal.Text = SubTotalValue.ToString("0.0#") + " " + Currency;
            ShippingFees.Text = ShippingFeesValue.ToString("0.0#") + " " + Currency; ;
            OtherFees.Text = OtherFeesValue.ToString("0.0#") + " " + Currency;
            Total = SubTotalValue + ShippingFeesValue + OtherFeesValue;
            OrderTotalPay.Text = Total.ToString("0.0#") + " " + Currency;
            HideLoading();
        }

        private void ShowOrdering()
        {
            MainPageHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingMessage.Text = "Processing Order";
        }

        private void HideLoading()
        {
            MainPageHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private class AddressData
        {
            public string Address { get; set; }
            public int Id { get; set; }
        }

        private bool CheckAddresses(){
            if (ShippingMethodPicker.SelectedItem != null && BillingPicker.SelectedItem != null && ShippingPicker.SelectedItem != null)
            {
                foreach (AddressDTO a in Customer.Addresses)
                {
                    if ((BillingPicker.SelectedItem as AddressData).Id == a.Id)
                    {
                        BillingAddress = a;
                    }
                    if ((ShippingPicker.SelectedItem as AddressData).Id == a.Id)
                    {
                        ShippingAddress = a;
                    }
                }
                return true;
            }
            else return false;
        }

        private bool CheckPayment()
        {
            if (PaymentPicker.SelectedItem != null)
            {
                return true;
            }
            else return false;
        }

        private async void Proceed_Click(object sender, EventArgs e)
        {
            ShowOrdering();
            var Errors = await api.AddNewOrder(Customer.Id, BillingAddress.Id, ShippingAddress.Id, ShippingMethodPicker.ToString(), PaymentPicker.SelectedItem.ToString(), Convert.ToBoolean(GiftWrapCheck.IsChecked));
            HideLoading();
            if (Errors.Length == 0)
            {
                CustomMessageBox SuccessMessage = new CustomMessageBox() { 
                    Title = "Order Successful",
                    Message = "Your order was successful",
                    LeftButtonContent = "Review Order",
                    RightButtonContent = "Go to Store"
                };
                SuccessMessage.Show();
                SuccessMessage.Dismissed += (s1, e1) =>
                {
                    if (e1.Result.Equals(CustomMessageBoxResult.RightButton))
                    {
                        UserSettings.Remove("refresh_cart");
                        UserSettings.Add("refresh_cart", true);
                        NavigationService.GoBack();

                    }
                    else if (e1.Result.Equals(CustomMessageBoxResult.LeftButton))
                    {
                        Finished = true;
                        ApplicationBar.Buttons.RemoveAt(0);
                        ApplicationBarIconButton b = new ApplicationBarIconButton();
                        b.Text = "To Store";
                        b.IconUri = new Uri("/Images/arrowLeft.png", UriKind.Relative);
                        b.Click += (e2, s2) => {
                            UserSettings.Remove("refresh_cart");
                            UserSettings.Add("refresh_cart", true);
                            NavigationService.GoBack();
                        };
                        ApplicationBar.Buttons.Add(b);

                        CheckoutPivot.IsLocked = true;
                        CheckoutProgress.Value = 100;
                        
                    }
                };
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Finished)
            {
                UserSettings.Remove("refresh_cart");
                UserSettings.Add("refresh_cart", true);
            }
            NavigationService.GoBack();
        }

        private void CheckoutPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Finished)
            {
                if (CheckoutPivot.SelectedIndex == 0)
                {
                    ApplicationBar.IsVisible = false;
                }
                else if (CheckoutPivot.SelectedIndex == 1)
                {
                    ApplicationBar.IsVisible = false;
                    if (!CheckAddresses())
                    {
                        PaymentItem.IsEnabled = false;
                    }
                    else
                    {
                        PaymentItem.IsEnabled = true;
                        CheckoutProgress.Value = 100 / 3;
                    }

                }
                else if (CheckoutPivot.SelectedIndex == 2)
                {
                    if (!CheckPayment())
                    {
                        ConfirmItem.IsEnabled = false;
                    }
                    else
                    {
                        if (BillingAddress != null && ShippingAddress != null)
                        {
                            ApplicationBar.IsVisible = true;
                            LoadConfirmation();
                            ConfirmItem.IsEnabled = true;
                            CheckoutProgress.Value = 200 / 3;
                        }
                    }
                }    
            }
        }

        private void LoadConfirmation()
        {
            OrderTotalConfirmation.Text = OrderTotalPay.Text;

            BillingName.Text = BillingAddress.Firstname + " " + BillingAddress.Lastname;
            BillingEmail.Text = Customer.Email;
            BillingPhone.Text = BillingAddress.Phone;
            BillingStreet.Text = BillingAddress.Street;
            BillingCity.Text = BillingAddress.City + ", ";
            BillingPostal.Text = BillingAddress.PostalCode;
            BillingCountry.Text = BillingAddress.Country;

            ShippingName.Text = ShippingAddress.Firstname + " " + ShippingAddress.Lastname;
            ShippingEmail.Text = Customer.Email;
            ShippingPhone.Text = ShippingAddress.Phone;
            ShippingStreet.Text = ShippingAddress.Street;
            ShippingCity.Text = ShippingAddress.City + ", ";
            ShippingPostal.Text = ShippingAddress.PostalCode;
            ShippingCountry.Text = ShippingAddress.Country;

            var Result = new List<MainPage.CartData>();
            foreach (CartItemDTO c in Customer.ShoppingCart ){
                Result.Add(new MainPage.CartData { Id = c.Id, ProdId = c.Product.Id, Image = Helper.ConvertToBitmapImage(c.Product.Image.First()), 
                    ProductName = c.Product.Name, Quantity = c.Quantity, Total = (c.Quantity * c.Product.Price).ToString("0.#"), UnitPrice = c.Product.Price.ToString("0.#") + " " + Currency });
            }
            ProductsList.ItemsSource = Result;
        }

        private void GiftWrapCheck_Checked(object sender, RoutedEventArgs e)
        {
            Total += 10;
            OtherFeesValue += 10;
            OtherFees.Text = OtherFeesValue.ToString("0.0#");
            OrderTotalPay.Text = Total.ToString("0.0#") + " " + Currency;

        }

        private void GiftWrapCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Total -= 10;
            OtherFeesValue -= 10;
            OtherFees.Text = OtherFeesValue.ToString("0.0#") + " " + Currency;
            OrderTotalPay.Text = Total.ToString("0.0#") + " " + Currency;
        }
    }
}