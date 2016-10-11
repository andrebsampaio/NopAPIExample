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
using Android.Content.PM;
using System.Threading.Tasks;
using NopMobile.AppCore.NopAPI;
using Android.Support.V7.App;

namespace NopMobile.Android
{
    [Activity(Label = "Cart Products", LaunchMode = LaunchMode.SingleTop, Theme = "@style/OrderSearchBar")]
    public class CartProductsList : ActionBarActivity
    {
        NopCore api = new NopCore();
        CustomerDTO Cart;
        ProgressDialog dialog;
        bool Wishlist = false;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.cart_products_list);
            if (Wishlist = Intent.GetBooleanExtra("wishlist", false))
            {
                SupportActionBar.Title = "Wishlist";
            }
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
            var Task = GetCart();

        }

        private async Task GetCart()
        {
            var CustomerEmail = Intent.GetStringExtra("customeremail");
            var CartTmp = await api.GetCustomersByEmail(CustomerEmail);
            Cart = CartTmp.First();
            var ProductsList = FindViewById<ListView>(Resource.Id.cartproductslist);

            if (Wishlist)
            {
                ProductsList.Adapter = new CartProductsAdapter(this, Cart.Wishlist.ToArray(), await api.GetCurrency());
            }
            else
            {
                ProductsList.Adapter = new CartProductsAdapter(this, Cart.ShoppingCart.ToArray(), await api.GetCurrency());
            }

            dialog.Dismiss();
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
                    dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
                    GetCart();
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