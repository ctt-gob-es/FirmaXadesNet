#region

using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace XadesNet.Crypto
{
    public class Signer : IDisposable
    {
        #region Private variables

        private bool _disposeCryptoProvider;

        #endregion

        #region Constructors

        public Signer(X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            if (!certificate.HasPrivateKey) throw new Exception("The certificate does not contain any private key");

            Certificate = certificate;

            SetSigningKey(Certificate);
        }

        #endregion

        #region Public methods

        public void Dispose()
        {
            if (_disposeCryptoProvider && SigningKey != null) SigningKey.Clear();
        }

        #endregion

        #region Private methods

        private void SetSigningKey(X509Certificate2 certificate)
        {
            var key = (RSACryptoServiceProvider) certificate.PrivateKey;

            if (key.CspKeyContainerInfo.ProviderName == CryptoConst.MS_STRONG_PROV ||
                key.CspKeyContainerInfo.ProviderName == CryptoConst.MS_ENHANCED_PROV ||
                key.CspKeyContainerInfo.ProviderName == CryptoConst.MS_DEF_PROV ||
                key.CspKeyContainerInfo.ProviderName == CryptoConst.MS_DEF_RSA_SCHANNEL_PROV)
            {
                var CspKeyContainerInfo_Type = typeof(CspKeyContainerInfo);

                var CspKeyContainerInfo_m_parameters =
                    CspKeyContainerInfo_Type.GetField("m_parameters", BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = (CspParameters) CspKeyContainerInfo_m_parameters.GetValue(key.CspKeyContainerInfo);

                var cspparams = new CspParameters(CryptoConst.PROV_RSA_AES, CryptoConst.MS_ENH_RSA_AES_PROV,
                    key.CspKeyContainerInfo.KeyContainerName)
                {
                    KeyNumber = parameters.KeyNumber, Flags = parameters.Flags
                };
                SigningKey = new RSACryptoServiceProvider(cspparams);

                _disposeCryptoProvider = true;
            }
            else
            {
                SigningKey = key;
                _disposeCryptoProvider = false;
            }
        }

        #endregion

        #region Public properties

        public X509Certificate2 Certificate { get; }

        public AsymmetricAlgorithm SigningKey { get; private set; }

        #endregion
    }
}