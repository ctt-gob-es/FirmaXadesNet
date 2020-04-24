#region

using System;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     SPUri represents the URL where the copy of the Signature Policy may be
    ///     obtained.  The class derives from SigPolicyQualifier.
    /// </summary>
    public class SPUri : SigPolicyQualifier
    {
        #region Private variables

        #endregion

        #region Constructors

        #endregion

        #region Public properties

        /// <summary>
        ///     Uri for the sig policy qualifier
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     Inherited generic element, not used in the SPUri class
        /// </summary>
        public override XmlElement AnyXmlElement
        {
            get => null; //This does not make sense for SPUri
            set => throw new CryptographicException("Setting AnyXmlElement on a SPUri is not supported");
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public override bool HasChanged()
        {
            var retVal = false;

            if (Uri != null && Uri != "") retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public override void LoadXml(XmlElement xmlElement)
        {
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:SPURI", xmlNamespaceManager);

            Uri = ((XmlElement) xmlNodeList.Item(0)).InnerText;
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public override XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement bufferXmlElement;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("SigPolicyQualifier", XadesSignedXml.XadesNamespaceUri);

            bufferXmlElement = creationXmlDocument.CreateElement("SPURI", XadesSignedXml.XadesNamespaceUri);
            bufferXmlElement.InnerText = Uri;
            retVal.AppendChild(creationXmlDocument.ImportNode(bufferXmlElement, true));

            return retVal;
        }

        #endregion
    }
}