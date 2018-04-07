using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class ResourceAuthorityInfo
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
        public ResourcePathInfo[] Paths { get; set; }

        public ResourceAuthorityInfo() { }

        public ResourceAuthorityInfo(ResourceAuthority source, List<Guid> pathIds = null)
        {
            if (source == null)
                return;
            source.Normalize();
            this.ID = source.__IDString;
            this.Active = source.Active;
            this.Order = source.Order;
            if (pathIds == null)
                this.Paths = source.Paths.Select(p => new ResourcePathInfo(p)).ToArray();
            else if (pathIds.Count == 0)
                this.Paths = new ResourcePathInfo[0];
            else
                this.Paths = source.Paths.Where(p => pathIds.Contains(p.ID)).Select(p => new ResourcePathInfo(p)).ToArray();
            this.Title = this.Title;
        }
    }
}