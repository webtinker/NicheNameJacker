using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Apis.YouTube.v3.Data;
using HtmlAgilityPack;
using NicheNameJacker.Common;
using NicheNameJacker.DataProviders;
using NicheNameJacker.Extensions;
using NicheNameJacker.Schema;

namespace NicheNameJacker.Parser
{
    static class YouTubeParser
    {
        public static YouTubeSearchResult ParseResult(Video video, ParserFunction parser, string keyword)
        {
            YouTubeSearchResult result = new YouTubeSearchResult();
            result.Id = video.Id;
            result.Title = video.Snippet.Title;
            result.ChannelTitle = video.Snippet.ChannelTitle;
            result.ViewCount = video.Statistics?.ViewCount;
            result.PublishedDate = video.Snippet.PublishedAt;
            result.LikeCount = video.Statistics?.LikeCount;
            result.DislikeCount = video.Statistics?.DislikeCount;
            result.Description = video.Snippet.Description;
            result.Source = "YouTube";
            result.SourceAddress = string.Format("https://www.youtube.com/watch?v={0}", video.Id);
            result.Domains = parser.Invoke(video.Snippet.Description, "YouTube", keyword);
            result.ThumbnailUrl = video.Snippet.Thumbnails?.Medium.Url;
            result.Duration = Assistant.ToTimeSpan(video.ContentDetails?.Duration).ToString();

            return result;
        }

        public static VideosPageInfo ParseVideosPage(string response)
        {
            List<string> links = new List<string>();
            string nextPage = string.Empty;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);
            
            try
            {
                var listItems = document.DocumentNode.SelectNodes(".//div[@data-context-item-id]//a[@aria-hidden]");
                if (listItems != null)
                    links.AddRange(listItems.Select(item => item.GetAttributeValue("href", String.Empty))
                        .Where(item => !item.IsNullOrEmpty())
                        .Select(System.Net.WebUtility.UrlDecode)
                        .Select(item => item.Match(@"(?>(?<Result>/(?!.*/).*$))", "Result")));

                
                //(?> (?< Result >/ (? !.*/).*$))
                var nextPageNode =
                        document.DocumentNode.SelectSingleNode(".//div[contains(@class,'search-pager')]/a[last()]");

                if (nextPageNode != null && !nextPageNode.InnerText.IsNullOrEmpty() && nextPageNode.InnerText.Contains("Next"))
                    nextPage = System.Net.WebUtility.HtmlDecode(nextPageNode.GetAttributeValue("href", string.Empty));
            }
            catch (Exception e)
            {
                
            }

            return new VideosPageInfo() { Links = links, NextPage = nextPage };
        }

        public static string ParseVideoPage(string response)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);

            string description = string.Empty;
            try
            {
                var descriptionNode =
                    document.DocumentNode.SelectSingleNode(
                        ".//*[@id='eow-description']");

                if (descriptionNode != null)
                {
                    description = descriptionNode.InnerText;
                }
            }
            catch (Exception)
            {
            }

            return description;
        }
    }
}
