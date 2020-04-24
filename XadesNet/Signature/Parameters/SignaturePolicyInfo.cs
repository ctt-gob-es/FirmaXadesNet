#region

using XadesNet.Crypto;

#endregion

namespace XadesNet.Signature.Parameters
{
    public class SignaturePolicyInfo
    {
        #region Private variables

        private readonly DigestMethod _defaultPolicyDigestAlgorithm = DigestMethod.SHA1;

        #endregion

        #region Constructors

        public SignaturePolicyInfo()
        {
            PolicyDigestAlgorithm = _defaultPolicyDigestAlgorithm;
        }

        #endregion

        #region Public properties

        public string PolicyIdentifier { get; set; }

        public string PolicyHash { get; set; }

        public DigestMethod PolicyDigestAlgorithm { get; set; }

        public string PolicyUri { get; set; }

        #endregion
    }
}