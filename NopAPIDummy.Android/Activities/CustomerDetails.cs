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
using Android.Content.PM;
using System.Threading.Tasks;
using NopMobile.AppCore.NopAPI;
using Android.Support.V7.App;
using Android.Util;

namespace NopMobile.Android
{
    [Activity(Label = "Customer Details", LaunchMode = LaunchMode.SingleTop, Theme = "@style/CustomerSearchBar")]
    public class CustomerDetails : ActionBarActivity
    {
        CustomerDTO Customer;
        NopCore api = new NopCore();
        ProgressDialog dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.customer_details);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            dialog = ProgressDialog.Show(this, "", "Loading. Please wait... ", true);
            InitalizeCustomer();

            var AdminComments = FindViewById<LinearLayout>(Resource.Id.customerdetailscomments);
            AdminComments.Click += delegate
            {
                AdminCommentsPopup();
            };
            var Cart = FindViewById<LinearLayout>(Resource.Id.customerdetailscart);
            Cart.Click += delegate
            {
                if (Customer.ShoppingCart.Count == 0)
                {
                    Toast.MakeText(this, Customer.FullName + " Cart is empty", ToastLength.Short).Show();
                }
                else
                {
                    Intent myIntent = new Intent(this, typeof(CartProductsList));
                    myIntent.PutExtra("customeremail", Customer.Email);
                    myIntent.PutExtra("wishlist", false);
                    StartActivity(myIntent);
                    OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
                }
                
            };
            var Wishlist = FindViewById<LinearLayout>(Resource.Id.customerdetailswishlist);
            Wishlist.Click += delegate
            {
                if (Customer.ShoppingCart.Count == 0)
                {
                    Toast.MakeText(this, Customer.FullName + " Wishlist is empty", ToastLength.Short).Show();
                }
                else
                {
                    Intent myIntent = new Intent(this, typeof(CartProductsList));
                    myIntent.PutExtra("customeremail", Customer.Email);
                    myIntent.PutExtra("wishlist", true);
                    StartActivity(myIntent);
                    OverridePendingTransition(Resource.Animation.activity_open_translate, Resource.Animation.activity_close_scale);
                }
                
            };
        }

        private async Task RefreshCustomer()
        {
            var GetCustomer = await api.GetCustomersByEmail(Intent.GetStringExtra("customeremail"));
            Customer = GetCustomer.First();
        }

        private async Task InitalizeCustomer()
        {
            await RefreshCustomer();
            var NameV = FindViewById<TextView>(Resource.Id.customerdetailsname);
            var StatusV = FindViewById<TextView>(Resource.Id.customerdetailsstatus);
            var EmailV = FindViewById<TextView>(Resource.Id.customerdetailsemail);
            var BirthdayV = FindViewById<TextView>(Resource.Id.customerdetailsbirthday);
            var RolesV = FindViewById<TextView>(Resource.Id.customerdetailsroles);
            var ActivityV = FindViewById<TextView>(Resource.Id.customerdetailsactivity);
            var GenderV = FindViewById<TextView>(Resource.Id.customerdetailsgender);
            var CompanyV = FindViewById<TextView>(Resource.Id.customerdetailscompany);
            var AddressesV = FindViewById<LinearLayout>(Resource.Id.customerdetailsadressesholder);

            NameV.Text = Customer.FullName ?? "Not available";
            EmailV.Text = Customer.Email;
            if (Customer.Active)
            {
                StatusV.Text = "Active";
                StatusV.SetTextColor(Resources.GetColor(Resource.Color.green));
            }
            else
            {
                StatusV.Text = "Inactive";
                StatusV.SetTextColor(Resources.GetColor(Resource.Color.red));
            }
            
            BirthdayV.Text = Customer.Birthday ?? "Not available";
            if (Customer.IsAdmin)
            {
                RolesV.Text = "Admin";
            } 
            else {
                RolesV.Text = "Customer";
            }
            
            ActivityV.Text = Customer.Activity.ToString();
            GenderV.Text = Customer.Gender ?? "Not available";
            CompanyV.Text = Customer.Company ?? "Not available";

            var tenDp = ConvertDpToPixels(10);

            if (Customer.Addresses.Count == 0)
            {
                LinearLayout holder = new LinearLayout(this);
                holder.SetPadding(tenDp, tenDp, tenDp, tenDp);
                holder.Orientation = Orientation.Vertical;
                TextView addressV = new TextView(this);
                addressV.Text = "No Addresses Available";
                addressV.SetTextSize(ComplexUnitType.Sp, 16);
                holder.AddView(addressV);
                AddressesV.AddView(holder);
            }

            foreach (AddressDTO address in Customer.Addresses)
            {
                LinearLayout holder = new LinearLayout(this);
                holder.SetPadding(tenDp, tenDp, tenDp, tenDp);
                holder.Orientation = Orientation.Vertical;
                TextView addressV = new TextView(this);
                addressV.Text = address.Firstname + " " + address.Lastname + ", " + address.Street + ", " + address.City + ", " + address.PostalCode + ", " + address.Country;
                addressV.SetTextSize(ComplexUnitType.Sp, 16);
                holder.AddView(addressV);
                View v = new View(this);
                v.SetBackgroundColor(Resources.GetColor(Resource.Color.hinttext));
                v.LayoutParameters = FindViewById<View>(Resource.Id.customerdetailsdivider).LayoutParameters;
                AddressesV.AddView(holder);
                if (!address.Equals(Customer.Addresses.Last()))
                    AddressesV.AddView(v);
            }

            dialog.Dismiss();

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.search_actionbar, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (Resources.GetResourceEntryName(item.ItemId))
            {
                case "actionbar_refresh":
                    InitalizeCustomer();
                    return true;
                case "home":
                    Finish();
                    OverridePendingTransition(Resource.Animation.activity_open_scale, Resource.Animation.activity_close_translate);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void AdminCommentsPopup()
        {
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;
            var FiveDp = ConvertDpToPixels(5);
            layout.SetPadding(FiveDp, FiveDp, FiveDp, FiveDp);
            TextView Comments = new TextView(this);
            Button EditB = new Button(this);
            Comments.SetTextSize(ComplexUnitType.Sp, 18);
            Comments.Text = Customer.AdminComment ?? "No Comments";

            if (Comments.Text.Equals("No Comments")){
                EditB.Text = "New Comment";
            }
            else
            {
                EditB.Text = "Edit Comments";
            }

            layout.AddView(Comments);
            layout.AddView(EditB);

            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Admin Comments");
            builder.SetView(layout);
            builder.SetNeutralButton("Done", (s, e) =>
            {
                builder.Create().Dismiss();
            });
            builder.Create().Show();

            EditB.Click += delegate
            {
                LinearLayout LayoutEdit = new LinearLayout(this);
                LayoutEdit.SetPadding(FiveDp, FiveDp, FiveDp, FiveDp);
                EditText EditComment = new EditText(this);
                LinearLayout.LayoutParams linearLayoutParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, 
                    LinearLayout.LayoutParams.WrapContent);
                EditComment.Text = Customer.AdminComment ?? "";
                EditComment.LayoutParameters = linearLayoutParams;
                LayoutEdit.AddView(EditComment);

                var EditCommentDialog = new AlertDialog.Builder(this);
                EditCommentDialog.SetTitle("Admin Comments");
                EditCommentDialog.SetView(LayoutEdit);
                EditCommentDialog.SetPositiveButton("Save", async (s, e) =>
                {
                    dialog = ProgressDialog.Show(this, "", "Adding Comment. Please wait... ", true);
                    await api.AddCommentToCustomer(Customer.Email,EditComment.Text);
                    await RefreshCustomer();
                    Comments.Text = Customer.AdminComment;
                    dialog.Dismiss();
                });
                EditCommentDialog.SetNegativeButton("Cancel", (s, e) =>
                {
                    EditCommentDialog.Create().Dismiss();
                });
                EditCommentDialog.Create().Show();
            };
        }

        private int ConvertDpToPixels(float dpValue)
        {
            var dp = (int)((dpValue) * Resources.DisplayMetrics.Density);
            return dp;
        }

    }
}