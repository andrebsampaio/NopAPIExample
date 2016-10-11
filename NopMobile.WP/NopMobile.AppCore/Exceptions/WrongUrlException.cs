using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NopMobile.AppCore.Exceptions
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