#region

using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.X509;
using XadesNet.Clients;
using XadesNet.Crypto;

#endregion

namespace XadesNet.Upgraders.Parameters
{
    public class UpgradeParameters
    {
        #region Constructors

        public UpgradeParameters()
        {
            OCSPServers = new List<string>();
            _crls = new List<X509Crl>();
            DigestMethod = _defaultDigestMethod;
            _crlParser = new X509CrlParser();
        }

        #endregion

        #region Private variables

        private readonly List<X509Crl> _crls;

        private readonly X509CrlParser _crlParser;

        private readonly DigestMethod _defaultDigestMethod = DigestMethod.SHA1;

        #endregion

        #region Public properties

        public List<string> OCSPServers { get; }

        public IEnumerable<X509Crl> CRL => _crls;

        public DigestMethod DigestMethod { get; set; }

        public TimeStampClient TimeStampClient { get; set; }

        #endregion

        #region Public methods

        public void AddCRL(Stream stream)
        {
            var x509crl = _crlParser.ReadCrl(stream);

            _crls.Add(x509crl);
        }

        public void ClearCRL()
        {
            _crls.Clear();
        }

        #endregion
    }
}