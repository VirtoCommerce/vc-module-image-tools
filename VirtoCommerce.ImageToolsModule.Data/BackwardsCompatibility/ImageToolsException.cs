using System;

namespace VirtoCommerce.ImageToolsModule.Data.Exceptions
{
#pragma warning disable 612, 618
    [Obsolete("Going to be moved to VirtoCommerce.ImageToolsModule.Core.Exceptions or deleted", false)]
#pragma warning restore 612, 618
    public class ImageToolsException : Exception
    {
        public ImageToolsException()
        { }

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