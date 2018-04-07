using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class HostWithCredentials : ResourceAuthorityInfo
    {
        [DataMember]
        CredentialData[] Credentials { get; set; }

        public HostWithCredentials(CredentialDomain domain, int index)
            : base(domain.Hosts[index])
        {
            Guid id = domain.Hosts[index].ID;
            domain.Normalize();
            this.Credentials = domain.Credentials.Where(c => c.Hosts.Contains(id)).Select(a => new CredentialData(a)).ToArray();
        }
    }
}