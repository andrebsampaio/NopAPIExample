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
    public class CustomersSearchAdapter : BaseAdapter<string>
    {
        CustomerDTO[] Customers;      
        Activity context;
        public CustomersSearchAdapter(Activity context, CustomerDTO [] Customers) : base()
        {
            this.context = context;
            this.Customers = Customers;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Customers[position].Email; }
        }
        public override int Count
        {
            get { return Customers.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Name = Customers[position].FullName;
            var Email = Customers[position].Email;
            var Username = Customers[position].Username;
            var Id = Customers[position].Id;

            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.customers_search_result, null);
                view.FindViewById<TextView>(Resource.Id.customername).Text = Name;
                view.FindViewById<TextView>(Resource.Id.customerid).Text = Id.ToString();
                view.FindViewById<TextView>(Resource.Id.customeremail).Text = Email;

            return view;
        }

    }
}