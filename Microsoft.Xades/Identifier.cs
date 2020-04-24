#region

using System;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The Identifier element contains a permanent identifier. Once assigned the
    ///     identifier can never be re-assigned	again. It supports both the mechanism
    ///     that is used to identify objects in ASN.1 and the mechanism that is
    ///     usually used to identify objects in an XML environment.
    /// </summary>
    public class Identifier
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public Identifier()
        {
            Qualifier = KnownQualifier.Uninitalized;
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     The optional Qualifier attribute can be used to provide a hint about the
        ///     applied encoding (values OIDAsURN or OIDAsURI)
        /// </summary>
        public KnownQualifier Qualifier { get; set; }

        /// <summary>
        ///     Identification of the XML environment object
        /// </summary>
        public string IdentifierUri { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (Qualifier != KnownQualifier.Uninitalized) retVal = true;

            if (!string.IsNullOrEmpty(IdentifierUri)) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            if (xmlElement.HasAttribute("Qualifier"))
                Qualifier = (KnownQualifier) Enum.Parse(typeof(KnownQualifier), xmlElement.GetAttribute("Qualifier"),
                    true);
            else
                Qualifier = KnownQualifier.Uninitalized;

            IdentifierUri = xmlElement.InnerText;
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Identifier",
                XadesSignedXml.XadesNamespaceUri);

            if (Qualifier != KnownQualifier.Uninitalized) retVal.SetAttribute("Qualifier", Qualifier.ToString());

            retVal.InnerText = IdentifierUri;

            return retVal;
        }

        #endregion
    }
}