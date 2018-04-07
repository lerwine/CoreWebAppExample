using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace CoreWebAppExample
{
    [DataContract]
    public class CredentialDataStore
    {
        private static CredentialDataStore _cached = null;

        private List<CredentialDomain> _domains = new List<CredentialDomain>();
        private IHostingEnvironment _env;

        public string Subpath { get; private set; }

        [DataMember]
        public List<CredentialDomain> Domains
        {
            get
            {
                List<CredentialDomain> domains = _domains;
                if (domains == null)
                {
                    domains = new List<CredentialDomain>();
                    _domains = domains;
                }
                return domains;
            }
            set { _domains = value; }
        }

        public CredentialDataStore() { }

        public static CredentialDataStore Load(IHostingEnvironment env, string subpath)
        {
            if (env == null)
                throw new ArgumentNullException("env");
            
            CredentialDataStore cached = _cached;
            if (cached != null)
            {
                if (env != null)
                    return cached;
                if (cached._env == null)
                {
                    cached._env = env;
                    return cached;
                }
                else if (ReferenceEquals(cached._env, env))
                    return cached;
            }
            IFileInfo fileInfo = env.ContentRootFileProvider.GetFileInfo(subpath);
            if (fileInfo.Exists)
            {
                using (Stream stream = fileInfo.CreateReadStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CredentialDataStore));
                    cached = (CredentialDataStore)(serializer.ReadObject(stream));
                }
                cached._env = env;
                cached.Normalize();
            }
            else
            {
                cached = new CredentialDataStore();
                cached._env = env;
                cached.Save();
            }
            _cached = cached;
            return cached;
        }

        private void Save(IHostingEnvironment env = null)
        {
            if (env == null)
                env = _env;

            this.Normalize();
            IFileInfo fileInfo = env.ContentRootFileProvider.GetFileInfo(this.Subpath);
            using (FileStream stream = new FileStream(fileInfo.PhysicalPath, FileMode.Create, FileAccess.Write))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CredentialDataStore));
                serializer.WriteObject(stream, this);
                stream.Flush();
            }

            if (env != null)
                _env = env;
        }

        public static void NormalizeIDs<T>(List<T> source)
            where T : CredentialObject
        {
            if (source == null || source.Count == 0)
                return;
            if (source.Count == 1)
            {
                if (source[0] == null)
                    source.Clear();
                else
                    source[0].Order = 100;
                return;
            }
            List<T> normalized = _AsSorted(source.Where(d => d != null).GroupBy(d => d.ID).SelectMany(g =>
                g.Take(1).Concat(g.Skip(1).Where((v, i) => !g.Take(i + 1).Any(a => ReferenceEquals(a, v))).Select(a =>
                {
                    a.ID = Guid.NewGuid();
                    return a;
                })))).ToList();
            for (int i = 0; i < normalized.Count && i < source.Count; i++)
                source[i] = normalized[i];
            while (source.Count > normalized.Count)
                source.RemoveAt(normalized.Count);
        }

        private static IEnumerable<T> _AsSorted<T>(IEnumerable<T> source)
            where T : ICredentialObject
        {
            return source.Where(i => i.HasExplicitOrder).OrderBy(i => i.Order).Concat(source.Where(i => !i.HasExplicitOrder))
                .Select((a, i) => {
                    a.Order = (i + 1) * 100;
                    return a;
                });
        }

        public static void EnsureOrder<T>(List<T> source)
            where T : ICredentialObject
        {
            if (source == null || source.Count == 0)
                return;
            if (source.Count == 1)
            {
                if (source[0] == null)
                    source.Clear();
                else
                    source[0].Order = 100;
                return;
            }

            IEnumerable<T> items = source.Where(d => d != null);
            List<T> sorted = _AsSorted(items.Take(1).Concat(items.Skip(1).Where((a, i) => !items.Take(i + 1).Any(v => ReferenceEquals(v, a))))).ToList();
            for (int i = 0; i < sorted.Count && i < source.Count; i++)
                source[i] = sorted[i];
            while (source.Count > sorted.Count)
                source.RemoveAt(sorted.Count);
        }

        public void Normalize() { NormalizeIDs<CredentialDomain>(_domains); }
    }
}