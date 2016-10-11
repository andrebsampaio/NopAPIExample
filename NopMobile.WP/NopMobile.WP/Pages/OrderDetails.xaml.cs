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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Tasks;

namespace NopMobile.WP
{
    public partial class OrderDetails : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        OrderDTO Order;
        CustomerDTO Customer;
        int Id = 0;
        string[] OrderStatusValues = { "Complete", "Pending", "Cancelled", "Processing" };
        string[] ShippingStatusValues = { "Delivered", "Not Yet Shipped", "Partially Shipped", "Shipped", "Shipping Not Required" };
        string[] PayStatusValues = { "Paid", "Authorized", "Partially Refunded","Pending", "Refunded", "Voided" };
        public OrderDetails()
        {
            InitializeComponent();
        }

        private async Task InitializeOrder(){
            
            Order = await api.GetOrderById(Id);
            this.Date.Text = Order.CreateDate.ToString();
            decimal result = 0;
            var Currency = await api.GetCurrency();
            OrderStatus.Text = Order.OrderStatus.ToString();
            ProductTile.Message = Order.ProductsList.First().Product.Price.ToString("0.0#") + " " + Currency;
            ProductTile.Title = Order.ProductsList.First().Product.Name;
            ProductTile.Source = (ImageSource) ConvertToBitmapImage(Order.ProductsList.First().Product.Image.First());
            var Products = new List<OrderDetailsProduct>();
            foreach (OrderItemDTO item in Order.ProductsList)
            {
                var toDecimal = (decimal)item.Quantity;
                result += item.Product.Price * toDecimal;
                Products.Add(new OrderDetailsProduct { Product = item.Product.Name, Quantity = item.Quantity, Total = (item.Product.Price * toDecimal).ToString("0.0#"),
                                                       Image = (ImageSource)ConvertToBitmapImage(item.Product.Image.First())
                });
            }

            ProductsList.ItemsSource = Products;

            Total.Text = result.ToString("0.0#") + " " + Currency ;
        
            switch (Order.ShippingStatus){
                case ShippingStatus.NotYetShipped:
                    ShippingStatusDetails.Text = "Not Yet Shipped";
                    break;
                case ShippingStatus.PartiallyShipped:
                    ShippingStatusDetails.Text = "Partially Shipped";
                    break;
                case ShippingStatus.ShippingNotRequired:
                    ShippingStatusDetails.Text = "Shipping Not Required";
                    break;
                default:
                    ShippingStatusDetails.Text = Order.ShippingStatus.ToString();
                    break;
            }
            switch (Order.PayStatus)
            {
                case PaymentStatus.PartiallyRefunded:
                    PaymentStatusDetails.Text = "Partially Refunded";
                    break;
                default:
                    PaymentStatusDetails.Text = Order.PayStatus.ToString();
                    break;
            }

            var CustomerTask = await api.GetCustomersByEmail(Order.OrderEmail);
            Customer = CustomerTask.First();

            Email.Text = Customer.Email;
            Name.Text = Customer.FullName;
            Address.Text = Order.Address;
            if (Customer.Phone == null)
            {
                Phone.Text = "Not Available";
            }
            else
            {
                Phone.Text = Customer.Phone;
            }

        }

        private static BitmapImage ConvertToBitmapImage(byte[] image)
        {
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream(image);
            bitmapImage.SetSource(memoryStream);
            return bitmapImage;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Id = Int32.Parse(NavigationContext.QueryString["selectedId"]);
            var InitTask = InitializeOrder();
        }

