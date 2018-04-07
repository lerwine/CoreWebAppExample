using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class ResourcePath : CredentialObject
    {
        private string _title = "";
        private List<string> _segments = new List<string>();

        [DataMember]
        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; }
        }

        [DataMember]
        public List<string> Segments
        {
            get
            {
                List<string> segments = _segments;
                if (segments == null)
                {
                    segments = new List<string>();
                    _segments = segments;
                }
                return segments;
            }
            set { _segments = value; }
        }

        public override void Normalize()
        {
            List<string> segments = _segments;
            if (segments == null || segments.Count == 0)
                return;
                
            for (int i = 0; i < segments.Count; i++)
            {
                if (String.IsNullOrEmpty(segments[i]))
                {
                    segments.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}