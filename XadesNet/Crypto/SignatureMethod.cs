namespace XadesNet.Crypto
{
    public class SignatureMethod
    {
        #region Constructors

        private SignatureMethod(string name, string uri)
        {
            Name = name;
            URI = uri;
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        public static SignatureMethod RSAwithSHA1 =
            new SignatureMethod("RSAwithSHA1", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");

        public static SignatureMethod RSAwithSHA256 =
            new SignatureMethod("RSAwithSHA256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");

        public static SignatureMethod RSAwithSHA512 =
            new SignatureMethod("RSAwithSHA512", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");

        public string Name { get; }

        public string URI { get; }

        #endregion
    }
}