using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.ImageToolsModule.Web.Exceptions
{
    public class ImageToolsException : Exception
    {
        public ImageToolsException()
            : base() { }

        public ImageToolsException(string message)
            : base(message) { }

        public ImageToolsException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public ImageToolsException(string message, Exception innerException)
            : base(message, innerException) { }

        public ImageToolsException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }

}