##Used Guid Reporting

Ever wondered if that that GUID you're about to use has already been consumed? 

No. 

Well you should. Check the twitter feed - [twitter.com/UsedGuid]( https://twitter.com/UsedGuid ), and start reporting your usage of GUIDs. It's the right thing to do.

Throw away those old paper based systems.

![]( http://i.stack.imgur.com/Tuwra.png )

[Nick's complete post](http://blog.nick.josevski.com/2012/10/28/playing-with-appharbor-twitter-and-webapi) on this.

## API endpoint

  http://usedguids.apphb.com/api/UsedGuid

## Usage

1. Supply the guid you would like to use/reserve.
2. Get a response
  - Either 
    - 200 ok, you're set.
    - 4xx, 5xx something went wrong, see the response message.
  
### Curl

    curl http://usedguids.apphb.com/api/UsedGuid \
        -d Guid="64b798cb-1013-48bb-8f30-474e35fbba7a" \
        -d UsedBy="CurlTest" \


## Tech

 ASP.NET [Web API](http://www.asp.net/web-api) on [AppHarbor](https://appharbor.com/), TweetSharp Nuget [package](http://nuget.org/packages/TweetSharp)

## More Usage Samples

### Message Sample (e.g. fiddler)

Header:

    Content-Type: application/json; charset=utf-8
    X-Requested-With: XMLHttpRequest
    Host: usedguids.apphb.com

Request Body:

    { "Guid":"64b798cb-1013-48bb-8f30-474e35fbba7a" , "UsedBy":"Your Name" }



### jQuery ajax post exmaple:

        $.ajax({
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            url: 'http://usedguids.apphb.com/api/UsedGuid',
            data: JSON.stringify({ Guid: '*your_guid*', UsedBy: '*your_name*' })
        });
