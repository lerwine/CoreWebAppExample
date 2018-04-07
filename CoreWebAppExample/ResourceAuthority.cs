using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class ResourceAuthority : CredentialObject
    {
        private string _title = "";
        private List<ResourcePath> _paths = null;

        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; }
        }

        [DataMember]
        public List<ResourcePath> Paths
        {
            get
            {
                List<ResourcePath> paths = _paths;
                if (paths == null)
                {
                    paths = new List<ResourcePath>();
                    _paths = paths;
                }
                return paths;
            }
            set { _paths = value; }
        }

        public override void Normalize() { CredentialDataStore.NormalizeIDs<ResourcePath>(_paths); }

        public ResourceAuthorityInfo AsServiceObject()
        {
            Normalize();
            return new ResourceAuthorityInfo(this);
        }
    }
}