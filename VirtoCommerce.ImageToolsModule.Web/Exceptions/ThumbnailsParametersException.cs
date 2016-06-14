using System;


namespace VirtoCommerce.ImageToolsModule.Web.Exceptions
{
    public class ThumbnailsParametersException : ImageToolsException
    {
        public ThumbnailsParametersException()
            : base() { }

        public ThumbnailsParametersException(string message)
            : base(message) { }

        public ThumbnailsParametersException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public ThumbnailsParametersException(string message, Exception innerException)
            : base(message, innerException) { }

        public ThumbnailsParametersException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }

}