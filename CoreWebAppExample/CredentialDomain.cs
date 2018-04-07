using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialDomain : CredentialObject
    {
        private List<ResourceAuthority> _hosts = null;
        private List<CredentialInfo> _credentials = null;

        [DataMember]
        public List<ResourceAuthority> Hosts
        {
            get
            {
                List<ResourceAuthority> hosts = _hosts;
                if (hosts == null)
                {
                    hosts = new List<ResourceAuthority>();
                    _hosts = hosts;
                }
                return hosts;
            }
            set { _hosts = value; }
        }

        [DataMember]
        public List<CredentialInfo> Credentials
        {
            get
            {
                List<CredentialInfo> credentials = _credentials;
                if (credentials == null)
                {
                    credentials = new List<CredentialInfo>();
                    _credentials = credentials;
                }
                return credentials;
            }
            set { _credentials = value; }
        }
        
        public override void Normalize()
        {
            CredentialDataStore.NormalizeIDs<ResourceAuthority>(_hosts);
            CredentialDataStore.NormalizeIDs<CredentialInfo>(_credentials);
            if (_credentials == null || _credentials.Count == 0)
                return;

            if (_hosts == null || _hosts.Count == 0)
            {
                foreach (CredentialInfo c in _credentials)
                {
                    if (c.Hosts.Count > 0)
                        c.Hosts.Clear();
                    if (c.Paths.Count > 0)
                        c.Paths.Clear();
                }
                return;
            }
            
            IEnumerable<Guid> hostIds = _hosts.Select(h => h.ID);
            foreach (CredentialInfo c in _credentials)
            {
                if (c.Hosts.Count == 0)
                    continue;
                List<Guid> hosts = c.Hosts.Where(i => hostIds.Any(h => h.Equals(i))).ToList();
                if (hosts.Count < c.Hosts.Count)
                {
                    for (int i = 0; i < hosts.Count; i++)
                        c.Hosts[i] = hosts[i];
                    while (c.Hosts.Count > hosts.Count)
                        c.Hosts.RemoveAt(hosts.Count);
                }
            }
            
            List<Guid> pathIds = _hosts[0].Paths.Select(p => p.ID).ToList();
            for (int i = 1; i < _hosts.Count; i++)
                pathIds.AddRange(_hosts[i].Paths.Select(p =>
                {
                    if (pathIds.Any(g => p.ID.Equals(g)))
                        p.ID = Guid.NewGuid();
                    return p.ID;
                }));

            if (pathIds.Count == 0)
            {
                foreach (CredentialInfo c in _credentials)
                {
                    if (c.Paths.Count > 0)
                        c.Paths.Clear();
                }
                return;
            }
        
            foreach (CredentialInfo c in _credentials)
            {
                if (c.Paths.Count == 0)
                    continue;
                List<Guid> paths = c.Paths.Where(i => pathIds.Contains(i)).ToList();
                if (paths.Count < c.Paths.Count)
                {
                    for (int i = 0; i < paths.Count; i++)
                        c.Paths[i] = paths[i];
                    while (c.Paths.Count > paths.Count)
                        c.Paths.RemoveAt(paths.Count);
                }
            }
        }
    }
}