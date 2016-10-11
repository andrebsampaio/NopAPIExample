using System;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Widget;
using NopMobile.Android;
using Android.Content;
using System.Threading.Tasks;
using Android.Util;
using System.Collections.Generic;
using Android.Views.InputMethods;
using Android.Views;
using NopMobile.AppCore.Exceptions;
using Android.Preferences;
using AndroidApi = Android;
using Android.Support.V7.App;
using Android.Text;
using NopMobile.AppCore;
using OxyPlot.XamarinAndroid;
using AndroidNet = Android.Net;
using NopMobile.AppCore.NopAPI;

namespace NopMobile.Android
{
    [Activity(Label = "Dashboard", Theme = "@style/DashboardBar")]
    public class Dashboard : ActionBarActivity
    {
        private SlidingPaneLayout _slidingLayout;
        ProgressDialog dialog;
        NopCore api = new NopCore();
        OrderDTO[] Orders;
        CustomerDTO[] Customers;
        IList<String> urls = new List<String>();
        IDictionary<String, String> UrlsMap = new Dictionary<String, String>();
        public static Activity DashboardA;
        private const string DASHTITLE = "Dashboard";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dashboard);

            _slidingLayout = FindViewById<SlidingPaneLayout>(Resource.Id.sliding_pane_layout);
            
            SupportActionBar.Title = DASHTITLE;

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(true);

            _slidingLayout.PanelOpened += (sender, args) =>
            {
                SupportActionBar.SetHomeButtonEnabled(false);
                SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            };