        public class OrderDetailsProduct
        {
            public string Product { get; set; }
            public int Quantity { get; set; }
            public string Total { get; set; }
            public ImageSource Image { get; set; }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            var ConfirmText = new PhoneTextBox();
            sp.Children.Add(ConfirmText);
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                
                Caption = "Delete Order",
                Message = "Write 'delete' to confirm and delete order",
                Content = sp,
                LeftButtonContent = "Confirm",
                RightButtonContent = "Cancel"
            };
             messageBox.Dismissed += async (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            if (ConfirmText.Text.Equals("delete")){
                                await api.CancelOrders(new int[] { Order.OrderID });
                                MessageBox.Show("The order was deleted sucessfully");
                                NavigationService.Navigate(new Uri("/Pages/SearchOrders.xaml?delete=1", UriKind.Relative));
                            }
                            else
                            {
                                MessageBox.Show("You didn't confirm the command");
                            }
                            break;
                    }
                };
             messageBox.Show();
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            ListPicker ShippingDrop = new ListPicker();
            ListPicker PaymentDrop = new ListPicker();
            PaymentDrop.SetValue(Microsoft.Phone.Controls.ListPicker.ItemCountThresholdProperty, 6);
            ListPicker OrderDrop = new ListPicker();
            var OrderIndex = Array.IndexOf(OrderStatusValues, Order.OrderStatus.ToString());
            var ShippingIndex = 0;
            switch (Order.ShippingStatus)
            {
                case ShippingStatus.NotYetShipped:
                    ShippingIndex = 1;
                    break;
                case ShippingStatus.PartiallyShipped:
                    ShippingIndex = 2;
                    break;
                case ShippingStatus.ShippingNotRequired:
                    ShippingIndex = 4;
                    break;
                default:
                    ShippingIndex = Array.IndexOf(ShippingStatusValues, Order.ShippingStatus.ToString());
                    break;
            }
            var PaymentIndex = 0;
            switch (Order.PayStatus)
            {
                case PaymentStatus.PartiallyRefunded:
                   PaymentIndex = 2;
                    break;
                default:
                    PaymentIndex = Array.IndexOf(PayStatusValues, Order.PayStatus.ToString());
                    break;
            }

            

            ShippingDrop.ItemsSource = ShippingStatusValues;
            PaymentDrop.ItemsSource = PayStatusValues;
            OrderDrop.ItemsSource = this.OrderStatusValues;

            ShippingDrop.SelectedIndex = ShippingIndex;
            OrderDrop.SelectedIndex = OrderIndex;
            PaymentDrop.SelectedIndex = PaymentIndex;

            ShippingDrop.SelectionChanged += (se, ev) => {
                ShippingIndex = ShippingDrop.SelectedIndex;
            };
            OrderDrop.SelectionChanged += (se, ev) => {
                OrderIndex = OrderDrop.SelectedIndex;
            };
            PaymentDrop.SelectionChanged += (se, ev) => {
                PaymentIndex = PaymentDrop.SelectedIndex;
            };

            sp.Children.Add(ShippingDrop);
            sp.Children.Add(PaymentDrop);
            sp.Children.Add(OrderDrop);

            CustomMessageBox messageBox = new CustomMessageBox()
            {   

                Caption = "Change Order Status",
                Message = "Change the status accordingly",
                Content = sp,
                LeftButtonContent = "Submit",
                RightButtonContent = "Cancel"
            };

            messageBox.Dismissed += async (s1, e1) => {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        await api.ChangeOrderStatus(Order.OrderID,OrderStatusValues[OrderIndex]);
                        var PayS = new PaymentStatus[] { PaymentStatus.Paid, PaymentStatus.Authorized, PaymentStatus.PartiallyRefunded,PaymentStatus.Pending, PaymentStatus.Refunded, PaymentStatus.Voided };
                        await api.ChangePaymentStatus(Order.OrderID,PayS[PaymentIndex]);
                        var ShipS = new ShippingStatus[]{ShippingStatus.Delivered,ShippingStatus.NotYetShipped,ShippingStatus.PartiallyShipped,ShippingStatus.Shipped,ShippingStatus.ShippingNotRequired};
                        await api.ChangeShippingStatus(Order.OrderID,ShipS[ShippingIndex]);
                        MessageBox.Show("Status changed sucessfuly");
                        InitializeOrder();
                        break;
                }
            };

            messageBox.Show();
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            var NoteText = new PhoneTextBox();
            var ShowUser = new CheckBox();
            ShowUser.Content = "Show to user";
            NoteText.AcceptsReturn = true;
            sp.Children.Add(NoteText);
            sp.Children.Add(ShowUser);
            CustomMessageBox messageBox = new CustomMessageBox()
            {

                Caption = "Add Note to Order",
                Message = "Add some notes to this order",
                Content = sp,
                LeftButtonContent = "Submit",
                RightButtonContent = "Cancel"
            };

            messageBox.Dismissed += async (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        api.NewOrderNote(Order.OrderID, NoteText.Text, Convert.ToBoolean(ShowUser.IsChecked));
                        MessageBox.Show("Note added sucessfuly");
                        break;
                }
            };

            messageBox.Show();
        }

        private void ContactCustomer_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            StackPanel sp = new StackPanel();
            ListPicker reasons = new ListPicker();
            reasons.ItemsSource = new string[] {"Problem", "Update", "Question"};
            var reason = "";
            reasons.SelectionChanged += (se, ev) => {
                reason = reasons.SelectedItem.ToString();
            };
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Add Note to Order",
                Message = "Add some notes to this order",
                Content = sp,
                LeftButtonContent = "Submit",
                RightButtonContent = "Cancel"
            };
            switch (reason)
            {
                case "Problem":
                    emailComposeTask.Subject = "[ Order ID: " + Order.OrderID + " ] A Problem has ocurred with your order";
                    break;
                case "Update":
                     emailComposeTask.Subject = "[ Order ID: " + Order.OrderID + " ] An Update has ocurred in your order";
                    break;
                case "Question":
                     emailComposeTask.Subject = "[ Order ID: " + Order.OrderID + " ] A Question about your order has risen";
                    break;
            }
            emailComposeTask.To = Order.OrderEmail;

            emailComposeTask.Show();
        }
    }

    
}