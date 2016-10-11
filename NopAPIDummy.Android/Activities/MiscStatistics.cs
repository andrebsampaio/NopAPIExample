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
using NopMobile.Android.Adapters;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Support.V7.App;
using Android.Support.V4.App;
using Android.Preferences;
using NopMobile.AppCore;
using OxyPlot.XamarinAndroid;

namespace NopMobile.Android
{
    [Activity(Label = "Misc Statistics", Theme = "@style/MiscStatsBar")]
    public class MiscStatistics : ActionBarActivity
    {
        private SeparatedListAdapter mAdapter;
        private NopCore api = new NopCore();
        private ProgressDialog dialog;
        private ListView StatsList;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.misc_stats);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            StatsList = FindViewById<ListView>(Resource.Id.misc_stats_list);

            mAdapter = new SeparatedListAdapter(this);
            dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
            var Task = UpdateValues();

            StatsList.Adapter = mAdapter;
        }

        private async Task UpdateValues()
        {
            mAdapter = new SeparatedListAdapter(this);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            mAdapter.AddSectionHeaderItem("Popular Keywords");
            var Keywords = await api.GetPopularKeywords(prefs.GetInt("keywords_stats", 5));
            for (int i = 0; i < prefs.GetInt("keywords_stats", 5); i++)
                mAdapter.AddItem(Keywords[i].Keyword);

            mAdapter.AddSectionHeaderItem("Registered Users");

            var WeekCustomers = await api.GetCustomerCountByTime(7);
            var TwoWeeksCustomers = await api.GetCustomerCountByTime(14);
            var MonthCustomers = await api.GetCustomerCountByTime(30);
            var YearCustomers = await api.GetCustomerCountByTime(365);

            mAdapter.AddItem("Last 7 Days: " + WeekCustomers );
            mAdapter.AddItem("Last 14 Days: " + TwoWeeksCustomers);
            mAdapter.AddItem("Last Month: " + MonthCustomers);
            mAdapter.AddItem("Last Year: " + YearCustomers);

            mAdapter.AddSectionHeaderItem("Total Sales");
            var WeekSales = await api.GetTotalSalesByTime(7);
            var TwoWeeksSales = await api.GetTotalSalesByTime(14);
            var MonthSales = await api.GetTotalSalesByTime(30);
            var YearSales = await api.GetTotalSalesByTime(365);
            var Currency = await api.GetCurrency();
            mAdapter.AddItem("Last 7 Days: " + WeekSales.ToString("0.0#") + " " + Currency );
            mAdapter.AddItem("Last 14 Days: " + TwoWeeksSales.ToString("0.0#") + " " + Currency);
            mAdapter.AddItem("Last Month: " + MonthSales.ToString("0.0#") + " " + Currency);
            mAdapter.AddItem("Last Year: " + YearSales.ToString("0.0#") + " " + Currency);

            StatsList.Adapter = mAdapter;
            dialog.Dismiss();
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
                    dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
                    var Task = UpdateValues();
                    return true;
                case "home":
                    Finish();
                    OverridePendingTransition(Resource.Animation.activity_open_scale, Resource.Animation.activity_close_translate);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
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