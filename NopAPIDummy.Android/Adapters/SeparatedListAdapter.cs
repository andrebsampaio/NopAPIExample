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

namespace NopMobile.Android.Adapters
{
    [Activity()]
    public class SeparatedListAdapter : BaseAdapter
    {
        private const int TYPE_ITEM = 0;
        private const int TYPE_SEPARATOR = 1;

        private IList<String> MData = new List<String>();
        private SortedSet<int> SectionHeader = new SortedSet<int>();

        private LayoutInflater mInflater;

        public SeparatedListAdapter(Context context)
        {
            mInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        public void AddItem(String item)
        {
            MData.Add(item);
            NotifyDataSetChanged();
        }

        public void AddSectionHeaderItem(String item)
        {
            MData.Add(item);
            SectionHeader.Add(MData.Count - 1);
            NotifyDataSetChanged();
        }

        public override int GetItemViewType(int position)
        {
            return SectionHeader.Contains(position) ? TYPE_SEPARATOR : TYPE_ITEM;
        }

        public override int ViewTypeCount
        {
            get
            {
                return 2;
            }
        }

        public override int Count
        {
            get { return MData.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return MData[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder = null;
            int rowType = GetItemViewType(position);

            if (convertView == null)
            {
                holder = new ViewHolder();
                switch (rowType)
                {
                    case TYPE_ITEM:
                        convertView = mInflater.Inflate(Resource.Layout.stats_list_item, null);
                        holder.textView = (TextView)convertView.FindViewById(Resource.Id.list_item_title);
                        break;
                    case TYPE_SEPARATOR:
                        convertView = mInflater.Inflate(Resource.Layout.stats_list_header, null);
                        holder.textView = (TextView)convertView.FindViewById(Resource.Id.list_header_title);
                        break;
                }

                convertView.Tag = holder;
            }
            else
            {
                holder = convertView.Tag as ViewHolder;
            }
            holder.textView.Text = MData[position];

            return convertView;
        }

        public class ViewHolder : Java.Lang.Object
        {
            public TextView textView { get; set; }
        }

    }
}