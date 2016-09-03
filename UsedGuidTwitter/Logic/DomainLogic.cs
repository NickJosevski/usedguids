using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using UsedGuidTwitter.Models;

namespace UsedGuidTwitter.Logic
{
    public static class DomainLogic
    {
        public static HttpResponseMessage DetermineRequestValidity(UsedGuidInputModel ug)
        {
            if (ug == null || ug.Guid == Guid.Empty)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Did not supply a valid guid."
                };
            }

            if (ContainsUrl(ug.UsedBy))
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Won't tweet if your data contains a url or something similar"
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // Everyone was warned this could happen!
        public static HttpResponseMessage OhNoExistingGuid()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Gone,
                ReasonPhrase = "Guid Collision, you were warned it was possible. You just saw it happen. Just hope the next guid you try to use is still available, quick hurry."
            };
        }

        // Business requirement, don't let people submit URLs along with their GUID
        private static bool ContainsUrl(string checkThis)
        {
            var regx = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?", RegexOptions.IgnoreCase);

            var mactches = regx.Matches(checkThis);

            return mactches.Count > 0;
        }
    }
}