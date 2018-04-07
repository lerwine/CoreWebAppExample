using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace CoreWebAppExample
{
    public class GetFileError : NotFoundFileInfo
    {
        public Exception Error { get; private set; }

        public GetFileError(string name, Exception exception) : base(name) { Error = exception; }
    }
}