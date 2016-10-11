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
    [Activity(Label = "Current Shopping Carts", LaunchMode = LaunchMode.SingleTop, Theme = "@style/CartsBar")]
    public class SearchCarts : ActionBarActivity
    {
        String searchFilter = "Email";
        CustomerDTO[] CartsList;
        NopCore api = new NopCore();
        ProgressDialog dialog;
        String LastSearch = "";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.search_carts);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            Spinner Filter = FindViewById<Spinner>(Resource.Id.searchfiltercart);
            var FilterOriginal = Filter.LayoutParameters;
            ListView FetchedCarts = FindViewById<ListView>(Resource.Id.searchlistcart);
            EditText SearchParams = FindViewById<EditText>(Resource.Id.searchtextcart);

            string[] FilterOpts = { "Email", "> Items", "< Items", "> Total", "< Total", "Abandoned", "Active" };

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.simple_spinner_dropdown_item, FilterOpts);

            Filter.Adapter = adapter;

            FetchedCarts.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {

                Intent intent = new Intent(this, typeof(CartProductsList));
                intent.PutExtra("customeremail", CartsList[e.Position].Email);
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
            };

            Filter.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {
                searchFilter = FilterOpts[e.Position];
                switch(searchFilter){
                    case "Abandoned":
                    case "Active":
                        dialog = ProgressDialog.Show(this, "", "Searching. Please wait... ", true);
                        SearchParams.Visibility = ViewStates.Gone;
                        SelectedFilter(searchFilter, LastSearch);
                        Filter.LayoutParameters = new TableLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                        break;
                    case "> Items":
                    case "< Items":
                    case "> Total":
                    case "< Total":
                        Filter.LayoutParameters = FilterOriginal;
                        SearchParams.Text = "";
                        SearchParams.Visibility = ViewStates.Visible;
                        SearchParams.InputType = InputTypes.ClassNumber;
                        break;
                    default:
                        Filter.LayoutParameters = FilterOriginal;
                        SearchParams.Visibility = ViewStates.Visible;
                        SearchParams.InputType = InputTypes.ClassText;
                        break;
                }
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
                    hideSoftKeyboard(this, FindViewById<EditText>(Resource.Id.searchtextcart));
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            EditText SearchParams = FindViewById<EditText>(Resource.Id.searchtextcart);
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
            ListView FetchedCarts = FindViewById<ListView>(Resource.Id.searchlistcart);
            TextView ResultCount = FindViewById<TextView>(Resource.Id.searchresultscart);
            switch (Filter)
            {
                case "Email":
                    CartsList = await api.GetCurrentCarts("email",Search,0,0,0,0,false);
                    break;
                case "> Items":
                    CartsList = await api.GetCurrentCarts("higher items",null,0,Int32.Parse(Search),0,0,false);
                    break;
                case "< Items":
                    CartsList = await api.GetCurrentCarts("lower items", null, Int32.Parse(Search), 0, 0, 0, false);
                    break;
                case "> Total":
                    CartsList = await api.GetCurrentCarts("higher total", null, 0, 0, 0, Int32.Parse(Search), false);
                    break;
                case "< Total":
                    CartsList = await api.GetCurrentCarts("lower total", null, 0, 0, Int32.Parse(Search), 0, false);
                    break;
                case "Abandoned":
                    CartsList = await api.GetCurrentCarts("abandoned", null, 0, 0, 0, 0, true);
                    break;
                case "Active":
                    CartsList = await api.GetCurrentCarts("active", null, 0, 0, 0, 0, false);
                    break;
            }
            FetchedCarts.Adapter = new CartsSearchAdapter(this, CartsList,await api.GetCurrency());
            ResultCount.Text = CartsList.Length.ToString() + " Results Found";
            ResultCount.Visibility = ViewStates.Visible;
            dialog.Dismiss();
        }



        
    }
}