using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    public abstract class CredentialObject : ICredentialObject
    {
        private Guid? _id = null;

        private bool _active = true;
        private int? _order = null;

        public Guid ID
        {
            get
            {
                if (!_id.HasValue)
                    _id = Guid.NewGuid();
                return _id.Value;
            }
            set { _id = value; }
        }

        [DataMember(Name = "ID")]
        public string __IDString
        {
            get { return ID.ToString("d"); }
            set
            {
                Guid id;
                if (!String.IsNullOrEmpty(value) && Guid.TryParse(value, out id))
                    _id = id;
            }
        }

        [DataMember]
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        [DataMember]
        public int Order
        {
            get
            {
                if (!_order.HasValue)
                    return 0;
                return _order.Value;
            }
            set { _order = value; }
        }

        bool ICredentialObject.HasExplicitOrder { get { return _order.HasValue; } }
        
        public virtual void Normalize() { }
    }
}