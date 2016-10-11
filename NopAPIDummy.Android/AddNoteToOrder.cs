using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Views.InputMethods;
using Android.Content.PM;

namespace NopMobile.Android
{
    [Activity(Label = "NopMobile", Icon = "@drawable/icon", LaunchMode=LaunchMode.SingleTop)]
    public class AddNoteToOrder : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.AddNote);

            NopCore api = new NopCore();

            Button apply = FindViewById<Button>(Resource.Id.applyButton);
            EditText noteField = FindViewById<EditText>(Resource.Id.note);

           string user =  Intent.GetStringExtra("user") ?? "Data not available";
           string password = Intent.GetStringExtra("pass") ?? "Data not available";
           int orderID = Intent.GetIntExtra("orderid", -1);

           apply.Click += delegate
           {
               string note = noteField.Text.ToString();

               api.NewOrderNote(orderID, note, "admin@yourstore.com", "adminadmin");
               Toast.MakeText(this,"Note Added!", ToastLength.Long).Show();
               hideSoftKeyboard(this,apply);
               this.Finish();
           };
        }

        public static void hideSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }
    }
}

