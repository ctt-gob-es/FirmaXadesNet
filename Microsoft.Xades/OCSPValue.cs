namespace Microsoft.Xades
{
    /// <summary>
    ///     This class consist of a sequence of at least one OCSP Response. The
    ///     EncapsulatedOCSPValue element contains the base64 encoding of a
    ///     DER-encoded OCSP Response
    /// </summary>
    public class OCSPValue : EncapsulatedPKIData
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public OCSPValue()
        {
            TagName = "EncapsulatedOCSPValue";
        }

        #endregion
    }
}