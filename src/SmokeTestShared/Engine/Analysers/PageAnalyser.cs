using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

using Shared.Classes;

namespace SmokeTest.Shared.Engine
{
    public sealed class PageAnalyser : ThreadManager
    {
        #region Private Members 

        private readonly Report _report;
        private readonly bool _clearData;
        private readonly bool _clearImages;
        private const int MaxDepth = 30;

        #endregion Private Members

        #region Constructors

        public PageAnalyser(in Report report, in PageReport pageReport, in ThreadManager parent,
            in bool clearData, in bool clearImages)
            : base(pageReport, new TimeSpan(), parent)
        {
            _report = report ?? throw new ArgumentNullException(nameof(report));
            _clearData = clearData;
            _clearImages = clearImages;
            ContinueIfGlobalException = false;
        }

        #endregion Constructors

        #region Overridden Methods

        protected override bool Run(object parameters)
        {
            if (parameters == null)
                return false;

            PageReport page = parameters as PageReport;

            if (page == null || String.IsNullOrEmpty(page.Content))
                return false;

            try
            {
                page.ClearAnalysisData();
                XmlDocument doc = null;

                using (TextReader reader = new StringReader(page.Content))
                {
                    Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader();
                    sgmlReader.DocType = "HTML";
                    sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
                    sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
                    sgmlReader.InputStream = reader;

                    doc = new XmlDocument();
                    doc.PreserveWhitespace = true;
                    doc.XmlResolver = null;
                    doc.Load(sgmlReader);
                }

                AnalyseXmlDocument(page, doc.FirstChild, 0);
            }
            catch (Exception err)
            {
                ErrorData errorData = new ErrorData(err, new Uri(page.Url), null, null);
                _report.AddError(errorData);
            }
            finally
            {
                while (!page.ProcessingComplete)
                {
                    Thread.Sleep(50);
                }

                if (_clearData)
                {
                    page.Content = GenerateCheckSum(page.Content);
                }

                if (_clearImages)
                {
                    foreach (ImageReport img in page.Images)
                    {
                        img.Bytes = GenerateCheckSum(img.Bytes);
                    }
                }

                // finally force a garbage collection
                GC.Collect(2, GCCollectionMode.Forced);

                page.AnalysisComplete = true;
                _report.FormsAnalysed.AddRange(page.Analysis.Body.Forms);
            }

            return false;
        }

        #endregion Overridden Methods

        public bool Analyse(in PageReport pageReport)
        {
            return !Run(pageReport);
        }

        public void AnalyseXmlDocument(in PageReport page, in XmlNode node, in int depth)
        {
            if (node == null)
                return;

            page.NodeCount++;

            if (depth > page.Depth)
                page.Depth = depth;

            if (depth > MaxDepth)
            {
                _report.AddError(new ErrorData(new Exception("Node depth exceeded during AnalyseXmlDocument"),
                    new Uri(page.Url), null, null));
                return;
            }

            if (node.NodeType == XmlNodeType.DocumentType)
            {
                page.DocumentTypeFound = true;
            }

            if (node.NodeType == XmlNodeType.Element && !page.HtmlNodeFound && node.Name == "html")
            {
                page.HtmlNodeFound = true;
            }

            if (node.NodeType == XmlNodeType.Element && page.HtmlNodeFound && node.Name == "head")
            {
                page.HeadFound = true;
                AnalyseHeader(page, node.FirstChild, depth);
            }
            else if (node.NodeType == XmlNodeType.Element && page.HtmlNodeFound && node.Name == "body")
            {
                page.BodyFound = true;

                XmlNode bodyNode = node.FirstChild;

                while (bodyNode != null)
                {
                    AnalyseBody(page, bodyNode, depth + 1);
                    bodyNode = bodyNode.NextSibling;
                }
            }
            else if (node.HasChildNodes)
            {
                AnalyseXmlDocument(page, node.FirstChild, depth + 1);
            }

            if (node.NextSibling != null)
                AnalyseXmlDocument(page, node.NextSibling, depth);
        }

