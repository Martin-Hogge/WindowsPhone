using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.Exceptions
{
    class LocalStorageException : Exception
    {
        public enum exceptionType { ALREADY_EXISTS, TOO_MUCH }
        private exceptionType Type { get; set; }

        public LocalStorageException(exceptionType type)
        {
            Type = type;
        }

        public override string Message
        {
            get
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                switch (Type)
                {
                    case exceptionType.ALREADY_EXISTS:
                        return loader.GetString("ErrorMessageAddFavoriteAlreadyExist");
                    case exceptionType.TOO_MUCH:
                        return loader.GetString("ErrorMessageAddFavoriteTooMuch");
                    default: return loader.GetString("UnknownError");
                }
            }
        }
    }
}
