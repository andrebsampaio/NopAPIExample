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
using Microsoft.Phone.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;

namespace NopMobile.WP.Client.Pages
{
    public partial class OrderDetails : PhoneApplicationPage
    {
        OrderDTO Order;
        int Id;
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        NopCore api = new NopCore();
        CustomerDTO Customer;
        string Currency;
        public OrderDetails()
        {
            InitializeComponent();
        }

        private async void InitializeOrder(){
            Order = await api.GetOrderById(Id);
            Currency = await api.GetCurrency();
            var CustomerTmp = await api.GetCustomersByEmail(Helpers.Helper.CurrentCustomer());
            Customer = CustomerTmp.First();
           
            OrderTotalConfirmation.Text = Order.Total.ToString("0.0#") + " " +  Currency;

            BillingName.Text = Order.BillingAddress.Firstname + " " + Order.BillingAddress.Lastname;
            BillingEmail.Text = Customer.Email;
            BillingPhone.Text = Order.BillingAddress.Phone;
            BillingStreet.Text = Order.BillingAddress.Street;
            BillingCity.Text = Order.BillingAddress.City + ", ";
            BillingPostal.Text = Order.BillingAddress.PostalCode;
            BillingCountry.Text = Order.BillingAddress.Country;

            ShippingName.Text = Order.ShippingAddress.Firstname + " " + Order.ShippingAddress.Lastname;
            ShippingEmail.Text = Customer.Email;
            ShippingPhone.Text = Order.ShippingAddress.Phone;
            ShippingStreet.Text = Order.ShippingAddress.Street;
            ShippingCity.Text = Order.ShippingAddress.City + ", ";
            ShippingPostal.Text = Order.ShippingAddress.PostalCode;
            ShippingCountry.Text = Order.ShippingAddress.Country;

            var Result = new List<MainPage.CartData>();
            foreach (OrderItemDTO c in Order.ProductsList)
            {
                Result.Add(new MainPage.CartData
                {
                    Id = c.OrderId,
                    ProdId = c.Product.Id,
                    Image = Helper.ConvertToBitmapImage(c.Product.Image.First()),
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    Total = (c.Quantity * c.Product.Price).ToString("0.#"),
                    UnitPrice = c.Product.Price.ToString("0.#") + " " + Currency
                });
            }
            ProductsList.ItemsSource = Result;
            HideLoading();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Id = Int32.Parse(NavigationContext.QueryString["OrderId"]);
            InitializeOrder();
        }

        private async void ReOrder_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("Processing Re-Order");
            var Result = await api.ReOrder(Order.OrderID);
            var ResultMessage = "";
            HideLoading();
            if (Result)
            {
                ResultMessage = "Products re-ordered with success";
            } else {
                ResultMessage = "An error ocurred while processing re-order";
            }

            CustomMessageBox messageBox = new CustomMessageBox
            {
                Message = ResultMessage,
                Title = "Re-Order",
                LeftButtonContent = "OK"
            };
            messageBox.Show();
            if (UserSettings.Contains("refresh_cart"))
            {
                UserSettings.Remove("refresh_cart");
                UserSettings.Add("refresh_cart", false);
            }
            else
            {
                UserSettings.Add("refresh_cart", false);
            }
            
        }

        private async void Pdf_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("Generating PDF");
            var PdfPath = await api.GetPdfInvoice(Order.OrderID);
            var Url = await api.GetStoreUrl();
            var CleanPath = Regex.Replace(PdfPath,@"\\",@"/");
            Url += CleanPath.Substring(CleanPath.IndexOf("content"));
            HideLoading();
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(Url, UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void ShowLoading(string Message)
        {
            DetailsHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
            SearchHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingMessage.Text = Message;
        }

        private void HideLoading()
        {
            DetailsHolder.Visibility = System.Windows.Visibility.Visible;
            SearchHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }


    }
}