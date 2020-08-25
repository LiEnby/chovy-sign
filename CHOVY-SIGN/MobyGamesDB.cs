using HtmlAgilityPack;
using System;
using System.Net;

namespace CHOVY_SIGN
{
    class MobyGamesDB
    {
       
        // Scraping there site because there API requires auth and API keys,
        // And fuck that lol
        // Freedom for ALL!
        
        public static string CoverImage = "";
        public static string Name = "";
        public static void GetGameInformation(string DiscId)
        {
            try
            {
                WebClient wc = new WebClient();
                string SearchQuery = wc.DownloadString("https://www.mobygames.com/search/quick?q=" + DiscId);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(SearchQuery);

                HtmlNode imgNode = doc.DocumentNode.SelectNodes("//*[@id=\"coreGameCover\"]/a/img")[0];
                CoverImage = "https://www.mobygames.com/" + imgNode.GetAttributeValue("src", "(none)");

                HtmlNode titleNode = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div/div[2]/h1/a")[0];
                Name = titleNode.InnerText;
            }
            catch(Exception)
            {
                CoverImage = "(none)";
                Name = "Epic PS1 Game!";
            }

        }
    }
}
