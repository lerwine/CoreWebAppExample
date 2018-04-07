using System;

namespace CoreWebAppExample
{
    public interface ICredentialObject
    {
        Guid ID { get; set; }
        int Order { get; set; }
        bool HasExplicitOrder { get; }
    }
}