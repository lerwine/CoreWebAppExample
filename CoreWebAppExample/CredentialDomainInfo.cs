using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialDomainInfo
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public ResourceAuthorityInfo[] ResourceAuthority { get; set; }

        [DataMember]
        public CredentialInfo[] Credentials { get; set; }

        public CredentialDomainInfo() { }

        public CredentialDomainInfo(CredentialDomain source)
        {
            if (source == null)
                return;
            source.Normalize();
            this.ID = source.__IDString;
            this.Active = source.Active;
            this.Order = source.Order;
            this.ResourceAuthority = source.Hosts.Select(h => new ResourceAuthorityInfo(h)).ToArray();
            this.Credentials = source.Credentials.ToArray();
        }
    }
}