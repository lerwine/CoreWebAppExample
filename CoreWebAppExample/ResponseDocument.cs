using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace CoreWebAppExample
{
    public class ResponseDocument : IDisposable
    {
        public const string Default_Title = "Example Core Web App";

        private MemoryStream _headStream = new MemoryStream();
        private MemoryStream _bodyStream = new MemoryStream();

        public string Title { get; private set; }

        public Encoding CharSet { get { return BodyWriter.Settings.Encoding; } }

        public XmlWriter HeadWriter { get; private set; }

        public XmlWriter BodyWriter { get; private set; }

        public int StatusCode { get; set; }

        public ResponseDocument(string title = null, int statusCode = StatusCodes.Status200OK, Encoding encoding = null)
        {
            StatusCode = statusCode;
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = encoding ?? new UTF8Encoding(false),
                Indent = true,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            HeadWriter = XmlWriter.Create(_headStream, settings);
            BodyWriter = XmlWriter.Create(_bodyStream, settings);
            HeadWriter.WriteStartElement("meta");
            HeadWriter.WriteAttributeString("charset", settings.Encoding.WebName);
            HeadWriter.WriteEndElement();
            HeadWriter.WriteStartElement("meta");
            HeadWriter.WriteAttributeString("name", "viewport");
            HeadWriter.WriteAttributeString("content", "width=800, initial-scale=1, shrink-to-fit=no");
            HeadWriter.WriteEndElement();
            Title = (String.IsNullOrWhiteSpace(title)) ? Default_Title : title;
            HeadWriter.WriteElementString("title", Title);
        }

        public string GetHtml()
        {
            XmlDocument html = new XmlDocument();
            html.AppendChild(html.CreateElement("html"));
            XmlElement element = (XmlElement)(html.DocumentElement.AppendChild(html.CreateElement("head")));
            HeadWriter.Flush();
            Encoding charset = CharSet;
            element.InnerXml = CharSet.GetString(_headStream.ToArray());
            element = (XmlElement)(html.DocumentElement.AppendChild(html.CreateElement("head")));
            BodyWriter.Flush();
            element.InnerXml = CharSet.GetString(_bodyStream.ToArray());
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, charset))
                {
                    sw.WriteLine("<!DOCTYPE html>");
                    XmlWriterSettings settings = BodyWriter.Settings.Clone();
                    settings.ConformanceLevel = ConformanceLevel.Document;
                    using (XmlWriter writer = XmlWriter.Create(sw, settings))
                    {
                        html.WriteTo(writer);
                        writer.Flush();
                        return charset.GetString(ms.ToArray());
                    }
                }
            }
        }

        #region IDisposable Support
        private bool _isDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            if (disposing)
            {
                try { HeadWriter.Close(); }
                finally
                {
                    try { BodyWriter.Close(); }
                    finally
                    {
                        try { _headStream.Dispose(); }
                        finally{ _bodyStream.Dispose(); }
                    }
                }
            }
        }
        public void Dispose() { Dispose(true); }
        #endregion
    }
}