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
using NopMobile.AppCore.NopAPI;
using System.Threading.Tasks;

namespace NopMobile.Android
{


    [Activity()]
    public class CartProductsAdapter : BaseAdapter<string>
    {
        CartItemDTO[] CartItems;
        Activity context;
        string Currency;
        public CartProductsAdapter(Activity context, CartItemDTO[] Cart, string Currency)
            : base()
        {
            this.context = context;
            this.CartItems = Cart;
            this.Currency = Currency;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return CartItems[position].Product.Id.ToString(); }
        }
        public override int Count
        {
            get { return CartItems.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Quantity = CartItems[position].Quantity;
            var Product = CartItems[position].Product.Name;
            var Total = Quantity * CartItems[position].Product.Price;

            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.order_product_item, null);
            view.FindViewById<TextView>(Resource.Id.orderdetailsproduct).Text = Product;
            view.FindViewById<TextView>(Resource.Id.orderdetailstotal).Text = Total.ToString("0.0#") + " " + Currency;
            view.FindViewById<TextView>(Resource.Id.orderdetailsquantity).Text = Quantity.ToString();

            return view;
        }

    }
}