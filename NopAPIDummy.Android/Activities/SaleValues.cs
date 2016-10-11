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
using NopMobile.AppCore.NopAPI;

namespace NopMobile.Android
{
    [Activity(Label = "Sale Values" , Theme = "@style/SaleValuesBar")]
    public class SaleValues : ActionBarActivity
    {
        private SeparatedListAdapter mAdapter;
        private NopCore api = new NopCore();
        private ProgressDialog dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.sale_values);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            var StatsList = FindViewById<ListView>(Resource.Id.sale_values_list);

            mAdapter = new SeparatedListAdapter(this);
            dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
            var Task = UpdateValues();

            StatsList.Adapter = mAdapter;
        }

        private async Task UpdateValues()
        {   
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            int Counter = 0;
            mAdapter.AddSectionHeaderItem("Incomplete Orders");

            var Unpaid = await api.GetPendingOrdersByReason("unpaid");
            var NotShipped = await api.GetPendingOrdersByReason("not shipped");
            var PendingCount = await api.GetPendingOrdersCount();
            var Currency = await api.GetCurrency();
            mAdapter.AddItem("Unpaid: " + Unpaid.ToString("0.0#") + " " + Currency);
            mAdapter.AddItem("Not Yet Shipped: " + NotShipped.ToString("0.0#") + " " + Currency);
            mAdapter.AddItem("Pending Count: " + PendingCount);

            mAdapter.AddSectionHeaderItem("Bestsellers (Quantity)");
            var BestQuantity = await api.GetBestsellerByQuantity();
            foreach (BestsellerDTO elem in BestQuantity){
                if (Counter != prefs.GetInt("bestsellers_quantity", 5) - 1)
                    mAdapter.AddItem(elem.Product.Name);
                else
                    break;
                Counter++;
            }

            mAdapter.AddSectionHeaderItem("Bestsellers (Amount)");
            Counter = 0;
            var BestAmount = await api.GetBestsellerByAmount();
            foreach (BestsellerDTO elem in BestAmount)
            {
                if (Counter != prefs.GetInt("bestsellers_amount", 5) - 1)
                    mAdapter.AddItem(elem.Product.Name);
                else
                    break;
                Counter++;
            }

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