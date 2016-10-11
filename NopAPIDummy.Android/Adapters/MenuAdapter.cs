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
    public class MenuAdapter : BaseAdapter<string>
    {
        string[] MenuItems;
        Activity context;
        public MenuAdapter(Activity context, string[] MenuItems)
            : base()
        {
            this.context = context;
            this.MenuItems = MenuItems;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return MenuItems[position]; }
        }
        public override int Count
        {
            get { return MenuItems.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = MenuItems[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.menu_item_row, null);
            view.FindViewById<TextView>(Resource.Id.menurowitem).Text = item;

            return view;
        }
    }
}