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

namespace NopMobile.Android
{
    [Activity()]
    public class OrdersAdapter : BaseAdapter<string>
    {
        string [] orderEmail;
        string[] orderID;
        string[] products;
        string[] dates;
        Activity context;
        public OrdersAdapter(Activity context, string [] orderEmail, string[] orderID,string [] dates, string [] products)
            : base()
        {
            this.context = context;
            this.orderEmail = orderEmail;
            this.orderID = orderID;
            this.products = products;
            this.dates = dates;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return orderID[position]; }
        }
        public override int Count
        {
            get { return orderID.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var id = orderID[position];
            var email = orderEmail[position];
            var product = products[position];
            var date = dates[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.OrderRow, null);
            view.FindViewById<TextView>(Resource.Id.orderrowid).Text = id;
            view.FindViewById<TextView>(Resource.Id.product).Text = product;
            view.FindViewById<TextView>(Resource.Id.date).Text = date;
            
            return view;
        }
    }
}