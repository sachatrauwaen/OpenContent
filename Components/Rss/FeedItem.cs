using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Rss
{
    public class FeedItem
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public FeedType FeedType { get; set; }

        public string ZipEnclosure { get; set; }

        public FeedItem()
        {
            Link = "";
            Title = "";
            Content = "";
            PublishDate = DateTime.Today;
            FeedType = FeedType.RSS;
            ZipEnclosure = "";
        }
    }
}