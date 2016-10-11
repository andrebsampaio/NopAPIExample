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
using System.Windows.Media;

namespace NopMobile.WP
{
    public partial class CustomerDetails : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        CustomerDTO Customer = new CustomerDTO();
        string CustomerEmail;
        public CustomerDetails()
        {
            InitializeComponent();
        }

        private async Task InitializeCustomer(){
            var CustomerA = await api.GetCustomersByEmail(CustomerEmail);
            Customer = CustomerA.First();
            if (Customer.Active)
            {
                Status.Text = "Active";
                Status.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                Status.Text = "Inactive";
                Status.Foreground = new SolidColorBrush(Colors.Red);
            }
            Name.Text = Customer.FullName ?? "Not Available";
            Email.Text = Customer.Email;
            Gender.Text = Customer.Gender ?? "Not Available";
            Birthday.Text = Customer.Birthday ?? "Not Available";
            Company.Text = Customer.Company ?? "Not Available";
            if (Customer.IsAdmin)
            {
                Roles.Text = "Admin";
            }
            else
            {
                Roles.Text = "Customer";
            }
            LastActivity.Text = Customer.Activity.ToString();
            TextBlock Comment = new TextBlock();
            if (Customer.AdminComment == null)
            {
                Comment.Text = "No Comments";
                AddComment.Content = "Add Comment";
            } 
            else 
            {
                Comment.Text = Customer.AdminComment;
                AddComment.Content = "Edit Comment";
            }
            Comments.Children.Add(Comment);
            foreach (AddressDTO ad in Customer.Addresses)
            {
                TextBlock Address = new TextBlock();
                Address.TextWrapping = TextWrapping.Wrap;
                Address.Text = ad.Firstname + " " + ad.Lastname + ", " + ad.Street + ", " + ad.City + ", " + ad.PostalCode + ", " + ad.Country; ;
                Addresses.Children.Add(Address);
            }

            var CartRes = new List<OrderDetails.OrderDetailsProduct>();
            foreach (CartItemDTO p in Customer.ShoppingCart)
            {
                CartRes.Add(new OrderDetails.OrderDetailsProduct
                {
                    Image = ConvertToBitmapImage(p.Product.Image.First()),
                    Product = p.Product.Name,
                    Quantity = p.Quantity, 
                    Total = ((decimal) p.Quantity * p.Product.Price).ToString() });
            }
            Cart.ItemsSource = CartRes;
            var WishRes = new List<OrderDetails.OrderDetailsProduct>();
            foreach (CartItemDTO p in Customer.Wishlist)
            {
                WishRes.Add(new OrderDetails.OrderDetailsProduct
                {
                    Image = ConvertToBitmapImage(p.Product.Image.First()),
                    Product = p.Product.Name,
                    Quantity = p.Quantity,
                    Total = ((decimal)p.Quantity * p.Product.Price).ToString()
                });
            }
            Wishlist.ItemsSource = WishRes;
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            var Comment = new PhoneTextBox();
            Comment.Hint = "Enter your comment here";
            Comment.AcceptsReturn = true;
            sp.Children.Add(Comment);
            CustomMessageBox messageBox = new CustomMessageBox()
            {

                Caption = "Add Comment",
                Message = "Add a comment to this customer",
                Content = sp,
                LeftButtonContent = "Submit",
                RightButtonContent = "Cancel"
            };
            messageBox.Dismissed += async (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        await api.AddCommentToCustomer(Customer.Email, Comment.Text);
                        await this.InitializeCustomer();
                        MessageBox.Show("Comment added sucessfully");
                        break;
                }
            };
            messageBox.Show();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CustomerEmail = NavigationContext.QueryString["Email"];
            var InitTask = InitializeCustomer();
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