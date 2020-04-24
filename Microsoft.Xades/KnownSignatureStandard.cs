namespace Microsoft.Xades
{
    /// <summary>
    ///     Types of signature standards that can be contained in XadesSignedXml class instance
    /// </summary>
    public enum KnownSignatureStandard
    {
        /// <summary>
        ///     XML Digital Signature (XMLDSIG)
        /// </summary>
        XmlDsig,

        /// <summary>
        ///     XML Advanced Electronic Signature (XAdES)
        /// </summary>
        Xades
    }
}