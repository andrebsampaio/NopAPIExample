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
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;
using NopMobile.WP.Client.Helpers;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace NopMobile.WP.Client.Pages
{
    public partial class Wishlist : PhoneApplicationPage
    {
        CustomerDTO Customer;
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        NopCore api = new NopCore();
        public Wishlist()
        {
            InitializeComponent();
            InitializeWishlist();
        }

        private async void InitializeWishlist(){
            var Currency = await api.GetCurrency();
            var tmp = await api.GetCustomersByEmail(Helper.CurrentCustomer());
            Customer = tmp.First();
            var CartList = new List<MainPage.CartData>();
            var ItemCount = 0;
            foreach (CartItemDTO c in Customer.Wishlist)
            {
                ItemCount += c.Quantity;
                CartList.Add(new MainPage.CartData { ProdId = c.Product.Id, Id = c.Id, ProductName = c.Product.Name, Quantity = c.Quantity, Image = Helper.ConvertToBitmapImage(c.Product.Image.First()), UnitPrice = c.Product.Price.ToString("0.0#") + " " + Currency, Total = (c.Product.Price * c.Quantity).ToString("0.0#") + " " + Currency });
            }
            if (CartList.Count == 0)
            {
                ProductCount.Text = "no";
            }
            else
            {
                ProductCount.Text = ItemCount.ToString();
            }
            ProductsList.ItemsSource = CartList;
        }

        public async void RemoveFromCart(ListBoxItem Item)
        {
            var CartItem = Item.Content as MainPage.CartData;
            SwipeAnimation(Item);
            var RemoveResult = await api.RemoveFromCart(Customer.Email, CartItem.Id);
            if (RemoveResult)
            {
                string Current = "";
                UserSettings.TryGetValue("current_user", out Current);
                var CustomerTmp = await api.GetCustomersByEmail(Current);
                Customer = CustomerTmp.First();
                InitializeWishlist();
            }
            else
            {
                MessageBox.Show("An error ocurred while trying to erase " + CartItem.ProductName + " from the cart");
            }
        }

        private async void SwipeAnimation(ListBoxItem listBoxItem)
        {
            listBoxItem.RenderTransform = new CompositeTransform();

            var swipeAnimation = new DoubleAnimationUsingKeyFrames();
            swipeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                Value = App.RootFrame.ActualWidth,
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 250))
            });

            Storyboard.SetTarget(swipeAnimation, listBoxItem);
            Storyboard.SetTargetProperty(swipeAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));

            var sb = new Storyboard();
            sb.Children.Add(swipeAnimation);

            await BeginAsync(sb);
        }

        private static Task BeginAsync(Storyboard storyboard)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler completedEventHandler = null;
            completedEventHandler = (sender, o) =>
            {
                storyboard.Completed -= completedEventHandler;
                tcs.TrySetResult(true);
            };
            storyboard.Completed += completedEventHandler;
            storyboard.Begin();

            return tcs.Task;
        }
    }
}