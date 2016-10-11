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
using Android.Support.V7.App;
using Android.Support.V4.App;
using NopMobile.AppCore.NopAPI;
using Android.Support.V4.View;
using Android.Views.InputMethods;
using Android.Content.PM;
using Android.Text;
using System.Threading.Tasks;

namespace NopMobile.Android
{

    [Activity(Label = "Search Customers", LaunchMode = LaunchMode.SingleTop, Theme = "@style/CustomerSearchBar")]
    public class SearchCustomers : ActionBarActivity
    {
        private String Filter = "Email";
        private String LastSearch = "";
        private NopCore api = new NopCore();
        private ProgressDialog dialog;
        private CustomerDTO[] CustomersList;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.search_customers);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

           SupportActionBar.SetDisplayShowHomeEnabled(false);

            var SearchText = FindViewById<EditText>(Resource.Id.searchtextcustomer);
            var SearchFilter = FindViewById<Spinner>(Resource.Id.searchfiltercustomer);

            string[] FilterOpts = { "Email", "Username", "Firstname", "Lastname", "Fullname", "Company", "Phone", "Postal Code" };

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, FilterOpts);

            SearchFilter.Adapter = adapter;

            SearchFilter.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {
                Filter = FilterOpts[e.Position];
                switch (Filter)
                {
                    case "Email":
                    case "Username":
                    case "Firstname":
                    case "Lastname":
                    case "Fullname":
                    case "Company":
                        SearchText.InputType = InputTypes.ClassText;
                        break;
                    case "Phone":
                        SearchText.InputType = InputTypes.ClassPhone;
                        break;
                    case "Postal Code":
                        SearchText.InputType = InputTypes.ClassNumber;
                        break;
                }
            };

            SearchText.EditorAction += HandleEditorAction;

            var SearchResultsList = FindViewById<ListView>(Resource.Id.searchlistcustomer);

            SearchResultsList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
                Intent intent = new Intent(this, typeof(CustomerDetails));
                intent.PutExtra("customeremail", CustomersList[e.Position].Email);
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };


        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {            
            MenuInflater.Inflate(Resource.Menu.search_actionbar, menu);

            var refresh = menu.FindItem(Resource.Id.action_refresh);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (Resources.GetResourceEntryName(item.ItemId))
            {
                case "actionbar_refresh":
                    if (!LastSearch.Equals("")) SelectedFilter(Filter, LastSearch);
                    return true;
                case "home":
                    Finish();
                    OverridePendingTransition(Resource.Animation.activity_open_scale, Resource.Animation.activity_close_translate);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private async Task SelectedFilter(string Filter,string Search)
        {
            var SearchResultsList = FindViewById<ListView>(Resource.Id.searchlistcustomer);
            var SearchResultsCount = FindViewById<TextView>(Resource.Id.searchresultscustomer);
            switch(Filter){
                case "Email":
                    CustomersList = await api.GetCustomersByEmail(Search);
                    break;
                case "Username":
                    CustomersList = await api.GetCustomersByUsername(Search);
                    break;
                case "Firstname":
                    CustomersList = await api.GetCustomersByFirstname(Search);
                    break;
                case "Lastname":
                    CustomersList = await api.GetCustomersByLastname(Search);
                    break;
                case "Fullname":
                    CustomersList = await api.GetCustomersByFullname(Search);
                    break;
                case "Company":
                    CustomersList = await api.GetCustomersByCompany(Search);
                    break;
                case "Phone":
                    CustomersList = await api.GetCustomersByPhone(Search);
                    break;
                case "Postal Code":
                    CustomersList = await api.GetCustomersByPostalCode(Search);
                    break;
            }
            SearchResultsList.Adapter = new CustomersSearchAdapter(this, CustomersList);
            SearchResultsCount.Text = CustomersList.Length.ToString() + " Results Found";
            SearchResultsCount.Visibility = ViewStates.Visible;
            dialog.Dismiss();
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            var SearchText = FindViewById<EditText>(Resource.Id.searchtextcustomer);

            e.Handled = false;
            if (e.ActionId == ImeAction.Search)
            {
                dialog = ProgressDialog.Show(this, "", "Searching. Please wait... ", true);
                LastSearch = SearchText.Text.ToString();
                var TaskList = SelectedFilter(Filter, LastSearch);
                hideSoftKeyboard(this, SearchText);
                e.Handled = true;
            }
        }

        private static void hideSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            imm.HideSoftInputFromWindow(view.WindowToken, 0);
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