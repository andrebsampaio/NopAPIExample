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
    public class CartsSearchAdapter : BaseAdapter<string>
    {
        CustomerDTO[] Carts;      
        Activity context;
        string Currency;
        public CartsSearchAdapter(Activity context, CustomerDTO [] Carts, string Currency) : base()
        {
            this.context = context;
            this.Carts = Carts;
            this.Currency = Currency;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return Carts[position].Email; }
        }
        public override int Count
        {
            get { return Carts.Length; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Prods = Carts[position].ShoppingCart;
            var Email = Carts[position].Email;
            decimal Value = 0; var Quantity = 0;

            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.cart_list_item, null);
                view.FindViewById<TextView>(Resource.Id.cartemail).Text = Email;
                view.FindViewById<TextView>(Resource.Id.cartproducts).Text = "";
                foreach (CartItemDTO c in Prods)
                {
                    if (c.Equals(Prods.Last()))
                    {
                        view.FindViewById<TextView>(Resource.Id.cartproducts).Text = c.Product.Name;
                    }
                    else
                    {
                        view.FindViewById<TextView>(Resource.Id.cartproducts).Text = c.Product.Name + ", ";
                    }

                    Quantity += c.Quantity;
                    Value += c.Product.Price;
                }

                view.FindViewById<TextView>(Resource.Id.cartvalue).Text = Value.ToString("0.0#") + " " + Currency;
                view.FindViewById<TextView>(Resource.Id.cartquantity).Text = Quantity.ToString();
              
            return view;
        }
    }
}