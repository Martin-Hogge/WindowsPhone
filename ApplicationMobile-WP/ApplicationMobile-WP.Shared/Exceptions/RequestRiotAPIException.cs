using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ApplicationMobile_WP.Exceptions
{
    public class RequestRiotAPIException : Exception
    {
        public HttpStatusCode Code { get; set; }

        public RequestRiotAPIException (HttpStatusCode code)
        {
            Code = code;
        }

        public override string Message
        {
            get
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                switch(Code)
                {
                    case HttpStatusCode.NotFound:
                        return loader.GetString("SummonerNotFound");
                    case HttpStatusCode.ServiceUnavailable:
                        return loader.GetString("ServiceUnavailable");
                    case HttpStatusCode.Unauthorized:
                        return loader.GetString("Unauthorized");
                    case (HttpStatusCode)429 :
                        return loader.GetString("RateLimiteExceed");
                    case HttpStatusCode.InternalServerError:
                        return loader.GetString("InternalServerError");
                    default: return loader.GetString("UnknownError"); ;
                }
            }
        }
    }
}
