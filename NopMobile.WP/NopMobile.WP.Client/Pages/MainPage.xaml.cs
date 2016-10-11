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
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Input;
using NopMobile.WP.Client.Helpers;

namespace NopMobile.WP.Client.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DependencyProperty _horizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(MainPage), new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        CustomerDTO Customer;
        NopCore api = new NopCore();
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        private DoubleAnimation _scrollAnimation;
        private Storyboard _scrollViewerStoryboard;
        private bool isClicked = false;
        private int CarouselCount = 1;
        private int FeaturedCount = 0;
        private string Currency = "";

        private const int Bestseller = 0;
        private const int Featured = 1;

        public MainPage()
        {
            InitializeComponent();
            ShowLoading();
            MainPan.DefaultItem = MainPan.Items[1];
            InitializeMainPages();
            FeaturedCarouselRotate();
        }

        private async void InitializeMainPages()
        {
            string Current = "";
            UserSettings.TryGetValue("current_user", out Current);
            var tmp = await api.GetCustomersByEmail(Current);
            Currency = await api.GetCurrency();
            Customer = tmp.First();
            CustomerName.Text = Customer.FullName;
            var Featured = await api.FeaturedProducts();
            FeaturedCount = Featured.Count();
            var FeaturedList = new List<ProductData>();
            int FeatCount = Helper.GetSetting<int>("featured_count");
            int tmpCount = 0;
            foreach (ProductDTO p in Featured){
                if (tmpCount == FeatCount) break;
                FeaturedList.Add(new ProductData { Id = p.Id, ProductName = p.Name, Description = p.Description, Image = Helper.ConvertToBitmapImage(p.Image.First()), Value = p.Price.ToString("0") + " " + Currency });
                tmpCount++;
            }
            FeaturedProducts.ItemsSource = FeaturedList;
            var BestsellersA = await api.GetBestsellerByAmount();
            var BestsellersQ = await api.GetBestsellerByQuantity();
            var Bests = new List<ProductData>();
            var BestCount = Helper.GetSetting<int>("bestsellers_count") / 2;
            for (int i = 0; i < BestCount ; i++ )
            {
                if (i < BestsellersA.Count())
                {
                    Bests.Add(new ProductData { Id = BestsellersA[i].Product.Id, ProductName = BestsellersA[i].Product.Name, Value = BestsellersA[i].Product.Price.ToString("0.#") + " " + Currency, Image = Helper.ConvertToBitmapImage(BestsellersA[i].Product.Image.First()) });
                }
                if (i < BestsellersQ.Count())
                {
                    Bests.Add(new ProductData { Id = BestsellersQ[i].Product.Id, ProductName = BestsellersQ[i].Product.Name, Value = BestsellersQ[i].Product.Price.ToString("0.#") + " " + Currency, Image = Helper.ConvertToBitmapImage(BestsellersQ[i].Product.Image.First()) });
                }
            }
            BestsellersList.ItemsSource = Bests;
            var Categories = await api.GetMainCategories();
            var CategoriesList = new List<CategoryData>();
            foreach (CategoryDTO c in Categories)
            {
                CategoriesList.Add(new CategoryData {Id = c.Id, Name = c.Name, Image = Helper.ConvertToBitmapImage(c.Image)});
            }
            CategoriesListbox.ItemsSource = CategoriesList;
            HideLoading();
        }

        private async void RefreshCart()
        {
            string Current = "";
            UserSettings.TryGetValue("current_user", out Current);
            var tmp = await api.GetCustomersByEmail(Current);
            Customer = tmp.First();
            var CartList = new List<CartData>();
            var ItemCount = 0;
            foreach (CartItemDTO c in Customer.ShoppingCart)
            {
                ItemCount += c.Quantity;
                CartList.Add(new CartData { ProdId = c.Product.Id, Id = c.Id, ProductName = c.Product.Name, Quantity = c.Quantity, Image = Helper.ConvertToBitmapImage(c.Product.Image.First()), UnitPrice = c.Product.Price.ToString("0.0#") + " " + Currency, Total = (c.Product.Price * c.Quantity).ToString("0.0#") + " " + Currency });
            }
            if (CartList.Count == 0)
            {
                CartCount.Text = "no";
            }
            else
            {
                CartCount.Text = ItemCount.ToString();
            }
            
            ShoppingCartList.ItemsSource = CartList;
        }

        private void FeaturedCarouselRotate(){
            _scrollAnimation = new DoubleAnimation()
            {
                EasingFunction = new SineEase(),
                Duration = TimeSpan.FromSeconds(0.5)
            };

            Storyboard.SetTarget(_scrollAnimation, this);
            Storyboard.SetTargetProperty(_scrollAnimation, new PropertyPath(_horizontalOffsetProperty));

            _scrollViewerStoryboard = new Storyboard();
            _scrollViewerStoryboard.Children.Add(_scrollAnimation);
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!isClicked)
            {
                isClicked = true;
                var startPosition = this.Carousel.HorizontalOffset;
                if (startPosition > 0)
                {
                    if (startPosition % 480 != 0)
                    {
                        startPosition = 0;
                        CarouselCount = 0;
                    }
                    _scrollAnimation.From = startPosition;
                    _scrollAnimation.To = startPosition - 480;
                    _scrollViewerStoryboard.Begin();
                }
                isClicked = false;
                CarouselCount--;
            }
            
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            if (!isClicked)
            {
                isClicked = true;
                var startPosition = this.Carousel.HorizontalOffset;
                if (CarouselCount == FeaturedCount)
                {
                    if (startPosition % 480 != 0)
                    {
                        startPosition = 0;
                        CarouselCount = 0;
                    }
                    _scrollAnimation.From = 0 + startPosition;
                    _scrollAnimation.To = 0;
                    CarouselCount = 0;
                }
                else
                {
                    if (startPosition % 480 != 0)
                    {
                        startPosition = 0;
                        CarouselCount = 0;
                    }
                    _scrollAnimation.From = 0 + startPosition;
                    _scrollAnimation.To = 480 + startPosition;
                }
                
                _scrollViewerStoryboard.Begin();
                isClicked = false;
                CarouselCount++;
            }
            
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentIndex = MainPan.SelectedIndex;
            if (currentIndex == 3)
            {
                ApplicationBar.IsVisible = true;
            }
            else
            {
                ApplicationBar.IsVisible = false;
            }
            
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Checkout.xaml", UriKind.Relative));
        }

        private void Logout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Logout",
                Message = "Are you sure you want to exit store?",
                LeftButtonContent = "Yes",
                RightButtonContent = "No"
            };

            messageBox.Show();

            messageBox.Dismissed += (s1, e1) => {
                if (e1.Result.Equals(CustomMessageBoxResult.LeftButton))
                {
                    if (UserSettings.Contains("stay_connected"))
                    {
                        UserSettings.Remove("stay_connected");
                        UserSettings.Add("stay_connected", false);
                    }
                    NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
                }
            };
        }

        public class ProductData
        {
            public int Id { get; set; }
            public string ProductName { get; set; }
            public BitmapImage Image { get; set; }
            public string Value { get; set; }
            public string Description { get; set;}
        }

        public class CategoryData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public BitmapImage Image { get; set; }
        }

        public class CartData
        {
            public int ProdId { get; set; }
            public string ProductName { get; set; }
            public BitmapImage Image { get; set; }
            public string UnitPrice { get; set; }
            public int Quantity { get; set; }
            public string Total { get; set; }
            public int Id { get; set; }
        }

        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainPage app = d as MainPage;
            app.OnHorizontalOffsetChanged(e);
        }

        private void OnHorizontalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            this.Carousel.ScrollToHorizontalOffset((double)e.NewValue);
        }

        private async void RemoveCart_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            DependencyObject tappedElement = e.OriginalSource as UIElement;
            var TappedItem = this.FindParentOfType<ListBoxItem>(tappedElement);
            var Item = TappedItem.Content as CartData;
            SwipeAnimation(TappedItem);
            var RemoveResult = await api.RemoveFromCart(Customer.Email, Item.Id);
            if (RemoveResult)
            {
                string Current = "";
                UserSettings.TryGetValue("current_user", out Current);
                var CustomerTmp = await api.GetCustomersByEmail(Current);
                Customer = CustomerTmp.First();
                RefreshCart();
            }
            else
            {
                MessageBox.Show("An error ocurred while trying to erase " + Item.ProductName + " from the cart" );
            }
            
        }

        private T FindParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            T result = null;
            DependencyObject currentElement = element;
            while (currentElement != null)
            {
                result = currentElement as T;
                if (result != null)
                {
                    break;
                }
                currentElement = VisualTreeHelper.GetParent(currentElement);
            }

            return result;
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

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            AddToCustomerCart(sender, e); 
        }

        private async void AddToCustomerCart(object sender, RoutedEventArgs e)
        {
            DependencyObject tappedElement = e.OriginalSource as UIElement;
            var List = this.FindParentOfType<ItemsControl>(tappedElement);
            var ListName = List.Name;
            var TappedItem = this.FindParentOfType<ContentPresenter>(tappedElement);
            ProductData item = null;
            if (ListName.Equals("BestsellersList"))
            {
                item = BestsellersList.ItemContainerGenerator.ItemFromContainer(TappedItem) as ProductData;
            }
            else
            {
                item = FeaturedProducts.ItemContainerGenerator.ItemFromContainer(TappedItem) as ProductData;
            }
            var Prod = await api.GetProductById(item.Id);
            if (Prod.Attributes.Count > 0){
                NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + Prod.Id  , UriKind.Relative));
            }
            else
            {
                StackPanel s = new StackPanel();
                PhoneTextBox Quantity = new PhoneTextBox();
                InputScope scope = new InputScope();
                InputScopeName number = new InputScopeName();

                number.NameValue = InputScopeNameValue.Number;
                scope.Names.Add(number);
                Quantity.Hint = "Quantity";
                Quantity.InputScope = scope;
                s.Children.Add(Quantity);
                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Caption = "Select Quantity",
                    Message = "Select how many " + item.ProductName + " do you want?",
                    LeftButtonContent = "Add To Cart",
                    Content = s,
                    RightButtonContent = "Cancel"
                };
                messageBox.Show();
                messageBox.Dismissed += async (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            if (Quantity.Text == "") Quantity.Text = "1";
                            var AddResult = await api.AddToCart(Customer.Email, item.Id, Int32.Parse(Quantity.Text), new String []{""}, ShoppingCartType.ShoppingCart);
                            if (AddResult)
                            {
                                CustomMessageBox SuccessToast = new CustomMessageBox()
                                {
                                    Caption = "Added Successfully",
                                    Message = "The product was added to your cart sucessfuly",
                                    LeftButtonContent = "Dismiss"
                                };
                                SuccessToast.Show();
                                await Task.Delay(2500);
                                SuccessToast.Dismiss();
                            }
                            RefreshCart();
                            break;
                    }
                };
            }
        }

        private void ShowLoading()
        {
            MainPageHolder.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideLoading()
        {
            MainPageHolder.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ProductDetails_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           
            DependencyObject tappedElement = e.OriginalSource as UIElement;
            var List = this.FindParentOfType<ItemsControl>(tappedElement);
            var ListName = List.Name;
            var TappedItem = this.FindParentOfType<ContentPresenter>(tappedElement);
            ProductData item = null;
            if (ListName.Equals("BestsellersList"))
            {
                item = BestsellersList.ItemContainerGenerator.ItemFromContainer(TappedItem) as ProductData;
            }
            else
            {
                item = FeaturedProducts.ItemContainerGenerator.ItemFromContainer(TappedItem) as ProductData;
            }
            NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + item.Id  , UriKind.Relative));
        }

        private void CartProduct_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var Prod = ShoppingCartList.SelectedItem as CartData;
            NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + Prod.ProdId, UriKind.Relative));

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                RefreshCart();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }  
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Save();
            Application.Current.Terminate();
        }

        private void CategoriesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesListbox.SelectedIndex != -1)
            {
                var item = CategoriesListbox.SelectedItem as CategoryData;
                NavigationService.Navigate(new Uri("/Pages/Category.xaml?CatId=" + item.Id, UriKind.Relative));
            }
            CategoriesListbox.SelectedIndex = -1;
        }

        private void Wishlist_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Wishlist.xaml", UriKind.Relative));
        }

        private void Addresses_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Addresses.xaml", UriKind.Relative));
        }

        private void MyOrders_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/MyOrders.xaml", UriKind.Relative));
        }

        private void Preferences_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
        }
    }
}