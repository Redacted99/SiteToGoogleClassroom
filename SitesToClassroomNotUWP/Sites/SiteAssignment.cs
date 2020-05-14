using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SitesToClassroom.Sites
{
    /// <summary>
    /// entry for a Google Site Assignment
    /// </summary>
    public class SiteAssignment
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }

        public static SiteAssignment Create(XmlNode entryNode, XmlNamespaceManager nsmgr, string siteUrl)
        {
            SiteAssignment siteAssignment = new SiteAssignment();
            siteAssignment.Title = entryNode.SelectSingleNode("atom:title", nsmgr)?.InnerText;
            siteAssignment.Url = siteUrl + entryNode.SelectSingleNode("sites:pageName", nsmgr)?.InnerText;
            siteAssignment.Published = ConvertToDate(entryNode.SelectSingleNode("atom:published", nsmgr)?.InnerText);
            siteAssignment.Updated = ConvertToDate(entryNode.SelectSingleNode("atom:updated", nsmgr)?.InnerText);
            return siteAssignment;
        }

        private static DateTime ConvertToDate(string innerText)
        {
            DateTime result = DateTime.Now;
            if (!DateTime.TryParse(innerText, out result))
            {
                Logger.Instance.Log($"Unable to parse Timestamp {innerText}");
            }
            return result;
        }
    }
}