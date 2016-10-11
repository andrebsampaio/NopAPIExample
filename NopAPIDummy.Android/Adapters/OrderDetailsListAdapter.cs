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

namespace NopMobile.Android
{


    [Activity()]
    public class OrderDetailsListAdapter : BaseAdapter<string>
    {
        OrderItemDTO[] Orders;
        Activity context;
        string Currency;
        public OrderDetailsListAdapter(Activity context, OrderItemDTO[] Order, string Currency)
            : base()
        {
            this.context = context;
            this.Orders = Order;
            this.Currency = Currency;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Orders[position].OrderId.ToString(); }
        }
        public override int Count
        {
            get { return Orders.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Quantity = Orders[position].Quantity;
            var Product = Orders[position].Product.Name;
            var Total = Quantity * Orders[position].Product.Price;

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