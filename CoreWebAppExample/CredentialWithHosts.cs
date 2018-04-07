using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialWithHosts : CredentialData
    {
        [DataMember]
        ResourceAuthorityInfo[] Hosts { get; set; }

        public CredentialWithHosts(CredentialDomain domain, int index)
            : base(domain.Credentials[index])
        {
            List<Guid> paths = domain.Credentials[index].Paths;
            domain.Normalize();
            this.Hosts = domain.Credentials[index].Hosts.SelectMany(g => domain.Hosts.Where(h => h.ID.Equals(g))).Select(a => new ResourceAuthorityInfo(a, paths)).ToArray();
        }
    }
}