using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Content.Res;

namespace NopMobile.Android
{


    [Activity()]
    public class SideMenuAdapter : BaseAdapter<string>
    {
        string [] Options;
        TypedArray Icons; 
        Activity context;
        public SideMenuAdapter(Activity context, string [] options, TypedArray icons) : base()
        {
            this.context = context;
            this.Options = options;
            this.Icons = icons;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Options[position]; }
        }
        public override int Count
        {
            get { return Options.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.side_menu_options_item, null);
                view.FindViewById<ImageView>(Resource.Id.item_icon).SetImageResource(Icons.GetResourceId(position, -1));
                view.FindViewById<TextView>(Resource.Id.item_title).Text = Options[position];
              
            return view;
        }

    }
}