using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;
using Android.Content.PM;
using NopMobile.AppCore.NopAPI;
using Android.Support.V7.App;
using Android.Support.V4.App;
using System.Threading.Tasks;
using Android.Text;

namespace NopMobile.Android
{
    [Activity(Label = "Search Orders", LaunchMode = LaunchMode.SingleTop, Theme = "@style/OrderSearchBar")]
    public class SearchOrders : ActionBarActivity
    {
        String searchFilter = "Email";
        OrderDTO[] OrdersList;
        NopCore api = new NopCore();
        ProgressDialog dialog;
        String LastSearch = "";
        String StatusFilter;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.search_orders);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            Spinner Filter = FindViewById<Spinner>(Resource.Id.searchfilterorder);
            ListView FetchedOrders = FindViewById<ListView>(Resource.Id.searchlistorder);
            EditText SearchParams = FindViewById<EditText>(Resource.Id.searchtextorder);
            Spinner StatusSpinner = FindViewById<Spinner>(Resource.Id.searchstatusorder);

            string[] FilterOpts = { "Email", "Shipping Status", "Order Status", "Pay Status", "Store ID", "Vendor ID", "Customer ID", "Product ID", "Affiliate ID" };
            string[] OrderStatus = { "Complete", "Pending", "Cancelled", "Processing" };
            string[] ShippingStatus = { "Delivered", "Not Yet Shipped", "Partially Shipped", "Shipped", "Shipping Not Required" };
            string[] PayStatus = { "Paid", "Authorized", "Partially Refunded", "Refunded", "Voided" };

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, FilterOpts);

            Filter.Adapter = adapter;

            FetchedOrders.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                Intent intent = new Intent(this, typeof(OrderDetails));
                intent.PutExtra("orderid", OrdersList[e.Position].OrderID);
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };

            Filter.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {
                searchFilter = FilterOpts[e.Position];
                switch(searchFilter){
                    case "Shipping Status":
                        SearchParams.Visibility = ViewStates.Gone;
                        StatusSpinner.Visibility = ViewStates.Visible;
                        StatusSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, ShippingStatus);
                        break;
                    case "Order Status":
                        SearchParams.Visibility = ViewStates.Gone;
                        StatusSpinner.Visibility = ViewStates.Visible;
                        StatusSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, OrderStatus);
                        break;
                    case "Pay Status":
                        SearchParams.Visibility = ViewStates.Gone;
                        StatusSpinner.Visibility = ViewStates.Visible;
                        StatusSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, PayStatus);
                        break;
                    case "Stored ID":
                    case "Vendor ID":
                    case "Customer ID":
                    case "Product ID":
                    case "Affiliate ID":
                        SearchParams.Visibility = ViewStates.Visible;
                        StatusSpinner.Visibility = ViewStates.Gone;
                        SearchParams.InputType = InputTypes.ClassNumber;
                        break;
                    default:
                        SearchParams.Visibility = ViewStates.Visible;
                        StatusSpinner.Visibility = ViewStates.Gone;
                        SearchParams.InputType = InputTypes.ClassText;
                        break;
                }
            };

            StatusSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {

                switch (searchFilter)
                {
                    case "Shipping Status":
                        StatusFilter = ShippingStatus[e.Position];
                        break;
                    case "Order Status":
                        StatusFilter = OrderStatus[e.Position];
                        break;
                    case "Pay Status":
                        StatusFilter = PayStatus[e.Position];
                        break;
                }
                dialog = ProgressDialog.Show(this, "", "Searching. Please wait... ", true);
                var TaskList = SelectedFilter(searchFilter, LastSearch);
            };

            SearchParams.EditorAction += HandleEditorAction;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.search_actionbar, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (Resources.GetResourceEntryName(item.ItemId))
            {
                case "actionbar_refresh":
                    if (!LastSearch.Equals(""))
                    {
                        dialog = ProgressDialog.Show(this, "", "Searching. Please wait... ", true);
                        SelectedFilter(searchFilter, LastSearch);
                    }
                    return true;
                case "home":
                    Finish();
                    OverridePendingTransition(Resource.Animation.activity_open_scale, Resource.Animation.activity_close_translate);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            EditText SearchParams = FindViewById<EditText>(Resource.Id.searchtextorder);
            e.Handled = false;
            if (e.ActionId == ImeAction.Search)
            {
                
                LastSearch = SearchParams.Text.ToString();
                dialog = ProgressDialog.Show(this, "", "Searching. Please wait... ", true);
                var TaskList = SelectedFilter(searchFilter, LastSearch);
                hideSoftKeyboard(this, SearchParams);
                e.Handled = true;
            }
        }

        private static void hideSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            
            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }

        private async Task SelectedFilter(string Filter, string Search)
        {
            ListView FetchedOrders = FindViewById<ListView>(Resource.Id.searchlistorder);
            TextView ResultCount = FindViewById<TextView>(Resource.Id.searchresultsorder);
            switch (Filter)
            {
                case "Email":
                    OrdersList = await api.UserOrders(Search);
                    break;
                case "Shipping Status":
                    OrdersList = await api.ShippingStatusOrders(StatusFilter);
                    break;
                case "Order Status":
                    OrdersList = await api.OrderStatusOrders(StatusFilter);
                    break;
                case "Pay Status":
                    OrdersList = await api.PayStatusOrders(StatusFilter);
                    break;
                case "Store ID":
                    OrdersList = await api.StoreIdOrders(Int32.Parse(Search));
                    break;
                case "Vendor ID":
                    OrdersList = await api.VendorIdOrders(Int32.Parse(Search));
                    break;
                case "Customer ID":
                    OrdersList = await api.CustomerIdOrders(Int32.Parse(Search));
                    break;
                case "Product ID":
                    OrdersList = await api.ProductIdOrders(Int32.Parse(Search));
                    break;
                case "Affiliate ID":
                    OrdersList = await api.AffiliateIdOrders(Int32.Parse(Search));
                    break;
            }
            FetchedOrders.Adapter = new OrderSearchAdapter(this, OrdersList);
            ResultCount.Text = OrdersList.Length.ToString() + " Results Found";
            ResultCount.Visibility = ViewStates.Visible;
            dialog.Dismiss();
        }


        public override void OnBackPressed()
        {
            //you can do your other onBackPressed logic here..

            //Then just call finish()
            Finish();
            OverridePendingTransition(Resource.Animation.activity_open_scale, Resource.Animation.activity_close_translate);
        }
        
    }
}