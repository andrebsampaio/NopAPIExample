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

namespace NopMobile.Core.Exceptions
{
    public class WrongUrlException : Exception
    {
        public WrongUrlException()
        {
        }

        public WrongUrlException(string message)
            : base(message)
        {
        }

        public WrongUrlException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}