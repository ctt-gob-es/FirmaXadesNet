#region

using System;
using System.Security.Cryptography;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    /// </summary>
    public sealed class RSAPKCS1SHA512SignatureDescription : SignatureDescription
    {
        /// <summary>
        /// </summary>
        public RSAPKCS1SHA512SignatureDescription()
        {
            KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            DigestAlgorithm = typeof(SHA512Managed).FullName;
            FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).FullName;
            DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var deformatter = new RSAPKCS1SignatureDeformatter(key);
            deformatter.SetHashAlgorithm("SHA512");
            return deformatter;
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var formatter = new RSAPKCS1SignatureFormatter(key);
            formatter.SetHashAlgorithm("SHA512");
            return formatter;
        }
    }
}