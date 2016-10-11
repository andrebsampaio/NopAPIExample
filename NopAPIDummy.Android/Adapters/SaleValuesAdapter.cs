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
    public class SaleValuesAdapter : BaseAdapter<string>
    {
        string [] Complete;
        string[] Pending;
        string[] Cancelled;
        Activity context;
        public SaleValuesAdapter(Activity context, string [] Complete, string[] Pending, string [] Cancelled)
            : base()
        {
            this.context = context;
            this.Complete = Complete;
            this.Pending = Pending;
            this.Cancelled = Cancelled;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Pending[position]; }
        }
        public override int Count
        {
            get { return Pending.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var pending = Pending[position];
            var complete = Complete[position];
            var cancelled = Cancelled[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.sale_values_card, null);
                view.FindViewById<TextView>(Resource.Id.pending).Text = pending;
                view.FindViewById<TextView>(Resource.Id.complete).Text = complete;
                view.FindViewById<TextView>(Resource.Id.cancelled).Text = cancelled;
            
            
            return view;
        }

    }
}