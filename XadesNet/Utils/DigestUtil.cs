#region

using Microsoft.Xades;
using DigestMethod = XadesNet.Crypto.DigestMethod;

#endregion

namespace XadesNet.Utils
{
    internal class DigestUtil
    {
        #region Public methods

        public static void SetCertDigest(byte[] rawCert, DigestMethod digestMethod, DigestAlgAndValueType destination)
        {
            using (var hashAlg = digestMethod.GetHashAlgorithm())
            {
                destination.DigestMethod.Algorithm = digestMethod.URI;
                destination.DigestValue = hashAlg.ComputeHash(rawCert);
            }
        }

        public static byte[] ComputeHashValue(byte[] value, DigestMethod digestMethod)
        {
            using (var alg = digestMethod.GetHashAlgorithm())
            {
                return alg.ComputeHash(value);
            }
        }

        #endregion
    }
}