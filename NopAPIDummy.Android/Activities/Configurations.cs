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
using NopMobile.Android.Adapters;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Support.V7.App;
using Android.Support.V4.App;
using Android.Preferences;
using AndroidSupport = Android.Support;

namespace NopMobile.Android
{
    [Activity(Label = "Configurations", Theme = "@style/CartsBar")]
    public class Configurations : ActionBarActivity
    {
        private SeparatedListAdapter mAdapter;
        
        private string SaleFormat = "Integer";
        private ListView Configs;
        private const int BQ = 0;
        private const int BA = 1;
        private const int PKD = 2;
        private const int PK = 3;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            if (prefs.GetBoolean("default_settings", true))
            {
                editor.PutInt("bestsellers_quantity", 5);
                editor.PutInt("bestsellers_amount", 5);
                editor.PutInt("keywords_dashboard", 3);
                editor.PutInt("keywords_stats", 5);
                editor.PutString("sales_format", "Integer");
                editor.Apply();
            }

            SetContentView(Resource.Layout.configurations);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.SetDisplayShowHomeEnabled(false);

            Configs = FindViewById<ListView>(Resource.Id.configs_list);

            mAdapter = new SeparatedListAdapter(this);

            InitializeConfigs(prefs);

            Configs.ItemClick += Configs_ItemClick;

            Configs.Adapter = mAdapter;
        }

        void Configs_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var Dialog = new NumberPickerDialogFragment(this,1,5,5,BQ);
            switch (e.Position)
            {
                case 1:
                    Dialog = new NumberPickerDialogFragment(this,1,5,5,BQ);
                    Dialog.Show(SupportFragmentManager, "change_setting_bq");
                    break;
                case 2:
                    Dialog = new NumberPickerDialogFragment(this, 1, 5, 5,BA);
                    Dialog.Show(SupportFragmentManager, "change_setting_ba");
                    break;
                case 4:
                    Dialog = new NumberPickerDialogFragment(this, 1, 3, 3,PKD);
                    Dialog.Show(SupportFragmentManager, "change_setting_dash_pk");
                    break;
                case 5:
                    ShowDialog(1);
                    break;
                case 7:
                    Dialog = new NumberPickerDialogFragment(this, 1, 5, 5,PK);
                    Dialog.Show(SupportFragmentManager, "change_setting_pk");
                    break;
            }
            
        }

        

        protected void InitializeConfigs(ISharedPreferences prefs){
            mAdapter = new SeparatedListAdapter(this);
            mAdapter.AddSectionHeaderItem("Sale Values Settings");
            mAdapter.AddItem("Number of Bestsellers by Quantity: " + prefs.GetInt("bestsellers_quantity", 5));
            mAdapter.AddItem("Number of Bestsellers by Amount:"  + prefs.GetInt("bestsellers_amount", 5));
            mAdapter.AddSectionHeaderItem("Dashboard Settings");
            mAdapter.AddItem("Number of Popular Keywords: " + prefs.GetInt("keywords_dashboard", 3));
            mAdapter.AddItem("Sale Values in " + prefs.GetString("sales_format", "Integer"));
            mAdapter.AddSectionHeaderItem("Statistics Settings");
            mAdapter.AddItem("Number of Popular Keywords: " + prefs.GetInt("keywords_stats", 5));
            Configs.Adapter = mAdapter;
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
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override Dialog OnCreateDialog(int id)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Set Value");

            builder.SetSingleChoiceItems(new string [] {"Integer", "Decimal"}, 0, (o, e) => {
                if (e.Which == 0)
                {
                    SaleFormat = "Integer";
                }
                else if (e.Which == 1)
                {
                    SaleFormat = "Decimal";
                }

                editor.PutBoolean("default_settings",false);
          });

            builder.SetNegativeButton("Cancel", (s, a) => { DismissDialog(1); });
            builder.SetPositiveButton("Confirm", (s, a) =>
            {
                editor.PutString("sales_format", SaleFormat );
                editor.Apply();
                InitializeConfigs(prefs);
            });

            return builder.Create();
        }

        private class NumberPickerDialogFragment : AndroidSupport.V4.App.DialogFragment, NumberPicker.IOnValueChangeListener
        {
            private readonly Context _context;
            private readonly int _min, _max, _current;
            private int CurrentSetting = 0;
            private const int BQ = 0;
            private const int BA = 1;
            private const int PKD = 2;
            private const int PK = 3;
            private int value = -1;

            public NumberPickerDialogFragment(Context context, int min, int max, int current, int CurrentSetting)
            {
                _context = context;
                _min = min;
                _max = max;
                _current = current;
                this.CurrentSetting = CurrentSetting;
            }

            public override Dialog OnCreateDialog(Bundle savedState)
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
                ISharedPreferencesEditor editor = prefs.Edit();
                var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
                var view = inflater.Inflate(Resource.Layout.numberpicker_dialog, null);
                var numberPicker = view.FindViewById<NumberPicker>(Resource.Id.numberPicker);
                numberPicker.MaxValue = _max;
                numberPicker.MinValue = _min;
                numberPicker.Value = _current;
                numberPicker.SetOnValueChangedListener(this);

                var dialog = new AlertDialog.Builder(_context);
                dialog.SetTitle("Set Value");
                dialog.SetView(view);
                dialog.SetNegativeButton("Cancel", (s, a) => { Dismiss(); });
                dialog.SetPositiveButton("Confirm", (s, a) => {
                    editor.PutBoolean("default_settings", false);
                    switch (CurrentSetting)
                    {
                        case BQ:
                            editor.PutInt("bestsellers_quantity", value);
                            break;
                        case BA:
                            editor.PutInt("bestsellers_amount", value);
                            break;
                        case PKD:
                            editor.PutInt("keywords_dashboard", value);
                            break;
                        case PK:
                            editor.PutInt("keywords_stats", value);
                            break;
                    }
                    editor.Apply();
                    var Parent = (Configurations)Activity;
                    Parent.InitializeConfigs(prefs);
                });
                return dialog.Create();
            }

            public void OnValueChange(NumberPicker picker, int oldVal, int newVal)
            {
                value = newVal;
            }

        }

    }
}