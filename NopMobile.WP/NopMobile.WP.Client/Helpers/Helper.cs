using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NopMobile.WP.Client.Helpers
{
    class Helper
    {
        static IsolatedStorageSettings UserSettings = IsolatedStorageSettings.ApplicationSettings;
        public static BitmapImage ConvertToBitmapImage(byte[] image)
        {
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream(image);
            bitmapImage.SetSource(memoryStream);
            return bitmapImage;
        }

        public static string CurrentCustomer()
        {
            string Current = "";
            UserSettings.TryGetValue("current_user", out Current);
            return Current;
        }

        public static bool AddOrUpdateValue(string Key, Object value)
        {
            IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;
            bool valueChanged = false;

            // If the key exists
            if (Settings.Contains(Key))
            {
                // If the value has changed
                if (Settings[Key] != value)
                {
                    // Store the new value
                    Settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                Settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }


        public static T GetSetting<T>(string Key)
        {
            Object Result = null;
            if (UserSettings.Contains(Key))
            {
                UserSettings.TryGetValue(Key, out Result);
                if (Result is T)
                {
                    return (T)Convert.ChangeType(Result, typeof(T));
                }
            }
            return default(T);
        }

        public static List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                    _List.Add(_Child as Control);
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }

        public static T FindParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            T result = null;
            DependencyObject currentElement = element;
            while (currentElement != null)
            {
                result = currentElement as T;
                if (result != null)
                {
                    break;
                }
                currentElement = VisualTreeHelper.GetParent(currentElement);
            }

            return result;
        }

        public static T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }
    }
}
