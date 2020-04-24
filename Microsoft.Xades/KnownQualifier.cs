namespace Microsoft.Xades
{
    /// <summary>
    ///     Possible values for Qualifier
    /// </summary>
    public enum KnownQualifier
    {
        /// <summary>
        ///     Value has not been set
        /// </summary>
        Uninitalized,

        /// <summary>
        ///     OID encoded as Uniform Resource Identifier (URI).
        /// </summary>
        OIDAsURI,

        /// <summary>
        ///     OID encoded as Uniform Resource Name (URN)
        /// </summary>
        OIDAsURN
    }
}