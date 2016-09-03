using System;

namespace UsedGuidTwitter.Logic
{
    public static class TweetTextBuilder
    {
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string[] AvailableResponses;

        public static int Selection;

        public static int Max;

        public static Random Random;

        static TweetTextBuilder()
        {
            Random = new Random((int)DateTime.Now.Ticks);

            AvailableResponses = new[]
            {
                "{0} is no longer available.",
                "{0} has just been taken, please make a note.",
                "{0} has just been used.",
                "{0} was just taken.",
                "{0} is gone, update your records.",
                "{0} all used up.",
                "{0} was just snapped up.",
                "{0} bam! used!",
                "{0} used. Take note.",
            };

            Max = AvailableResponses.Length;
        }

        public static string ProduceTweetText(string usedBy, Guid guid)
        {
            var msg = AvailableResponses[Selection];

            Selection = (++Selection % Max);

            if (string.IsNullOrWhiteSpace(usedBy))
            {
                usedBy = "Anonymous Coward";
            }

            return string.Format($"{msg} -{usedBy}", guid);
        }
    }
}