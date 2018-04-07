using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CoreWebAppExample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;
                response.ContentType = "text/html";
                XmlDocument document = new XmlDocument();
                document.AppendChild(document.CreateElement("html"));
                XmlElement headElement = document.DocumentElement.AppendElement("head");
                headElement.AppendElement("meta").ApplyAttributeValue("charset", "utf-8");
                headElement.AppendElement("meta")
                    .ApplyAttributeValue("name", "viewport")
                    .ApplyAttributeValue("content", "width=device-width, initial-scale=1, shrink-to-fit=no");
                headElement.AppendTextElement("title", "Example Web Application");
                XmlElement bodyElement = document.DocumentElement.AppendElement("body");
                XmlElement tableElement = bodyElement.AppendElement("table");
                XmlElement rowElement = tableElement.AppendElement("tr");
                rowElement.AppendTextElement("th", "IHostingEnvironment.ContentRootPath").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", env.ContentRootPath ?? "");
                rowElement.AppendTextElement("th", "IHostingEnvironment.ContentRootFileProvider").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", (env.ContentRootFileProvider == null) ? "null" : env.ContentRootFileProvider.GetType().FullName);
                rowElement = tableElement.AppendElement("tr");
                rowElement.AppendTextElement("th", "IHostingEnvironment.WebRootPath").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", env.WebRootPath ?? "");
                rowElement.AppendTextElement("th", "IHostingEnvironment.WebRootFileProvider").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", (env.WebRootFileProvider == null) ? "null" : env.WebRootFileProvider.GetType().FullName);
                rowElement = tableElement.AppendElement("tr");
                rowElement.AppendTextElement("th", "HttpRequest.Path.Hasvalue").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", request.Path.HasValue.ToString());
                rowElement.AppendTextElement("th", "HttpRequest.Path.Value").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", (request.Path.HasValue) ? request.Path.Value : "");
                rowElement = tableElement.AppendElement("tr");
                rowElement.AppendTextElement("th", "HttpRequest.PathBase.Hasvalue").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", request.PathBase.HasValue.ToString());
                rowElement.AppendTextElement("th", "HttpRequest.PathBase.Value").ApplyAttributeValue("style", "text-align: right");
                rowElement.AppendTextElement("th", (request.PathBase.HasValue) ? request.PathBase.Value : "");
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = new System.Text.UTF8Encoding(false),
                    Indent = true
                };
                
                string html;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (XmlWriter writer = XmlWriter.Create(ms, settings))
                    {
                        document.WriteTo(writer);
                        writer.Flush();
                        html = settings.Encoding.GetString(ms.ToArray());
                    }
                }
                await context.Response.WriteAsync("<!DOCTYPE html>\n" + html);
            });
        }
    }
}
