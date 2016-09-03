using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using UsedGuidTwitter.Logic;
using UsedGuidTwitter.Models;

namespace UsedGuidTwitter.Controllers
{
    public class UsedGuidController : ApiController
    {
        private readonly IManageYourData _dataStore;
        private readonly ITweet _tweeter;

        public UsedGuidController()
        {
            // TODO: implement an IoC container, like Autofac so we don't need this constructor
            _dataStore = new DataStore();
            _tweeter = new Tweeter();
        }

        public UsedGuidController(IManageYourData dataStore, ITweet tweeter)
        {
            _dataStore = dataStore;
            _tweeter = tweeter;
        }

        public HttpResponseMessage Post(UsedGuidInputModel ug)
        {
            // 1. screening of inputs / unit tested
            var initialCheck = DomainLogic.DetermineRequestValidity(ug);

            if (initialCheck.StatusCode != HttpStatusCode.OK)
            {
                // NOTE: initial check produces exactly what we need to return to API caller for failure cases
                return initialCheck;
            }

            // 2. the main event THE guid colision check ;) / unit & integration testing
            if (_dataStore.GuidExistsAlready(ug.Guid))
            {
                return DomainLogic.OhNoExistingGuid();
            }

            // 3. ok we're good to go it's a guid we haven't seen before
            try
            {
                if (_dataStore.SaveGuid(ug.Guid))
                {
                    // this is unit tested
                    var tweetText = TweetTextBuilder.ProduceTweetText(ug.UsedBy, ug.Guid);

                    // this is for integration testing
                    var publistTweetResult = _tweeter.PublishTweet(tweetText);

                    if (publistTweetResult.StatusCode != HttpStatusCode.OK)
                    {
                        return publistTweetResult;
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = ex.Message
                };
            }
        }
    }
}
