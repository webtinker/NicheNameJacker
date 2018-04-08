using HtmlAgilityPack;
using NicheNameJacker.Parser;
using NicheNameJacker.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NicheNameJacker.DataProviders
{
    public class YahooDataProvider
    {
        private readonly string _query;
        private readonly IList<BaseSearchResult> _store;
        private readonly CancellationToken _cancellationToken;
        private readonly ParserFunction _parser;
        private readonly uint _maxSearchResults;

        /// <summary>
        /// Initializes the new instance of <see cref="HuffingtonpostDataProvider"/> class.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        public YahooDataProvider(
            string query, IList<BaseSearchResult> store, CancellationToken cancellationToken, ParserFunction parser = null,uint maxResults = 25)
        {
            this._query = query;
            this._store = store;
            this._cancellationToken = cancellationToken;
            _parser = parser ?? DomainParser.Parse;
            _maxSearchResults = maxResults;
        }

        public async Task<bool> LoadMoreItemsAsync(uint count)
        {
           return await LoadMoreItemsInternalAsync();
        }
        private string _baseURI = "https://answers.search.yahoo.com/search?p=";

        private async Task<bool> LoadMoreItemsInternalAsync()
        {
            try
            {
                string urlToSearch = string.Concat(_baseURI, this._query);
                HtmlDocument doc = GetDocumentFromURL(urlToSearch);
                await ScrapYahooAnswersPages(doc, baseURL: urlToSearch);
                return true;
            }
            catch (Exception ex)
            {
                // log the exception
                return false;
            }
            
        }
        private async Task ScrappYahooAnswersForQuery(string query)
        {
            try
            {
                string urlToSearch = string.Concat(_baseURI, query);
                HtmlDocument doc = GetDocumentFromURL(urlToSearch);
                await ScrapYahooAnswersPages(doc, baseURL: urlToSearch);
            }
            catch (Exception ex)
            {
                // log the exception
            }
        }


        private async Task ScrapYahooAnswersPages(HtmlDocument doc, string baseURL)
        {
            int totalScrapped = 1;
            while (doc != null && !this._cancellationToken.IsCancellationRequested && totalScrapped< _maxSearchResults)
            {
                totalScrapped += 10; // 10 questions per page
                HtmlNodeCollection allQuestionsOnPage = SelectAllNodesWithXPath(doc, "//*[contains(@class,'lh-17 fz-m')]");
                if (allQuestionsOnPage == null) break; // when no more questions are found
                List<Task> tasks = new List<Task>();
                var nextPageTask = Task.Factory.StartNew(() => GetDocumentFromURL($"{baseURL}&b={totalScrapped}"));
                tasks.Add(nextPageTask);// while this pages results are collected get the next page
                foreach (var question in allQuestionsOnPage)
                {
                    string url = question.GetAttributeValue("href", string.Empty);
                    if (url == string.Empty) continue;
                    tasks.Add(Task.Factory.StartNew(() => GetQuestionsDetails(url))); // get details of 10 questions in parallel
                }
                if (tasks.Count > 0) await Task.WhenAll(tasks);
                doc = nextPageTask.Result;
            }


        }

        private HtmlDocument GetDocumentFromURL(string urlToScrap)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(urlToScrap);
                return doc;
            }
            catch (Exception ex)
            {
                return null;
                //log the exception
            }
        }

        private HtmlNodeCollection SelectAllNodesWithXPath(HtmlDocument doc, string xPath)
        {
            return doc.DocumentNode?.SelectNodes(xPath);
        }
        private int GetResultsCount(HtmlDocument doc)
        {
            var node = doc.DocumentNode.SelectSingleNode("/div[@class='compPagination']/span");
            if (node == null)
                return 0;
            var innerText = node.InnerText;
            try
            {
                var splittedString = innerText.Split(new char[] { ' ' });
                var countString = splittedString[0];
                int totalResults = 0;
                int.TryParse(countString, out totalResults);
                return totalResults;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task GetQuestionsDetails(string url)
        {
            HtmlDocument doc = GetDocumentFromURL(url);
            if (doc == null) return;
            List<string> urlsFromQA = new List<string>();
            var questionLinks = GetLinksFromQuestionText(doc);
            if(questionLinks!=null && questionLinks.Count>0)
                urlsFromQA.AddRange(questionLinks);
            var answersLink = await Task.Run(() => GetLinksFromAnswersText(doc));
            if (answersLink!= null && answersLink.Count > 0)
                urlsFromQA.AddRange(answersLink);
            

            if(urlsFromQA.Count>0) await UpdateStore(urlsFromQA, url);
        }

        private List<string> GetLinksFromQuestionText(HtmlDocument doc)
        {
            var questions =
                 SelectAllNodesWithXPath(doc, "//div[@id='ya-question-detail']/div/div/span[@class='ya-q-text']/a");
            if (questions != null) // if question contains a link to a URL 
            {
                List<string> questionsLinks = new List<string>();
                foreach (var question in questions)
                {
                    var link = question.GetAttributeValue("href", string.Empty);
                    if (link != string.Empty)
                    {
                        questionsLinks.Add(link);
                    }
                }
                return questionsLinks;
            }
            return null;
        }

        private List<string> GetLinksFromAnswersText(HtmlDocument doc)
        {
            var answersWithLinks =
                  doc.DocumentNode.SelectNodes(
                      "//div[@class='Fw-n']/span[@class='ya-q-full-text']/a");
            if (answersWithLinks == null)
            {
                answersWithLinks =
                  doc.DocumentNode.SelectNodes(
                      "//div[@class='answer-detail Fw-n']/span[@class='ya-q-full-text']/a"); // sometimes class is this
            }
            var linksInSourcePara = doc.DocumentNode.SelectNodes("//div[@class='ya-ans-ref Mt-10']/span[@class='ya-ans-ref-text']/a");
            if (linksInSourcePara == null && answersWithLinks == null)
                return null;
            answersWithLinks =  AppendNodeCollections(linksInSourcePara, answersWithLinks);
            var answerURLs = new List<string>();
            foreach (var answer in answersWithLinks)
            {
                var link = answer.GetAttributeValue("href", string.Empty);
                if (link != string.Empty) answerURLs.Add(link);
            }
            return answerURLs;
        }

        private HtmlNodeCollection AppendNodeCollections(HtmlNodeCollection linksInSourcePara, HtmlNodeCollection answersWithLinks)
        {
            if (linksInSourcePara == null)
                return answersWithLinks;
            else if (answersWithLinks == null)
                return linksInSourcePara;
            else
            {
                foreach (var node in linksInSourcePara)
                    answersWithLinks.Append(node);
                return answersWithLinks;
            }
        }

        private async Task UpdateStore(List<string> questionLinks, string url)
        {
            List<BaseSearchResult> results = new List<BaseSearchResult>();
            results.Add(new BaseSearchResult()
            {
                Source = "YahooAnswers",
                SourceAddress = url,
                Domains = (questionLinks).SelectMany(d => _parser.Invoke(d, "YahooAnswers", _query))

            });

            await System.Windows.Application.Current.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
               {
                   foreach (var result in results)
                   {
                       if (!_cancellationToken.IsCancellationRequested)
                       {
                           _store.Add(result);
                       }
                   }
               }));
        }

    }
}
