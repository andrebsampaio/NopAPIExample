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
using System.IO.IsolatedStorage;
using NopMobile.WP.Client.Helpers;
using System.Threading.Tasks;

namespace NopMobile.WP.Client.Pages
{
    public partial class Category : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        int Id = 0;
        CategoryDTO[] Categories;
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        string Currency;
        bool Ordered = false; bool Filtered = false;
        bool Initialized = false;
        public Category()
        {
            InitializeComponent();
        }

        private async void InitializeCategory(){
            Categories = await api.GetSubCategoriesFromParent(Id);
            var ParentCat = await api.GetCategoryById(Id);
            CategoryPivot.Title = ParentCat.Name.ToUpper();
            Currency = await api.GetCurrency();
            if (Categories.Count() == 0)
            {
                ToggleBar(true);
                try
                {
                    var Products = await api.GetAllProductsFromCategory(Id);
                    var Page = new PivotItem();
                    Page.Header = "all products";
                    CategoryPivot.Items.Add(Page);
                    var Control = new ListBox();
                    Control.ItemContainerStyle = Application.Current.Resources["NoSelectColor"] as Style;
                    Control.ItemsPanel = Application.Current.Resources["ProductCategoryWrapper"] as ItemsPanelTemplate;
                    Control.ItemTemplate = Application.Current.Resources["ProductCategoryTemplate"] as DataTemplate;
                    foreach (ProductDTO p in Products)
                    {
                        Control.Items.Add(new MainPage.ProductData { Id = p.Id, Description = p.Description, Image = Helper.ConvertToBitmapImage(p.Image.First()), ProductName = p.Name, Value = p.Price.ToString("0.0#") + " " + Currency });
                    }
                    Control.SelectionChanged += Control_SelectionChanged;

                    Page.Content = Control;
                }
                catch (Exception ex)
                {

                }
                
            }
            else
            {
               
                foreach(CategoryDTO c in Categories){
                    var CatList = new ListBox();
                    CatList.ItemContainerStyle = Application.Current.Resources["NoSelectColor"] as Style;
                    CatList.ItemsPanel = Application.Current.Resources["ProductCategoryWrapper"] as ItemsPanelTemplate;
                    
                    var Page = new PivotItem();
                    Page.Header = c.Name.ToLower();
                    CategoryPivot.Items.Add(Page);
                    var SubCats = await api.GetSubCategoriesFromParent(c.Id);
                    
                    if (SubCats.Count() == 0)
                    {
                        if (Categories.First().Equals(c))
                        {
                            ToggleBar(true);
                        }
                        CatList.ItemTemplate = Application.Current.Resources["ProductCategoryTemplate"] as DataTemplate;
                        var Products = await api.GetAllProductsFromCategory(c.Id);
                        foreach (ProductDTO p in Products)
                        {
                            CatList.Items.Add(new MainPage.ProductData { Id = p.Id, Description = p.Description, Image = Helper.ConvertToBitmapImage(p.Image.First()), ProductName = p.Name, Value = p.Price.ToString("0.0#") + " " + Currency });
                        }
                        CatList.SelectionChanged += Control_SelectionChanged;
                    }
                    else
                    {
                        CatList.ItemTemplate = Application.Current.Resources["CategoryTemplate"] as DataTemplate;
                        foreach (CategoryDTO subC in SubCats)
                        {
                            CatList.Items.Add(new MainPage.CategoryData { Id = subC.Id, Image = Helper.ConvertToBitmapImage(subC.Image), Name = subC.Name });
                        }
                    }
                    Page.Content = CatList;
                }
            }
            Initialized = true;
        }

        private void ToggleBar(bool Show)
        {
            if (Show)
            {
                ApplicationBar.IsVisible = true;
            }
            else
            {
                ApplicationBar.IsVisible = false;
            }

        }

