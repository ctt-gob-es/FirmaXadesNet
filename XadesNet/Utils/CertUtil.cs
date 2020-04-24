#region

using System;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace XadesNet.Utils
{
    public class CertUtil
    {
        #region Public methods

        public static X509Chain GetCertChain(X509Certificate2 certificate, X509Certificate2[] certificates = null)
        {
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.IgnoreWrongUsage
                }
            };


            if (certificates != null) chain.ChainPolicy.ExtraStore.AddRange(certificates);

            if (!chain.Build(certificate)) throw new Exception("The certification chain cannot be built");

            return chain;
        }

        /// <summary>
        ///     Selecciona un certificado del almacén de certificados
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 SelectCertificate(string message = null, string title = null)
        {
            X509Certificate2 cert = null;

            try
            {
                // Open the store of personal certificates.
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                var collection = store.Certificates;
                var fcollection = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                if (string.IsNullOrEmpty(message)) message = "Select a certificate.";

                if (string.IsNullOrEmpty(title)) title = "Sign file";

                var scollection = X509Certificate2UI.SelectFromCollection(fcollection, title, message,
                    X509SelectionFlag.SingleSelection);

                if (scollection.Count == 1)
                {
                    cert = scollection[0];

                    if (cert.HasPrivateKey == false)
                        throw new Exception("The certificate does not have a private key associated with it.");
                }

                store.Close();
            }
            catch (Exception ex)
            {
                // Thx @rasputino
                throw new Exception("Failed to get private key.", ex);
            }

            return cert;
        }

        #endregion
    }
}