using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using NopMobile.AppCore.NopAPI;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using NopMobile.WP.Client.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;

namespace NopMobile.WP.Client.Pages
{
    public partial class ProductDetails : PhoneApplicationPage
    {
        DoubleAnimation _scrollAnimation;
        Storyboard _scrollViewerStoryboard;
        private DependencyProperty _horizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(ProductDetails), new PropertyMetadata(0.0, OnHorizontalOffsetChanged));
        ProductDTO Product;
        private IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        NopCore api = new NopCore();
        int Id = -1;
        bool RefreshCart = false;
        string URL;
        IList<Object> Controls = new List<Object>();
        public ProductDetails()
        {
            InitializeComponent();
            ImagesCarousel();
        }

        private async void InitializeDetails(){
            Product = await api.GetProductById(Id);
            Name.Text = Product.Name;
            URL = await api.GetStoreUrl();
            var Currency = await api.GetCurrency();
            Price.Text = Product.Price.ToString("0.0#") + " " + Currency;
            if (Product.inStock)
            {
                Availability.Text = "In Stock";
                AvailabilityHolder.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                AvailabilityHolder.Background = new SolidColorBrush(Colors.Red);
                Availability.Text = "Out of Stock";
            }
            Description.Text = CleanDescription(Product.ExtendedDescription);
            foreach (byte[] i in Product.Image)
            {
                StackPanel sp = new StackPanel();
                sp.Width = 480;
                sp.Height = 300;
                var img = new Image();
                img.Source = Helper.ConvertToBitmapImage(i) as ImageSource;
                img.MinHeight = 100;
                img.MinWidth = 240;
                img.MaxHeight = 250;
                img.MaxWidth = 400;
                sp.Children.Add(img);
                sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                sp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                ImageList.Children.Add(sp);
            }

            for (int i = 0; i < Product.Attributes.Count; i++)
            {
                StackPanel sp = new StackPanel();
                switch(Product.Attributes[i].AttributeControl){
                    case AttributeControlType.TextBox:
                        PhoneTextBox p = new PhoneTextBox();
                        p.Hint = Product.Attributes[i].Name;
                        Controls.Add(p);
                        sp.Children.Add(p);
                        break;
                    case AttributeControlType.DropdownList:
                        ListPicker l = new ListPicker();
                        l.Header = Product.Attributes[i].Name;
                        foreach (string v in Product.Attributes[i].Values)
                        {
                            l.Items.Add(v);
                        }
                        Controls.Add(l);
                        sp.Children.Add(l);
                        break;
                    case AttributeControlType.MultilineTextbox:
                        PhoneTextBox m = new PhoneTextBox();
                        m.MinHeight = 150;
                        m.Hint = Product.Attributes[i].Name;
                        Controls.Add(m);
                        sp.Children.Add(m);
                        break;
                    case AttributeControlType.RadioList:
                        TextBlock titleRadio = new TextBlock();
                        titleRadio.Text = Product.Attributes[i].Name;
                        titleRadio.Style = Resources["PhoneTextSubtleStyle"] as Style;
                        sp.Children.Add(titleRadio);
                        var TmpRadio = new List<RadioButton> (Product.Attributes[i].Values.Count);
                        foreach (string s in Product.Attributes[i].Values)
                        {
                            RadioButton rv = new RadioButton();
                            rv.GroupName = Product.Attributes[i].Name;
                            rv.Content = s;
                            TmpRadio.Add(rv);
                            sp.Children.Add(rv);
                        }
                        Controls.Add(TmpRadio);
                        break;
                    case AttributeControlType.Checkboxes:
                        TextBlock titleCheck = new TextBlock();
                        titleCheck.Text = Product.Attributes[i].Name;
                        titleCheck.Style =  Resources["PhoneTextSubtleStyle"] as Style;
                        sp.Children.Add(titleCheck);
                        var TmpCheck = new List<CheckBox>(Product.Attributes[i].Values.Count);
                        foreach (string s in Product.Attributes[i].Values)
                        {
                            CheckBox c = new CheckBox();
                            c.Content = s;
                            TmpCheck.Add(c);
                            sp.Children.Add(c);
                        }
                        Controls.Add(TmpCheck);
                        break;
                        
                }
                Attributes.Children.Add(sp);
            }
        }
         
        private void ImagesCarousel(){
 
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

        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProductDetails app = d as ProductDetails;
            app.OnHorizontalOffsetChanged(e);
        }
 
