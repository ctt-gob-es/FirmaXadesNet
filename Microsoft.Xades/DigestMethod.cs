#region

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     DigestMethod indicates the digest algorithm
    /// </summary>
    public class DigestMethod
    {
        #region Public properties

        /// <summary>
        ///     Contains the digest algorithm
        /// </summary>
        public string Algorithm { get; set; }

        #endregion

        #region Private variables

        #endregion

        #region Constructors

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(Algorithm)) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            Algorithm = xmlElement.GetAttribute("Algorithm");
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
            //retVal = creationXmlDocument.CreateElement("DigestMethod", XadesSignedXml.XadesNamespaceUri);
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlDSigPrefix, "DigestMethod",
                SignedXml.XmlDsigNamespaceUrl);


            retVal.SetAttribute("Algorithm", Algorithm);

            return retVal;
        }

        #endregion
    }
}