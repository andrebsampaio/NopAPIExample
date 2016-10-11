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
using System.Windows.Media.Imaging;
using System.IO;
using NopMobile.WP.Client.Helpers;
using System.Windows.Input;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Text.RegularExpressions;

namespace NopMobile.WP.Client.Pages
{
    public partial class Search : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        FoundProductData SelectedProduct;
        string URL;
        public Search()
        {
            InitializeComponent();
            GetStoreUrl();
        }

        private async void GetStoreUrl()
        {
            URL = await api.GetStoreUrl();
        }

        private async void SearchForProducts(string Parameters)
        {
            var Search = await api.SearchProducts(Parameters);
            ShowResults(Search);
            
        }

        private async void ShowResults(ProductDTO[] Found)
        {
            var Currency = await api.GetCurrency();
            var ResultList = new List<FoundProductData>();
            foreach (ProductDTO p in Found)
            {
                
                try 
                {
                    string Rating = "";
                    if (p.Rating == 0)
                    {
                        Rating = "No Rating available";
                    }
                    else
                    {
                        for (int i = 0; i < p.Rating; i++)
                        {
                            Rating += "\ue113".ToString();
                        }
                    }
                    var Descript = p.Description;
                    if (Descript.Equals(string.Empty))
                    {
                        Descript = "No description";
                    }
                    ResultList.Add(new FoundProductData { Id = p.Id, Description = Descript, Name = p.Name, Image = Helper.ConvertToBitmapImage(p.Image.First()), Price = p.Price.ToString("0.0#") + " " + Currency, Rating = Rating });
                }
                catch (Exception ex)
                {

                }
                
            }
            if (ResultList.Count > 0)
            {
                HideNoResultsMessage();
            }
            else
            {
                ShowNoResultsMessage();
            }
            ProductsList.ItemsSource = ResultList;
            HideLoading();
            
        }

        private void ShowNoResultsMessage()
        {
            NoResults.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideNoResultsMessage()
        {
            NoResults.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SearchBox_ActionIconTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowLoading();
            SearchForProducts(SearchBox.Text);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                ApplicationBar.IsVisible = false;
                ShowLoading();
                var Search = NavigationContext.QueryString["Search"];
                SearchBox.Text = Search;
                SearchForProducts(Search);  

            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
           
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                ShowLoading();
                SearchForProducts(SearchBox.Text);
            }
        }

        private void ShowLoading()
        {
            ProductsList.Visibility = System.Windows.Visibility.Collapsed;
            LoadingHolder.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideLoading()
        {
            ProductsList.Visibility = System.Windows.Visibility.Visible;
            LoadingHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private class FoundProductData
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Name { get; set; }
            public string Price { get; set; }
            public string Rating { get;set; }
            public BitmapImage Image { get; set; }
        }

        private async Task<bool> HasAttributes(int ProdId){
            var Product = await api.GetProductById(ProdId);
            return Product.Attributes.Count != 0;

        }

        private async void AddToWish_Click(object sender, EventArgs e)
        {
            var attributes = await HasAttributes(SelectedProduct.Id);
            if (!attributes)
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
                    Message = "Select how many " + SelectedProduct.Name + " do you want?",
                    LeftButtonContent = "Add To Wishlist",
                    Content = s,
                    RightButtonContent = "Cancel"
                };
                messageBox.Show();
                messageBox.Dismissed += async (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            var Customer = await api.GetCustomersByEmail(Helper.CurrentCustomer());
                            if (Quantity.Text == "") Quantity.Text = "1";
                            var AddResult = await api.AddToCart(Customer.First().Email, SelectedProduct.Id, Int32.Parse(Quantity.Text), new String[] { "" }, ShoppingCartType.Wishlist);
                            if (AddResult)
                            {
                                CustomMessageBox SuccessToast = new CustomMessageBox()
                                {
                                    Caption = "Added Successfully",
                                    Message = "The product was added to your wishlist sucessfuly",
                                    LeftButtonContent = "Dismiss"
                                };
                                SuccessToast.Show();
                                await Task.Delay(2000);
                                SuccessToast.Dismiss();
                            }
                            break;
                    }
                };
            }
            else
            {
                NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + SelectedProduct.Id, UriKind.Relative));
            }
        }

        private async void AddToCart_Click(object sender, EventArgs e)
        {
            var attributes = await HasAttributes(SelectedProduct.Id);
            if (!attributes)
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
                    Message = "Select how many " + SelectedProduct.Name + " do you want?",
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
                            var Customer = await api.GetCustomersByEmail(Helper.CurrentCustomer());
                            if (Quantity.Text == "") Quantity.Text = "1";
                            var AddResult = await api.AddToCart(Customer.First().Email, SelectedProduct.Id, Int32.Parse(Quantity.Text), new String[] { "" }, ShoppingCartType.ShoppingCart);
                            if (AddResult)
                            {
                                CustomMessageBox SuccessToast = new CustomMessageBox()
                                {
                                    Caption = "Added Successfully",
                                    Message = "The product was added to your cart sucessfuly",
                                    LeftButtonContent = "Dismiss"
                                };
                                SuccessToast.Show();
                                if (UserSettings.Contains("refresh_cart"))
                                {
                                    UserSettings.Remove("refresh_cart");
                                    UserSettings.Add("refresh_cart", true);
                                }
                                await Task.Delay(2000);
                                SuccessToast.Dismiss();
                            }
                            break;
                    }
                };
            }
            else
            {
                NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + SelectedProduct.Id, UriKind.Relative));
            }
        }

        private void Share_Click(object sender, EventArgs e)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = SelectedProduct.Name;
            shareLinkTask.LinkUri = new Uri(URL + PrepareUrl(), UriKind.Absolute);
            shareLinkTask.Message = SelectedProduct.Description;
            shareLinkTask.Show();
        }

        private string PrepareUrl()
        {
            var NoSpaces = SelectedProduct.Name.Replace(" ", "-");
            return Regex.Replace(NoSpaces, "[^-0-9a-zA-Z]+", "");

        }

        private void ProductDetails_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + SelectedProduct.Id, UriKind.Relative));
        }

        private void ProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
            SelectedProduct = ProductsList.SelectedItem as FoundProductData;
        }
    }
}