using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using NSubstitute;
using NUnit.Framework;

using UsedGuidTwitter.Controllers;
using UsedGuidTwitter.Logic;
using UsedGuidTwitter.Models;

namespace Tests
{
    [TestFixture]
    public class ControllerTests
    {
        [Test]
        // I sometimes like to make the case for death by a thousand of cuts form unit tests (when you have too many)
        // This test may seem long, but I want to test 2 major paths, failure and sucess of Post
        // The smaller moving parts are tested on their own
        // So this is leaning towards integration, but it's 1 unit of the flow of the checks in the Post action
        // with 2 of the real services mocked/stubbed out
        // hence the usage of new List<Tuple<HttpStatusCode, UsedGuidInputModel>>
        public void UsedGuidController_Post_CompleteMockRunThrough()
        {
            // Arrange
            var dataStore = Substitute.For<IManageYourData>();
            var tweeter = Substitute.For<ITweet>();

            dataStore.GuidExistsAlready(Arg.Any<Guid>()).Returns(false);
            dataStore.SaveGuid(Arg.Any<Guid>()).Returns(true);

            tweeter.PublishTweet(Arg.Any<string>()).Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

            var controller = new UsedGuidController(dataStore, tweeter);

            new List<Tuple<HttpStatusCode, UsedGuidInputModel>>
            {
                new Tuple<HttpStatusCode, UsedGuidInputModel>(
                    HttpStatusCode.BadRequest,
                    null),

                new Tuple<HttpStatusCode, UsedGuidInputModel>(
                    HttpStatusCode.BadRequest,
                    new UsedGuidInputModel
                    {
                        Guid = Guid.Empty,
                        UsedBy = ""
                    }),

                new Tuple<HttpStatusCode, UsedGuidInputModel>(
                    HttpStatusCode.OK,
                    new UsedGuidInputModel
                    {
                        Guid = Guid.NewGuid(),
                        UsedBy = "Unit Test"
                    }),
            }
            // Have to loop over null cases, because TestCase[] doesn't support Guid.Empty since it's not a constant
            .ForEach(resultAndInputData =>
            {
                var data = resultAndInputData.Item2;
                var expectedResult = resultAndInputData.Item1;

                // Act
                var result = controller.Post(data);

                // Assert
                Assert.AreEqual(expectedResult, result.StatusCode);
            });
        }

        [Test]
        public void UsedGuidController_Post_ReturnsMessageWhenItFindsGuidAlreadyExists()
        {
            // Arrange
            var dataStore = Substitute.For<IManageYourData>();
            var tweeter = Substitute.For<ITweet>();

            dataStore.GuidExistsAlready(Arg.Any<Guid>()).Returns(true);
            dataStore.SaveGuid(Arg.Any<Guid>()).Returns(true);

            var controller = new UsedGuidController(dataStore, tweeter);

            var data = new UsedGuidInputModel
            {
                Guid = Guid.NewGuid(), //NOTE above: GuidExistsAlready(Arg.Any<Guid>()).Returns(true) 
                UsedBy = "Unit Test"
            };

            // Act
            var result = controller.Post(data);

            // Assert
            Assert.AreEqual(HttpStatusCode.Gone, result.StatusCode);
        }

        [Test]
        public void UsedGuidController_Post_UncaughExceptionComesUpToController()
        {
            // Arrange
            var dataStore = Substitute.For<IManageYourData>();
            var tweeter = Substitute.For<ITweet>();

            dataStore.GuidExistsAlready(Arg.Any<Guid>()).Returns(false);

            dataStore.SaveGuid(Arg.Any<Guid>()).Returns(x => { throw new Exception("oops"); });

            var controller = new UsedGuidController(dataStore, tweeter);

            var data = new UsedGuidInputModel
            {
                Guid = Guid.NewGuid(),
                UsedBy = "Unit Test"
            };

            // Act
            var result = controller.Post(data);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }

    [TestFixture]
    public class TweetTextBuilderTests
    {
        [Test]
        public void ProduceTweetText_LoopsThroughAllMessages()
        {
            var last = "";
            for (var x = 0; x < 12; x++)
            {
                var tf = TweetTextBuilder.ProduceTweetText("a", Guid.Empty);

                Console.WriteLine(tf);
                Assert.AreNotEqual(tf, last);
                last = tf;
            }
        }

        [Test]
        [TestCase("Anonymous Coward", null)]
        [TestCase("Anonymous Coward", "")]
        [TestCase("Anonymous Coward", " ")]
        [TestCase("Anonymous Coward", "\t")]
        [TestCase("Anonymous Coward", "\r")]
        [TestCase("Anonymous Coward", "\r\n")]
        [TestCase("UserName", "UserName")]
        public void ProduceTweetText_ReplacesBlankUsedBy(string expectedContents, string usedBy)
        {
            var result = TweetTextBuilder.ProduceTweetText(usedBy, Guid.Empty);

            Assert.That(result.Contains(expectedContents), $"ProduceTweetText didn't deal with '{Regex.Escape(usedBy ?? "")}'");
        }
    }

    [TestFixture]
    public class DomainLogicTests
    {
        [Test]
        public void DetermineRequestValidity_BadRequest_OnEmptyGuid()
        {
            new List<UsedGuidInputModel>
            {
                null,

                new UsedGuidInputModel
                {
                    Guid = Guid.Empty,
                    UsedBy = ""
                }

            }
            // Have to loop over null cases, because TestCase[] doesn't support Guid.Empty since it's not a constant
            .ForEach(nullCase =>
                {
                    var testResult = DomainLogic.DetermineRequestValidity(nullCase);

                    Assert.AreEqual("Did not supply a valid guid.", testResult.ReasonPhrase);
                    Assert.AreEqual(HttpStatusCode.BadRequest, testResult.StatusCode);
                });
        }

        [Test]
        [TestCase("http://a.c")]
        [TestCase("http://www.abc.com")]
        [TestCase("http://dont.accept.this.com")]
        [TestCase("a http://dont.accept.this.com")]
        [TestCase("http://dont.accept.this.com b")]
        [TestCase("https://dont.accept.this.com b")]
        [TestCase("a https://dont.accept.this.com")]
        public void DetermineRequestValidity_BadRequest_OnUrlInUsedBy(string containingUrl)
        {
            var ug = new UsedGuidInputModel
            {
                Guid = Guid.NewGuid(),
                UsedBy = containingUrl
            };

            var testResult = DomainLogic.DetermineRequestValidity(ug);

            Assert.True(testResult.ReasonPhrase.Contains("data contains a url"));
            Assert.AreEqual(HttpStatusCode.BadRequest, testResult.StatusCode);
        }

        [Test]
        public void OhNoExistingGuidDesGiveNonOkHttpStatusCode()
        {
            var testResult = DomainLogic.OhNoExistingGuid();

            Assert.AreNotEqual(HttpStatusCode.OK, testResult.StatusCode);
        }
    }
}
