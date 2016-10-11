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
    public class CustomersAdapter : BaseAdapter<string>
    {
        string [] Online;
        string[] Registered;
        Activity context;
        public CustomersAdapter(Activity context, string [] Online, string[] Registered)
            : base()
        {
            this.context = context;
            this.Online = Online;
            this.Registered = Registered;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Registered[position]; }
        }
        public override int Count
        {
            get { return Registered.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var online = Registered[position];
            var registered = Online[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.customers_card, null);
                view.FindViewById<TextView>(Resource.Id.online).Text = online;
                view.FindViewById<TextView>(Resource.Id.registered).Text = registered;
            
            
            return view;
        }

    }
}