        void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var List = sender as ListBox;
            var Selected = List.SelectedItem;
            if (List.SelectedIndex != -1)
            {
                if (Selected is MainPage.ProductData)
                {
                    var Convert = Selected as MainPage.ProductData;
                    NavigationService.Navigate(new Uri("/Pages/ProductDetails.xaml?ProdId=" + Convert.Id, UriKind.Relative));
                }
                else
                {
                    var Convert = Selected as MainPage.CategoryData;
                    NavigationService.Navigate(new Uri("/Pages/Category.xaml?CatId=" + Convert.Id, UriKind.Relative));
                }
            }
            List.SelectedIndex = -1;
            
        }

        public async void RemoveFromCart(ListBoxItem Item)
        {
            var CartItem = Item.Content as MainPage.CartData;
            var Result = await api.RemoveFromCart(Helper.CurrentCustomer(),CartItem.Id);
            if (Result)
            {
                var success = new CustomMessageBox()
                {
                    Title = "Removed Successfully",
                    Message = CartItem.ProductName + " was removed from your cart"
                };
                success.Show();
                await Task.Delay(2000);
                success.Dismiss();
            }
            else
            {

            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Id = Int32.Parse(NavigationContext.QueryString["CatId"]);
            if (!Initialized)
            {
                InitializeCategory();
            }
            
        }

        private void Filter_Click(object sender, EventArgs e)
        {
            StackPanel s = new StackPanel();
            var Min = new PhoneTextBox();
            var Max = new PhoneTextBox();
            Min.Hint = "Minimum Price";
            Max.Hint = "Maximum Price";
            s.Children.Add(Min);
            s.Children.Add(Max);
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Filter by",
                Message = "Select the minimum and maximum prices, leave in blank to disable limit",
                LeftButtonContent = "Submit",
                Content = s,
                RightButtonContent = "Cancel"
            };
            messageBox.Show();
            messageBox.Dismissed += async (s1, e1) => {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        var Count = 0;
                        foreach (PivotItem pivot in CategoryPivot.Items)
                        {
                            var Listbox = Helper.FindFirstElementInVisualTree<ListBox>(pivot);
                            Listbox.Items.Clear();
                            if (Listbox.ItemTemplate.Equals(Application.Current.Resources["ProductCategoryTemplate"] as DataTemplate))
                            {
                                var Prods = await api.CategoryProductsSortedFiltered(Categories[Count].Id, false, true, ProductSortingEnum.Position, Decimal.Parse(Max.Text), Decimal.Parse(Min.Text));
                                foreach (ProductDTO p in Prods)
                                {
                                    Listbox.Items.Add(new MainPage.ProductData { Id = p.Id, Description = p.Description, Image = Helper.ConvertToBitmapImage(p.Image.First()), ProductName = p.Name, Value = p.Price.ToString("0.0#") + " " + Currency });
                                }
                            }
                            Count++;
                        }
                        break;
                }
            };

        }

        private void OrderBy_Click(object sender, EventArgs e)
        {
            StackPanel s = new StackPanel();
            var Picker = new ListPicker();
            Picker.SetValue(ListPicker.ItemCountThresholdProperty, 6);
            var PickerItems = new string[] { ProductSortingEnum.CreatedOn.ToString(),ProductSortingEnum.NameAsc.ToString(),
                ProductSortingEnum.NameDesc.ToString(), ProductSortingEnum.Position.ToString(), ProductSortingEnum.PriceAsc.ToString(), ProductSortingEnum.PriceDesc.ToString() };
            Picker.ItemsSource = PickerItems;
            s.Children.Add(Picker);
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Order By",
                Message = "Select the order you want the products to be displayed",
                LeftButtonContent = "Submit",
                Content = s,
                RightButtonContent = "Cancel"
            };
            messageBox.Show();
            messageBox.Dismissed += async (s1, e1) => {
                ProductSortingEnum Result = ProductSortingEnum.Position;
                if (e1.Result.Equals(CustomMessageBoxResult.LeftButton))
                {
                   
                    foreach (ProductSortingEnum pse in Enum.GetValues(typeof(ProductSortingEnum)))
                    {
                        if (pse.ToString().Equals(Picker.SelectedItem.ToString()))
                        {
                            Result = pse;
                            break;
                        }
                    }
                    var Count = 0;
                    foreach (PivotItem pivot in CategoryPivot.Items)
                    {
                        var Listbox = Helper.FindFirstElementInVisualTree<ListBox>(pivot);
                        Listbox.Items.Clear();
                        if (Listbox.ItemTemplate.Equals(Application.Current.Resources["ProductCategoryTemplate"] as DataTemplate))
                        {
                            var Prods = await api.CategoryProductsSortedFiltered(Categories[Count].Id, true, false, Result, 0, 0);
                            foreach (ProductDTO p in Prods)
                            {
                               Listbox.Items.Add(new MainPage.ProductData { Id = p.Id, Description = p.Description, Image = Helper.ConvertToBitmapImage(p.Image.First()), ProductName = p.Name, Value = p.Price.ToString("0.0#") + " " + Currency });
                            }
                        }
                        Count++;
                    }
                }
            };
        }

        private async void Cart_Click(object sender, EventArgs e)
        {
            ListBox Cart = new ListBox();
            Cart.Margin = new Thickness(0, 12, 0, 0);
            Cart.ItemContainerStyle = Application.Current.Resources["NoSelectColor"] as Style;
            Cart.ItemTemplate = Application.Current.Resources["CartItemTemplate"] as DataTemplate;
            var Customer = await api.GetCustomersByEmail(Helper.CurrentCustomer());
            var Products = Customer.First().ShoppingCart;
            foreach (CartItemDTO c in Products)
            {
                Cart.Items.Add(new MainPage.CartData { ProdId = c.Product.Id, Id = c.Id, ProductName = c.Product.Name, Quantity = c.Quantity, Image = Helper.ConvertToBitmapImage(c.Product.Image.First()), UnitPrice = c.Product.Price.ToString("0.0#") + " " + Currency, Total = (c.Product.Price * c.Quantity).ToString("0.0#") + " " + Currency });
            }
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Shopping Cart",
                LeftButtonContent = "Dismiss",
                Content = Cart
            };
            messageBox.Show();
        }

        private void CategoryPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Pivot = sender as Pivot;
            var Item = Pivot.SelectedItem as PivotItem;
            var List = Item.Content as ListBox;
            if (List != null && List.Items.Count > 0)
            {
                if (List.Items[0] is MainPage.ProductData)
                {
                    ToggleBar(true);
                }
                else
                {
                    ToggleBar(false);
                }
            }
            
        }
    }
}