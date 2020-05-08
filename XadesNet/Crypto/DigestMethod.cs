#region

using System;
using System.Security.Cryptography;

#endregion

namespace XadesNet.Crypto
{
    public class DigestMethod
    {
        #region Constructors

        private DigestMethod(string name, string uri, string oid)
        {
            Name = name;
            URI = uri;
            Oid = oid;
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        public static DigestMethod SHA1 =
            new DigestMethod("SHA1", "http://www.w3.org/2000/09/xmldsig#sha1", "1.3.14.3.2.26");

        public static DigestMethod SHA256 = new DigestMethod("SHA256", "http://www.w3.org/2001/04/xmlenc#sha256",
            "2.16.840.1.101.3.4.2.1");

        public static DigestMethod SHA512 = new DigestMethod("SHA512", "http://www.w3.org/2001/04/xmlenc#sha512",
            "2.16.840.1.101.3.4.2.3");


        public string Name { get; }

        public string URI { get; }

        public string Oid { get; }

        #endregion

        #region Public methods

        public static DigestMethod GetByOid(string oid)
        {
            if (oid == SHA1.Oid)
                return SHA1;
            if (oid == SHA256.Oid)
                return SHA256;
            if (oid == SHA512.Oid)
                return SHA512;
            throw new Exception("Unsupported digest method");
        }

        public HashAlgorithm GetHashAlgorithm()
        {
            if (Name == "SHA1")
                return System.Security.Cryptography.SHA1.Create();
            if (Name == "SHA256")
                return System.Security.Cryptography.SHA256.Create();
            if (Name == "SHA512")
                return System.Security.Cryptography.SHA512.Create();
            throw new Exception("Algorithm not supported");
        }

        #endregion
    }
}