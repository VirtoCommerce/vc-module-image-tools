using System;


namespace VirtoCommerce.ImageToolsModule.Data.Exceptions
{
    public class ThumbnailGenetationException : ImageToolsException
    {
        public ThumbnailGenetationException()
            : base() { }

        public ThumbnailGenetationException(string message)
            : base(message) { }

        public ThumbnailGenetationException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public ThumbnailGenetationException(string message, Exception innerException)
            : base(message, innerException) { }

        public ThumbnailGenetationException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }

}