using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class ResourcePathInfo
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Path { get; set; }

        public ResourcePathInfo() { }

        public ResourcePathInfo(ResourcePath source)
        {
            if (source == null)
                return;
            source.Normalize();
            this.ID = source.__IDString;
            this.Active = source.Active;
            this.Order = source.Order;
            this.Title = this.Title;
            if (source.Segments.Count == 0)
                this.Path = "/";
            else if (source.Segments.Count == 1)
                this.Path = "/" + Uri.EscapeDataString(source.Segments[0]);
            else
                this.Path = "/" + String.Join("/", source.Segments.Select(s => Uri.EscapeDataString(s)));
        }
    }
}