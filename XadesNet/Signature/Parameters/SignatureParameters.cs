#region

using System;
using System.Collections.Generic;
using XadesNet.Crypto;

#endregion

namespace XadesNet.Signature.Parameters
{
    public class SignatureParameters
    {
        #region Constructors

        public SignatureParameters()
        {
            XPathTransformations = new List<SignatureXPathExpression>();
            SignatureMethod = _defaultSignatureMethod;
            DigestMethod = _defaultDigestMethod;
        }

        #endregion

        #region Private variables

        private readonly SignatureMethod _defaultSignatureMethod = SignatureMethod.RSAwithSHA256;
        private readonly DigestMethod _defaultDigestMethod = DigestMethod.SHA256;

        #endregion

        #region Public properties

        public Signer Signer { get; set; }

        public SignatureMethod SignatureMethod { get; set; }

        public DigestMethod DigestMethod { get; set; }

        public DateTime? SigningDate { get; set; }

        public SignerRole SignerRole { get; set; }

        public List<SignatureXPathExpression> XPathTransformations { get; }

        public SignaturePolicyInfo SignaturePolicyInfo { get; set; }

        public SignatureXPathExpression SignatureDestination { get; set; }

        public SignaturePackaging SignaturePackaging { get; set; }

        public string InputMimeType { get; set; }

        public string ElementIdToSign { get; set; }

        public string ExternalContentUri { get; set; }

        #endregion
    }
}