﻿using Rsbc.Dmf.PhsaAdapter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rsbc.Dmf.PhsaAdapter.ViewModels
{
    public class Key : IKey
    {
        public string Base { get; set; }
        public string TypeName { get; set; }
        public string ResourceId { get; set; }
        public string VersionId { get; set; }

        public Key() { }

        public Key(string _base, string type, string resourceid, string versionid)
        {
            Base = _base != null ? _base.TrimEnd('/') : null;
            TypeName = type;
            ResourceId = resourceid;
            VersionId = versionid;
        }

        public static Key Create(string type)
        {
            return new Key(null, type, null, null);
        }

        public static Key Create(string type, string resourceid)
        {
            return new Key(null, type, resourceid, null);
        }

        public static Key Create(string type, string resourceid, string versionid)
        {
            return new Key(null, type, resourceid, versionid);
        }

        public static Key Null
        {
            get
            {
                return default;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var other = (Key)obj;
            return this.ToUriString() == other.ToUriString();
        }

        public override int GetHashCode()
        {
            return this.ToUriString().GetHashCode();
        }

        public static Key ParseOperationPath(string path)
        {
            Key key = new Key();
            path = path.Trim('/');
            int indexOfQueryString = path.IndexOf('?');
            if (indexOfQueryString >= 0)
            {
                path = path.Substring(0, indexOfQueryString);
            }
            string[] segments = path.Split('/');
            if (segments.Length >= 1) key.TypeName = segments[0];
            if (segments.Length >= 2) key.ResourceId = segments[1];
            if (segments.Length == 4 && segments[2] == "_history") key.VersionId = segments[3];
            return key;
        }

        public override string ToString()
        {
            return this.ToUriString();
        }
    }
}
