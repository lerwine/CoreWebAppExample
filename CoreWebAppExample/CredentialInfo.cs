using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialInfo : CredentialObject
    {
        private string _title = "";
        private string _userName = "";
        private string _password = "";
        private string _pin = "";
        private string _notes = "";

        private List<Guid> _hosts = null;
        private List<Guid> _paths = null;

        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; }
        }

        [DataMember]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value ?? ""; }
        }

        [DataMember]
        public string Password
        {
            get { return _password; }
            set { _password = value ?? ""; }
        }

        [DataMember]
        public string Pin
        {
            get { return _pin; }
            set { _pin = value ?? ""; }
        }

        [DataMember]
        public string Notes
        {
            get { return _notes; }
            set { _notes = value ?? ""; }
        }

        [DataMember]
        public List<Guid> Hosts
        {
            get
            {
                List<Guid> hosts = _hosts;
                if (hosts == null)
                {
                    hosts = new List<Guid>();
                    _hosts = hosts;
                }
                return hosts;
            }
            set { _hosts = value; }
        }

        [DataMember]
        public List<Guid> Paths
        {
            get
            {
                List<Guid> paths = _paths;
                if (paths == null)
                {
                    paths = new List<Guid>();
                    _paths = paths;
                }
                return paths;
            }
            set { _paths = value; }
        }
        
        public override void Normalize()
        {
            foreach (List<Guid> list in (new List<Guid>[] { _paths, _hosts }))
            {
                if (list == null || list.Count < 2)
                    continue;
                List<Guid> normalized = list.GroupBy(i => i).Select(g => g.Key).ToList();
                if (normalized.Count < list.Count)
                {
                    for (int i = 0; i < normalized.Count; i++)
                        list[i] = normalized[i];
                    while (list.Count > normalized.Count)
                        list.RemoveAt(normalized.Count);
                }
            }
        }
    }
}