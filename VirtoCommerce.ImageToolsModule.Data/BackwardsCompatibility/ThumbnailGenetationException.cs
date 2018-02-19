using System;

namespace VirtoCommerce.ImageToolsModule.Data.Exceptions
{
    [Obsolete("Going to be moved to VirtoCommerce.ImageToolsModule.Core.Exceptions or deleted", false)]
    public class ThumbnailGenetationException : ImageToolsException
    {
        public ThumbnailGenetationException()
        { }

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