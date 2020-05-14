using SitesToClassroom.Sites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace SitesToClassroom
{
    /// <summary>
    /// Load classroom assignments
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Access to my local settings
        /// </summary>
        private IDictionary localSettings = App.Current.Properties;

        /// <summary>
        /// Logger instance
        /// </summary>
        private Logger logger = Logger.Instance;
        public MainWindow()
        {
            InitializeComponent();
            App.CreateLogWindow();
            this.SiteURL.Text = (string)localSettings["SiteURL"];
            logger.Log("Starting...");
        }

        //https://sites.google.com/feeds/content/site/wardmaribeth/
        //https://sites.google.com/site/wardmaribeth/

        private async void FetchSite_Click(object sender, RoutedEventArgs e)
        {
            int startIndex;
            try
            {
                // check input parameter
                if (string.IsNullOrEmpty(SiteURL.Text))
                {
                    logger.Log("A URL is required!");
                    return;
                }

                Uri siteUri;
                if (!Uri.TryCreate(SiteURL.Text, UriKind.Absolute, out siteUri))
                {
                    logger.Log("Site URL is not in a valid format!");
                    return;
                }

                // prohibit clicking
                ((Button)sender).IsEnabled = false;
                this.Wait.Visibility = Visibility.Visible;

                // loop through all pages ?start-index=1 ...
                startIndex = 1;

                // fix-up construct the URL base for content and structure
                string contentUrl = (!SiteURL.Text.EndsWith("/")) ? SiteURL.Text + "/" : SiteURL.Text;
                string siteMetaUrl = contentUrl.Replace("sites.google.com/","sites.google.com/feeds/content/");

                // extract the assignments
                List<SiteAssignment> siteAssignments = new List<SiteAssignment>();
                while (true)
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        logger.Log("Starting next page fetch");
                        // load descriptions from Site
                        HttpResponseMessage response = await httpClient.GetAsync($"{siteMetaUrl}?start-index={startIndex}");
                        HttpContent content = response.Content;

                        // bring data into an XML document, setup the atom namespace
                        XmlDocument document = new XmlDocument();
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                        nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                        nsmgr.AddNamespace("sites", "http://schemas.google.com/sites/2008");
                        document.Load(await content.ReadAsStreamAsync());

                        // extract the assignments
                        logger.Log($"Processing page starting with {startIndex}");
                        var thisPagesExtract = ExtractAssignments(document, nsmgr, contentUrl);
                        siteAssignments.AddRange(thisPagesExtract);
                        if (thisPagesExtract.Count == 0)
                        {
                            logger.Log("Completed processing");
                            break;
                        }
                        startIndex += document.SelectNodes("//atom:entry", nsmgr).Count;
                    }
                    App.SiteAssignments = siteAssignments;
                }
                localSettings["SiteURL"] = SiteURL.Text;                    // update persistent parameter
            }
            catch (Exception eX)
            {
                logger.Log(eX, "Accessing Site URL");
                ((Button)sender).IsEnabled = true;
                this.Wait.Visibility = Visibility.Hidden;
                return;
            }

            this.Wait.Visibility = Visibility.Hidden;
            Status.Text = $"Google Site processed, {startIndex - 1} assignments read";
            Status.Visibility = Visibility.Visible;
            if (startIndex > 1)
                Next.Visibility = Visibility.Visible;
        }

        private List<SiteAssignment> ExtractAssignments(XmlDocument document, XmlNamespaceManager nsmgr, string siteUrl)
        {
            var siteAssignments = new List<SiteAssignment>();
            var entryNodes = document.SelectNodes("//atom:entry", nsmgr);
            if (entryNodes == null || entryNodes.Count == 0)            // did we find any entry nodes?
            {
                logger.Log("Site XML was in an unexpected format, no entry nodes! " + document.InnerText.Substring(0, Math.Min(document.InnerText.Length, 250)));
                return siteAssignments;
            }
            foreach (XmlNode entryNode in entryNodes)
            {
                var contentNode = entryNode.SelectSingleNode("atom:content", nsmgr);        // is this an entry with content?
                string type = contentNode?.Attributes["type"].Value;
                if (type == "xhtml")
                {
                    siteAssignments.Add(SiteAssignment.Create(entryNode, nsmgr, siteUrl));
                    logger.Log($"Found {siteAssignments.Last().Title} last updated {siteAssignments.Last().Published} url:{siteAssignments.Last().Url}");
                }
            }


            return siteAssignments;
        }


        private void Next_Click(object sender, RoutedEventArgs e)
        {
            App.GoToWindow(new ClassroomWindow(), this);
        }
    }
}
