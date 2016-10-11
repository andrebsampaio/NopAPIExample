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
using Android.Content.Res;

namespace NopMobile.Android
{


    [Activity()]
    public class OrderSearchAdapter : BaseAdapter<string>
    {
        OrderDTO[] Orders;
        Activity context;
        public OrderSearchAdapter(Activity context, OrderDTO[] Customers)
            : base()
        {
            this.context = context;
            this.Orders = Customers;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Orders[position].OrderID.ToString(); }
        }
        public override int Count
        {
            get { return Orders.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Id = Orders[position].OrderID;
            var Products = Orders[position].ProductsList;
            var Email = Orders[position].OrderEmail;
            var Status = Orders[position].OrderStatus;
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.orders_search_result, null);
            for (int i = 0; i < Products.Count; i++)
            {
                if (i == Products.Count - 1)
                {
                    view.FindViewById<TextView>(Resource.Id.orderproducts).Text += Products[i].Product.Name;
                }
                else
                {
                    view.FindViewById<TextView>(Resource.Id.orderproducts).Text += Products[i].Product.Name + ",";
                }

            }
            var Indicator = view.FindViewById<LinearLayout>(Resource.Id.orderstatusindicator);
            switch (Status)
            {
                case OrderStatus.Complete:
                    Indicator.SetBackgroundColor(context.Resources.GetColor(Resource.Color.green));
                    break;
                case OrderStatus.Pending:
                    Indicator.SetBackgroundColor(context.Resources.GetColor(Resource.Color.lightgray));
                    break;
                case OrderStatus.Processing:
                    Indicator.SetBackgroundColor(context.Resources.GetColor(Resource.Color.orange));
                    break;
                case OrderStatus.Cancelled:
                    Indicator.SetBackgroundColor(context.Resources.GetColor(Resource.Color.red));
                    break;
            }
            view.FindViewById<TextView>(Resource.Id.orderid).Text = Id.ToString();
            view.FindViewById<TextView>(Resource.Id.orderemail).Text = Email;

            return view;
        }

    }
}