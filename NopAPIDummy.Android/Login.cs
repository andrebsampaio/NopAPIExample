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
using Android.Graphics;
using Android.Content.PM;
using NopMobile.AppCore.Exceptions;
using Android.Preferences;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Android.Support.V7.App;
using Android.Support.V4.App;
using System.Net;

namespace NopMobile.Android
{
    [Activity(Label = "Login to Nop Admin", MainLauncher = true,LaunchMode = LaunchMode.SingleTop, Theme = "@style/LoginBar")]
    public class Login : ActionBarActivity
    {
        private ProgressDialog dialog;
        private bool Processing = false;
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            NopCore api = new NopCore();
            if (prefs.GetBoolean("login_state", false) && !prefs.GetString("current_url", "").Equals("")
                && prefs.GetBoolean("stay_connected", false) && !Intent.GetBooleanExtra("associate", false))
            {
                api.SetStoreUrl(prefs.GetString("current_url", ""));
                Intent intent = new Intent(this, typeof(Dashboard));
                intent.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(intent);
                Finish();
            }

            SetContentView(Resource.Layout.login);

            if (!Intent.GetBooleanExtra("associate", false))
            {
                SupportActionBar.Hide();
            }
            else
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowHomeEnabled(false);
                SupportActionBar.Title = "Associate New Store";
            }

            EditText email = FindViewById<EditText>(Resource.Id.email);
            Spinner StoreDrop = FindViewById<Spinner>(Resource.Id.storedrop);
            EditText passwordField = FindViewById<EditText>(Resource.Id.password);
            TextView ErrorMessage = FindViewById<TextView>(Resource.Id.errorMessage);
            TextView Http = FindViewById<TextView>(Resource.Id.http);
            EditText SiteUrl = FindViewById<EditText>(Resource.Id.siteurl);
            Button loginB = FindViewById<Button>(Resource.Id.login);
            Button NewStoreB = FindViewById<Button>(Resource.Id.loginwithnew);
            CheckBox SaveUrl = FindViewById<CheckBox>(Resource.Id.saveUrl);
            CheckBox StayConnected = FindViewById<CheckBox>(Resource.Id.stayConnected);

            bool newStore = true;

            var keys = prefs.All;
            var urls = new List<string>();
            foreach (KeyValuePair<string, object> url in keys)
            {
                if (url.Key.StartsWith("store_url_"))
                {
                    urls.Add((string)url.Value);
                }
            }

            if (urls.Count > 0)
            {
                newStore = false;
                StoreDrop.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.login_url_dropdown_item, urls);
                SiteUrl.Visibility = ViewStates.Gone;
                Http.Visibility = ViewStates.Gone;
                SaveUrl.Visibility = ViewStates.Gone;
            }
            else
            {
                newStore = true;
                NewStoreB.Visibility = ViewStates.Gone;
                StoreDrop.Visibility = ViewStates.Gone;
                SaveUrl.Visibility = ViewStates.Visible;
            }

            if (Intent.GetBooleanExtra("associate", false))
            {
                StoreDrop.Visibility = ViewStates.Gone;
                Http.Visibility = ViewStates.Visible;
                SiteUrl.Visibility = ViewStates.Visible;
                SaveUrl.Visibility = ViewStates.Gone;
                NewStoreB.Visibility = ViewStates.Gone;
                StayConnected.Visibility = ViewStates.Gone;
                newStore = true;

            }

            NewStoreB.Click += delegate
            {
                if (!newStore)
                {
                    StoreDrop.Visibility = ViewStates.Gone;
                    Http.Visibility = ViewStates.Visible;
                    SiteUrl.Visibility = ViewStates.Visible;
                    SaveUrl.Visibility = ViewStates.Visible;
                    NewStoreB.Text = "Login with existing store";
                    newStore = true;
                }
                else
                {
                    StoreDrop.Visibility = ViewStates.Visible;
                    Http.Visibility = ViewStates.Gone;
                    SiteUrl.Visibility = ViewStates.Gone;
                    SaveUrl.Visibility = ViewStates.Gone;
                    NewStoreB.Text = "Login with new store";
                    newStore = false;
                }

            };

            loginB.Click += delegate
            {
                if (!Processing)
                {
                    Processing = true;
                    var Task = ProcessLogin(SiteUrl, email, passwordField, loginB, api, SaveUrl, prefs, editor, StayConnected,
                    ErrorMessage, newStore, StoreDrop);
                }
                
            };

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (Resources.GetResourceEntryName(item.ItemId))
            {
                case "home":
                    hideSoftKeyboard(this, FindViewById<EditText>(Resource.Id.email));
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private async Task ProcessLogin(EditText SiteUrl, EditText email, EditText passwordField, Button loginB, NopCore api,
            CheckBox SaveUrl, ISharedPreferences prefs, ISharedPreferencesEditor editor, CheckBox StayConnected, TextView ErrorMessage,
            Boolean newStore, Spinner Urls)
        {
            string url = "";
            if (newStore)
            {
                url = SiteUrl.Text.ToString();
            }
            else
            {
                url = Urls.SelectedItem.ToString();
            }

            string user = email.Text.ToString();
            string password = passwordField.Text.ToString();
            try
            {
                hideSoftKeyboard(this, loginB);
                dialog = ProgressDialog.Show(this, "", "Connecting. Please wait... ", true);
                
                api.SetStoreUrl(url);
                var LoginSuccess = false;
                LoginSuccess = await api.CheckLogin(user, password);
                
                if (LoginSuccess)
                {
                Intent intent = new Intent(this, typeof(Dashboard));

                editor.PutString("current_user", user );
                var Associate = Intent.GetBooleanExtra("associate", false);
                if ((SaveUrl.Checked || Associate ) && !prefs.GetString("store_url_" + api.GetStoreName(), "").Equals(url) && newStore)
                {
                    editor.PutString("store_url_" + api.GetStoreName(), url);
                    editor.PutString("current_url", url);
                }

                if (StayConnected.Checked)
                {
                    editor.PutBoolean("stay_connected", true);
                }

                editor.PutBoolean("login_state", true);
                editor.Apply();
                dialog.Dismiss();
                Processing = false;
                if (Associate)
                {
                    Dashboard.DashboardA.Finish();
                }
                
                    StartActivity(intent);
                    Finish();
                }
                else
                {
                    throw new LoginException("The credentials seem to be incorrect");
                }
                
            }
            
            catch (LoginException ex)
            {
                Console.WriteLine(ex.Message);
                loginB.Text = "Login";
                api = new NopCore();
                dialog.Dismiss();
                Processing = false;
                ErrorMessage.Text = ex.Message;

            }
            catch (WrongUrlException wue)
            {
                Console.WriteLine(wue.Message);
                loginB.Text = "Login";
                api = new NopCore();
                dialog.Dismiss();
                Processing = false;
                ErrorMessage.Text = wue.Message;
            }
        }

        private static void hideSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }
    }
}