        #region Private Methods

        private void AnalyseHeader(in PageReport page, in XmlNode node, in int depth)
        {
            if (node == null)
                return;

            page.NodeCount++;

            if (depth > page.Depth)
                page.Depth = depth;

            if (depth > MaxDepth)
            {
                _report.AddError(new ErrorData(new Exception("Node depth exceeded during AnalyseHeader"),
                    new Uri(page.Url), null, null));
                return;
            }

            bool hasAttributes = node.Attributes != null && node.Attributes.Count > 0;

            if (hasAttributes && node.Name == "meta")
            {
                switch (node.Attributes[0].Name)
                {
                    case "charset":
                        page.Analysis.Header.Charset = node.Attributes[0].Value;
                        break;
                    case "name":
                        switch (node.Attributes[0].Value)
                        {
                            case "viewport":
                                page.Analysis.Header.ViewPort = node.Attributes[1].Value;
                                break;
                            case "author":
                                page.Analysis.Header.Author = node.Attributes[1].Value;
                                break;
                            case "description":
                                page.Analysis.Header.Description = node.Attributes[1].Value;
                                break;
                            case "keywords":
                                page.Analysis.Header.Keywords = node.Attributes[1].Value;
                                break;
                        }
                        break;
                }
            }
            else if (node.Name == "title")
            {
                page.Analysis.Header.Title = node.InnerText;
            }
            else if (node.Name == "script")
            {
                page.Analysis.Header.Scripts++;
            }
            else if (node.Name == "link" && hasAttributes && node.Attributes["rel"]?.Value == "stylesheet")
            {

            }

            if (node.HasChildNodes)
            {
                AnalyseHeader(page, node.FirstChild, depth + 1);
            }

            AnalyseHeader(page, node.NextSibling, depth + 1);
        }

        private void AnalyseBody(in PageReport page, in XmlNode node, in int depth)
        {
            if (node == null)
                return;

            page.NodeCount++;

            if (depth > page.Depth)
            { 
                page.Depth = depth;
            }

            if (depth > MaxDepth)
            {
                _report.AddError(new ErrorData(new Exception("Node depth exceeded during AnalyseBody"),
                    new Uri(page.Url), null, null));
                return;
            }

            bool hasAttributes = node.Attributes != null && node.Attributes.Count > 0;

            if (node.Name == "script")
            {
                page.Analysis.Body.Scripts++;
            }
            else if (node.Name == "link" && hasAttributes && node.Attributes["rel"]?.Value == "stylesheet")
            {
                page.Analysis.Body.CssDocuments++;
            }
            else if (node.Name == "form")
            {
                FormAnalysis form = new FormAnalysis();
                form.Id = node.Attributes["id"]?.Value;
                form.Action = node.Attributes["action"]?.Value;
                form.Method = node.Attributes["method"]?.Value;
                page.Analysis.Body.Forms.Add(form);
                AnalyseForm(page, node.FirstChild, depth + 1, form);
            }
            else if (node.Name == "div")
            {
                page.Analysis.Body.Div++;
            }
            else if (node.Name == "h1")
            {
                page.Analysis.Body.Header1++;
            }
            else if (node.Name == "h2")
            {
                page.Analysis.Body.Header2++;
            }
            else if (node.Name == "h3")
            {
                page.Analysis.Body.Header3++;
            }
            else if (node.Name == "h4")
            {
                page.Analysis.Body.Header4++;
            }
            else if (node.Name == "h5")
            {
                page.Analysis.Body.Header5++;
            }
            else if (node.Name == "h6")
            {
                page.Analysis.Body.Header6++;
            }
            else if (node.Name == "ul")
            {
                page.Analysis.Body.UnorderedList++;
            }
            else if (node.Name == "ol")
            {
                page.Analysis.Body.OrderedList++;
            }
            else if (node.Name == "table")
            {

            }

            if (node.HasChildNodes)
            {
                XmlNode child = node.FirstChild;

                while (child != null)
                {
                    AnalyseBody(page, child, depth + 1);
                    child = child.NextSibling;
                }
            }
        }

