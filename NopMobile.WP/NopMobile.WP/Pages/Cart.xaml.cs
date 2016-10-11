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
using System.Windows.Media.Imaging;
using System.IO;

namespace NopMobile.WP
{
    public partial class Cart : PhoneApplicationPage
    {
        CustomerDTO Customer;
        string CustomerEmail;
        NopCore api = new NopCore();
        public Cart()
        {
            InitializeComponent();
        }

        private async Task InitializeCart()
        {
            var CustomerA = await api.GetCustomersByEmail(CustomerEmail);
            Customer = CustomerA.First();
            Name.Text = Customer.FullName;

            var CartRes = new List<OrderDetails.OrderDetailsProduct>();
            foreach (CartItemDTO p in Customer.ShoppingCart)
            {
                CartRes.Add(new OrderDetails.OrderDetailsProduct
                {
                    Image = ConvertToBitmapImage(p.Product.Image.First()),
                    Product = p.Product.Name,
                    Quantity = p.Quantity,
                    Total = ((decimal)p.Quantity * p.Product.Price).ToString()
                });
            }
            CartProducts.ItemsSource = CartRes;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CustomerEmail = NavigationContext.QueryString["Email"];
            var InitTask = InitializeCart();
        }

        private static BitmapImage ConvertToBitmapImage(byte[] image)
        {
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream(image);
            bitmapImage.SetSource(memoryStream);
            return bitmapImage;
        }
    }
}