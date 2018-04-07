using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialData
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
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Pin { get; set; }

        [DataMember]
        public string Notes { get; set; }

        public CredentialData() { }

        public CredentialData(CredentialInfo credential)
        {
            if (credential == null)
                return;
            
            this.ID = credential.__IDString;
            this.Active = credential.Active;
            this.Order = credential.Order;
            this.Title = credential.Title;
            this.UserName = credential.UserName;
            this.Password = credential.Password;
            this.Pin = credential.Pin;
            this.Notes = credential.Notes;
        }
    }
}