        private void OnHorizontalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            this.ImageCarousel.ScrollToHorizontalOffset((double)e.NewValue);
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            var startPosition = this.ImageCarousel.HorizontalOffset;
            if (startPosition > 0)
            {
                _scrollAnimation.From = startPosition;
                _scrollAnimation.To = startPosition - 480;
                _scrollViewerStoryboard.Begin();
            }
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            var startPosition = this.ImageCarousel.HorizontalOffset;
            if (startPosition == (Product.Image.Count - 1) * 480)
            {
                _scrollAnimation.From = (Product.Image.Count - 1) * 480;
                _scrollAnimation.To = 0;
            }
            else
            {
                _scrollAnimation.From = 0 + startPosition;
                _scrollAnimation.To = 480 + startPosition;
               
            }
            _scrollViewerStoryboard.Begin();
            
        }

        private void AddToWishlist_Click(object sender, EventArgs e)
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
                Message = "Select how many " + Product.Name + " do you want?",
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
                        var Current = "";
                        var allChecked = true;
                        UserSettings.TryGetValue<string>("current_user", out Current);
                        var AttributesArray = new List<string>();
                        for (int i = 0; i < Product.Attributes.Count; i++)
                            switch (Product.Attributes[i].AttributeControl)
                            {
                                case AttributeControlType.TextBox:
                                    var TmpText = (PhoneTextBox)Controls[i];
                                    if (TmpText.Text.Equals("") && Product.Attributes[i].isRequired)
                                    {
                                        MessageBox.Show("You must fill " + Product.Attributes[i].Name);
                                        allChecked = false;
                                    }
                                    AttributesArray.Add(TmpText.Text);
                                    break;
                                case AttributeControlType.DropdownList:
                                    var TmpDrop = (ListPicker)Controls[i];
                                    if (TmpDrop.SelectedItem == null && Product.Attributes[i].isRequired)
                                    {
                                        MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                        allChecked = false;
                                    }
                                    AttributesArray.Add(TmpDrop.SelectedItem.ToString());
                                    break;
                                case AttributeControlType.MultilineTextbox:
                                    var TmpTextM = (PhoneTextBox)Controls[i];
                                    if (TmpTextM.Text.Equals("") && Product.Attributes[i].isRequired)
                                    {
                                        MessageBox.Show("You must fill " + Product.Attributes[i].Name);
                                        allChecked = false;
                                    }
                                    AttributesArray.Add(TmpTextM.Text);
                                    break;
                                case AttributeControlType.RadioList:
                                    var TmpRadio = (List<RadioButton>)Controls[i];
                                    var Count = 0;
                                    foreach (RadioButton r in TmpRadio)
                                    {
                                        if ((bool)r.IsChecked)
                                        {
                                            AttributesArray.Add(r.Content.ToString());
                                            Count++;
                                        }
                                    }
                                    if (Count == 0 && Product.Attributes[i].isRequired)
                                    {
                                        MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                        allChecked = false;
                                    }
                                    break;
                                case AttributeControlType.Checkboxes:
                                    var TmpCheck = (List<CheckBox>)Controls[i];
                                    var CountCheck = 0;
                                    foreach (CheckBox r in TmpCheck)
                                    {
                                        if ((bool)r.IsChecked)
                                        {
                                            AttributesArray.Add(r.Content.ToString());
                                            CountCheck++;
                                        }
                                    }
                                    if (CountCheck == 0 && Product.Attributes[i].isRequired)
                                    {
                                        MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                        allChecked = false;
                                    }
                                    break;
                            }
                        if (allChecked)
                        {
                            if (Quantity.Text == "") Quantity.Text = "1";
                            var AddResult = await api.AddToCart(Current, Product.Id, Int32.Parse(Quantity.Text), AttributesArray.ToArray(), ShoppingCartType.Wishlist);
                            if (AddResult)
                            {
                                RefreshCart = true;
                                CustomMessageBox SuccessToast = new CustomMessageBox()
                                {
                                    Caption = "Added Successfully",
                                    Message = "The product was added to your wishlist sucessfuly",
                                    LeftButtonContent = "Dismiss"
                                };
                                SuccessToast.Show();
                                await Task.Delay(2500);
                                SuccessToast.Dismiss();
                            }
                        }
                        break;
                }
            };
        }

        private void Share_Click(object sender, EventArgs e)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = Product.Name;
            shareLinkTask.LinkUri = new Uri(URL + PrepareUrl(), UriKind.Absolute);
            shareLinkTask.Message = Product.Description;
            shareLinkTask.Show();
        }

        private string PrepareUrl()
        {
            var NoSpaces = Product.Name.Replace(" ", "-");
            return Regex.Replace(NoSpaces, "[^-0-9a-zA-Z]+", "");

        }

        private void AddToCart_Click(object sender, EventArgs e)
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
                    Message = "Select how many " + Product.Name + " do you want?",
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
                            var Current = "";
                            var allChecked = true;
                            UserSettings.TryGetValue<string>("current_user", out Current);
                            var AttributesArray = new List<string>();
                            for (int i = 0; i < Product.Attributes.Count; i++ )
                                switch (Product.Attributes[i].AttributeControl)
                                {
                                    case AttributeControlType.TextBox:
                                        var TmpText = (PhoneTextBox) Controls[i];
                                        if (TmpText.Text.Equals("") && Product.Attributes[i].isRequired)
                                        {
                                            MessageBox.Show("You must fill " + Product.Attributes[i].Name);
                                            allChecked = false;
                                        }
                                        AttributesArray.Add(TmpText.Text);
                                        break;
                                    case AttributeControlType.DropdownList:
                                        var TmpDrop = (ListPicker) Controls[i];
                                        if (TmpDrop.SelectedItem == null && Product.Attributes[i].isRequired)
                                        {
                                            MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                            allChecked = false;
                                        }
                                        AttributesArray.Add( TmpDrop.SelectedItem.ToString());
                                        break;
                                    case AttributeControlType.MultilineTextbox:
                                        var TmpTextM = (PhoneTextBox) Controls[i];
                                        if (TmpTextM.Text.Equals("") && Product.Attributes[i].isRequired)
                                        {
                                            MessageBox.Show("You must fill " + Product.Attributes[i].Name);
                                            allChecked = false;
                                        }
                                        AttributesArray.Add(TmpTextM.Text);
                                        break;
                                    case AttributeControlType.RadioList:
                                        var TmpRadio = (List <RadioButton>) Controls[i];
                                        var Count = 0;
                                        foreach (RadioButton r in TmpRadio){
                                            if ((bool)r.IsChecked){
                                                AttributesArray.Add( r.Content.ToString());
                                                Count++;
                                            }
                                        }
                                        if (Count == 0 && Product.Attributes[i].isRequired)
                                        {
                                            MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                            allChecked = false;
                                        }
                                        break;
                                    case AttributeControlType.Checkboxes:
                                        var TmpCheck = (List <CheckBox>) Controls[i];
                                        var CountCheck = 0;
                                        foreach (CheckBox r in TmpCheck){
                                            if ((bool)r.IsChecked){
                                                AttributesArray.Add( r.Content.ToString());
                                                CountCheck++;
                                            }
                                        }
                                        if (CountCheck == 0 && Product.Attributes[i].isRequired)
                                        {
                                            MessageBox.Show("You must select " + Product.Attributes[i].Name);
                                            allChecked = false; 
                                        }
                                        break;
                                }
                            if (allChecked)
                            {
                                if (Quantity.Text == "") Quantity.Text = "1";
                                var AddResult = await api.AddToCart(Current, Product.Id, Int32.Parse(Quantity.Text), AttributesArray.ToArray(), ShoppingCartType.ShoppingCart);
                                if (AddResult)
                                {
                                    RefreshCart = true;
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
                            }
                            break;
                    }
                };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Id = Int32.Parse(NavigationContext.QueryString["ProdId"]);
            InitializeDetails();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (RefreshCart)
            {
                try
                {
                    UserSettings.Remove("refresh_cart");
                    UserSettings.Add("refresh_cart", true);

                } catch (Exception ex){
                    Console.WriteLine(ex.Message);
                    UserSettings.Add("refresh_cart", true);
                }
                NavigationService.GoBack();
            }
            else
            {
                if (UserSettings.Contains("refresh_cart"))
                {
                    UserSettings.Remove("refresh_cart");
                    UserSettings.Add("refresh_cart", false);
                }
            }
            base.OnBackKeyPress(e);
        }

        private void ReadDescription_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer s = new ScrollViewer();
            s.Height = 500;
            TextBlock t = new TextBlock();
            t.TextWrapping = TextWrapping.Wrap;
            t.Text = CleanDescription(Product.ExtendedDescription);
            t.Margin = new Thickness(12, 0, 12, 0);
            t.FontSize = 20;
            s.Content = t;
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Full Description",
                LeftButtonContent = "close",
                Content = s
            };
            messageBox.Show();
        }

        private string CleanDescription(string desc)
        {
            return desc.Substring(3, desc.Length - 7);
        }
    }
}