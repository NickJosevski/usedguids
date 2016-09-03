using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using TweetSharp;

namespace UsedGuidTwitter.Logic
{
    public interface ITweet
    {
        HttpResponseMessage PublishTweet(string tweetText);
    }

    public class Tweeter : ITweet
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _token;
        private readonly string _tokenSecret;

        public Tweeter()
        {
            _consumerKey = ConfigurationManager.AppSettings["TWITTER_consumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["TWITTER_consumerSecret"];

            _token = ConfigurationManager.AppSettings["TWITTER_token"];
            _tokenSecret = ConfigurationManager.AppSettings["TWITTER_tokenSecret"];

            if (new List<string> { _consumerKey, _consumerSecret, _token, _tokenSecret }.Any(string.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("could not find all the twitter integration keys");
            }
        }

        public HttpResponseMessage PublishTweet(string tweetText)
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);

            service.AuthenticateWith(_token, _tokenSecret);

            var tweet = new SendTweetOptions
            {
                DisplayCoordinates = false,
                Status = tweetText
            };

            service.SendTweet(tweet);

            return new HttpResponseMessage
            {
                StatusCode = service.Response.StatusCode,
                ReasonPhrase = service.Response.StatusDescription
            };
        }
    }
}