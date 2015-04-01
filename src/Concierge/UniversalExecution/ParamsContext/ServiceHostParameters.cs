using System;
using System.Collections.Generic;
using System.Linq;

namespace Qoollo.Concierge.UniversalExecution.ParamsContext
{
    internal class ServiceHostParameters
    {
        private readonly List<Uri> _uri;

        public ServiceHostParameters()
        {
            _uri = new List<Uri>();
        }

        public Uri[] Uri
        {
            get
            {
                return _uri.ToArray();
            }
        }

        public string Parameters
        {
            get { return Uri.Aggregate("", (current, address) => current + ("-host " + address.AbsoluteUri + " ")); }
        }

        public int Count
        {
            get { return _uri.Count; }
        }

        public void Add(Uri address)
        {
            _uri.Add(address);
        }

        public void Clear()
        {
            _uri.Clear();
        }
    }
}