            _slidingLayout.PanelClosed += (sender, args) =>
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            };

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();

            dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
            InitializeDashboard();
            InitializeSideMenu(prefs);

            DashboardA = this;

            var ValuesTask = UpdateValues(api);
            CardsClick();
            SideMenuClick(editor);
            LatestOrdersClick();
            BestCustomersClick();
        }

        private void BestCustomersClick()
        {
            var BestCustomers = FindViewById<Button>(Resource.Id.dashboardbestcustomers);
            BestCustomers.Click += BestCustomers_Click;
        }

        void BestCustomers_Click(object sender, EventArgs e)
        {
            dialog = ProgressDialog.Show(this, "", "Fetching Customers. Please wait... ", true);
            ShowBestCustomersPopup();
        }

        private async void ShowBestCustomersPopup()
        {
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;
            Customers = await api.GetBestCustomers();
            ListView CustomerList = new ListView(this);
            CustomerList.Adapter = new CustomersSearchAdapter(this, Customers);
            CustomerList.ItemClick += CustomerList_ItemClick;
            layout.AddView(CustomerList);
            var builder = new AlertDialog.Builder(this);
            builder.SetView(layout);
            builder.SetTitle("Best Customers");
            builder.SetNeutralButton("Dismiss", (s, e) => { builder.Create().Dismiss(); });
            builder.Create().Show();
            dialog.Dismiss();
        }

        void CustomerList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(CustomerDetails));
            intent.PutExtra("customeremail", Customers[e.Position].Email);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
        }

        private void LatestOrdersClick()
        {
            var LatestOrdersB = FindViewById<Button>(Resource.Id.dashboardlatestorders);
            LatestOrdersB.Click += LatestOrdersB_Click;
        }

        void LatestOrdersB_Click(object sender, EventArgs e)
        {
            dialog = ProgressDialog.Show(this, "", "Fetching Orders. Please wait... ", true);
            ShowLatestOrdersPopup();
        }

        private async void ShowLatestOrdersPopup(){
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;
            Orders = await api.GetLatestOrders(5);
            ListView OrdersList = new ListView(this);
            OrdersList.Adapter = new OrderSearchAdapter(this, Orders);
            OrdersList.ItemClick += OrdersList_ItemClick;
            layout.AddView(OrdersList);
            var builder = new AlertDialog.Builder(this);
            builder.SetView(layout);
            builder.SetTitle("Latest Orders");
            builder.SetNeutralButton("Dismiss", (s, e) => { builder.Create().Dismiss(); });
            builder.Create().Show();
            dialog.Dismiss();
        }

        void OrdersList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(OrderDetails));
            intent.PutExtra("orderid", Orders[e.Position].OrderID);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.search_actionbar, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (Resources.GetResourceEntryName(item.ItemId))
            {
                case "actionbar_refresh":
                    RefreshClick(api);
                    return true;
                case "home":
                if (!_slidingLayout.IsOpen)
                _slidingLayout.SmoothSlideOpen();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task UpdateValues(NopCore api)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var PendingV = FindViewById<TextView>(Resource.Id.pending);
            var CompleteV = FindViewById<TextView>(Resource.Id.complete);
            var CancelledV = FindViewById<TextView>(Resource.Id.cancelled);
            var PendingCountV = FindViewById<TextView>(Resource.Id.pendingorders);
            var CartsCountV = FindViewById<TextView>(Resource.Id.carts);
            var WishlistCountV = FindViewById<TextView>(Resource.Id.wishlists);
            var RegisteredV = FindViewById<TextView>(Resource.Id.registered);
            var OnlineV = FindViewById<TextView>(Resource.Id.online);
            var VendorsV = FindViewById<TextView>(Resource.Id.vendors);
            var BestsellerQV = FindViewById<TextView>(Resource.Id.bestsellersquantity);
            var BestsellerAV = FindViewById<TextView>(Resource.Id.bestsellersamount);
            var KeywordsLoadingV = FindViewById<TextView>(Resource.Id.keywords);
            var KeywordsLayoutHolder = FindViewById<LinearLayout>(Resource.Id.wordlayout);

            var Complete = await api.GetStats(2);
            var Pending = await api.GetStats(3);
            var Cancelled = await api.GetStats(1);
            var Currency = await api.GetCurrency();

            if (prefs.GetString("sales_format", "Integer").Equals("Integer"))
            {
                PendingV.Text = Pending.ToString("0") + " " + Currency;
                CompleteV.Text = Complete.ToString("0") + " " + Currency;
                CancelledV.Text = Cancelled.ToString("0") + " " + Currency;
            }
            else
            {
                PendingV.Text = Pending.ToString("0.0#") + " " + Currency;
                CompleteV.Text = Complete.ToString("0.0#") + " " + Currency;
                CancelledV.Text = Cancelled.ToString("0.0#") + " " + Currency;
            }



            var PendingCount = await api.GetPendingOrdersCount();
            var CartsCount = await api.GetCartsCount();
            var WishlistCount = await api.GetWishlistCount();

            PendingCountV.Text = PendingCount.ToString();
            CartsCountV.Text = CartsCount.ToString();
            WishlistCountV.Text = WishlistCount.ToString();

            var Registered = await api.GetRegisteredCustomersCount();
            var Online = await api.GetOnlineCount();
            var Vendors = await api.GetVendorsCount();

            RegisteredV.Text = Registered.ToString();
            OnlineV.Text = Online.ToString();
            VendorsV.Text = Vendors.ToString();

            var Keywords = await api.GetPopularKeywords(3);
            var BestsellerQ = await api.GetBestsellerByQuantity();
            var BestsellerA = await api.GetBestsellerByAmount();

            int wordSpace = DensityPixel(2);
            KeywordsLayoutHolder.RemoveAllViews();
            for (int i = 0; i < prefs.GetInt("keywords_dashboard", 3); i++)
            {
                LinearLayout WordHolder = new LinearLayout(this);
                LinearLayout.LayoutParams WordLayout = (LinearLayout.LayoutParams)KeywordsLayoutHolder.LayoutParameters;
                WordHolder.Orientation = Orientation.Horizontal;
                var WHParams = WordLayout;
                WHParams.SetMargins(0, 0, wordSpace, 0);
                WordHolder.LayoutParameters = WHParams;
                TextView word = new TextView(this);
                word.Text = Keywords[i].Keyword;
                word.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.keyword_holder_bg));
                word.SetTextColor(Resources.GetColor(Resource.Color.white));
                word.SetPadding(wordSpace, wordSpace, wordSpace, wordSpace);
                word.SetSingleLine();
                WordHolder.AddView(word);
                KeywordsLayoutHolder.AddView(WordHolder);
            }

            BestsellerAV.Text = BestsellerA[0].Product.Name;
            BestsellerQV.Text = BestsellerQ[0].Product.Name;

            var WeekCustomers = await api.GetCustomerCountByTime(7);
            var TwoWeeksCustomers = await api.GetCustomerCountByTime(14);
            var MonthCustomers = await api.GetCustomerCountByTime(30);
            var YearCustomers = await api.GetCustomerCountByTime(365);

            var registered = new int [4] { WeekCustomers,TwoWeeksCustomers,MonthCustomers,YearCustomers};

            RegisteredUsersGraph Graph = new RegisteredUsersGraph(registered,false);
            var plotView = FindViewById<PlotView>(Resource.Id.plotView_Temp);
            plotView.Model = Graph.MyModel;

            dialog.Dismiss();

        }

        private void InitializeDashboard()
        {
            var CustomersCard = FindViewById<LinearLayout>(Resource.Id.customerscard);
            var SalesCard = FindViewById<LinearLayout>(Resource.Id.salevaluescard);
            var OrdersCard = FindViewById<LinearLayout>(Resource.Id.orderscard);
            var StatsCard = FindViewById<LinearLayout>(Resource.Id.statisticscard);
            View vi = LayoutInflater.Inflate(Resource.Layout.sale_values_card, SalesCard, false);
            SalesCard.AddView(vi);
            vi = LayoutInflater.Inflate(Resource.Layout.customers_card, CustomersCard, false);
            CustomersCard.AddView(vi);
            vi = LayoutInflater.Inflate(Resource.Layout.orders_card, OrdersCard, false);
            OrdersCard.AddView(vi);
            vi = LayoutInflater.Inflate(Resource.Layout.stats_card, StatsCard, false);
            StatsCard.AddView(vi);
        }

        private async void InitializeSideMenu(ISharedPreferences prefs)
        {
            var Store = FindViewById<TextView>(Resource.Id.storename);
            var Name = FindViewById<TextView>(Resource.Id.welcomebackname);
            var CName = await api.GetFullName(prefs.GetString("current_user", "Error"));
            Name.Text = CName;
            Store.Text = await api.GetStoreName();
            var options = FindViewById<ListView>(Resource.Id.sidemenuoptions);
            
            options.Adapter = new SideMenuAdapter(this, Resources.GetStringArray(Resource.Array.side_menu_options), Resources.ObtainTypedArray(Resource.Array.options_icons));
            GetAssociatedStores(prefs);
        }

        private void RefreshClick(NopCore api)
        {
                dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
                var ValuesTask = UpdateValues(api);
        }

        private int DensityPixel(float InDP)
        {
            return Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, InDP, Resources.DisplayMetrics));
        }

        private void CardsClick()
        {
            var SearchOrdersB = FindViewById<LinearLayout>(Resource.Id.searchordersgo);
            var SearchCustomersB = FindViewById<LinearLayout>(Resource.Id.searchcustomersgo);
            var MiscStatsB = FindViewById<LinearLayout>(Resource.Id.miscstatsgo);
            var SaleValuesB = FindViewById<LinearLayout>(Resource.Id.salevaluesgo);
            SearchOrdersB.Click += delegate(object sender, EventArgs e)
            {
                Intent myIntent = new Intent(this, typeof(SearchOrders));
                StartActivity(myIntent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };
            SearchCustomersB.Click += delegate(object sender, EventArgs e)
            {
                Intent myIntent = new Intent(this, typeof(SearchCustomers));
                StartActivity(myIntent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };
            MiscStatsB.Click += delegate(object sender, EventArgs e)
            {
                Intent myIntent = new Intent(this, typeof(MiscStatistics));
                StartActivity(myIntent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };
            SaleValuesB.Click += delegate(object sender, EventArgs e)
            {
                Intent myIntent = new Intent(this, typeof(SaleValues));
                StartActivity(myIntent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };
        }

        private void SideMenuClick(ISharedPreferencesEditor editor)
        {
            var options = FindViewById<ListView>(Resource.Id.sidemenuoptions);
            var MyStores = FindViewById<ListView>(Resource.Id.mystoreslist);
            options.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                Intent myIntent = null;
                switch (args.Position)
                {
                    case 0:
                        myIntent = new Intent(this, typeof(Login));
                        myIntent.PutExtra("associate", true);
                        StartActivity(myIntent);
                        break;
                    case 1:
                        myIntent = new Intent(this, typeof(SearchCarts));
                        StartActivity(myIntent);
                        break;
                    case 2:
                        myIntent = new Intent(this, typeof(Configurations));
                        StartActivity(myIntent);
                        break;
                    case 3: // Logout
                        Logout(editor);
                        break;
                    case 4:
                        AndroidNet.Uri uri = AndroidNet.Uri.Parse("http://www.nopcommerce.com/documentation.aspx");
                        Intent intent = new Intent(Intent.ActionView).SetData(uri);
                        StartActivity(intent);
                        break;
                }
            };

            MyStores.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                var StoreName = urls[args.Position];
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle("Change to " + StoreName + " ?");
                builder.SetPositiveButton("Yes", (s, e) =>
                {
                    builder.Create().Dismiss();
                    LoginToAssociated(StoreName);
                });
                builder.SetNegativeButton("Cancel", (s, e) => { builder.Create().Dismiss(); });
                builder.Create().Show();

            };

        }

        private void Logout(ISharedPreferencesEditor editor)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Proceed with Logout?");
            builder.SetPositiveButton("Yes", (s, e) =>
            {
                Intent myIntent = new Intent(this, typeof(Login));
                myIntent.PutExtra("Associate", false);
                editor.PutBoolean("stay_connected", false);
                editor.PutBoolean("login_state", false);
                editor.PutString("current_url", "");
                editor.Apply();
                api.EndSession();
                StartActivity(myIntent);
                Finish();
            });
            builder.SetNegativeButton("Cancel", (s, e) => { builder.Create().Dismiss(); });
            builder.Create().Show();
        }

        private void GetAssociatedStores(ISharedPreferences prefs)
        {

            var keys = prefs.All;
            var myStores = FindViewById<ListView>(Resource.Id.mystoreslist);
            foreach (KeyValuePair<string, object> url in keys)
            {
                if (url.Key.StartsWith("store_url_"))
                {
                    var Name = url.Key.Substring(10);
                    UrlsMap.Add(new KeyValuePair<String, String>(Name, (string)url.Value));
                    urls.Add(Name);
                }
            }
            myStores.Adapter = new ArrayAdapter<String>(this, Resource.Layout.one_line_item_list, urls);
        }

        private static void hideSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }

        private void LoginToAssociated(string StoreName)
        {
            LinearLayout Layout = new LinearLayout(this);
            Layout.Orientation = Orientation.Vertical;
            EditText Username = new EditText(this);
            Username.Hint = "Username";
            EditText Password = new EditText(this);
            Password.Hint = "Password";
            Password.InputType = (InputTypes.ClassText | InputTypes.TextVariationPassword);
            TextView Error = new TextView(this);
            Error.Visibility = ViewStates.Gone;
            Layout.AddView(Username);
            Layout.AddView(Password);
            Layout.AddView(Error);

            var builder = new AlertDialog.Builder(this);
            
            builder.SetView(Layout);
            builder.SetTitle("Login to " + StoreName);
            builder.SetPositiveButton("Login", async (s, e) =>{

                    dialog = ProgressDialog.Show(this, "", "Connecting. Please wait... ", true);
                    var Url = UrlsMap[StoreName];
                    var DialogBuilder = builder.Create();
                    await LoginToExisting(Url, Username.Text.ToString(), Password.Text.ToString(),Error);
                    dialog.Dismiss();  
            });
            builder.SetNegativeButton("Cancel", (s, e) => { builder.Create().Dismiss(); });
            builder.Create().Show();
        }

        private async Task LoginToExisting(string url, string user, string pass, TextView Error)
        {
            try{
                api.SetStoreUrl(url);
                await api.CheckLogin(user, pass);
                Intent myIntent = new Intent(this, typeof(Dashboard));
                StartActivity(myIntent);
                Finish();
                dialog.Dismiss();
            }
                
            catch (LoginException ex)
                {
                    Console.WriteLine(ex.Message);
                    dialog.Dismiss();
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                    Error.Text = ex.Message;
                }
        }

        public override void OnBackPressed()
        {
            //you can do your other onBackPressed logic here..

            //Then just call finish()
            MoveTaskToBack(true);
        }
    }
}