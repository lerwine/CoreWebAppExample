using System;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace CoreWebAppExample
{
    public static class ResponseHelper
    {
        public static async Task HandleRequestAsync(HttpContext context, IHostingEnvironment env)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            if (request.Path.HasValue)
            {
                PathString rootApiPath = new PathString("/_api/credentials/");
                if (request.Path.StartsWithSegments(rootApiPath))
                {
                    await HandleApiRequestAsync(rootApiPath, request, response, env);
                    return;
                }
            }
            await HandleGetFileRequestAsync(request, response, env);
        }

        public static async Task SendTextResponseAsync(HttpResponse response, string text, ContentType contentType = null, int statusCode = StatusCodes.Status200OK)
        {
            response.StatusCode = statusCode;
            response.ContentType = (contentType == null) ? MediaTypeNames.Text.Plain : contentType.ToString();
            await response.WriteAsync(text);
        }

        public static async Task SendHtmlResponseAsync(HttpResponse response, ResponseDocument document)
        {
            await SendTextResponseAsync(response, document.GetHtml(), new ContentType(MediaTypeNames.Text.Html), document.StatusCode);
        }

        public static async Task SendErrorResponseAsync(HttpResponse response, string title, IHostingEnvironment env, Action<XmlWriter> writeMessage = null,
            Exception exception = null, int statusCode = StatusCodes.Status500InternalServerError)
        {
            using (ResponseDocument responseDocument = new ResponseDocument(title))
            {
                string cssClass = (exception != null) ? "danger" : "warning";
                responseDocument.BodyWriter.WriteStartElement("h1");
                responseDocument.BodyWriter.WriteAttributeString("class", cssClass);
                responseDocument.BodyWriter.WriteString(responseDocument.Title);
                responseDocument.BodyWriter.WriteEndElement();
                if (writeMessage != null)
                {
                    responseDocument.BodyWriter.WriteStartElement("div");
                    responseDocument.BodyWriter.WriteAttributeString("class", "alert-" + cssClass);
                    if (writeMessage != null)
                        writeMessage(responseDocument.BodyWriter);
                    responseDocument.BodyWriter.WriteEndElement();
                }
                if (exception != null && env.IsDevelopment())
                {
                    responseDocument.BodyWriter.WriteStartElement("dl");
                    responseDocument.BodyWriter.WriteElementString("dt", "Exception Type");
                    responseDocument.BodyWriter.WriteElementString("dd", exception.GetType().FullName);
                    responseDocument.BodyWriter.WriteElementString("dt", "Message");
                    responseDocument.BodyWriter.WriteElementString("dd", exception.Message);
                    responseDocument.BodyWriter.WriteEndElement();
                    try
                    {
                        if (!String.IsNullOrWhiteSpace(exception.StackTrace))
                        {
                            responseDocument.BodyWriter.WriteElementString("h3", "Stack trace");
                            responseDocument.BodyWriter.WriteElementString("pre", "exception.StackTrace");
                        }
                    } catch { }
                }
                responseDocument.StatusCode = statusCode;
                await SendHtmlResponseAsync(response, responseDocument);
            }
        }

        public static async Task HandleGetFileRequestAsync(HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {
            IFileInfo fileInfo = request.Path.GetContentFileInfo(env.WebRootFileProvider);
            if (fileInfo.Exists)
            {
                System.Net.Mime.ContentType contentType = ResponseHelper.ContentTypeFromName(fileInfo.Name);
                contentType.Name = fileInfo.Name;
                response.ContentType = contentType.ToString();
                response.StatusCode = StatusCodes.Status200OK;
                await response.SendFileAsync(fileInfo);
            } else if (fileInfo is GetFileError)
                await SendErrorResponseAsync(response, "Error locating resource", env, tw =>
                {
                    tw.WriteString("Unexpected error locating file at ");
                    tw.WriteElementString("code", fileInfo.Name);
                    tw.WriteString(".");
                }, ((GetFileError)fileInfo).Error);
            else
                await SendErrorResponseAsync(response, "Error locating resource", env, tw =>
                {
                    tw.WriteString("Unable to locate file at ");
                    tw.WriteElementString("code", fileInfo.Name);
                    tw.WriteString(".");
                }, ((GetFileError)fileInfo).Error);
        }

        public static async Task HandleApiRequestAsync(PathString rootApiPath, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {
            PathString apiPath = rootApiPath.Add(new PathString("list"));
            Action<string, HttpRequest, HttpResponse, IHostingEnvironment> handlerFunc;

            if (request.Path.StartsWithSegments(apiPath))
                handlerFunc = HandleListApi;
            else if (request.Path.StartsWithSegments((apiPath = rootApiPath.Add(new PathString("item")))))
                handlerFunc = HandleGetItemApi;
            else if (request.Path.StartsWithSegments((apiPath = rootApiPath.Add(new PathString("update")))))
                handlerFunc = HandleUpdateItemApi;
            else if (request.Path.StartsWithSegments((apiPath = rootApiPath.Add(new PathString("delete")))))
                handlerFunc = HandleDeleteItemApi;
            else
            {
                apiPath = rootApiPath;
                handlerFunc = Handle404Api;
            }
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    handlerFunc((request.Path.Value.Length > apiPath.Value.Length) ? request.Path.Value.Substring(apiPath.Value.Length) : "", request, response, env);
                }
                catch (Exception exc)
                {
                    if (response.HasStarted)
                        response.Clear();
                    await SendErrorResponseAsync(response, "Unexpected Error", env, tw =>
                    {
                        tw.WriteString("Unexpected error handling API request ");
                        tw.WriteElementString("code", request.Path.Value);
                        tw.WriteString(".");
                    }, exc);
                }
            });
        }

        private static void HandleReturnResult<T>(T result, HttpResponse response, IHostingEnvironment env)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/json";
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(response.Body, result);
        }

        private static void HandleListApi(string path, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {
            if (path.StartsWith("/"))
                path = (path.Length == 1) ? "" : path.Substring(1);
            
            CredentialDataStore dataStore = CredentialDataStore.Load(env, "credentials.json");
            Guid id;
            switch (path)
            {
                case "domain":
                    HandleReturnResult<CredentialDomainInfo[]>(dataStore.Domains.Select(d => new CredentialDomainInfo(d)).ToArray(), response, env);
                    break;
                case "host":
                    HostWithCredentials[] hwc = new HostWithCredentials[0];
                    if (request.Query.ContainsKey("domain") && Guid.TryParse(request.Query["domain"], out id))
                    {
                        CredentialDomain domain = dataStore.Domains.FirstOrDefault(d => d.ID.Equals(id));
                        if (domain != null)
                        {
                            if (request.Query.ContainsKey("id"))
                            {
                                if (Guid.TryParse(request.Query["id"], out id))
                                    hwc = domain.Hosts.Where(h => h.ID.Equals(id)).Select((h, i) => new HostWithCredentials(domain, i)).ToArray();
                            }
                            else
                                hwc = domain.Hosts.Select((h, i) => new HostWithCredentials(domain, i)).ToArray();
                        }
                    }
                    HandleReturnResult<HostWithCredentials[]>(hwc, response, env);
                break;  
            }
        }

        private static void HandleGetItemApi(string path, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {

        }

        private static void HandleDeleteItemApi(string path, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {

        }

        private static void HandleUpdateItemApi(string path, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {

        }

        private static void Handle404Api(string path, HttpRequest request, HttpResponse response, IHostingEnvironment env)
        {

        }

        public static readonly string[] DefaultPageNames = { "index.html", "index.htm", "default.html", "default.htm" };
        public static IFileInfo GetContentFileInfo(this PathString path, IFileProvider fileProvider)
        {
            string rPath = (path == null || !path.HasValue || path.Value == "") ? "/" : path.Value;
            IFileInfo fileInfo = null;
            try
            {
                fileInfo = fileProvider.GetFileInfo(rPath);
                if (fileInfo == null)
                    fileInfo = new NotFoundFileInfo(rPath);
                else if (fileInfo.Exists)
                {
                    if (!fileInfo.IsDirectory)
                        return fileInfo;
                    IDirectoryContents contents = fileProvider.GetDirectoryContents(rPath);
                    IFileInfo page = DefaultPageNames.Select(p => contents.FirstOrDefault(c => String.Equals(p, c.Name, StringComparison.InvariantCultureIgnoreCase)))
                        .FirstOrDefault(f => f != null);
                    if (page != null)
                        fileInfo = page;
                }
            } catch (Exception exception) { fileInfo = new GetFileError(rPath, exception); }
            return fileInfo;
        }
        
        public static ContentType ContentTypeFromName(string fileName)
        {
            string extensionLc = "";
            try
            {
                extensionLc = System.IO.Path.GetExtension(fileName).ToLower();

                switch (extensionLc)
                {
                    case ".html":
                    case ".htm":
                        return new ContentType(MediaTypeNames.Text.Html);
                    case ".txt":
                        return new ContentType(MediaTypeNames.Text.Plain);
                    case ".xml":
                    case ".xsd":
                    case ".xsl":
                    case ".xslt":
                        return new ContentType(MediaTypeNames.Text.Xml);
                    case ".pdf":
                        return new ContentType(MediaTypeNames.Application.Pdf);
                    case ".rtf":
                        return new ContentType(MediaTypeNames.Application.Rtf);
                    case ".zip":
                        return new ContentType(MediaTypeNames.Application.Zip);
                    case ".gif":
                        return new ContentType(MediaTypeNames.Image.Gif);
                    case ".jpg":
                    case ".jpeg":
                        return new ContentType(MediaTypeNames.Image.Jpeg);
                    case ".tif":
                    case ".tiff":
                        return new ContentType(MediaTypeNames.Image.Tiff);
                    case ".json":
                        return new ContentType("application/json");
                    case ".png":
                        return new ContentType("image/png");
                    case ".css":
                        return new ContentType("text/css");
                    case ".js":
                        return new ContentType("text/javascript");
                    case ".md":
                        return new ContentType("text/markdown");
                    case ".sgm":
                    case ".sgml":
                        return new ContentType("text/SGML");
                }
                Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extensionLc);
                if (regKey != null && regKey.GetValue("Content Type") != null)
                {
                    string contentType = regKey.GetValue("Content Type").ToString();
                    if (contentType != null && (contentType = contentType.Trim()).Length > 0)
                        return new ContentType(contentType);
                }
            }
            catch { }

            switch (extensionLc)
            {
                case ".csv":
                    return new ContentType("text/csv");
                case ".tsv":
                    return new ContentType("text/tab-separated-values");
                case ".mpg":
                case ".mpeg":
                    return new ContentType("audio/mpeg");
                case ".ogg":
                    return new ContentType("audio/ogg");
                case ".mp4":
                    return new ContentType("video/mp4");
            }

            return new ContentType(MediaTypeNames.Application.Octet);
        }

    }
}