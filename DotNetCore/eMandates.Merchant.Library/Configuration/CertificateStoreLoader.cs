﻿using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace eMandates.Merchant.Library.Configuration
{
    public class CertificateStoreLoader : ICertificateLoader
    {
        private readonly ConcurrentDictionary<string, X509Certificate2> _certificates = new ConcurrentDictionary<string, X509Certificate2>();

        public X509Certificate2 Load(string fingerprint)
        {
            return _certificates.GetOrAdd(fingerprint, Get);
        }

        private X509Certificate2 Get(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint))
            {
                return null;
            }

            {
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                if (col.Count != 0)
                {
                    return col[0];
                }
            }

            {
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                if (col.Count != 0)
                {
                    return col[0];
                }
            }

            throw new CommunicatorException(string.Format("Certificate with thumbprint '{0}' not found.", thumbprint));
        }
    }
}
