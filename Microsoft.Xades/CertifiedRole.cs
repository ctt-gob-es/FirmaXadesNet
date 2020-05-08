namespace Microsoft.Xades
{
    /// <summary>
    ///     The CertifiedRoles element contains one or more wrapped attribute
    ///     certificates for the signer
    /// </summary>
    public class CertifiedRole : EncapsulatedPKIData
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CertifiedRole()
        {
            TagName = "CertifiedRole";
        }

        #endregion
    }
}