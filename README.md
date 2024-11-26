# Hacker News Client API

## Overview

This API fetches popular articles from the Hacker News API and returns a list of stories.

## Endpoints
### GET /api/HackerNews/{amount}

    Returns a list of popular Hacker News stories.
    The {amount} parameter specifies the number of stories to return, up to a maximum of 200.

Response

The API returns a list of HackerNewsStory objects, which contain the following properties:

    Id: The unique identifier of the story.
    Title: The title of the story.
    Url: The URL of the story.
    Score: The score of the story.
    Time: The time the story was posted.

## Example Use:

curl https://example.com/api/HackerNews/2

Result:
    [
      {
        "title": "ICC issues warrants for Netanyahu, Gallant, and Hamas officials",
        "uri": "https://www.icc-cpi.int/news/situation-state-palestine-icc-pre-trial-chamber-i-rejects-state-israels-challenges",
        "postedBy": "runarberg",
        "time": "2024-11-21T12:13:17+00:00",
        "score": 858,
        "commentCount": 80
      },
      {
        "title": "RFC 35140: HTTP Do-Not-Stab (2023)",
        "uri": "https://www.5snb.club/posts/2023/do-not-stab/",
        "postedBy": "zkldi",
        "time": "2024-11-25T00:43:03+00:00",
        "score": 768,
        "commentCount": 36
      }
    ]

## TODO
 - Caching logic should be moved to a separate class like a decorator for IHackerNewsService or other kind intermediate between the service and the controller
 - ~~Articles could be stored in a long-term storage (i.e. SQL database) as they are not expected to change over time.~~ Titles can change, score and comment count will change quite often.
 - Rate limiting, could be a middleware class.