        private void AnalyseForm(in PageReport page, in XmlNode node, in int depth, in FormAnalysis form)
        {
            if (node == null)
                return;

            bool processChildres = true;
            page.NodeCount++;

            if (depth > page.Depth)
                page.Depth = depth;

            if (depth > MaxDepth)
            {
                _report.AddError(new ErrorData(new Exception("Node depth exceeded during AnalyseForm"),
                    new Uri(page.Url), null, null));
                return;
            }

            if (node.Name == "input")
            {
                FormInput inputAnalysis = new FormInput();
                inputAnalysis.Contents = node.Attributes["value"]?.Value;
                inputAnalysis.Id = node.Attributes["id"]?.Value;
                inputAnalysis.Name = node.Attributes["name"]?.Value;
                inputAnalysis.Type = node.Attributes["type"]?.Value;
                inputAnalysis.Value = node.InnerText;

                form.Inputs.Add(inputAnalysis);
            }
            else if (node.Name == "button")
            {
                FormButton button = new FormButton();
                button.Name = node.Attributes["name"]?.Value;
                button.Type = node.Attributes["type"]?.Value;
                button.Value = node.Attributes["value"]?.Value;
                button.Id = node.Attributes["id"]?.Value;
                form.Buttons.Add(button);
            }
            else if (node.Name == "select")
            {
                FormOption option = new FormOption();
                form.Options.Add(option);
                AnalyseSelect(page, node.FirstChild, option, depth + 1);
                processChildres = false;
            }
            else if (node.Name == "textarea")
            {
                FormTextArea textArea = new FormTextArea();
                textArea.Columns = node.Attributes["cols"]?.Value;
                textArea.Rows = node.Attributes["rows"]?.Value;
                textArea.Id = node.Attributes["id"]?.Value;
                textArea.Name = node.Attributes["name"]?.Value;
                textArea.Value = node.InnerText;

                form.TextAreas.Add(textArea);
            }
            else if (node.Name == "optgroup")
            {

            }
            else if (node.Name == "datalist")
            {

            }
            else
            {
                //what is this
                if (node.Name == "")
                {

                }
            }

            if (processChildres && node.HasChildNodes)
            {
                AnalyseForm(page, node.FirstChild, depth + 1, form);
            }

            AnalyseForm(page, node.NextSibling, depth, form);
        }

        private void AnalyseSelect(in PageReport page, in XmlNode node, in FormOption option, in int depth)
        {
            if (node == null)
                return;

            page.NodeCount++;

            if (depth > page.Depth)
                page.Depth = depth;


            if (depth > MaxDepth)
            {
                _report.AddError(new ErrorData(new Exception("Node depth exceeded during AnalyseSelect"),
                    new Uri(page.Url), null, null));
                return;
            }

            bool hasAttributes = node.Attributes != null && node.Attributes.Count > 0;

            if (hasAttributes && node.Name == "option")
            {
                FormOptionValue optionValue = new FormOptionValue();
                optionValue.Value = node.Attributes["value"]?.Value;
                optionValue.Selected = node.Attributes["selected"]?.Value;
                optionValue.Text = node.InnerText;
                option.Options.Add(optionValue);
            }

            if (node.HasChildNodes)
            {
                AnalyseSelect(page, node.FirstChild, option, depth + 1);
            }

            AnalyseSelect(page, node.NextSibling, option, depth);
        }

        private string GenerateCheckSum(in string data)
        {
            return Convert.ToBase64String(GenerateCheckSum(Encoding.UTF8.GetBytes(data)));
        }

        private byte[] GenerateCheckSum(in byte[] data)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return sha.ComputeHash(data);
            }
        }

        #endregion Private Methods
    }
}
